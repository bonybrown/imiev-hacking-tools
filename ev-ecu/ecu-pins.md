# 9499A182 and 9499B131 EV-ECU Terminal maps

This documents the signals going into and out of the ECU module.

The aim is to eventually trace each signal back to a CPU pin on IC1 or IC2

Adapted from the content in the Mitsubishi Service Manual.

## Table 1 (Terminals 1–35) C-106


| Terminal | Wiring Diagram Name | Type             | Description                                                           | Active Condition                                | CPU Port (Pin) |
| -------- | ------------------- | ---------------- | --------------------------------------------------------------------- | ----------------------------------------------- | -------------- |
| 1        | BAT                 | Power In         | Backup power supply input                                             | Always: Battery voltage                         | -              |
| 2        | CTL                 | Output (Digital) | ECU control power supply relay drive signal via D14, Q3?              | ON: Battery voltage                             | ?              |
| 3        | IG1                 | Input (Digital)  | IG switch signal via D15, R38, Q5                                    | ON: Battery voltage                             | P9.5 (88)      |
| 4        | CH+B                | Input (Digital)  | Quick charging power supply signal via D16, Q6                        | Quick charging: 8 to 16 V                       | P8.4 (68)      |
| 5        | -                   | -                | -                                                                     | -                                               | -              |
| 6        | -                   | -                | -                                                                     | -                                               | -              |
| 7        | VCSP                | Power Out        | Brake booster vacuum sensor power supply voltage, 6 pin SOT near IC47 | ON: 4.7 to 5.3 V                                | ?              |
| 8        | APP2                | Power Out        | Accelerator pedal position sensor (sub) power supply voltage, IC41    | ON: 4.9 to 5.1 V                                | AD0IN11 (55)   |
| 9        | IGCT                | Power In         | ECU control power supply input                                        | ON: Battery voltage                             | ?              |
| 10       | GND1                | Power GND        | ECU power supply earth                                                | 1 V or less                                     | ?              |
| 11       | CHGP                | Input (Digital)  | On board charging start signal                                        | LOCK (OFF): Battery voltage                     | ?              |
| 12       | -                   | -                | -                                                                     | -                                               | -              |
| 13       | -                   | -                | -                                                                     | -                                               | -              |
| 14       | DCNT                | Input (Digital)  | Diagnosis control signal                                              | ON: Battery voltage                             | /P2.25 (2)     |
| 15       | ?                   | Input (Digital)  | Timer presetting signal via D46, purpose unknown                      | ON: Battery voltage                             | ?              |
| 16       | KLN2                | Bus (I/O)        | K-LINE communication, R356                                            | -                                               | ?              |
| 17       | TCL                 | Input (Digital)  | Traction control switch signal                                        | TCL OFF switch OFF: Battery voltage             | ?              |
| 18       | -                   | -                | -                                                                     | -                                               | -              |
| 19       | VPSR                | Output (Digital) | Brake electric vacuum pump main relay drive signal via IC44           | ON: 1 V or less                                 | ?              |
| 20       | VPR1                | Output (Digital) | Brake electric vacuum pump control relay 1 drive signal via IC47      | Brake depressed: Battery voltage → 1 V or less | ?              |
| 21       | VPR2                | Output (Digital) | Brake electric vacuum pump control relay 2 drive signal via IC47      | Brake depressed: Battery voltage → 1 V or less | ?              |
| 22       | VCSN                | Power GND        | Brake booster vacuum sensor power supply earth                        | 1 V or less                                     | ?              |
| 23       | APN2                | Power GND        | Accelerator pedal position sensor (sub) power supply earth            | 1 V or less                                     | ?              |
| 24       | IG2                 | Power In         | ECU control power supply input                                        | ON: Battery voltage                             | ?              |
| 25       | GND2                | Power GND        | ECU power supply earth                                                | 1 V or less                                     | ?              |
| 26       | VPM1                | Input (Digital)  | Brake electric vacuum pump main relay operation check signal          | ON: Battery voltage                             | ?              |
| 27       | VPM2                | Input (Digital)  | Brake electric vacuum pump operation check signal                     | Brake depressed: 1 V or less → Battery voltage | ?              |
| 28       | -                   | -                | -                                                                     | -                                               | -              |
| 29       | -                   | -                | -                                                                     | -                                               | -              |
| 30       | VPP                 | Input (Digital)  | Holds IC1 and IC2 in reset when asserted, via inverter IC31           | -                                               | /RESET (91)    |
| 31       | -                   | -                | -                                                                     | -                                               | -              |
| 32       | -                   | -                | -                                                                     | -                                               | -              |
| 33       | -                   | -                | -                                                                     | -                                               | -              |
| 34       | VCS1                | Input (Analog)   | Brake electric vacuum pump vacuum 1 signal                            | ON: 0.2 to 1.7 V                                | AD0IN4 (48)    |
| 35       | APS2                | Input (Analog)   | Accelerator pedal position sensor (sub) signal                        | Released: 0.3-0.7 V, Fully depressed: 2.0-2.5 V | AD0IN5 (49)    |

