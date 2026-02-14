# MUT3 Tools

Information here applies to MUT3 _Second Edition_ (MUT3 SE)

## MUT3 SE Info

### Finding details of your vehicle

All data is contained in XML files that are encrypted with
the [xor-swap](decrypt-xor-swap) scheme.

From a given VIN number, the VIN number lookup file to
search is determined by consulting the ____.exdf file
for which file contains the range of VINs

The VIN is then located in that file.

The vehicle x,y and z is obtained.

The x,y and z are then sought in the ____.exdf file,
which gives the list of components (ECU/BMU/xxU)
that should exist in the vehicle.

### xxU firmware updates

Details of firmware updates are delivered in the ____.exdf
file. It provides details of what software part numbers
in which hardware part numbers have updates, and to
which software part number it can be upgraded.

The update entry will point to a .mff file.
This file is a xor-swap encrypted "cabinet" file.

In the .mff file, the flash files exist, as Motorola
SREC format files.

The binary these SREC files represent are encrypted (again)
but this time with the ["firmware" encryption](decrypt-firmware) type.

## Tools

### decrypt-xor-swap

This tool decrypts files encrypted with the XOR and nibble swap scheme.

These MUT3 files are :
* the .exdf files (Encrypted XML Data Files) that contain
the MUT3 databases of vehicles, components and how MUT3 
obtains data from the components.
* The .mff files (Mitsubishi Flash Format?) files. These
are actually Microsoft cabinet archive files encrypted
with this scheme. These contain the flash updates for
the ECUs in the vehicle.

### decrypt-firmware

This tool decrypts the binary files that are generated
from the SREC formatted flash update files contained in the
encrypted CAB file in the .mff file. It is a simple byte
substitution cipher.
