using SAE.J2534;

enum ECUIdentificationOption : byte
{
    ECUIdentification = 0x87,
    OriginalVIN = 0x88,
    DiagnosticVariantCode = 0x89,
    CurrentVIN = 0x90,
    ECUCodeFingerprint = 0x9A,
    ECUCodeIdentification = 0x9C,
    BootCodeIdentification = 0x9E
}

enum ResponseRequired : byte
{
    ResponseRequired = 0x01,
    NoResponseRequired = 0x02
}

enum DtcGroup : ushort
{
    AllPowertrain = 0x0000,
    AllChassis = 0x4000,
    AllBody = 0x8000,
    AllNetwork = 0xC000,
    AllDtcs = 0xFF00
}

enum DtcSettingMode : byte
{
    On = 0x01,
    Off = 0x02
}

/// <summary>
/// Result of a KWP2000 command exchange.
/// </summary>
record Kwp2000Result(
    bool Success,
    byte[]? ResponsePayload,
    byte? NegativeResponseCode,
    string? ErrorMessage)
{
    public static Kwp2000Result Positive(byte[] payload) =>
        new(true, payload, null, null);

    public static Kwp2000Result Negative(byte nrc, byte[] fullPayload) =>
        new(false, fullPayload, nrc, $"Negative response: NRC 0x{nrc:X2}");

    public static Kwp2000Result Error(string message) =>
        new(false, null, null, message);
}

class Kwp2000(J2534Channel channel, CanAddress txCanId, CanAddress rxCanId)
{
    /// <summary>
    /// Default receive timeout in milliseconds used when no explicit timeout is provided.
    /// </summary>
    public int DefaultTimeoutMs { get; set; } = 1000;

    /// <summary>
    /// Default timeout in milliseconds for waiting after an NRC 0x78 (response pending).
    /// </summary>
    public int DefaultPendingTimeoutMs { get; set; } = 30_000;

    /// <summary>
    /// When true, TX and RX frame data is written to the console.
    /// </summary>
    public bool Debug { get; set; }

    // ── Low-level transport ──────────────────────────────────────────

    /// <summary>
    /// Transmits a KWP2000 service request over ISO15765. Does not read any response.
    /// </summary>
    Kwp2000Result Send(byte serviceId, ReadOnlySpan<byte> data)
    {
        //channel.ClearRxBuffer();

        var payload = new byte[4 + 1 + data.Length];
        txCanId.Bytes.CopyTo(payload, 0);
        payload[4] = serviceId;
        if (data.Length > 0)
            data.CopyTo(payload.AsSpan(5));

        if (Debug)
            Console.WriteLine($"TX KWP [{serviceId:X2}]: {BitConverter.ToString(payload)}");

        var sendResult = channel.SendMessage(payload, TxFlag.NONE, 1000);
        if (!sendResult.IsSuccess)
            return Kwp2000Result.Error($"Send failed: {sendResult.Status}");

        return Kwp2000Result.Positive([]);
    }

    /// <summary>
    /// Reads raw response bytes from the channel, accumulating until at least
    /// <paramref name="minimumBytes"/> are received or timeout expires.
    /// </summary>
    public Kwp2000Result Receive(int timeoutMs = -1, int minimumBytes = 1)
    {
        if (timeoutMs < 0) timeoutMs = DefaultTimeoutMs;

        const int PollIntervalMs = 100;
        var resultBytes = new List<byte>();
        int elapsedMs = 0;

        while (resultBytes.Count < minimumBytes)
        {
            if (elapsedMs >= timeoutMs)
            {
                if (resultBytes.Count > 0)
                    break;
                return Kwp2000Result.Error("Timeout waiting for response.");
            }

            var readResult = channel.ReadMessages(1, PollIntervalMs);
            Console.WriteLine($"ReadMessages result: {readResult}");
            elapsedMs += PollIntervalMs;

            if (!readResult.IsSuccess)
            {
                if (readResult.IsTimeout)
                    continue;
                if (readResult.IsBufferEmpty)
                    continue;
                return Kwp2000Result.Error($"Read failed: {readResult.Status}");
            }

            foreach (var msg in readResult.Messages)
            {
                Console.WriteLine($"Received message: {BitConverter.ToString(msg.Data)}");
                var d = msg.Data;
                if (d.Length < 5)
                    continue;

                var canId = (d[0] << 24) | (d[1] << 16) | (d[2] << 8) | d[3];
                if (canId != rxCanId.Value)
                    continue;

                for (int i = 4; i < d.Length; i++)
                    resultBytes.Add(d[i]);
            }
        }

        if (resultBytes.Count == 0)
            return Kwp2000Result.Error($"No response from CAN ID {rxCanId}.");

        var responsePayload = resultBytes.ToArray();
        if (Debug)
            Console.WriteLine($"RX KWP: {BitConverter.ToString(responsePayload)}");

        return Kwp2000Result.Positive(responsePayload);
    }