## Table 2 (Terminals 41–66) C-108


| Terminal | Wiring Diagram Name | Type             | Description                                                 | Active Condition                                | CPU Port (Pin)               |
| -------- | ------------------- | ---------------- | ----------------------------------------------------------- | ----------------------------------------------- | ---------------------------- |
| 41       | -                   | -                | -                                                           | -                                               | -                            |
| 42       | APP1                | Power Out        | Accelerator pedal position sensor (main) power supply, IC40 | ON: 4.9 to 5.1 V                                | AD0IN10 (54)                 |
| 43       | BPP2                | Power Out        | Brake pedal stroke sensor power supply                      | ON: 4.9 to 5.1 V                                | AD0IN1 (45)                  |
| 44       | BPS2                | Input (Analog)   | Brake pedal stroke sensor signal                            | Released: 0.5-2.5 V, Fully depressed: 2.5-4.5 V | AD0IN3 (47)                  |
| 45       | RFR2                | Output (Digital) | Cooling fan HI relay drive signal via IC22, R256            | Fan operating: 1 V or less                      | ?                            |
| 46       | RFR1                | Output (Digital) | Cooling fan LO relay drive signal via IC22, R259            | Fan operating: 1 V or less                      | ?                            |
| 47       | ?                   | Power GND        | Brake fluid pressure sensor power supply earth              | 1 V or less                                     | ?                            |
| 48       | APN1                | Power GND        | Accelerator pedal position sensor (main) power supply earth | 1 V or less                                     | ?                            |
| 49       | BPN2                | Power GND        | Brake pedal stroke sensor power supply earth                | 1 V or less                                     | ?                            |
| 50       | ARBS                | Input (Digital)  | Air bag collision signal                                    | -                                               | ?                            |
| 51       | ARBG                | Power GND        | Air bag collision signal earth                              | 1 V or less                                     | ?                            |
| 52       | ?                   | Bus (I/O)        | K-LINE communication                                        | -                                               | ?                            |
| 53       | KLNM                | Bus (I/O)        | K-LINE communication                                        | -                                               | ?                            |
| 54       | CNVL                | Bus (I/O)        | Main CAN bus L via IC6, L2, R284                           | -                                               | P20/CTX0 (144), P21/CRX0 (1) |
| 55       | CNVH                | Bus (I/O)        | Main CAN bus H via IC6, L2, R283                            | -                                               | P20/CTX0 (144), P21/CRX0 (1) |
| 56       | WPR-                | Output (Digital) | Water pump relay drive signal via IC21, R58                 | ON: 1 V or less                                 | ?                            |
| 57       | BLP                 | Output (Digital) | Backup lamp relay drive signal via IC23, R258               | R position: 1 V or less                         | ?                            |
| 58       | -                   | -                | -                                                           | -                                               | -                            |
| 59       | APS1                | Input (Analog)   | Accelerator pedal position sensor (main) signal, D182       | Released: 0.8-1.2 V, Fully depressed: 4.0-4.8 V | AD0IN2 (46)                  |
| 60       | PBLP                | Input (Digital)  | Parking brake lamp signal D182 A                            | Released: Battery voltage, Applied: 1 V or less | ?                            |
| 61       | SBSW                | Input (Digital)  | Parking brake switch signal D182 K                          | -                                               | ?                            |
| 62       | BKSW                | Input (Digital)  | Brake switch signal, R101, R102                             | Depressed: Battery voltage                      | ?                            |
| 63       | ST                  | Input (Digital)  | ST switch signal, R46, R47, Q5                              | START: Battery voltage                          | P9.6 (89)                    |
| 64       | -                   | -                | -                                                           | -                                               | -                            |
| 65       | ACB                 | Output (Digital) | A/C relay drive signal via IC23, R261                       | ON: 1 V or less                                 | ?                            |
| 66       | CHGB                | Output (Digital) | On board charging relay drive signal via IC21, R54          | Charging cable connected: 1 V or less           | ?                            |

