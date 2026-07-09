# upload-miev-firmware

Console app that uses `J2534-Sharp.Core` to reprogram Mitsubishi i-MiEV ECU/BMU flash memory via KWP2000 over CAN (ISO15765).

Features:

- Validate firmware file structure (magic bytes, minimum size, boot area).
- Compute and inject the required checksum adjustment value at offset 0xFFFF8.
- Build optimized flash regions (skips 0xFF-filled chunks).
- Security Access seed-to-key authentication (level 0x05/0x06).
- Upload SWIL loaders, erase flash, program all regions, and finalize.
- Verify ECU identification before and after programming.
- Battery voltage check (minimum 10 V) before proceeding.
- Target either the ECU (0x751/0x752) or BMU (0x761/0x762).

> [!CAUTION]
> This tool erases and reprograms ECU flash memory. A failed or interrupted upload may leave the ECU in reprogamming failsafe mode. Ensure stable power and a reliable connection before proceeding.

## Requirements

- Windows (32-bit J2534 only)
- .NET 10 SDK (x86)
- **32-bit .NET 10 Runtime** (install from https://dotnet.microsoft.com/download/dotnet/10.0 — select the x86 installer)
- A working J2534 adapter and vendor J2534 driver/API installed
- Vehicle bus available at 500 kbps

## Command Line Options

```
Usage:
  upload-miev-firmware <firmware> [options]

Arguments:
  <firmware>           Path to the raw binary firmware file

Options:
  --target <ecu|bmu>  Target module: ecu (0x751/0x752) or bmu (0x761/0x762)
  --debug             Enable verbose TX/RX frame logging
  -?, -h, --help      Show help and usage information
  --version           Show version information
```

### Examples

```powershell
# Upload firmware to ECU (will prompt for target if not specified)
upload-miev-firmware firmware.bin

# Upload firmware to BMU with verbose logging
upload-miev-firmware firmware.bin --target bmu --debug
```

## Reprogramming Sequence

1. Validate firmware file (magic bytes, boot area, size).
2. Compute checksum and inject adjustment value.
3. Connect via J2534 and read ECU identification.
4. Prompt for final confirmation (type `YES` to proceed).
5. Enter diagnostic session 0x92, then programming session 0x85.
6. Security Access (seed-to-key, level 0x05/0x06).
7. Write programming date stamp.
8. Upload SWIL1 + checksum, validate upload.
9. **Erase flash** (~15 seconds) — point of no return.
10. Upload SWIL2 + checksum, validate upload.
11. Program all flash regions (256-byte chunks).
12. Upload final checksum and finalize programming.
13. Reset ECU and verify post-reset identification.

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

- `dist\win-x86\` — self-contained single-file publish output
- `dist\upload-miev-firmware-win-x86.zip` — zip ready to distribute

Equivalent manual command:

```powershell
dotnet publish .\upload-miev-firmware.csproj -c Release /p:PublishProfile=WinX86SelfContained
```

## Run

```powershell
upload-miev-firmware <path-to-firmware.bin>
```

## Notes

- J2534 is a 32-bit only standard — all adapters (OpenPort 2.0, GODIAG, Sardine, etc.) provide 32-bit DLLs. This app is built as win-x86 accordingly.
- The app opens `Protocol.ISO15765` with `Baud.ISO15765_500000`.
- Mitsubishi uses non-standard CAN response addressing: requests to 0x751 are answered on 0x752 (not 0x759 as standard ISO15765 would dictate).
- Flash address range: 0x008000 – 0x100000. The boot area (0x0000–0x7FFF) is not reprogrammed.
- The firmware file must contain valid magic bytes: `5A A5` at offset 0x8000 and `A5 5A` at offset 0xFFFFE.
- Checksum target: the sum of all 32-bit big-endian values across the full 1 MB must equal 0x5AA55AA5.
- Security Access KDF parameters: ECU uses multiplier 0xB1 / addend 0xCB14; BMU uses multiplier 0xE0 / addend 0x2AB6.
- SWIL (Software In the Loop) loaders are embedded resources (`swil1.bin`, `swil2.bin`) uploaded before and after flash erase.
- KWP2000 services used:
  - StartDiagnosticSession (0x10)
  - ECUReset (0x11)
  - ReadECUIdentification (0x1A)
  - SecurityAccess (0x27)
  - WriteDataByLocalIdentifier (0x3B)
  - RequestDownload (0x34)
  - TransferData (0x36)
  - RequestTransferExit (0x37)
  - StartRoutineByLocalId (0x31)
