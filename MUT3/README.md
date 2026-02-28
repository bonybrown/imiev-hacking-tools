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

The python program [exploring-commondb.py](mut-metadata-extraction/exploring-commondb.py) is an extension of that
tool that also outputs the CAN ids and ECU filenames of the relevant units in the vehicle.
(CAN ids in the output are hexadecimal)

```
$ python3 exploring-commondb.py 'your-vin-here' /MUT3_SE/CommonDB
VIN information:
{'VehicleKind': '2380', 'VehicleType': '309', 'VehicleYear': '12'}
vehicle configuration:
{'DCSMV': '4D69455620202020',
 'ENGINE_ID': 'Y01',
 'MAKER_ID': '1',
 'TRANSMISSION_ID': 'A01',
 'VEHICLE_FAMILY': '-',
 'VHCL_ID': '48',
 'VHCL_VIEW': '0'}
canbus specification:
{'CANBUS_ID': '545104', 'PASSTHRU': '0'}
engine name: Y4F1
transmission name: F1E1A
model name: i-MiEV
destination name: MMAL
maker name: MITSUBISHI
vehicle kind: LDDR8
vehicle type: HA3W
vehicle key (vkey): 1107180001
canbus info:
{'canbus_id': '545104',
 'parts': [{'can_id_mon': '424', 'name': 'ETACS', 'parts_id': '2080'},
           {'can_id_mon': '231', 'name': 'ASC', 'parts_id': '2081'},
           {'can_id_mon': '2F2', 'name': 'EPS', 'parts_id': '2082'},
           {'can_id_mon': '412', 'name': 'METER', 'parts_id': '2083'},
           {'can_id_mon': '236', 'name': 'SAS', 'parts_id': '2084'},
           {'can_id_mon': '149', 'name': 'YR/G', 'parts_id': '2085'},
           {'can_id_mon': '38D', 'name': 'REMOTE', 'parts_id': '2086'},
           {'can_id_mon': '385', 'name': 'COMP&HTR', 'parts_id': '2102'},
           {'can_id_mon': '375', 'name': 'BMU', 'parts_id': '2088'},
           {'can_id_mon': '389', 'name': 'OBC', 'parts_id': '2089'},
           {'can_id_mon': '565', 'name': 'MCU', 'parts_id': '2090'},
           {'can_id_mon': '308', 'name': 'EV', 'parts_id': '2091'}]}
diagnostic system info:
[{'ecu_id': '45',
  'ecu_name': 'ETACS_83040300',
  'rx_can_id': '7A1',
  'tx_can_id': '7A0'},
 {'ecu_id': '43',
  'ecu_name': 'EPS_45800010',
  'rx_can_id': '797',
  'tx_can_id': '796'},
 {'ecu_id': '129',
  'ecu_name': 'ASC_4200000E',
  'rx_can_id': '785',
  'tx_can_id': '784'},
  
  ...<snipped>...
```

We can see here there is an ETACS module with can_id_mon=424 (unsure what that does) and
the name of the file containing ETACS specific data is in `ETACS_83040300`
(specifically, `Ecu/ETACS_83040300.exdf` relative to the `MUT3_SE/DiagDB` directory)

### Reading ECU specific data and function definitions

The python program [ecu_function_translator.py](mut-metadata-extraction/ecu_function_translator.py) is a
tool that will dump the definitions of the data that can be read and written, and the
routines that can be performed with this unit.

**NOTE:** there is some `skey` value that I don't know how to map to the ECU files, so this 
tool dumps _all_ the definitions in the file. I'd guess that only one `skey` set
would apply to an individual ECU unit, but I don't know how to determine which one.

The definitions seem to be how to operate the KWP2000/UDS service ISO 14230 diagnostic protocol to
read, write and run routines on the unit, via the can PIDs that are defined for each.

