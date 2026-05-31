using SAE.J2534;
using System.Buffers.Binary;
using System.CommandLine;
using System.Globalization;
using System.Reflection;

var fileArgument = new Argument<FileInfo>("firmware")
{
    Description = "Path to the raw binary firmware file"
};
var debugOption = new Option<bool>("--debug")
{
    Description = "Enable verbose TX/RX frame logging"
};

var rootCommand = new RootCommand("i-MiEV ECU firmware uploader")
{
    fileArgument,
    debugOption
};

rootCommand.SetAction(parseResult =>
{
    var file = parseResult.GetValue(fileArgument)!;
    var debug = parseResult.GetValue(debugOption);
    try{
        Run(file, debug);
    }catch(Exception ex){
        Console.WriteLine($"Error: {ex.Message}");
        Environment.ExitCode = 1;
    }
});

return rootCommand.Parse(args).Invoke();

static void Run(FileInfo firmwareFile, bool debug)
{
    const uint FlashStart = 0x008000;
    const uint FlashEnd = 0x100000;
    const int ChunkSize = 256;
    const uint ChecksumAddress = 0x200000;
    const uint Swil1Address = 0x808538;
    const uint Swil2Address = 0x808484;
    const byte ByteMapEncrypted = 0x01;
    const byte NoEncryption = 0x00;

    Console.WriteLine("i-MiEV ECU Firmware Uploader");
    Console.WriteLine();

    // --- 1. Validate firmware file ---
    if (!firmwareFile.Exists)
    {
        Console.WriteLine($"File not found: {firmwareFile.FullName}");
        Environment.ExitCode = 1;
        return;
    }

    var firmware = File.ReadAllBytes(firmwareFile.FullName);
    Console.WriteLine($"Firmware: {firmwareFile.Name} ({firmware.Length:N0} bytes)");

    if (firmware.Length < 0x100000)
    {
        Console.WriteLine($"File too small: {firmware.Length} bytes (expected at least 1,048,576)");
        Environment.ExitCode = 1;
        return;
    }

    bool bootAreaErased = true;
    for (int i = 0; i < 0x8000; i++)
    {
        if (firmware[i] != 0xFF) { bootAreaErased = false; break; }
    }
    if (bootAreaErased)
    {
        Console.WriteLine("Invalid firmware: boot area (0x0000-0x7FFF) is erased (all 0xFF)");
        Console.WriteLine("Although the boot area is not programmed, the checksum calculation requires it.\nPlease ensure you are using the correct input file.");
        Environment.ExitCode = 1;
        return;
    }

    if (firmware[0x8000] != 0x5A || firmware[0x8001] != 0xA5)
    {
        Console.WriteLine($"Invalid firmware: expected 5A A5 at offset 0x8000, got {firmware[0x8000]:X2} {firmware[0x8001]:X2}");
        Environment.ExitCode = 1;
        return;
    }

    if (firmware[0xFFFFE] != 0xA5 || firmware[0xFFFFF] != 0x5A)
    {
        Console.WriteLine($"Invalid firmware: expected A5 5A at offset 0xFFFFE, got {firmware[0xFFFFE]:X2} {firmware[0xFFFFF]:X2}");
        Environment.ExitCode = 1;
        return;
    }


    // --- Checksum calculation ---
    uint currentChecksum = ChecksumCalculator.Checksum(firmware, includeChecksumOffset: false);
    Console.WriteLine($"Current checksum (excluding 0xFFFF8): 0x{currentChecksum:X8}");

    uint fillValue = ChecksumCalculator.RequiredFillValue(firmware);
    Console.WriteLine($"Required adjustment value: 0x{fillValue:X8}");

    BinaryPrimitives.WriteUInt32BigEndian(firmware.AsSpan(ChecksumCalculator.ChecksumOffset, 4), fillValue);

    uint finalChecksum = ChecksumCalculator.Checksum(firmware, includeChecksumOffset: true);
    Console.WriteLine($"Final checksum (with adjustment): 0x{finalChecksum:X8}");
    if (finalChecksum != ChecksumCalculator.TargetChecksum)
    {
        Console.WriteLine($"Checksum mismatch: expected 0x{ChecksumCalculator.TargetChecksum:X8}, got 0x{finalChecksum:X8}");
        Environment.ExitCode = 1;
        return;
    }

    // --- 2. Load embedded SWIL resources ---
    var swil1 = LoadResource("swil1.bin");
    var swil1Checksum = LoadResource("swil1_checksum.bin");
    var swil2 = LoadResource("swil2.bin");
    var swil2Checksum = LoadResource("swil2_checksum.bin");

    // --- 3. Build flash regions (contiguous runs of non-0xFF 256-byte chunks) ---
    var regions = new List<(uint Address, byte[] Data)>();
    uint regionStart = 0;
    List<byte>? regionData = null;

    for (uint offset = FlashStart; offset < FlashEnd; offset += ChunkSize)
    {
        int len = (int)Math.Min(ChunkSize, FlashEnd - offset);
        bool allFF = true;
        for (int i = 0; i < len; i++)
        {
            if (firmware[offset + i] != 0xFF) { allFF = false; break; }
        }

        if (!allFF)
        {
            if (regionData is null)
            {
                regionStart = offset;
                regionData = [];
            }
            regionData.AddRange(firmware.AsSpan((int)offset, len));
        }
        else if (regionData is not null)
        {
            regions.Add((regionStart, regionData.ToArray()));
            regionData = null;
        }
    }

    if (regionData is not null)
        regions.Add((regionStart, regionData.ToArray()));

    Console.WriteLine($"Flash regions: {regions.Count}");
    uint totalBytes = 0;
    foreach (var (addr, data) in regions)
    {
        Console.WriteLine($"  0x{addr:X6}: {data.Length:N0} bytes ({data.Length / ChunkSize} chunks)");
        totalBytes += (uint)data.Length;
    }

    // Compute checksum: uint16 sum of all bytes actually sent
    uint checksumAccum = 0;
    foreach (var (_, data) in regions)
        foreach (byte b in data)
            checksumAccum += b;
    ushort checksum = (ushort)(checksumAccum & 0xFFFF);

    Console.WriteLine($"Total: {totalBytes:N0} bytes, Checksum: 0x{checksum:X4}");
    Console.WriteLine();

    // --- 4. Select J2534 adapter ---
    var canTx = new CanAddress(0x751);
    var canRx = new CanAddress(0x752);

    var discoveredApis = J2534APIFactory.DiscoverAPIs().ToList();
    if (discoveredApis.Count == 0)
    {
        Console.WriteLine("No registered J2534 APIs found. Install your adapter driver first.");
        Environment.ExitCode = 2;
        return;
    }

    Console.WriteLine("Discovered J2534 APIs:");
    for (int i = 0; i < discoveredApis.Count; i++)
        Console.WriteLine($"  [{i}] {discoveredApis[i].Name} - {discoveredApis[i].FileName}");
    Console.WriteLine();

    int selectedIndex = discoveredApis.Count == 1
        ? 0
        : PromptForIndex(discoveredApis.Count);

    var selectedApi = discoveredApis[selectedIndex];
    Console.WriteLine($"Using API: {selectedApi.Name}");

    // --- 5. Open J2534 connection ---
    var apiResult = J2534APIFactory.LoadAPI(selectedApi.FileName);
    if (!apiResult.IsSuccess)
    {
        Console.WriteLine($"Failed to load API: {apiResult.ErrorMessage}");
        Environment.ExitCode = 3;
        return;
    }

    using var api = apiResult.Value;

    var deviceResult = api.OpenDevice();
    if (!deviceResult.IsSuccess)
    {
        Console.WriteLine($"Failed to open device: {deviceResult.ErrorMessage}");
        Environment.ExitCode = 4;
        return;
    }

    using var device = deviceResult.Value;
    Console.WriteLine($"Device: {device.DeviceName}");

    var channelResult = device.OpenChannel(Protocol.ISO15765, Baud.ISO15765_500000, ConnectFlag.NONE);
    if (!channelResult.IsSuccess)
    {
        Console.WriteLine($"Failed to open channel: {channelResult.ErrorMessage}");
        Environment.ExitCode = 5;
        return;
    }

    using var channel = channelResult.Value;
    Console.WriteLine("ISO15765 channel opened at 500 kbps.");
    channel.ClearMessageFilters();

    var battResult = channel.MeasureBatteryVoltage();
    if (!battResult.IsSuccess)
    {
        Console.WriteLine($"Failed to measure battery voltage: {battResult.ErrorMessage}");
        Environment.ExitCode = 6;
        return;
    }
    double batteryVoltage = battResult.Value / 1000.0; // J2534 READ_VBATT returns millivolts
    Console.WriteLine($"Battery voltage: {batteryVoltage:F2} V");
    if (batteryVoltage < 10.0)
    {
        Console.WriteLine("Battery voltage too low (minimum 10 V). Aborting.");
        Environment.ExitCode = 6;
        return;
    }

    // --- 6. Mitsubishi flow control filter (responses on CAN ID+1, not +8) ---
    var filter = new MessageFilter
    {
        FilterType = Filter.FLOW_CONTROL_FILTER,
        Mask = [0x00, 0x00, 0x07, 0xFF],
        Pattern = [0x00, 0x00, 0x07, 0x52],
        FlowControl = [0x00, 0x00, 0x07, 0x51],
        TxFlags = TxFlag.NONE
    };
    channel.ClearMessageFilters();
    // var passFilter = new MessageFilter(UserFilterType.PASSALL, Array.Empty<byte>());
    // var filterResult = channel.StartMessageFilter(passFilter);
    // if (!filterResult.IsSuccess)
    // {
    //     Console.WriteLine($"Filter setup failed: {filterResult.ErrorMessage}");
    //     Environment.ExitCode = 6;
    //     return;
    // }
    var filterResult = channel.StartMessageFilter(filter, Protocol.ISO15765);
    if (!filterResult.IsSuccess)
    {
        Console.WriteLine($"Filter setup failed: {filterResult.ErrorMessage}");
        Environment.ExitCode = 6;
        return;
    }
    //channel.SetConfig(ConfigParameter.ISO15765_STMIN, 10);
    channel.ClearRxBuffer();

    var kwp = new Kwp2000(channel, canTx, canRx) { Debug = debug };

    // --- 7. Identify ECU ---
    Console.WriteLine();
    Console.WriteLine("Reading ECU identification...");
    var idResult = kwp.ReadECUIdentification(ECUIdentificationOption.ECUIdentification);
    if (!idResult.Success)
    {
        Console.WriteLine($"Failed to read ECU identification: {idResult.ErrorMessage}");
        Environment.ExitCode = 7;
        return;
    }
    PrintIdentification("ECU ID", idResult.ResponsePayload!);

    var codeResult = kwp.ReadECUIdentification(ECUIdentificationOption.ECUCodeIdentification);
    if (!codeResult.Success)
    {
        Console.WriteLine($"Failed to read ECU code identification: {codeResult.ErrorMessage}");
        Environment.ExitCode = 7;
        return;
    }
    PrintIdentification("Code ID", codeResult.ResponsePayload!);

    // --- 8. Final confirmation ---
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║     WARNING: ECU REPROGRAMMING       ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.WriteLine($"  File:     {firmwareFile.Name}");
    Console.WriteLine($"  Regions:  {regions.Count}");
    Console.WriteLine($"  Size:     {totalBytes:N0} bytes");
    Console.WriteLine($"  Checksum: 0x{checksum:X4}");
    Console.WriteLine();
    Console.Write("Type YES to proceed with reprogramming: ");
    if (!string.Equals(Console.ReadLine(), "YES", StringComparison.Ordinal))
    {
        Console.WriteLine("Aborted.");
        return;
    }

    // ============================================================
    //  REPROGRAMMING SEQUENCE (derived from protocol capture)
    // ============================================================
    // --- 9. Enter diagnostic session (0x92) ---
    Console.Write("Entering diagnostic session (0x92) ... ");
    var result = kwp.StartDiagnosticSession(0x92, timeoutMs: 2000);
    if (!result.Success) { Fail(10, result); return; }
    Console.WriteLine("OK");
    Thread.Sleep(500);
    // --- 9. Enter programming session (0x85) ---
    Console.Write("Entering programming session (0x85) ... ");
    result = kwp.StartDiagnosticSession(0x85, timeoutMs: 2000);
    if (!result.Success) { Fail(10, result); return; }
    Console.WriteLine("OK");
    Thread.Sleep(500);

    // --- 10. Security Access (level 0x05/0x06) ---
    Console.Write("Requesting security seed... ");
    result = kwp.SecurityAccessRequestSeed(0x05);
    if (!result.Success) { Fail(11, result); return; }

    // Response payload: [67 05 seed0 seed1 seed2 seed3]
    var seedPayload = result.ResponsePayload!;
    if (seedPayload.Length < 6)
    {
        Console.WriteLine($"FAILED: unexpected seed response length {seedPayload.Length}");
        Environment.ExitCode = 11;
        return;
    }

    var seed = seedPayload.AsSpan(2, 4);
    Console.WriteLine($"Seed: {BitConverter.ToString(seed.ToArray())}");

    Console.Write("Sending key... ");
    var key = SecurityAccess.ComputeKey(seed, 0x05);
    result = kwp.SecurityAccessSendKey(0x06, key);
    if (!result.Success) { Fail(12, result); return; }
    Console.WriteLine("OK");
    Thread.Sleep(500);

    // --- 11. Write programming date stamp (service 0x3B, record 0x9A) ---
    Console.Write("Writing programming date... ");
    var now = DateTime.Now;
    byte bcdYear = ToBcd(now.Year % 100);
    byte bcdMonth = ToBcd(now.Month);
    byte bcdDay = ToBcd(now.Day);
    result = kwp.WriteDataByLocalIdentifier(0x9A,
        [0x01, 0x01, 0x00, bcdYear, bcdMonth, bcdDay, 0x00, 0x00, 0x00, 0x00],
        timeoutMs: 2000);
    Console.WriteLine($"returned: {result}");
    if (!result.Success) { Fail(13, result); return; }
    Console.WriteLine("OK");
    Thread.Sleep(500);

    // --- 12. Upload SWIL1 + checksum ---
    Console.Write("Uploading SWIL1... ");
    if (!UploadRegion(kwp, Swil1Address, swil1, ByteMapEncrypted)) { Environment.ExitCode = 14; return; }
    if (!UploadRegion(kwp, ChecksumAddress, swil1Checksum, ByteMapEncrypted)) { Environment.ExitCode = 14; return; }
    Console.WriteLine("OK");

    // --- 13. Upload validation (routine 0xE1, option 0x01) ---
    Console.Write("Upload validation... ");
    result = kwp.StartRoutine(0xE1, [0x01]);
    if (!result.Success) { Fail(15, result); return; }
    Console.WriteLine("OK");

    Console.WriteLine();
    Console.WriteLine("=== POINT OF NO RETURN ===");
    Console.WriteLine();

    // --- 14. Erase flash (routine 0xE0) ~15 seconds ---
    Console.Write("Erasing flash (please wait)... ");
    result = kwp.StartRoutine(0xE0, [], pendingTimeoutMs: 60_000);
    if (!result.Success) { Fail(16, result); return; }
    Console.WriteLine("OK");

    // --- 15. Upload SWIL2 + checksum ---
    Console.Write("Uploading SWIL2... ");
    if (!UploadRegion(kwp, Swil2Address, swil2, ByteMapEncrypted)) { Environment.ExitCode = 17; return; }
    if (!UploadRegion(kwp, ChecksumAddress, swil2Checksum, ByteMapEncrypted)) { Environment.ExitCode = 17; return; }
    Console.WriteLine("OK");

    // --- 16. Upload validation ---
    Console.Write("Upload validation... ");
    result = kwp.StartRoutine(0xE1, [0x01]);
    if (!result.Success) { Fail(18, result); return; }
    Console.WriteLine("OK");

    // --- 17. Program flash regions ---
    Console.WriteLine("Programming flash...");
    for (int r = 0; r < regions.Count; r++)
    {
        var (addr, rdata) = regions[r];
        int chunks = rdata.Length / ChunkSize;
        Console.Write($"  [{r + 1}/{regions.Count}] 0x{addr:X6} ({rdata.Length:N0} bytes, {chunks} chunks): ");

        if (!UploadRegion(kwp, addr, rdata, NoEncryption, showProgress: true))
        {
            Environment.ExitCode = 19;
            return;
        }

        Console.WriteLine(" OK");
    }

    // --- 18. Upload final checksum ---
    Console.Write($"Uploading checksum 0x{checksum:X4}... ");
    byte[] checksumBytes = [(byte)(checksum >> 8), (byte)(checksum & 0xFF)];
    if (!UploadRegion(kwp, ChecksumAddress, checksumBytes, NoEncryption))
    {
        Environment.ExitCode = 20;
        return;
    }
    Console.WriteLine("OK");

    // --- 19. Finalize programming (routine 0xE1, option 0x02) ---
    Console.Write("Finalizing... ");
    result = kwp.StartRoutine(0xE1, [0x02], pendingTimeoutMs: 5_000);
    if (!result.Success) { Fail(21, result); return; }
    Console.WriteLine("OK");

    // --- 20. Reset ECU ---
    Console.Write("Resetting ECU... ");
    result = kwp.ECUReset(0x01, timeoutMs: 2000);
    if (!result.Success) { Fail(22, result); return; }
    Console.WriteLine("OK");

    // --- 21. Verify post-reset ---
    Console.WriteLine();
    Console.WriteLine("Waiting for ECU to restart (5 seconds)...");
    Thread.Sleep(5_000);

    Console.WriteLine("Reading ECU identification...");
    for (int attempt = 0; attempt < 3; attempt++)
    {
        idResult = kwp.ReadECUIdentification(ECUIdentificationOption.ECUIdentification, timeoutMs: 2000);
        if (idResult.Success)
        {
            PrintIdentification("ECU ID", idResult.ResponsePayload!);
            break;
        }
        Thread.Sleep(2000);
    }

    codeResult = kwp.ReadECUIdentification(ECUIdentificationOption.ECUCodeIdentification, timeoutMs: 2000);
    if (codeResult.Success)
        PrintIdentification("Code ID", codeResult.ResponsePayload!);

    Console.WriteLine();
    Console.WriteLine("Reprogramming complete.");
}

