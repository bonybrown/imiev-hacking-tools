# MUT3 Tools

Information here applies to MUT3 _Second Edition_ (MUT3 SE)

## MUT3 SE Info

### Finding details of your vehicle

All data is contained in XML files that are encrypted with
the [xor-swap](decrypt-xor-swap) scheme.

From a given VIN number, the VIN lookup file to
search is determined by consulting the VIN_Selection.exdf file.
It is an index of VIN ranges to the VIN_Search_XXX.exdf file that would contain that VIN.

The VIN is then located in the VIN_Search_XXX.exdf file.

The vehicle Type, Kind and Model Year is obtained from the file.

The Type, Kind and Model Year are then sought in the Vehicle_DB.exdf file,
with regard to the destination country (from DEST_DB.exdf)
which gives the list of components (ECU/BMU/xxU)
that should exist in the vehicle.

A tool that performs the above may be found at https://mohammad-ali.bandzar.com/tools/mut-iii-commondb-explorer/

### xxU firmware updates

Details of firmware updates are delivered in the SDB.exdf
file that comes with each release of software updates.
It provides details of what software part numbers
in which hardware part numbers have updates, and to
which software part number it can be upgraded.

For example:
```xml
    <mff>
      <mffFileName>9486A080.mff</mffFileName>
      <newHWPN>9499B056</newHWPN>
      <newSWPN>9499B05602</newSWPN>
      <targetHWPN>9499B056</targetHWPN>
      <targetSWPN>9499B05601</targetSWPN>
      <classification>7</classification>
      <displayOrder>000291</displayOrder>
      <description>12/13 i-MiEV (NAS Spec.)     Vacuum pump initial operation change.</description>
      <releaseDate>20161202</releaseDate>
      <releaseType>Recall Campaign</releaseType>
      <referenceNo/>
    </mff>
```

The `mffFileName` element will point to a .mff file.
This file is a xor-swap encrypted "Microsoft cabinet" file.

Once decrypted, it can be listed and extracted with the `7z` program.

```
   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
2016-12-02 16:24:30 ....A          932               9486A080.xml
2015-05-21 16:05:00 ....A          566               SWIL1_M32186_V03.mot
2015-05-21 16:05:00 ....A          496               SWIL2_M32186_V03.mot
2014-06-09 10:32:52 ....A      1691092               W635B_enc.mot
------------------- ----- ------------ ------------  ------------------------
2016-12-02 16:24:30            1693086       473034  4 files
```

In the cabinet file, the flash files exist, as Motorola
SREC format files.

An xml file exists that describes the other files:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<mff_define>
  <file_information>
    <version>1.00</version>
    <comment>Initial Release</comment>
  </file_information>
  <sequence_information>
    <sequence>01</sequence>
    <swils>
      <swil>swil1</swil>
      <swil>swil2</swil>
    </swils>
    <logicalBlocks>
      <logicalBlock>logicalBlock1</logicalBlock>
    </logicalBlocks>
  </sequence_information>
  <module_information>
    <swil1>
      <checkRoutineType>01</checkRoutineType>
      <encryptCompressType>01</encryptCompressType>
      <fileName>SWIL1_M32186_V03.mot</fileName>
      <format>01</format>
    </swil1>
    <swil2>
      <checkRoutineType>01</checkRoutineType>
      <encryptCompressType>01</encryptCompressType>
      <fileName>SWIL2_M32186_V03.mot</fileName>
      <format>01</format>
    </swil2>
    <logicalBlock1>
      <blockNumber>1</blockNumber>
      <blockType>01</blockType>
      <checkRoutineType>02</checkRoutineType>
      <encryptCompressType>01</encryptCompressType>
      <fileName>W635B_enc.mot</fileName>
      <format>01</format>
    </logicalBlock1>
  </module_information>
