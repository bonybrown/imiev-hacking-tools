# 9499A182 and 9499B131 EV-ECU Terminal maps

This documents the signals going into and out of the ECU module.

The aim is to eventually trace each signal back to a CPU pin on IC1 or IC2

Adapted from the content in the Mitsubishi Service Manual.

## Table 1 (Terminals 1–35) C-106

| Terminal | Wiring Diagram Name | Type | Description | Active Condition | CPU Port (Pin) |
| --- | --- | --- | --- | --- | --- |
| 1 | BAT | Power In | Backup power supply input | Always: Battery voltage | - |
| 2 | CTL | Output (Digital) | ECU control power supply relay drive signal | ON: Battery voltage | ? |
| 3 | IG1 | Input (Digital) | IG switch signal | ON: Battery voltage | ? |
| 4 | CH+B | Input (Digital) | Quick charging power supply signal | Quick charging: 8 to 16 V | ? |
| 5 | - | - | - | - | - |
| 6 | - | - | - | - | - |
| 7 | VCSP | Power Out | Brake booster vacuum sensor power supply voltage | ON: 4.7 to 5.3 V | ? |
| 8 | APP2 | Power Out | Accelerator pedal position sensor (sub) power supply voltage | ON: 4.9 to 5.1 V | AD0IN11 (55) |
| 9 | IGCT | Power In | ECU control power supply input | ON: Battery voltage | ? |
| 10 | GND1 | Power GND | ECU power supply earth | 1 V or less | ? |
| 11 | CHGP | Input (Digital) | On board charging start signal | LOCK (OFF): Battery voltage | ? |
| 12 | - | - | - | - | - |
| 13 | - | - | - | - | - |
| 14 | DCNT | Input (Digital) | Diagnosis control signal | ON: Battery voltage | ? |
| 15 | ? | Input (Digital) | Timer presetting signal | ON: Battery voltage | ? |
| 16 | KLN2 | Bus (I/O) | K-LINE communication | - | ? |
| 17 | TCL | Input (Digital) | Traction control switch signal | TCL OFF switch OFF: Battery voltage | ? |
| 18 | - | - | - | - | - |
| 19 | VPSR | Output (Digital) | Brake electric vacuum pump main relay drive signal | ON: 1 V or less | ? |
| 20 | VPR1 | Output (Digital) | Brake electric vacuum pump control relay 1 drive signal | Brake depressed: Battery voltage → 1 V or less | ? |
| 21 | VPR2 | Output (Digital) | Brake electric vacuum pump control relay 2 drive signal | Brake depressed: Battery voltage → 1 V or less | ? |
| 22 | VCSN | Power GND | Brake booster vacuum sensor power supply earth | 1 V or less | ? |
| 23 | APN2 | Power GND | Accelerator pedal position sensor (sub) power supply earth | 1 V or less | ? |
| 24 | IG2 | Power In | ECU control power supply input | ON: Battery voltage | ? |
| 25 | GND2 | Power GND | ECU power supply earth | 1 V or less | ? |
| 26 | VPM1 | Input (Digital) | Brake electric vacuum pump main relay operation check signal | ON: Battery voltage | ? |
| 27 | VPM2 | Input (Digital) | Brake electric vacuum pump operation check signal | Brake depressed: 1 V or less → Battery voltage | ? |
| 28 | - | - | - | - | - |
| 29 | - | - | - | - | - |
| 30 | VPP | - | - | - | - |
| 31 | - | - | - | - | - |
| 32 | - | - | - | - | - |
| 33 | - | - | - | - | - |
| 34 | VCS1 | Input (Analog) | Brake electric vacuum pump vacuum 1 signal | ON: 0.2 to 1.7 V | AD0IN4 (48) |
| 35 | APS2 | Input (Analog) | Accelerator pedal position sensor (sub) signal | Released: 0.3-0.7 V, Fully depressed: 2.0-2.5 V | AD0IN5 (49) |


## Table 2 (Terminals 41–66) C-108