## Table 3 (Terminals 71–98) C-110


| Terminal | Wiring Diagram Name | Type            | Description                                 | Active Condition                                                                                           | CPU Port (Pin) |
| -------- | ------------------- | --------------- | ------------------------------------------- | ---------------------------------------------------------------------------------------------------------- | -------------- |
| 71       | -                   | -               | -                                           | -                                                                                                          | -              |
| 72       | WTS                 | Input (Analog)  | Coolant temperature sensor signal           | -20°C: 3.9-4.5 V, 0°C: 3.2-3.8 V, 20°C: 2.3-2.9 V, 40°C: 1.4-2.0 V, 60°C: 0.7-1.3 V, 80°C: 0.3-0.9 V | AD0IN8 (52)    |
| 73       | -                   | -               | -                                           | -                                                                                                          | -              |
| 74       | ILK                 | Input (Digital) | Service plug switch signal                  | Interlock active: Battery voltage                                                                          | ?              |
| 75       | -                   | -               | -                                           | -                                                                                                          | -              |
| 76       | KLN1                | Bus (I/O)       | K-LINE signal, R354                         | -                                                                                                          | ?              |
| 77       | -                   | -               | -                                           | -                                                                                                          | -              |
| 78       | -                   | -               | -                                           | -                                                                                                          | -              |
| 79       | GWTS                | Power GND       | Coolant temperature sensor signal earth     | 1 V or less                                                                                                | ?              |
| 80       | -                   | -               | -                                           | -                                                                                                          | -              |
| 81       | SSB2                | Input (Digital) | Shift position B2 range signal (sub), R157  | C position: Battery voltage                                                                                | ?              |
| 82       | SFB2                | Input (Digital) | Shift position B2-range signal (main), R133 | C position: Battery voltage                                                                                | ?              |
| 83       | SFB1                | Input (Digital) | Shift position B1-range signal (main), R131 | B position: Battery voltage                                                                                | ?              |
| 84       | SFTD                | Input (Digital) | Shift position D-range signal (main), R129  | D position: Battery voltage                                                                                | ?              |
| 85       | SFTN                | Input (Digital) | Shift position N-range signal (main), R127  | N position: Battery voltage                                                                                | ?              |
| 86       | SFTR                | Input (Digital) | Shift position R-range signal (main), R125  | R position: Battery voltage                                                                                | ?              |
| 87       | SFTP                | Input (Digital) | Shift position P-range signal (main), R123  | P position: Battery voltage                                                                                | ?              |
| 88       | SFTV                | Power Out       | Shift position switch (main) power supply   | ON: Battery voltage                                                                                        | ?              |
| 89       | SFSV                | Power Out       | Shift position switch (sub) power supply    | ON: Battery voltage                                                                                        | ?              |
| 90       | -                   | -               | -                                           | -                                                                                                          | -              |
| 91       | -                   | -               | -                                           | -                                                                                                          | -              |
| 92       | SSB1                | Input (Digital) | Shift position B1 range signal (sub), R155  | B position: Battery voltage                                                                                | ?              |
| 93       | SFSD                | Input (Digital) | Shift position D range signal (sub), R153   | D position: Battery voltage                                                                                | ?              |
| 94       | SFSN                | Input (Digital) | Shift position N range signal (sub), R151   | N position: Battery voltage                                                                                | ?              |
| 95       | SFSR                | Input (Digital) | Shift position R range signal (sub), R149   | R position: Battery voltage                                                                                | ?              |
| 96       | SFSP                | Input (Digital) | Shift position P range signal (sub), R147   | P position: Battery voltage                                                                                | ?              |
| 97       | -                   | -               | -                                           | -                                                                                                          | -              |
| 98       | -                   | -               | -                                           | -                                                                                                          | -              |