</mff_define>
```

The `SWIL` files are I believe "SoftWare Inter-Lock" files.
They seems to contain the code to update the flash,
split into two parts. `W635B_enc.mot` is the actual firmware.


Use the `srec` suite to inspect the `.mot` files:

```
$ srec_info W635B_enc.mot
Format: Motorola S-Record
Header: ""
Execution Start Address: 00000000
Data:   008000 - 0080FF
        008200 - 01E6FF
        01FD00 - 01FDFF
        020000 - 0236FF
        025000 - 03D6FF
        03F800 - 03FDFF
        03FF00 - 040DFF
        04DB00 - 04F8FF
        050000 - 0BBAFF
        0E0000 - 0E02FF
        0E1200 - 0E12FF
        0E3000 - 0E33FF
        0E4000 - 0EB8FF
        0EBD00 - 0EBDFF
        0FFF00 - 0FFFFF
        200000 - 200001
```
Note the entry for address `0x20 0000`. This is:
* Outside of the 32186's ROM area
* Not in the SFR or RAM areas
* Only 2 bytes

Which makes me believe it's a checksum, not part of the code at all,
and is maybe only used as part of the upload process.

That section can be omitted when converting to binary

```bash
srec_cat W635B_enc.mot -exclude 0x200000 0x210000 -o W635B_enc.bin -binary
```

The binary these SREC files represent are encrypted (again)
but this time with the ["firmware" encryption](decrypt-firmware) type.

Use the `decrypt-firmware` tool to convert the encrypted binary to plaintext binary.

If done right, `strings` should yield interesting output when run against the binary.

```
$ strings -n 12 W635B.bin
T6T7T8T9T:T;T<T=T>T?
UUUTUUUUUUUVUUUXUUU`UUU
std_s(GAIO)    
MAB M32R       
V02.08         
math_s(GAIO)   
MAB M32R       
V02.08         
cs(GAIO)       
MAB M32R       
V01.01         
ert(MATLAB)    
MAB M32R       
2007a/7.4.0.287
Can Reproggram 
MH8106F 80MHz  
00.06.00       
2009/02/10(Tue)
BOOT           
MMC MIEV EV-ECU
00.07.07       
2009/08/03(Mon)
BACKUP RAM     
MMC MIEV EV-ECU
00.07.07       
2009/08/03(Mon)
EEPROM         
MMC MIEV EV-ECU
00.08.11       
2010/10/12(Tue)
TASK ciNAFWH   
MH8106F 80MHz  
00.01.30       
2009/05/20(Wed)
PORT           
MMC MIEV EV-ECU
00.05.01       
2009/01/29(Thu)
RAM MONITOR    
MMC MIEV EV-ECU
00.01.03       
2008/12/04(Thu)
Watch CPU
MMC MIEV EV-ECU
00.07.06       
2009/07/07(Tue)
CHECKER        
MMC MIEV EV-ECU
00.07.07       
2009/07/23(Wed)
DTC            
MMC MIEV EV-ECU
00.09.07       
Jan 28 2011    
KWP2000        
MMC MIEV EV-ECU
00.09.07       
Jan 28 2011    
ARBS-COMM      
MMC MIEV EV-ECU
00.01.01       
2008/11/18(Tue)
IMMOBI         
MMC MIEV EV-ECU
00.08.08       
2010/02/26(Fri)
DBKOM 12MY     
MMC MIEV EV-ECU
00.02.06       
CHG-CAN 12MY   
MMC MIEV EV-ECU
00.02.01       
Mon May 12 16:17:51 2014
BA871710_NAS88Q
MAB_MMC_3F45E_EV_9.07a
mab_m32r186_PC.tmf
DiagEv:0x0100
MMC MIEV EV-ECU
MH8106F 80MHz  
9.07.0013:59:05
W635BV000T90778H
 D d d d d d d d d d d d d d d d d d d da
JMBMNCJ19YU097209
9499B056  9499B05602
` `!`"`*`+`,`-`.
CAN_3F-E_for_ALL.MMC_20110118.dbc;
?333?333?333>
<<<<<<d<<<<d<
@c33@fff@fff@i
```

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
