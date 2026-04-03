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
    /// When true, TX and RX frame data is written to the console.
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    /// Sends a KWP2000 service request over ISO15765 and waits for the response.
    /// </summary>
    /// <param name="serviceId">KWP2000 service identifier (e.g. 0x1A, 0x10, 0x21).</param>
    /// <param name="data">Optional sub-function / data bytes following the service ID.</param>
    /// <param name="timeoutMs">Receive timeout in milliseconds.</param>
    /// <param name="expectResponse">If false, returns success immediately after sending without waiting for a response.</param>
    /// <param name="minimumResponseBytes">Minimum number of response payload bytes to accumulate before returning.</param>
    /// <returns>A <see cref="Kwp2000Result"/> indicating success or failure.</returns>
    public Kwp2000Result SendCommand(
        byte serviceId,
        ReadOnlySpan<byte> data = default,
        int timeoutMs = -1,
        bool expectResponse = true,
        int minimumResponseBytes = 1)
    {
        if (timeoutMs < 0) timeoutMs = DefaultTimeoutMs;

        channel.ClearRxBuffer();
        // Build ISO15765 frame: 4-byte CAN ID header (big-endian) + service ID + data
        var payload = new byte[4 + 1 + data.Length];
        txCanId.Bytes.CopyTo(payload, 0);
        payload[4] = serviceId;
        if (data.Length > 0)
        {
            data.CopyTo(payload.AsSpan(5));
        }

        if (Debug)
            Console.WriteLine($"TX KWP [{serviceId:X2}]: {BitConverter.ToString(payload)}");

        var sendResult = channel.SendMessage(payload);
        if (!sendResult.IsSuccess)
        {
            return Kwp2000Result.Error($"Send failed: {sendResult.ErrorMessage}");
        }

        if (!expectResponse)
        {
            return Kwp2000Result.Positive([]);
        }

        var result = ReadResponse(timeoutMs, minimumResponseBytes);
        if (result.ResponsePayload is null)
            return result;

        var responsePayload = result.ResponsePayload;

        // Positive response: service ID + 0x40
        byte expectedPositive = (byte)(serviceId + 0x40);
        if (responsePayload.Length >= 1 && responsePayload[0] == expectedPositive)
        {
            return Kwp2000Result.Positive(responsePayload);
        }

        // Negative response: 7F <serviceId> <NRC>
        if (responsePayload.Length >= 3 && responsePayload[0] == 0x7F && responsePayload[1] == serviceId)
        {
            return Kwp2000Result.Negative(responsePayload[2], responsePayload);
        }

        return Kwp2000Result.Error(
            $"Unexpected response format: {BitConverter.ToString(responsePayload)}");
    }

    /// <summary>
    /// Reads raw response data from the channel, returning the payload bytes without
    /// validating the service ID or positive/negative response format.
    /// Accumulates data from multiple messages until at least <paramref name="minimumBytes"/> are received.
    /// </summary>
    /// <param name="timeoutMs">Total timeout in milliseconds for the entire read operation.</param>
    /// <param name="minimumBytes">Minimum number of payload bytes to accumulate before returning.</param>
    /// <returns>A <see cref="Kwp2000Result"/> with the raw payload, or an error.</returns>
    public Kwp2000Result ReadResponse(int timeoutMs = -1, int minimumBytes = 1)
    {
        if (timeoutMs < 0) timeoutMs = DefaultTimeoutMs;

        const int PollIntervalMs = 10;
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

            var readResult = channel.ReadMessages(10, PollIntervalMs);
            elapsedMs += PollIntervalMs;

            if (!readResult.IsSuccess)
            {
                if (readResult.IsTimeout)
                    continue;
                if(readResult.IsBufferEmpty)
                    continue;
                return Kwp2000Result.Error($"Read failed: {readResult.Status}");
            }

            foreach (var msg in readResult.Messages)
            {
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
        {
            return Kwp2000Result.Error($"No response from CAN ID {rxCanId}.");
        }

        var responsePayload = resultBytes.ToArray();
        if (Debug)
            Console.WriteLine($"RX KWP: {BitConverter.ToString(responsePayload)}");

        return Kwp2000Result.Positive(responsePayload);
    }

    /// <summary>
    /// KWP2000 StartDiagnosticSession (service 0x10).
    /// </summary>
    /// <param name="sessionType">Session sub-function byte (e.g. 0x92).</param>
    /// <param name="timeoutMs">Receive timeout in milliseconds.</param>
    public Kwp2000Result StartDiagnosticSession(byte sessionType, int timeoutMs = -1)
    {
        return SendCommand(0x10, [sessionType], timeoutMs);
    }

    /// <summary>
    /// KWP2000 ReadECUIdentification (service 0x1A).
    /// </summary>
    /// <param name="identificationOption">Identification option to request.</param>
    /// <param name="timeoutMs">Receive timeout in milliseconds.</param>
    public Kwp2000Result ReadECUIdentification(ECUIdentificationOption identificationOption, int timeoutMs = -1)
    {
        return SendCommand(0x1A, [(byte)identificationOption], timeoutMs);
    }

    /// <summary>
    /// KWP2000 DisableNormalMessageTransmission (service 0x28).
    /// </summary>
    /// <param name="responseRequired">Whether a response is required from the ECU.</param>
    /// <param name="timeoutMs">Receive timeout in milliseconds.</param>
    public Kwp2000Result DisableNormalMessageTransmission(ResponseRequired responseRequired = ResponseRequired.ResponseRequired, int timeoutMs = -1)
    {
        return SendCommand(0x28, [(byte)responseRequired], timeoutMs,
            expectResponse: responseRequired == ResponseRequired.ResponseRequired);
    }

    /// <summary>
    /// KWP2000 ControlDTCSetting (service 0x85).
    /// </summary>
    /// <param name="responseRequired">Whether a response is required from the ECU.</param>
    /// <param name="dtcGroup">Group of DTCs to control.</param>
    /// <param name="settingMode">DTC setting mode (on/off).</param>
    /// <param name="timeoutMs">Receive timeout in milliseconds.</param>
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
    /// KWP2000 ReadMemoryByAddress (service 0x23).
    /// The first byte of the response payload is expected to be a service echo (0x23) and is skipped in the returned payload.
    /// </summary>
    /// <param name="address">Memory address (transmitted as 3 bytes, big-endian).</param>
    /// <param name="size">Number of bytes to read.</param>
    /// <param name="timeoutMs">Receive timeout in milliseconds.</param>
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
