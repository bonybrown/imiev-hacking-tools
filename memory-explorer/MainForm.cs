using System.Globalization;

class MainForm : Form
{
    readonly EcuConnection _ecu = new();

    // Controls
    readonly ComboBox _adapterCombo;
    readonly Button _connectButton;
    readonly Label _statusLabel;
    readonly TextBox _addressBox;
    readonly Button _goButton;
    readonly Panel _hexPanel;
    readonly VScrollBar _scrollBar;
    readonly System.Windows.Forms.Timer _pollTimer;
    readonly CheckBox _continuousPollCheckbox;
    readonly System.Windows.Forms.Timer _readPollTimer;

    // KWP command controls
    readonly GroupBox _kwpGroupBox;
    readonly ComboBox _kwpServiceCombo;
    readonly TextBox _kwpParamsBox;
    readonly Button _kwpSendButton;
    readonly TextBox _kwpResponseBox;

    bool _readInFlight;
    uint _currentAddress;
    byte[]? _displayData;
    bool _suppressAddressSync;

    // Address space: two contiguous regions with a gap in between.
    // Region 1: 0x000000 – 0x0FFFFF (1 MB)
    // Region 2: 0x800000 – 0x81FFFF (128 KB)
    // Virtual view treats them as contiguous: virtual offset 0x100000 maps to real 0x800000.
    const uint Region1Start = 0x000000;
    const uint Region1End = 0x0FFFFF;
    const uint Region1Size = Region1End - Region1Start + 1;
    const uint Region2Start = 0x800000;
    const uint Region2End = 0x81FFFF;
    const uint Region2Size = Region2End - Region2Start + 1;
    const uint TotalVirtualSize = Region1Size + Region2Size;

    const int BytesPerRow = 8;
    const int ReadSize = 128;
    const int RowsInView = ReadSize / BytesPerRow;
    static readonly int TotalRows = (int)(TotalVirtualSize / BytesPerRow);

