# Reprogram

J2534-based tools for downloading, uploading, and exploring Mitsubishi i-MiEV ECU/BMU firmware via KWP2000 over CAN (ISO15765).

All tools require Windows (32-bit J2534 only) and a compatible J2534 adapter.

## Contents

### [shared](shared)

Shared source files compiled into each project via file links:

- `Kwp2000.cs` — KWP2000 protocol implementation over ISO15765 with NRC 0x78 auto-retry, Security Access, flash transfer services, and diagnostic session management.
- `CanAddress.cs` — CAN address helper (numeric value + 4-byte big-endian representation).

### [download-miev-firmware](download-miev-firmware)

Console app that reads ECU/BMU identification data and dumps the full memory (1 MB) via ReadMemoryByAddress. Supports resume of interrupted dumps and outputs both hex text and raw binary files.

### [upload-miev-firmware](upload-miev-firmware)

Console app that reprograms ECU/BMU flash memory. Validates firmware structure, computes and injects checksums, performs Security Access authentication, uploads SWIL loaders, erases flash, programs all regions, and verifies the result.

### [memory-explorer](memory-explorer)

Windows Forms GUI for real-time ECU/BMU memory exploration. Browse memory in a hex viewer, decode values as different types, send arbitrary KWP2000 commands, and monitor changing values with continuous polling.