    /// <summary>
    /// Classifies a raw response payload as Positive, Negative, or error.
    /// </summary>
    static Kwp2000Result Interpret(byte serviceId, byte[] payload)
    {
        byte expectedPositive = (byte)(serviceId + 0x40);

        if (payload.Length >= 1 && payload[0] == expectedPositive)
            return Kwp2000Result.Positive(payload);

        if (payload.Length >= 3 && payload[0] == 0x7F && payload[1] == serviceId)
            return Kwp2000Result.Negative(payload[2], payload);

        return Kwp2000Result.Error($"Unexpected response format: {BitConverter.ToString(payload)}");
    }

    // ── Orchestration ────────────────────────────────────────────────

    /// <summary>
    /// Sends a KWP2000 service request and waits for a final response.
    /// Automatically handles NRC 0x78 (response pending) by re-polling.
    /// </summary>
    public Kwp2000Result SendCommand(
        byte serviceId,
        ReadOnlySpan<byte> data = default,
        int timeoutMs = -1,
        bool expectResponse = true,
        int minimumResponseBytes = 1,
        int pendingTimeoutMs = -1)
    {
        if (timeoutMs < 0) timeoutMs = DefaultTimeoutMs;
        if (pendingTimeoutMs < 0) pendingTimeoutMs = DefaultPendingTimeoutMs;
Console.WriteLine($"Sending service 0x{serviceId:X2} with data [{BitConverter.ToString(data.ToArray())}], timeout {timeoutMs} ms, expectResponse={expectResponse}");
        var sendResult = Send(serviceId, data);
Console.WriteLine($"Send result: {sendResult}");
        if (!sendResult.Success)
            return sendResult;

        if (!expectResponse)
            return Kwp2000Result.Positive([]);
Console.WriteLine($"Waiting for response with timeout {timeoutMs} ms and minimum bytes {minimumResponseBytes}...");
        var rxResult = Receive(timeoutMs, minimumResponseBytes);
Console.WriteLine($"Receive result: {rxResult}");
        if (rxResult.ResponsePayload is null)
            return rxResult;

        var result = Interpret(serviceId, rxResult.ResponsePayload);

        while (result.NegativeResponseCode == 0x78)
        {
            rxResult = Receive(pendingTimeoutMs);
            if (rxResult.ResponsePayload is null)
                return rxResult;

            result = Interpret(serviceId, rxResult.ResponsePayload);
        }

        return result;
    }

    // ── KWP2000 service methods ──────────────────────────────────────

    /// <summary>
    /// KWP2000 StartDiagnosticSession (service 0x10).
    /// </summary>
    public Kwp2000Result StartDiagnosticSession(byte sessionType, int timeoutMs = -1)
    {
        return SendCommand(0x10, [sessionType], timeoutMs);
    }

    /// <summary>
    /// KWP2000 ReadECUIdentification (service 0x1A).
    /// </summary>
    public Kwp2000Result ReadECUIdentification(ECUIdentificationOption identificationOption, int timeoutMs = -1)
    {
        return SendCommand(0x1A, [(byte)identificationOption], timeoutMs);
    }

    /// <summary>
    /// KWP2000 DisableNormalMessageTransmission (service 0x28).
    /// </summary>
    public Kwp2000Result DisableNormalMessageTransmission(ResponseRequired responseRequired = ResponseRequired.ResponseRequired, int timeoutMs = -1)
    {
        return SendCommand(0x28, [(byte)responseRequired], timeoutMs,
            expectResponse: responseRequired == ResponseRequired.ResponseRequired);
    }

    /// <summary>
    /// KWP2000 ControlDTCSetting (service 0x85).
    /// </summary>
    public Kwp2000Result ControlDTCSetting(
        ResponseRequired responseRequired,
        DtcGroup dtcGroup,
        DtcSettingMode settingMode,
        int timeoutMs = -1)
    {
        ushort group = (ushort)dtcGroup;
        byte[] payload =
        [
            (byte)responseRequired,
            (byte)((group >> 8) & 0xFF),
            (byte)(group & 0xFF),
            (byte)settingMode
        ];
        return SendCommand(0x85, payload, timeoutMs,
            expectResponse: responseRequired == ResponseRequired.ResponseRequired);
    }

