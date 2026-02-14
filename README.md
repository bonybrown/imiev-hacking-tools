# imiev-hacking-tools
Tools and info for working with Mitsubishi iMiev software

## EV-ECU and BMU Info

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

## What's in this repo

### [Tools for working with MUT3 files](MUT3)

For decrypting and navigating around the data in the MUT3 software.
