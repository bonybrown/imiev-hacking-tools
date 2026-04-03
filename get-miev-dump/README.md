# get-miev-dump

Console app that uses `J2534-Sharp.Core` to communicate with Mitsubishi i-MiEV ECUs and BMUs via KWP2000 over CAN (ISO15765).

Features:

- Enumerate installed J2534 APIs and prompt for selection.
- Read ECU identification data (VIN, hardware/software numbers, etc.).
- Dump ECU/BMU memory via ReadMemoryByAddress (service 0x23).
- Resume interrupted memory dumps automatically.
- Target either the ECU (0x751/0x752) or BMU (0x761/0x762).

## Requirements

- Windows (32-bit J2534 only)
- .NET 10 SDK (x86)
- **32-bit .NET 10 Runtime** (install from https://dotnet.microsoft.com/download/dotnet/10.0 — select the x86 installer)
- A working J2534 adapter and vendor J2534 driver/API installed
- Vehicle bus available at 500 kbps

## Command Line Options

```
Usage:
  get-miev-dump [options]

Options:
  --target <ecu|bmu>  Target module: ecu (0x751/0x752) or bmu (0x761/0x762) [default: ecu]
  --info              Read ECU identifiers and exit without dumping memory
  --debug             Enable verbose TX/RX frame logging
  -?, -h, --help      Show help and usage information
  --version           Show version information
```

### Examples

```powershell
# Dump ECU memory (default target)
dotnet run

# Read BMU identification only
dotnet run -- --target bmu --info

# Dump ECU memory with verbose frame logging
dotnet run -- --debug

# Resume an interrupted BMU dump
dotnet run -- --target bmu
```

## Output Files

- `{target}_identification.txt` — ECU/BMU identification data (e.g. `ecu_identification.txt`)
- `{target}_dump.txt` — Memory dump output (e.g. `ecu_dump.txt`)

If a dump file already exists, the program reads the last address and resumes from where it left off.

## Build

```powershell
dotnet restore
dotnet build
```

## Publish Redistributable (No .NET Runtime Required)

Use the included publish profile and script:

```powershell
.\publish-redistributable.ps1
```

Output:

- `dist\win-x86\` - self-contained single-file publish output
- `dist\get-miev-dump-win-x86.zip` - zip ready to distribute

Equivalent manual command:

```powershell
dotnet publish .\get-miev-dump.csproj -c Release /p:PublishProfile=WinX86SelfContained
```

## Run

```powershell
dotnet run
```

## Notes

- J2534 is a 32-bit only standard — all adapters (OpenPort 2.0, GODIAG, Sardine, etc.) provide 32-bit DLLs. This app is built as win-x86 accordingly.
- The app opens `Protocol.ISO15765` with `Baud.ISO15765_500000`.
- Mitsubishi uses non-standard CAN response addressing: requests to 0x751 are answered on 0x752 (not 0x759 as standard ISO15765 would dictate).
- KWP2000 services used: StartDiagnosticSession (0x10), ReadECUIdentification (0x1A), ControlDTCSetting (0x85), DisableNormalMessageTransmission (0x28), ReadMemoryByAddress (0x23).
- The process of fully dumping the ecu or bmu takes about 40 minutes.
- You can dump the BMU with the car in ready mode. The aux battery will continue
being charged by the OBC.
- Reading the ECU will disengage the OBC and the aux battery will not charge. Consider using an external charger while the ECU is being dumped.