    /// <summary>
    /// KWP2000 WriteDataByLocalIdentifier (service 0x3B).
    /// </summary>
    public Kwp2000Result WriteDataByLocalIdentifier(byte recordLocalIdentifier, ReadOnlySpan<byte> data, int timeoutMs = -1)
    {
        var payload = new byte[1 + data.Length];
        payload[0] = recordLocalIdentifier;
        data.CopyTo(payload.AsSpan(1));
        return SendCommand(0x3B, payload, timeoutMs);
    }

    /// <summary>
    /// KWP2000 SecurityAccess — request seed (service 0x27, odd access level).
    /// </summary>
    public Kwp2000Result SecurityAccessRequestSeed(byte accessLevel, int timeoutMs = -1)
    {
        return SendCommand(0x27, [accessLevel], timeoutMs);
    }

    /// <summary>
    /// KWP2000 SecurityAccess — send key (service 0x27, even access level).
    /// </summary>
    public Kwp2000Result SecurityAccessSendKey(byte accessLevel, ReadOnlySpan<byte> key, int timeoutMs = -1)
    {
        var payload = new byte[1 + key.Length];
        payload[0] = accessLevel;
        key.CopyTo(payload.AsSpan(1));
        return SendCommand(0x27, payload, timeoutMs);
    }

    /// <summary>
    /// KWP2000 RequestDownload (service 0x34).
    /// Address is 3 bytes big-endian, compressEncryptType is 1 byte, uncompressedSize is 3 bytes big-endian.
    /// </summary>
    public Kwp2000Result RequestDownload(uint address, byte compressEncryptType, uint uncompressedSize, int timeoutMs = -1)
    {
        byte[] payload =
        [
            (byte)((address >> 16) & 0xFF),
            (byte)((address >> 8) & 0xFF),
            (byte)(address & 0xFF),
            compressEncryptType,
            (byte)((uncompressedSize >> 16) & 0xFF),
            (byte)((uncompressedSize >> 8) & 0xFF),
            (byte)(uncompressedSize & 0xFF)
        ];
        return SendCommand(0x34, payload, timeoutMs);
    }

    /// <summary>
    /// KWP2000 TransferData (service 0x36).
    /// </summary>
    public Kwp2000Result TransferData(ReadOnlySpan<byte> data, int timeoutMs = -1)
    {
        if (timeoutMs < 0) timeoutMs = DefaultTimeoutMs;
        return SendCommand(0x36, data, timeoutMs, pendingTimeoutMs: timeoutMs * 3);
    }

    /// <summary>
    /// KWP2000 RequestTransferExit (service 0x37).
    /// </summary>
    public Kwp2000Result RequestTransferExit(int timeoutMs = -1)
    {
        return SendCommand(0x37, [], timeoutMs);
    }

    /// <summary>
    /// KWP2000 StartRoutine (service 0x31).
    /// </summary>
    public Kwp2000Result StartRoutine(byte routineId, byte[] parameters, int timeoutMs = -1, int pendingTimeoutMs = 30_000)
    {
        var payload = new byte[1 + parameters.Length];
        payload[0] = routineId;
        parameters.CopyTo(payload, 1);
        return SendCommand(0x31, payload, timeoutMs, pendingTimeoutMs: pendingTimeoutMs);
    }

    /// <summary>
    /// KWP2000 ECUReset (service 0x11).
    /// </summary>
    public Kwp2000Result ECUReset(byte resetType, int timeoutMs = -1)
    {
        if (timeoutMs < 0) timeoutMs = DefaultTimeoutMs;
        return SendCommand(0x11, [resetType], timeoutMs, pendingTimeoutMs: timeoutMs * 2);
    }

    /// <summary>
    /// KWP2000 TesterPresent (service 0x3E).
    /// </summary>
    public Kwp2000Result TesterPresent(int timeoutMs = -1)
    {
        return SendCommand(0x3E, [1], timeoutMs);
    }

    /// <summary>
    /// KWP2000 ReadMemoryByAddress (service 0x23).
    /// </summary>
    public Kwp2000Result ReadMemoryByAddress(uint address, byte size, int timeoutMs = -1)
    {
        byte[] requestPayload =
        [
            (byte)((address >> 16) & 0xFF),
            (byte)((address >> 8) & 0xFF),
            (byte)(address & 0xFF),
            size
        ];
        return SendCommand(0x23, requestPayload, timeoutMs, minimumResponseBytes: size + 1);
    }
}