| Terminal | Wiring Diagram Name | Type | Description | Active Condition | CPU Port (Pin) |
| --- | --- | --- | --- | --- | --- |
| 41 | - | - | - | - | - |
| 42 | APP1 | Power Out | Accelerator pedal position sensor (main) power supply | ON: 4.9 to 5.1 V | AD0IN10 (54) |
| 43 | BPP2 | Power Out | Brake pedal stroke sensor power supply | ON: 4.9 to 5.1 V | AD0IN1 (45) |
| 44 | BPS2 | Input (Analog) | Brake pedal stroke sensor signal | Released: 0.5-2.5 V, Fully depressed: 2.5-4.5 V | AD0IN3 (47) |
| 45 | RFR2 | Output (Digital) | Cooling fan HI relay drive signal | Fan operating: 1 V or less | ? |
| 46 | RFR1 | Output (Digital) | Cooling fan LO relay drive signal | Fan operating: 1 V or less | ? |
| 47 | ? | Power GND | Brake fluid pressure sensor power supply earth | 1 V or less | ? |
| 48 | APN1 | Power GND | Accelerator pedal position sensor (main) power supply earth | 1 V or less | ? |
| 49 | BPN2 | Power GND | Brake pedal stroke sensor power supply earth | 1 V or less | ? |
| 50 | ARBS | Input (Digital) | Air bag collision signal | - | ? |
| 51 | ARBG | Power GND | Air bag collision signal earth | 1 V or less | ? |
| 52 | ? | Bus (I/O) | K-LINE communication | - | ? |
| 53 | KLNM | Bus (I/O) | K-LINE communication | - | ? |
| 54 | CNVL | Bus (I/O) | CAN-L to Inverter | - | P20/CTX0 (144), P21/CRX0 (1) |
| 55 | CNVH | Bus (I/O) | CAN-H to Inverter | - | P20/CTX0 (144), P21/CRX0 (1) |
| 56 | ? | Output (Digital) | Water pump relay drive signal | ON: 1 V or less | ? |
| 57 | BLP | Output (Digital) | Backup lamp relay drive signal | R position: 1 V or less | ? |
| 58 | - | - | - | - | - |
| 59 | APS1 | Input (Analog) | Accelerator pedal position sensor (main) signal | Released: 0.8-1.2 V, Fully depressed: 4.0-4.8 V | AD0IN2 (46) |
| 60 | PBLP | Input (Digital) | Parking brake lamp signal | Released: Battery voltage, Applied: 1 V or less | ? |
| 61 | SBSW | Input (Digital) | Parking brake switch signal | - | ? |
| 62 | BKSW | Input (Digital) | Brake switch signal | Depressed: Battery voltage | ? |
| 63 | ST | Input (Digital) | ST switch signal | START: Battery voltage | ? |
| 64 | - | - | - | - | - |
| 65 | ACB | Output (Digital) | A/C relay drive signal | ON: 1 V or less | ? |
| 66 | CHGB | Output (Digital) | On board charging relay drive signal | Charging cable connected: 1 V or less | ? |


## Table 3 (Terminals 71–98) C-110

| Terminal | Wiring Diagram Name | Type | Description | Active Condition | CPU Port (Pin) |
| --- | --- | --- | --- | --- | --- |
| 71 | - | - | - | - | - |
| 72 | WTS | Input (Analog) | Coolant temperature sensor signal | -20°C: 3.9-4.5 V, 0°C: 3.2-3.8 V, 20°C: 2.3-2.9 V, 40°C: 1.4-2.0 V, 60°C: 0.7-1.3 V, 80°C: 0.3-0.9 V | AD0IN8 (52) |
| 73 | - | - | - | - | - |
| 74 | ILK | Input (Digital) | Service plug switch signal | Interlock active: Battery voltage | ? |
| 75 | - | - | - | - | - |
| 76 | KLN1 | Bus (I/O) | K-LINE signal | - | ? |
| 77 | - | - | - | - | - |
| 78 | - | - | - | - | - |
| 79 | GWTS | Power GND | Coolant temperature sensor signal earth | 1 V or less | ? |
| 80 | - | - | - | - | - |
| 81 | ? | Input (Digital) | Shift position B2 range signal (sub) | C position: Battery voltage | ? |
| 82 | SFB2 | Input (Digital) | Shift position B2-range signal (main) | C position: Battery voltage | ? |
| 83 | SFB1 | Input (Digital) | Shift position B1-range signal (main) | B position: Battery voltage | ? |
| 84 | SFTD | Input (Digital) | Shift position D-range signal (main) | D position: Battery voltage | ? |
| 85 | SFTN | Input (Digital) | Shift position N-range signal (main) | N position: Battery voltage | ? |
| 86 | SFTR | Input (Digital) | Shift position R-range signal (main) | R position: Battery voltage | ? |
| 87 | SFTP | Input (Digital) | Shift position P-range signal (main) | P position: Battery voltage | ? |
| 88 | SFTV | Power Out | Shift position switch (main) power supply | ON: Battery voltage | ? |
| 89 | SFSV | Power Out | Shift position switch (sub) power supply | ON: Battery voltage | ? |
| 90 | - | - | - | - | - |
| 91 | - | - | - | - | - |
| 92 | ? | Input (Digital) | Shift position B1 range signal (sub) | B position: Battery voltage | ? |
| 93 | SFSD | Input (Digital) | Shift position D range signal (sub) | D position: Battery voltage | ? |
| 94 | SFSN | Input (Digital) | Shift position N range signal (sub) | N position: Battery voltage | ? |
| 95 | SFSR | Input (Digital) | Shift position R range signal (sub) | R position: Battery voltage | ? |
| 96 | SFSP | Input (Digital) | Shift position P range signal (sub) | P position: Battery voltage | ? |
| 97 | - | - | - | - | - |
| 98 | - | - | - | - | - |