// ── Helper functions ──────────────────────────────────────────

static bool UploadRegion(Kwp2000 kwp, uint address, byte[] data, byte compressEncryptType, bool showProgress = false)
{
    const int ChunkSize = 256;

    var result = kwp.RequestDownload(address, compressEncryptType, (uint)data.Length);
    if (!result.Success)
    {
        Console.WriteLine($"RequestDownload failed: {result.ErrorMessage}");
        return false;
    }

    int totalChunks = (data.Length + ChunkSize - 1) / ChunkSize;
    for (int i = 0; i < totalChunks; i++)
    {
        int offset = i * ChunkSize;
        int len = Math.Min(ChunkSize, data.Length - offset);
        var chunk = data.AsSpan(offset, len);

        result = kwp.TransferData(chunk);
        if (!result.Success)
        {
            Console.WriteLine($"TransferData failed at chunk {i + 1}/{totalChunks}: {result.ErrorMessage}");
            return false;
        }

        if (showProgress && totalChunks > 1 && (i + 1) % 16 == 0)
            Console.Write(".");
    }

    result = kwp.RequestTransferExit();
    if (!result.Success)
    {
        Console.WriteLine($"RequestTransferExit failed: {result.ErrorMessage}");
        return false;
    }

    return true;
}

static byte ToBcd(int value) => (byte)(((value / 10) << 4) | (value % 10));

static void Fail(int exitCode, Kwp2000Result result)
{
    Console.WriteLine($"FAILED: {result.ErrorMessage}");
    Environment.ExitCode = exitCode;
}

static byte[] LoadResource(string name)
{
    var asm = Assembly.GetExecutingAssembly();
    var resName = asm.GetManifestResourceNames()
        .First(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));
    using var stream = asm.GetManifestResourceStream(resName)!;
    using var ms = new MemoryStream();
    stream.CopyTo(ms);
    return ms.ToArray();
}

static void PrintIdentification(string label, byte[] payload)
{
    var data = payload.Length > 2 ? payload.AsSpan(2) : ReadOnlySpan<byte>.Empty;
    var sb = new System.Text.StringBuilder();
    foreach (byte b in data)
    {
        char c = (char)b;
        sb.Append(char.IsAsciiLetterOrDigit(c) || c == ' ' ? c : $"[{b:X2}]");
    }
    Console.WriteLine($"  {label}: {sb}");
}

static int PromptForIndex(int count)
{
    while (true)
    {
        Console.Write($"Select API index (0-{count - 1}): ");
        if (int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx)
            && idx >= 0 && idx < count)
            return idx;
        Console.WriteLine("Invalid selection.");
    }
}
