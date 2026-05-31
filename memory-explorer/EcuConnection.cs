using SAE.J2534;

/// <summary>
/// Manages the J2534 connection lifecycle to the ECU.
/// </summary>
class EcuConnection : IDisposable
{
    readonly CanAddress _canTx = new(0x751);
    readonly CanAddress _canRx = new(0x752);

    J2534API? _api;
    J2534Device? _device;
    J2534Channel? _channel;
    Kwp2000? _kwp;
    bool _disposed;

    public Kwp2000? Kwp => _kwp;
    public bool IsConnected => _kwp is not null;

    /// <summary>
    /// Discovers installed J2534 APIs and returns their display names.
    /// </summary>
    public static List<(string Name, string FileName)> DiscoverAdapters()
    {
        return J2534APIFactory.DiscoverAPIs()
            .Select(a => (a.Name, a.FileName))
            .ToList();
    }

    /// <summary>
    /// Opens the J2534 device, ISO15765 channel, configures flow control filter,
    /// and enters diagnostic session 0x92.
    /// </summary>
    public string Connect(string apiFileName)
    {
        Disconnect();

        var apiResult = J2534APIFactory.LoadAPI(apiFileName);
        if (!apiResult.IsSuccess)
            return $"Failed to load API: {apiResult.ErrorMessage}";
        _api = apiResult.Value;

        var deviceResult = _api.OpenDevice();
        if (!deviceResult.IsSuccess)
        {
            Cleanup();
            return $"Failed to open device: {deviceResult.ErrorMessage}";
        }
        _device = deviceResult.Value;

        var channelResult = _device.OpenChannel(Protocol.ISO15765, Baud.ISO15765_500000, ConnectFlag.NONE);
        if (!channelResult.IsSuccess)
        {
            Cleanup();
            return $"Failed to open channel: {channelResult.ErrorMessage}";
        }
        _channel = channelResult.Value;
        _channel.ClearMessageFilters();

        // Mitsubishi flow control filter (responses on CAN ID+1, not +8)
        var filter = new MessageFilter
        {
            FilterType = Filter.FLOW_CONTROL_FILTER,
            Mask = [0x00, 0x00, 0x07, 0xFF],
            Pattern = [0x00, 0x00, 0x07, 0x52],
            FlowControl = [0x00, 0x00, 0x07, 0x51],
            TxFlags = TxFlag.NONE
        };

        var filterResult = _channel.StartMessageFilter(filter, Protocol.ISO15765);
        if (!filterResult.IsSuccess)
        {
            Cleanup();
            return $"Filter setup failed: {filterResult.ErrorMessage}";
        }
        _channel.ClearRxBuffer();

        _kwp = new Kwp2000(_channel, _canTx, _canRx);

        // Enter diagnostic session 0x92
        var sessionResult = _kwp.StartDiagnosticSession(0x92, timeoutMs: 2000);
        if (!sessionResult.Success)
        {
            Cleanup();
            return $"Failed to start diagnostic session: {sessionResult.ErrorMessage}";
        }

        return string.Empty; // success
    }

    public void Disconnect()
    {
        Cleanup();
    }

    void Cleanup()
    {
        _kwp = null;
        _channel?.Dispose();
        _channel = null;
        _device?.Dispose();
        _device = null;
        // Don't dispose _api — the factory caches and reuses the instance.
        _api = null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Cleanup();
            _disposed = true;
        }
    }
}
