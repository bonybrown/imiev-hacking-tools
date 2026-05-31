# imiev-hacking-tools
Tools and info for working with Mitsubishi i-MIEV
ECU and BMU hardware/firmware and the MUT3 diagnostic software.

> [!WARNING]
> ### Disclaimer
> This software is licensed under the [GNU General Public License v3.0](LICENSE). In accordance with **Sections 15 and 16** of the license, this program is provided "as is" without warranties of any kind. The author(s) assume zero liability for any damages, data loss, or system failures arising from its use.

## What's in this repo

### [Details of the EV-ECU](ev-ecu)

Hardware and software details of the EV-ECU

### [Tools for working with MUT3 files](MUT3)

For decrypting and navigating around the data in the MUT3-SE software.

### [A tool for retrieving the code off the BMU or ECU](get-miev-dump)

A tool that uses a J2534 adapter to fetch the code from the BMU or ECU
of the iMiev. 

Windows 32-bit only (as J2534 drivers are needed).

### [A tool for uploading new code to the ECU](upload-miev-firmware)

A tool that uses a J2534 adapter to upload code to the ECU.
Requires binary files that include the boot and ECU code for correct checksum calculation.

Windows 32-bit only (as J2534 drivers are needed).

### [A monitor tool for the ECU](memory-explorer)

Presents a windows form where the memory of the ECU can be
explored and KWP2000 diagnostic requests can be sent.

Windows 32-bit only (as J2534 drivers are needed).

### [M32R-FP lanaguage spec for Ghidra](Ghidra)

This is a refinement of one of the M32R processor specs found around the web.
This has the needed floating point instructions added to it, and also
these changes below that might make it unsuitable for any other use without modification:
* The [call spec](Ghidra/Processors/m32r/data/languages/m32r.cspec) includes `R3` and stack-based parameters. Stack parameters are used infrequently.
* A floating point comparison immediately followed by a branch instruction is
treated as if it were a single, 64-bit instruction. This gives a better disassembly
because the actual comparison is used in the decompiled branch, rather than
testing the result of the comparison for ==0, >0, <0 etc. Look for `FBGEZ` and friends in 
[Ghidra/Processors/m32r/data/languages/m32r.sinc](Ghidra/Processors/m32r/data/languages/m32r.sinc)
* The FP register is hard-coded to 0x80c000 and special handling exists
for FP register operations. See the end of the `m32r.sinc` file.

#### Recompilation of the m32 language file

from the `Ghidra` directory of your installation:

```bash
$ support/sleigh -a Ghidra/Processors/m32r/data/languages/
```
Then restart Ghidra entirely.

### [Using an ELM327](ELM327.md)

The magic incantation for getting an ELM-327 based CAN device
to send and recieve CAN-TP (ISO 15765-2) messages to the ECU or BMU

## Related works

An excellent walkthrough of how the XML-based database of MUT3-SE works
is [here](https://mohammad-ali.bandzar.com/categories/mut-iii-se-reverse-engineering/)
