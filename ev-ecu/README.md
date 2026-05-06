# Information about the EV-ECU module

## General Arrangement

(diagram of Main CPU, Secondary CPU, EEPROM, CAN interfaces x 2 and general IO device descriptions. Show reset circuit from connector)

## Main CPU (IC1)

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

## Secondary CPU (IC2)

Not much known about it at this time, although the main CPU communicates with it over what looks
to be an SPI interface.

## EEPROM (IC3)

This is marked `RN86` and seems to be a standard Microwire interface 16 bit x 1K word EEPROM.

Pinout and instruction set is identical to the 
[Microschip 93C86](https://ww1.microchip.com/downloads/aemDocuments/documents/MPD/ProductDocuments/DataSheets/21132F.pdf) 

This EEPROM holds stored DTCs, the brake pedal "learning" data and also seems to control some 
kind of reprogramming limit counter. It is read and written to by bit-banging the pins of PORT 7.

P70 - CS
P71 - CLK
P72 - DI
P73 - DO

It can be read while on the board by holding the main and secondary CPUs in reset state, which
keeps their pins in a high-impedence state.

### ECU Unit IO

[Table of ECU pins](ecu-pins.md)