## Table 4 (Terminals 101–130) C-111

| Terminal | Wiring Diagram Name | Type | Description | Active Condition | CPU Port (Pin) |
| --- | --- | --- | --- | --- | --- |
| 101 | CNEH | Bus (I/O) | CHAdeMO CAN-H (quick charge EVSE communication) | - | P137/CTX1 (130), P136/CRX1 (129) |
| 102 | CNEL | Bus (I/O) | CHAdeMO CAN-L (quick charge EVSE communication) | - | P137/CTX1 (130), P136/CRX1 (129) |
| 103 | CNCT | Input (Analog) | On board battery charging connection signal | Disconnected: 4.2-4.8 V, Release ON: 2.4-3.2 V, Release OFF: 1.2-2.0 V | AD0IN12 (56) |
| 104 | CNTF | Input (Digital) | Quick charging connection signal | Connected: 1 V or less | ? |
| 105 | CNTP | Output (Digital) | Charging contactor drive signal | START: 1 V or less | ? |
| 106 | CNT- | Output (Digital) | Main contactor (-) drive signal | After start: 1 V or less | ? |
| 107 | CNT+ | Output (Digital) | Main contactor (+) E signal | After start: 1 V or less | ? |
| 108 | CNIG | Power GND | On board battery charging connection earth | 1 V or less | ? |
| 109 | - | - | - | - | - |
| 110 | - | - | - | - | - |
| 111 | CINH | Input (Digital) | Quick charging approval signal | Quick charging: 10 to 16 V | ? |
| 112 | CHGC | Output (Digital) | Quick charger relay drive signal | Quick charging: 1 V or less | ? |
| 113 | WP | Output (Digital) | Water pump relay drive signal | After start, coolant ≥45°C: Battery voltage | ? |
| 114 | - | - | - | - | - |
| 115 | - | - | - | - | - |
| 116 | DCSW | Output (Digital) | 12 V DCDC converter shutdown signal | After start: Battery voltage | ? |
| 117 | RSDN | Output (Digital) | Inverter shutdown signal | After start: Battery voltage | ? |
| 118 | - | - | - | - | - |
| 119 | - | - | - | - | - |
| 120 | CNTB | Power In | Contactor power supply input | ON: Battery voltage | ? |
| 121 | - | - | - | - | - |
| 122 | - | - | - | - | - |
| 123 | - | - | - | - | - |
| 124 | S2 | Input (Digital) | Quick charging start signal | Quick charging: 1 V or less | ? |
| 125 | WPN | Input (Digital) | Water pump rotation signal | After start, coolant ≥45°C: Pulse signal | ? |
| 126 | - | - | - | - | - |
| 127 | SDW | Output (Digital) | 12-V converter drive signal | After start: Battery voltage | ? |
| 128 | - | - | - | - | - |
| 129 | - | - | - | - | - |
| 130 | - | - | - | - | - |
