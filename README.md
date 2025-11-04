# imiev-hacking-tools
Tools and info for working with Mitsubishi iMiev software

## ECU and BMU Info

The Engine Control Unit (ECU) and Battery Control Unit (BMU)
both use a Mitsubish branded CPU labelled `M8106`. 
This seems to be the [Renesas 3280186???](https://link-to-manual).
* [M32R-FP instruction set](https://link-to-software-manual) (M32R plus single precision floating point operations)
* Single precision floating point unit
* 1024kB of Flash (0x0 to 0x???)
* 32kB of RAM (0x080000 to ????) 
* Special function registers (0x80000 to ???)

## What's in this repo

### [Tools for working with MUT3 files](MUT3)

For decrypting and navigating around the data in the MUT3 software.