    public MainForm()
    {
        Text = "i-MiEV Memory Explorer";
        Width = 900;
        Height = 500;
        MinimumSize = new Size(750, 400);
        Font = new Font("Segoe UI", 9);
        DoubleBuffered = true;
        Padding = new Padding(8);

        // ── Main layout: vertical stack ──
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // adapter row
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // status
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // address row
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // content area

        // ── Row 0: adapter selection ──
        var adapterRow = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 4)
        };

        var adapterLabel = new Label
        {
            Text = "Adapter:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 4, 0)
        };

        _adapterCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 350,
            Margin = new Padding(0, 0, 4, 0)
        };

        _connectButton = new Button
        {
            Text = "Connect",
            AutoSize = true,
            Margin = new Padding(0, 0, 4, 0)
        };
        _connectButton.Click += ConnectButton_Click;

        var refreshButton = new Button
        {
            Text = "↻",
            Width = 30,
            Font = new Font("Segoe UI", 10),
            Margin = new Padding(0)
        };
        refreshButton.Click += (_, _) => RefreshAdapters();

        adapterRow.Controls.AddRange([adapterLabel, _adapterCombo, _connectButton, refreshButton]);

        // ── Row 1: status ──
        _statusLabel = new Label
        {
            Text = "Disconnected",
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = Color.Gray,
            Margin = new Padding(0, 0, 0, 4)
        };

        // ── Row 2: address row ──
        var addressRow = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 4)
        };

        var addressLabel = new Label
        {
            Text = "Address (hex):",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 4, 0)
        };

        _addressBox = new TextBox
        {
            Text = "000000",
            Width = 80,
            MaxLength = 8,
            Margin = new Padding(0, 0, 4, 0)
        };

        _goButton = new Button
        {
            Text = "Go",
            AutoSize = true,
            Enabled = false,
            Margin = new Padding(0, 0, 8, 0)
        };
        _goButton.Click += GoButton_Click;

        _continuousPollCheckbox = new CheckBox
        {
            Text = "Continuous",
            AutoSize = true,
            Checked = false,
            Enabled = false,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 3, 0, 0)
        };
        _continuousPollCheckbox.CheckedChanged += ContinuousPollCheckbox_Changed;

        addressRow.Controls.AddRange([addressLabel, _addressBox, _goButton, _continuousPollCheckbox]);

        // ── Row 3: content area (hex panel + KWP command) ──
        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // ── Hex display panel ──
        _hexPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 4, 0)
        };
        _hexPanel.Paint += HexPanel_Paint;
        _hexPanel.MouseWheel += HexPanel_MouseWheel;
        _hexPanel.Click += (_, _) => _hexPanel.Focus();

        _scrollBar = new VScrollBar
        {
            Dock = DockStyle.Right,
            Minimum = 0,
            Maximum = TotalRows,
            SmallChange = 1,
            LargeChange = RowsInView
        };
        _scrollBar.ValueChanged += ScrollBar_ValueChanged;
        _hexPanel.Controls.Add(_scrollBar);

        // ── KWP Command section ──
        _kwpGroupBox = new GroupBox
        {
            Text = "KWP2000 Command",
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 0, 0, 0),
            Padding = new Padding(8)
        };

        var kwpLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        kwpLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        kwpLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // service label
        kwpLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // service combo
        kwpLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // params label
        kwpLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // params + send row
        kwpLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // response label
        kwpLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // response box

        var serviceLabel = new Label
        {
            Text = "Service:",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 2)
        };

        _kwpServiceCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 6)
        };
        _kwpServiceCombo.Items.AddRange([
            new KwpServiceItem(0x10, "StartDiagnosticSession"),
            new KwpServiceItem(0x11, "ECUReset"),
            new KwpServiceItem(0x14, "ClearDiagnosticInformation"),
            new KwpServiceItem(0x17, "ReadStatusOfDTC"),
            new KwpServiceItem(0x18, "ReadDTCByStatus"),
            new KwpServiceItem(0x1A, "ReadECUIdentification"),
            new KwpServiceItem(0x21, "ReadDataByLocalIdentifier"),
            new KwpServiceItem(0x23, "ReadMemoryByAddress"),
            new KwpServiceItem(0x27, "SecurityAccess"),
            new KwpServiceItem(0x28, "DisableNormalMsgTransmission"),
            new KwpServiceItem(0x2E, "WriteDataByIdentifier"),
            new KwpServiceItem(0x30, "InputOutputControlByLocalId"),
            new KwpServiceItem(0x31, "StartRoutineByLocalId"),
            new KwpServiceItem(0x34, "RequestDownload"),
            new KwpServiceItem(0x36, "TransferData"),
            new KwpServiceItem(0x37, "RequestTransferExit"),
            new KwpServiceItem(0x3B, "WriteDataByLocalIdentifier"),
            new KwpServiceItem(0x3E, "TesterPresent"),
            new KwpServiceItem(0x85, "ControlDTCSetting")
        ]);
        _kwpServiceCombo.SelectedIndex = 0;

        var paramsLabel = new Label
        {
            Text = "Params (hex bytes, e.g. 01 FF A0):",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 2)
        };

        var paramsSendRow = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 6)
        };

        _kwpParamsBox = new TextBox
        {
            Width = 160,
            Margin = new Padding(0, 0, 6, 0)
        };

        _kwpSendButton = new Button
        {
            Text = "Send",
            AutoSize = true,
            Enabled = false,
            Margin = new Padding(0)
        };
        _kwpSendButton.Click += KwpSendButton_Click;

        paramsSendRow.Controls.AddRange([_kwpParamsBox, _kwpSendButton]);

        var responseLabel = new Label
        {
            Text = "Response:",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 2)
        };

        _kwpResponseBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9),
            Margin = Padding.Empty
        };

        kwpLayout.Controls.Add(serviceLabel, 0, 0);
        kwpLayout.Controls.Add(_kwpServiceCombo, 0, 1);
        kwpLayout.Controls.Add(paramsLabel, 0, 2);
        kwpLayout.Controls.Add(paramsSendRow, 0, 3);
        kwpLayout.Controls.Add(responseLabel, 0, 4);
        kwpLayout.Controls.Add(_kwpResponseBox, 0, 5);
        _kwpGroupBox.Controls.Add(kwpLayout);

        contentLayout.Controls.Add(_hexPanel, 0, 0);
        contentLayout.Controls.Add(_kwpGroupBox, 1, 0);

        mainLayout.Controls.Add(adapterRow, 0, 0);
        mainLayout.Controls.Add(_statusLabel, 0, 1);
        mainLayout.Controls.Add(addressRow, 0, 2);
        mainLayout.Controls.Add(contentLayout, 0, 3);

        Controls.Add(mainLayout);

        // ── Poll timer (1 second) — TesterPresent keepalive ──
        _pollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _pollTimer.Tick += PollTimer_Tick;

        // ── Continuous read poll timer ──
        _readPollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _readPollTimer.Tick += ReadPollTimer_Tick;

        _addressBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; GoButton_Click(null, e); }
        };

        // Populate adapters on load
        Load += (_, _) => RefreshAdapters();
    }

    void LayoutControls()
    {
        _hexPanel.Invalidate();
    }

    void RefreshAdapters()
    {
        _adapterCombo.Items.Clear();
        var adapters = EcuConnection.DiscoverAdapters();
        foreach (var (name, fileName) in adapters)
            _adapterCombo.Items.Add(new AdapterItem(name, fileName));

        if (_adapterCombo.Items.Count > 0)
            _adapterCombo.SelectedIndex = 0;
    }

    void ConnectButton_Click(object? sender, EventArgs e)
    {
        if (_ecu.IsConnected)
        {
            _pollTimer.Stop();
            _readPollTimer.Stop();
            _continuousPollCheckbox.Checked = false;
            _continuousPollCheckbox.Enabled = false;
            _kwpSendButton.Enabled = false;
            _ecu.Disconnect();
            SetStatus("Disconnected", Color.Gray);
            _connectButton.Text = "Connect";
            _goButton.Enabled = false;
            _adapterCombo.Enabled = true;
            return;
        }

        if (_adapterCombo.SelectedItem is not AdapterItem adapter)
        {
            SetStatus("No adapter selected", Color.Red);
            return;
        }

        SetStatus("Connecting...", Color.Orange);
        _connectButton.Enabled = false;
        Application.DoEvents();

        var error = _ecu.Connect(adapter.FileName);
        _connectButton.Enabled = true;

        if (!string.IsNullOrEmpty(error))
        {
            SetStatus(error, Color.Red);
            return;
        }

        SetStatus("Connected — session 0x92 active", Color.Green);
        _connectButton.Text = "Disconnect";
        _goButton.Enabled = true;
        _continuousPollCheckbox.Enabled = true;
        _kwpSendButton.Enabled = true;
        _adapterCombo.Enabled = false;
        _pollTimer.Start();

        NavigateToVirtualOffset(0);
    }

    // ── Navigation ──

    void GoButton_Click(object? sender, EventArgs e)
    {
        if (!TryParseAddress(out uint realAddress))
        {
            SetStatus("Invalid hex address", Color.Red);
            return;
        }

        if (!RealToVirtual(realAddress, out uint virtualOffset))
        {
            SetStatus("Address is in unreadable gap (0x100000–0x7FFFFF)", Color.Red);
            return;
        }

        NavigateToVirtualOffset(virtualOffset);
    }

    void ScrollBar_ValueChanged(object? sender, EventArgs e)
    {
        if (_suppressAddressSync) return;

        uint virtualOffset = (uint)_scrollBar.Value * BytesPerRow;
        if (virtualOffset + ReadSize > TotalVirtualSize)
            virtualOffset = TotalVirtualSize - ReadSize;

        uint realAddr = VirtualToReal(virtualOffset);
        if (realAddr != _currentAddress)
        {
            _currentAddress = realAddr;
            SyncAddressBox();
            ReadMemory();
        }
    }

    void HexPanel_MouseWheel(object? sender, MouseEventArgs e)
    {
        int rowDelta = -e.Delta / 120;
        int maxVal = _scrollBar.Maximum - _scrollBar.LargeChange + 1;
        int newVal = Math.Clamp(_scrollBar.Value + rowDelta, _scrollBar.Minimum, maxVal);
        _scrollBar.Value = newVal;
    }

    void NavigateToVirtualOffset(uint virtualOffset)
    {
        virtualOffset = (virtualOffset / BytesPerRow) * BytesPerRow;
        if (virtualOffset + ReadSize > TotalVirtualSize)
            virtualOffset = TotalVirtualSize - ReadSize;

        _currentAddress = VirtualToReal(virtualOffset);

        _suppressAddressSync = true;
        int maxVal = _scrollBar.Maximum - _scrollBar.LargeChange + 1;
        _scrollBar.Value = Math.Min((int)(virtualOffset / BytesPerRow), maxVal);
        _suppressAddressSync = false;

        SyncAddressBox();
        ReadMemory();
    }

    void SyncAddressBox()
    {
        _addressBox.Text = _currentAddress.ToString("X6");
    }

    // ── Address mapping ──

    static uint VirtualToReal(uint virtualOffset)
    {
        if (virtualOffset < Region1Size)
            return Region1Start + virtualOffset;
        return Region2Start + (virtualOffset - Region1Size);
    }

    static bool RealToVirtual(uint realAddress, out uint virtualOffset)
    {
        if (realAddress >= Region1Start && realAddress <= Region1End)
        {
            virtualOffset = realAddress - Region1Start;
            return true;
        }
        if (realAddress >= Region2Start && realAddress <= Region2End)
        {
            virtualOffset = Region1Size + (realAddress - Region2Start);
            return true;
        }
        virtualOffset = 0;
        return false;
    }

    void ContinuousPollCheckbox_Changed(object? sender, EventArgs e)
    {
        if (_continuousPollCheckbox.Checked)
            _readPollTimer.Start();
        else
            _readPollTimer.Stop();
    }

    // ── KWP Command ──

    async void KwpSendButton_Click(object? sender, EventArgs e)
    {
        if (!_ecu.IsConnected)
            return;

        if (_kwpServiceCombo.SelectedItem is not KwpServiceItem service)
            return;

        byte[]? paramBytes = ParseHexBytes(_kwpParamsBox.Text);
        if (paramBytes is null)
        {
            _kwpResponseBox.Text = "Error: invalid hex in params";
            return;
        }

        // Pause background polling to avoid conflicts on the channel
        _pollTimer.Stop();
        _readPollTimer.Stop();

        // Wait for any in-flight request to complete
        while (_readInFlight)
            await Task.Delay(50);

        _readInFlight = true;
        _kwpSendButton.Enabled = false;

        try
        {
            var kwp = _ecu.Kwp!;
            byte sid = service.ServiceId;
            var result = await Task.Run(() => kwp.SendCommand(sid, paramBytes));

            if (result.ResponsePayload is { Length: > 0 } payload)
                _kwpResponseBox.Text = string.Join(" ", payload.Select(b => b.ToString("X2")));
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
                _kwpResponseBox.Text = $"Error: {result.ErrorMessage}";
            else
                _kwpResponseBox.Text = "(no response)";
        }
        catch (Exception ex)
        {
            _kwpResponseBox.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _readInFlight = false;
            _kwpSendButton.Enabled = true;
            // Resume background polling
            _pollTimer.Start();
            if (_continuousPollCheckbox.Checked)
                _readPollTimer.Start();
        }
    }

    static byte[]? ParseHexBytes(string text)
    {
        text = text.Trim();
        if (string.IsNullOrEmpty(text))
            return [];

        var parts = text.Split([' ', ',', '-'], StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!byte.TryParse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
                return null;
        }
        return bytes;
    }

    void ReadPollTimer_Tick(object? sender, EventArgs e)
    {
        if (_ecu.IsConnected && !_readInFlight)
            ReadMemory();
    }

    async void PollTimer_Tick(object? sender, EventArgs e)
    {
        if (!_ecu.IsConnected || _readInFlight)
            return;

        _readInFlight = true;
        try
        {
            var kwp = _ecu.Kwp!;
            await Task.Run(() => kwp.TesterPresent());
        }
        catch (Exception ex)
        {
            SetStatus($"TesterPresent error: {ex.Message}", Color.Red);
        }
        finally
        {
            _readInFlight = false;
        }
    }

    async void ReadMemory()
    {
        if (!_ecu.IsConnected || _readInFlight)
            return;

        _readInFlight = true;

        try
        {
            var kwp = _ecu.Kwp!;
            uint address = _currentAddress;
            var result = await Task.Run(() => kwp.ReadMemoryByAddress(address, ReadSize));

            if (!result.Success)
            {
                SetStatus($"Read error: {result.ErrorMessage}", Color.Red);
                return;
            }

            var payload = result.ResponsePayload!;
            _displayData = payload.Length > 1 ? payload[1..] : [];
            _hexPanel.Invalidate();
            SetStatus($"Read {_displayData.Length} bytes from 0x{address:X6} at {DateTime.Now:HH:mm:ss}", Color.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.Red);
        }
        finally
        {
            _readInFlight = false;
        }
    }

    // ── Hex panel rendering ──

    void HexPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        using var font = new Font("Consolas", 10);
        var brush = Brushes.Black;
        float lineHeight = font.GetHeight(g) + 2;
        float x = 4, y = 4;

        if (_displayData is null || _displayData.Length == 0)
        {
            g.DrawString("(no data)", font, Brushes.Gray, x, y);
            return;
        }

        for (int i = 0; i < _displayData.Length; i += BytesPerRow)
        {
            uint rowAddr = _currentAddress + (uint)i;
            var line = $"{rowAddr:X6}  ";

            int rowEnd = Math.Min(i + BytesPerRow, _displayData.Length);
            for (int j = i; j < rowEnd; j++)
            {
                line += $"{_displayData[j]:X2}";
                if (j < rowEnd - 1) line += " ";
            }

            g.DrawString(line, font, brush, x, y);
            y += lineHeight;
        }
    }

    // ── Helpers ──

    bool TryParseAddress(out uint address)
    {
        var text = _addressBox.Text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            text = text[2..];

        if (!uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
            return false;

        if (address <= Region1End) return true;
        if (address >= Region2Start && address <= Region2End) return true;
        return false;
    }

    void SetStatus(string text, Color color)
    {
        _statusLabel.Text = text;
        _statusLabel.ForeColor = color;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pollTimer.Stop();
            _pollTimer.Dispose();
            _readPollTimer.Stop();
            _readPollTimer.Dispose();
            _ecu.Dispose();
        }
        base.Dispose(disposing);
    }

    record AdapterItem(string Name, string FileName)
    {
        public override string ToString() => Name;
    }

    record KwpServiceItem(byte ServiceId, string Name)
    {
        public override string ToString() => $"0x{ServiceId:X2} {Name}";
    }
}