[This](https://www.google.com/url?sa=t&source=web&rct=j&opi=89978449&url=https://andrewrevill.co.uk/ReferenceLibrary/OBDII%2520Specifications%2520-%2520KWP2000%2520DaimlerChrysler%25202002.pdf)
seems to be a good reference for the protocol.

An excerpt from the output against `ETACS_83040300.xml`...
```
    ├─ qual_id (sid=33 ReadDataByLocalIdentifier, lid=3)
          ├─ Item 17 [iid=22000055]: Battery voltage [byte_pos=4, byte_len=2, unit=V, scale(factor=0.0175781, offset=0)]
          └─ Item 19 [iid=22000058]: Vehicle speed signal [byte_pos=6, byte_len=1, unit=km/h, scale(factor=1, offset=0)]
     └─ qual_id (sid=33 ReadDataByLocalIdentifier, lid=2)
          ├─ Item 18 [iid=22000056]: Remain of head lamp timer [byte_pos=6, byte_len=1, unit=sec, scale(factor=1, offset=0)]
          └─ Item 20 [iid=22000086]: Intermittent wiper interval [byte_pos=2, byte_len=2, unit=sec, scale(factor=0.1, offset=0)]
  └─ Function fid="48": Actuator Test
  └─ Function fid="402": Freeze Frame Data
  └─ Function fid="1708": ECU Information
     ├─ qual_id (sid=26 ReadECUIdentification, lid=156)
          └─ Item 1 [iid=8026849]: Software part number [byte_pos=11, byte_len=10]
     ├─ qual_id (sid=26 ReadECUIdentification, lid=135)
          └─ Item 2 [iid=8026850]: Hardware part number [byte_pos=12, byte_len=10]
     └─ qual_id (sid=33 ReadDataByLocalIdentifier, lid=225)
          └─ Item 3 [iid=90000004]: ECU serial number [byte_pos=2, byte_len=-1]
  └─ Function fid="2808": Customization
     ├─ qual_id (sid=33 ReadDataByLocalIdentifier, lid=49)
          ├─ Item 4 [iid=222900084]: Door-ajar warning function [byte_pos=2[0..7], byte_len=1]
          │    ├─ [32] Constantly
          │    ├─ [64] Operation
          │    └─ [128] Only indicator
          ├─ Item 5 [iid=222900085]: Turn-signal lamp buzzer [byte_pos=3[0..7], byte_len=1]
          │    ├─ [64] operation
          │    └─ [128] inhibit
          ├─ Item 6 [iid=222900086]: Seatbelt warning buzzer [byte_pos=4[0..7], byte_len=1]
          │    ├─ [64] Operation
          │    └─ [128] Only IG ON
          ├─ Item 7 [iid=222900314]: Horn answer-back(Night time) [byte_pos=5[0..7], byte_len=1]
          │    ├─ [64] operation
          │    └─ [128] inhibit
          ├─ Item 8 [iid=222900087]: Turn-signal lamp [byte_pos=6[0..7], byte_len=1]
          │    ├─ [64] IG1
          │    └─ [128] ACC or IG1
          ├─ Item 9 [iid=222900088]: Comfort flashing function [byte_pos=7[0..7], byte_len=1]
          │    ├─ [64] operation
          │    └─ [128] inhibit
          ├─ Item 10 [iid=222900089]: Hazard answer-back [byte_pos=8[0..7], byte_len=1]
          │    ├─ [2] Lock 0 Unlock 0
          │    ├─ [4] Lock 2 Unlock 0
          │    ├─ [8] Lock 0 Unlock 1
          │    ├─ [16] Lock 2 Unlock 1
          │    ├─ [32] Lock 0 Unlock 2
          │    ├─ [64] Lock 1 Unlock 0
          │    └─ [128] Lock 1 Unlock 2
```

From this it would seem that by communicating with the ETACS unit via CAN PIDs
`7A1` and `7A0`, a ReadECUIdentification request with local_id=156 could be sent,
and the response would contain the Software part number in bytes 11 to 20 (10 bytes).

A ReadDataByLocalIdentifier request with local_id=49 would return the ETACS customisation
data, with fields as described.

Section of a BMU file run through the tool:
```
SKEY sid="750011902"
  └─ Function fid="4": Data List
     ├─ qual_id (sid=33 ReadDataByLocalIdentifier, lid=1)
          ├─ Item 1 [iid=75000001]: Charge state(Control) [byte_pos=2, byte_len=1, unit=%, scale(factor=0.5, offset=-5)]
          ├─ Item 2 [iid=75000002]: Charge state(Display) [byte_pos=3, byte_len=1, unit=%, scale(factor=0.5, offset=-5)]
          ├─ Item 3 [iid=75000003]: Battery cell maximum voltage [byte_pos=4, byte_len=2, unit=V, scale(factor=0.005, offset=2.1)]
          ├─ Item 4 [iid=75000004]: Maximum voltage cell ID [byte_pos=6, byte_len=1, scale(factor=1, offset=1)]
          ├─ Item 5 [iid=75000005]: Battery cell minimum voltage [byte_pos=7, byte_len=2, unit=V, scale(factor=0.005, offset=2.1)]
```

The scaling factors are consistent with what we already know eg `scale(factor=0.005, offset=2.1)` for a battery cell voltage representation.

Note these data list items are what MUT3 shows in the Data List function of the
specific ECU. They are reported by the ECU in this diagnostic service and don't
directly correlate to the CAN bus data the units send in normal operation.

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
