# memory-explorer

Windows Forms GUI app that uses `J2534-Sharp.Core` to communicate with Mitsubishi i-MiEV ECUs and BMUs via KWP2000 over CAN (ISO15765).

Features:

- Enumerate installed J2534 APIs and select from a dropdown.
- Connect to either the ECU (0x751/0x752) or BMU (0x761/0x762).
- Browse ECU/BMU memory in a scrollable hex viewer via ReadMemoryByAddress (service 0x23).
- Continuous polling mode to watch memory values change in real time.
- Decode memory as raw hex, Float, UInt16, or UInt8.
- Send arbitrary KWP2000 commands and view raw responses.
- TesterPresent keepalive runs automatically while connected.

> [!WARNING]
> A standard ECU/BMU will not respond to ReadMemoryByAddress requests until diagnostic session 0x85 is entered.
>
> Entering session 0x85 stops normal operation of the unit until the session is exited.

## Requirements

- Windows (32-bit J2534 only)
- .NET 10 SDK (x86)
- **32-bit .NET 10 Runtime** (install from https://dotnet.microsoft.com/download/dotnet/10.0 — select the x86 installer)
- A working J2534 adapter and vendor J2534 driver/API installed
- Vehicle bus available at 500 kbps

## Address Space

The hex viewer presents two contiguous memory regions as a single scrollable view:

| Region | Real Address Range | Size |
|--------|-------------------|------|
| ROM | 0x000000 – 0x0FFFFF | 1 MB |
| RAM | 0x800000 – 0x81FFFF | 128 KB |

Navigate by typing a hex address and pressing Go, or scroll through the virtual view.

## Build

```powershell
dotnet restore
dotnet build
```

## Run

```powershell
memory-explorer
```

## Notes

- J2534 is a 32-bit only standard — all adapters (OpenPort 2.0, GODIAG, Sardine, etc.) provide 32-bit DLLs. This app is built as win-x86 accordingly.
- The app opens `Protocol.ISO15765` with `Baud.ISO15765_500000`.
- Mitsubishi uses non-standard CAN response addressing: requests to 0x751 are answered on 0x752 (not 0x759 as standard ISO15765 would dictate).
- Enters diagnostic session 0x92 on connect.
- KWP2000 services available in the command panel:
  - StartDiagnosticSession (0x10)
  - ECUReset (0x11)
  - ClearDiagnosticInformation (0x14)
  - ReadStatusOfDTC (0x17)
  - ReadDTCByStatus (0x18)
  - ReadECUIdentification (0x1A)
  - ReadDataByLocalIdentifier (0x21)
  - ReadMemoryByAddress (0x23)
  - SecurityAccess (0x27)
  - DisableNormalMsgTransmission (0x28)
  - WriteDataByIdentifier (0x2E)
  - InputOutputControlByLocalId (0x30)
  - StartRoutineByLocalId (0x31)
  - RequestDownload (0x34)
  - TransferData (0x36)
  - RequestTransferExit (0x37)
  - WriteDataByLocalIdentifier (0x3B)
  - TesterPresent (0x3E)
  - ControlDTCSetting (0x85)