## Table 4 (Terminals 101–130) C-111


| Terminal | Wiring Diagram Name | Type             | Description                                             | Active Condition                                                       | CPU Port (Pin)                   |
| -------- | ------------------- | ---------------- | ------------------------------------------------------- | ---------------------------------------------------------------------- | -------------------------------- |
| 101      | CNEH                | Bus (I/O)        | CHAdeMO CAN bus H via IC5, L1, R282                     | -                                                                      | P137/CTX1 (130), P136/CRX1 (129) |
| 102      | CNEL                | Bus (I/O)        | CHAdeMO CAN bus L via IC5, L1, R281                     | -                                                                      | P137/CTX1 (130), P136/CRX1 (129) |
| 103      | CNCT <br> Type 1 charge port PP              | Input (Analog)   | On board battery charging connection signal, R381, R382 | Disconnected: 4.2-4.8 V, Release ON: 2.4-3.2 V, Release OFF: 1.2-2.0 V | AD0IN12 (56)                     |
| 104      | CNTF                | Input (Digital)  | Quick charging connection signal, D18                   | Connected: 1 V or less                                                 | ?                                |
| 105      | CNTP                | Output (Digital) | Charging contactor drive signal, IC25                   | START: 1 V or less (active low)                                        | ?                                |
| 106      | CNT-                | Output (Digital) | Main contactor (-) drive signal, IC25                   | After start: 1 V or less (active low)                                  | ?                                |
| 107      | CNT+                | Output (Digital) | Main contactor (+) drive signal, IC24                   | After start: 1 V or less (active low)                                  | ?                                |
| 108      | CNIG                | Power GND        | On board battery charging connection earth              | 1 V or less                                                            | ?                                |
| 109      | -                   | -                | -                                                       | -                                                                      | -                                |
| 110      | -                   | -                | -                                                       | -                                                                      | -                                |
| 111      | CINH                | Input (Digital)  | Quick charging approval signal via IC43, R314           | Quick charging: 10 to 16 V                                             | ?                                |
| 112      | CHGC                | Output (Digital) | Quick charger relay drive signal via IC43, R313         | Quick charging: 1 V or less                                            | ?                                |
| 113      | WP                  | Output (Digital) | Water pump relay drive signal                           | After start, coolant ≥45°C: Battery voltage                          | ?                                |
| 114      | -                   | -                | -                                                       | -                                                                      | -                                |
| 115      | -                   | -                | -                                                       | -                                                                      | -                                |
| 116      | DCSW                | Output (Digital) | 12 V DCDC converter shutdown signal via R227, Q64       | After start: Battery voltage                                           | ?                                |
| 117      | RSDN                | Output (Digital) | Inverter shutdown signal via R221, Q61                  | After start: Battery voltage                                           | ?                                |
| 118      | -                   | -                | -                                                       | -                                                                      | -                                |
| 119      | -                   | -                | -                                                       | -                                                                      | -                                |
| 120      | CNTB                | Power In         | Contactor power supply input to IC24, IC25              | ON: Battery voltage                                                    | ?                                |
| 121      | -                   | -                | -                                                       | -                                                                      | -                                |
| 122      | -                   | -                | -                                                       | -                                                                      | -                                |
| 123      | -                   | -                | -                                                       | -                                                                      | -                                |
| 124      | S2                  | Input (Digital)  | Quick charging start signal, D170                       | Quick charging: 1 V or less                                            | ?                                |
| 125      | WPN                 | Input (Digital)  | Water pump rotation signal, R240                        | After start, coolant ≥45°C: Pulse signal                             | ?                                |
| 126      | -                   | Input (Digital)  | D195, Q52                                               | -                                                                      | P12.5 (120)                      |
| 127      | SDW                 | Input (Digital) | 12-V converter drive signal, R230, R231, Q66            | After start: Battery voltage                                           | P2.0 (16)                        |
| 128      | -                   | -                | -                                                       | -                                                                      | -                                |
| 129      | -                   | -                | -                                                       | -                                                                      | -                                |
| 130      | -                   | -                | -                                                       | -                                                                      | -                                |
