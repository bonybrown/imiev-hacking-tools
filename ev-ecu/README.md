# Information about the EV-ECU module

## Hardware

The EV Engine Control Unit (EV-ECU) and Battery Control Unit (BMU)
both use a Mitsubish branded CPU labelled `M8106`, a 144pin LQFP package.

This seems to be the [Renesas 32186](https://www.renesas.com/en/document/mah/3218532186-group-hardware-manual).
* [M32R-FP instruction set](https://www.renesas.com/en/document/mah/m32r-fpu-software-manual) (M32R plus single precision floating point extensions)
* Single precision floating point unit
* 1024kB of Flash (`0x00 0000` to `0x0f ffff`)
* Special function registers (`0x80 0000` to `0x80 3fff`)
  * Not all this are is used. The last peripheral register is at `0x80 204e` 
* 64kB of RAM (`0x80 4000` to `0x81 3fff`)
* 80MHz operation
* 2 x CAN bus peripherals
  * On BMU, used for main CAN bus and secondary CAN bus to the battery pack
  * On EV-ECU, used for main CAN bus and secondary CAN bus to communicate with ChaDeMo charger

### ECU Unit IO

[Table of ECU pins](ecu-pins.md)

### ECU Identification data

#### Service $1a $87 ECU Identification

| Field | Size | Value |
|---|---|---|
| ECU Origin | byte | harcoded as 4 (MMC) |
| Supplier Identification | byte | hardcoded as 0x85 |
| Diagnostic version (high byte) | word | hardcoded 0x00 03 . This is the diagnostic revision used to decode the diagnostic data in the MUT3 EcuDiag data|
| Reserved | byte | hardcoded 0xFF |
| Hardware Revision | word | From EEPROM offset 0x42. Typical value 0x0006 |
| Software Revision | 3 bytes | From EEPROM offset 0x44. Typical value 0x606a08  |
| Hardware Part Number | 10 bytes | From EEPROM offset 0x47, "9499A182  " |


#### Service $1a $9a ECU Code Fingerprint

| Field | Size | Value |
|---|---|---|
| Number of Modules | byte | Hardcoded to 1 |
| Active Logical Block | byte | Harcoded to 0, meaning no erase performed |
| Tool Supplier ID | byte | EEPROM offset 0x5b |
| Programming Date Y/M/D | 3 bytes | EEEPROM offset 0x5c |
| Tester Serial Number | 4 bytes | Always seems to be zeros |

The 8 bytes from the EEPROM are set in the EEPROM by the boot loader 
`WriteDataByLocalIdentifier( id=$9A )` call.
This call is a neccessary step in the reprogramming sequence.



