using Microsoft.Extensions.Logging;
using SAE.J2534;
using System.CommandLine;
using System.Globalization;

var targetOption = new Option<Target>("--target")
{
    Description = "Target module: ecu (0x751/0x752) or bmu (0x761/0x762)",
    DefaultValueFactory = _ => Target.ecu
};

var infoOption = new Option<bool>("--info")
{
    Description = "Read ECU identifiers and exit without dumping memory"
};

var debugOption = new Option<bool>("--debug")
{
    Description = "Enable verbose TX/RX frame logging"
};

var rootCommand = new RootCommand("i-MiEV ECU memory dumper")
{
    targetOption,
    infoOption,
    debugOption
};

rootCommand.SetAction(parseResult =>
{
    var target = parseResult.GetValue(targetOption);
    var infoOnly = parseResult.GetValue(infoOption);
    var debug = parseResult.GetValue(debugOption);
    Run(target, infoOnly, debug);
});

return rootCommand.Parse(args).Invoke();

static void Run(Target target, bool infoOnly, bool debug)
{
    var (canTx, canRx) = target switch
    {
        Target.bmu => (new CanAddress(0x761), new CanAddress(0x762)),
        _ => (new CanAddress(0x751), new CanAddress(0x752)),
    };

    var logger = new StdoutLogger(LogLevel.Trace);

    Console.WriteLine($"i-MiEV {target.ToString().ToUpper()} Reader (TX={canTx}, RX={canRx})");
    Console.WriteLine();

    var discoveredApis = J2534APIFactory.DiscoverAPIs().ToList();
    if (discoveredApis.Count == 0)
    {
        Console.WriteLine("No registered J2534 APIs found. Install your adapter driver/API first.");
        Environment.ExitCode = 2;
        return;
    }

    Console.WriteLine("Discovered J2534 APIs:");
    for (var i = 0; i < discoveredApis.Count; i++)
    {
        var apiInfo = discoveredApis[i];
        Console.WriteLine($"  [{i}] {apiInfo.Name} - {apiInfo.FileName}");
    }
    Console.WriteLine();

    var selectedIndex = discoveredApis.Count == 1
        ? 0
        : PromptForIndex(discoveredApis.Count);

    var selectedApiInfo = discoveredApis[selectedIndex];
    Console.WriteLine($"Using API: {selectedApiInfo.Name}");

    var apiResult = J2534APIFactory.LoadAPI(selectedApiInfo.FileName);
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
    Console.WriteLine($"Opened device: {device.DeviceName}");
    Console.WriteLine($"Version: {device.ApiVersion}, Firmware: {device.FirmwareVersion}, Driver: {device.DriverVersion}");

    var channelResult = device.OpenChannel(
        Protocol.ISO15765,
        Baud.ISO15765_500000,
        ConnectFlag.NONE);

    if (!channelResult.IsSuccess)
    {
        Console.WriteLine($"Failed to open ISO15765 channel: {channelResult.ErrorMessage}");
        Environment.ExitCode = 5;
        return;
    }

    using var channel = channelResult.Value;
    Console.WriteLine("Opened channel at 500 kbps.");
    channel.ClearMessageFilters();
    var result = channel.MeasureBatteryVoltage();
    if (result.IsSuccess)
    {
        Console.WriteLine($"Battery voltage: {result.Value:F2} V");
    }
    else
    {
        Console.WriteLine($"Battery voltage measurement failed: {result.ErrorMessage}");
    }

    /*
    Bit of Mitsubishi Magic here. 
    In standard ISO15765, the responses to requests sent to CAN ID 0x751
    would be sent with CAN ID 0x751 + 8 (0x759)
    However, Mitsubishi's implementation sends responses using CAN ID (0x752)
    */
    var filter = new MessageFilter(
        UserFilterType.STANDARDISO15765,
        canTx.Bytes);
    filter.Pattern = canRx.Bytes;
    var filterResult = channel.StartMessageFilter(filter);
    if (filterResult.IsSuccess)
    {
        Console.WriteLine($"Started ISO15765 filter for RX ID {canRx}: {filterResult.Value}");
    }
    else
    {
        Console.WriteLine($"Filter setup failed (continuing): {filterResult.ErrorMessage}");
    }

    channel.ClearRxBuffer();

    var kwp = new Kwp2000(channel, canTx, canRx) { Debug = debug };

    // Enter diagnostic session 0x92
    var sessionResult = kwp.StartDiagnosticSession(0x92);
    if (!sessionResult.Success)
    {
        Console.WriteLine($"Session request failed: {sessionResult.ErrorMessage}");
        Environment.ExitCode = 6;
        return;
    }

    Console.WriteLine($"Session response: {BitConverter.ToString(sessionResult.ResponsePayload!)}");

    // Read all ECU identification options and output to console + file
    var idFileName = $"{target}_identification.txt";
    using var idFile = new StreamWriter(idFileName);
    var line = $"{target.ToString().ToUpper()} Identification for CAN ID {canTx} at {DateTime.Now}";
    Console.WriteLine(line);
    idFile.WriteLine(line);

    foreach (var option in Enum.GetValues<ECUIdentificationOption>())
    {
        var idResult = kwp.ReadECUIdentification(option);
        var label = option.ToString();
        var formatted = new System.Text.StringBuilder();
        if (!idResult.Success)
        {
            formatted.Append($"FAILED - {idResult.ErrorMessage}");
        }
        else
        {
            // Skip the 2-byte header (service echo + option echo)
            var raw = idResult.ResponsePayload!;
            var data = raw.Length > 2 ? raw.AsSpan(2) : ReadOnlySpan<byte>.Empty;

            foreach (var b in data)
            {
                char c = (char)b;
                bool printChar = Char.IsAsciiDigit(c) || Char.IsAsciiLetterUpper(c);
                formatted.Append(printChar ? c : $"[{b:X2}]");
            }
        }
        line = $"{label}: {formatted}";
        Console.WriteLine(line);
        idFile.WriteLine(line);
    }
    idFile.Flush();
    Console.WriteLine($"Identification written to {idFileName}");
    Console.WriteLine();

    if (infoOnly)
        return;

    kwp.StartDiagnosticSession(0x85);

    var response = kwp.ControlDTCSetting(ResponseRequired.NoResponseRequired, DtcGroup.AllDtcs, DtcSettingMode.Off);
    if (!response.Success)
    {
        Console.WriteLine($"ControlDTCSetting request failed: {response.ErrorMessage}");
        Environment.ExitCode = 8;
        return;
    }
    Thread.Sleep(500);

    response = kwp.DisableNormalMessageTransmission(ResponseRequired.NoResponseRequired);
    if (!response.Success)
    {
        Console.WriteLine($"DisableNormalMessageTransmission request failed: {response.ErrorMessage}");
        Environment.ExitCode = 9;
        return;
    }
    Thread.Sleep(500);


    channel.ClearRxBuffer();
    const byte blockSize = 255;
    uint startAddress = 0x00000;
    uint endAddress = 0xfffff;

    var dumpFileName = $"{target}_dump.txt";
    if (File.Exists(dumpFileName))
    {
        var lastLine = File.ReadLines(dumpFileName).LastOrDefault();
        if (lastLine is not null && lastLine.StartsWith("0x"))
        {
            var colonIndex = lastLine.IndexOf(':');
            if (colonIndex > 0 &&
                uint.TryParse(lastLine.AsSpan(2, colonIndex - 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var lastAddress))
            {
                startAddress = lastAddress + blockSize;
                Console.WriteLine($"Resuming dump from 0x{startAddress:X5} (found {dumpFileName})");
            }
        }
    }

    using var dumpFile = new StreamWriter(dumpFileName, append: true);
    var iterations = 0;
    for (uint address = startAddress; address <= endAddress; address += blockSize)
    {
        byte nextBlockSize = (byte)Math.Min(blockSize, endAddress - address + 1);
        var readResult = kwp.ReadMemoryByAddress(address, nextBlockSize, timeoutMs: 750);
        if (!readResult.Success)
        {
            Console.WriteLine($"ReadMemoryByAddress failed at 0x{address:X5}: {readResult.ErrorMessage}");
            Environment.ExitCode = 10;
            return;
        }

        var payload = readResult.ResponsePayload!;
        payload = payload.Skip(1).ToArray(); // skip the 1-byte service echo at the start of the payload
        if (payload.Length != nextBlockSize)
        {
            Console.WriteLine($"ReadMemoryByAddress returned insufficient data at 0x{address:X5}: {BitConverter.ToString(payload)}");
            Environment.ExitCode = 10;
            return;
        }
        var dumpLine = $"0x{address:X5}: {BitConverter.ToString(payload).Replace("-", " ")}";
        if(iterations % 16 == 0 ){
            Console.WriteLine();            
            Console.Write($"0x{address:X5}");
        }
        Console.Write(".");
        dumpFile.WriteLine(dumpLine);
        dumpFile.Flush();
        iterations++;
    }

    Console.WriteLine($"Dump complete. Written to {dumpFileName}");
}

static int PromptForIndex(int apiCount)
{
    while (true)
    {
        Console.Write($"Select API index (0-{apiCount - 1}): ");
        var input = Console.ReadLine();

        if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
            && index >= 0
            && index < apiCount)
        {
            return index;
        }

        Console.WriteLine("Invalid selection. Please enter a valid index.");
    }
}

enum Target { ecu, bmu }

