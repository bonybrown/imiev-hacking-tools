# 9499A182 and 9499B131 EV-ECU Terminal maps

This documents the signals going into and out of the ECU module.

The aim is to eventually trace each signal back to a CPU pin on IC1 or IC2

Adapted from the content in the Mitsubishi Service Manual.

## Table 1 (Terminals 1–35) C-106

| Terminal | Wiring Diagram Name | Type | Description | Active Condition | CPU Port (Pin)|
| --- | --- | --- | --- | --- | --- |
| 1 | BAT | Power In | Backup power supply input | +12V | Battery voltage | - |
| 2 | CTL | Output (Digital) | ECU control power supply relay drive signal | +12V active  | P2.5 (43) |
| 3 | IG1 | Input (Digital) | IG switch signal | +12V active | P10.0 (23) |
| 4 | CH+B | Input (Digital) | Quick charging power supply signal | +12V when quick charging | P10.2 (25)|
| 5 | - | - | - | - |
| 6 | - | - | - | - |
| 7 | VCSP | Power Out | Brake booster vacuum sensor power supply voltage | Electric motor switch: ON position  4.7 to 5.3 V | ? |
| 8 | APP2 | Power Out | Accelerator pedal position sensor (sub) power supply voltage | Electric motor switch: ON position  4.9 to 5.1 V | ? |
| 9 | IGCT | Power In | ECU control power supply input | Electric motor switch: ON position Battery voltage | ? |
| 10 | IGCT_RET |Power GND | ECU power supply earth | 0V | GND |
| 11 | CHGP | Input (Digital) | On board charging start signal | Electric motor switch: LOCK (OFF) position  Battery voltage | PX.Y (pin) |

| Terminal No. | Type | Check item | Inspection conditions | Normal conditions |
| --- | --- | --- | --- | --- |
| 1 | Power In | Backup power supply input | Always | Battery voltage |
| 2 | Output (Digital) | ECU control power supply relay drive signal | Electric motor switch: ON position | Battery voltage |
| 2 | Output (Digital) | ECU control power supply relay drive signal | Electric motor switch: LOCK (OFF) position | 1 V or less |
| 3 | Input (Digital) | IG switch signal | Electric motor switch: ON position | Battery voltage |
| 4 | Input (Digital) | Quick charging power supply signal | Electric motor switch: ON position | 1 V or less |
| 4 | Input (Digital) | Quick charging power supply signal | When quick charging is in progress | 8 to 16 V |
| 5 | - | - | - | - |
| 6 | - | - | - | - |
| 7 | Power Out | Brake booster vacuum sensor power supply voltage | Electric motor switch: ON position | 4.7 to 5.3 V |
| 8 | Power Out | Accelerator pedal position sensor (sub) power supply voltage | Electric motor switch: ON position | 4.9 to 5.1 V |
| 9 | Power In | ECU control power supply input | Electric motor switch: ON position | Battery voltage |
| 9 | Power In | ECU control power supply input | Electric motor switch: LOCK (OFF) position | 1 V or less |
| 10 | Power GND | ECU power supply earth | Electric motor switch: ON position | 1 V or less |
| 11 | Input (Digital) | On board charging start signal | Electric motor switch: LOCK (OFF) position | Battery voltage |
| 12 | - | - | - | - |
| 13 | - | - | - | - |
| 14 | Input (Digital) | Diagnosis control signal | Electric motor switch: ON position | Battery voltage |
| 15 | Input (Digital) | Timer presetting signal | Electric motor switch: ON position | Battery voltage |
| 16 | Bus (I/O) | K-LINE communication | - | - |
| 17 | Input (Digital) | Traction control switch signal | Set the electric motor switch to the ON position and the TCL OFF switch to OFF. | Battery voltage |
| 17 | Input (Digital) | Traction control switch signal | Set the electric motor switch to the ON position and the TCL OFF switch to ON. | 1 V or less |
| 18 | - | - | - | - |
| 19 | Output (Digital) | Brake electric vacuum pump main relay drive signal | Electric motor switch: ON position | 1 V or less |
| 20 | Output (Digital) | Brake electric vacuum pump control relay 1 drive signal | Set the electric motor switch to the ON position and depress the brake pedal firmly several times. | Battery voltage → 1 V or less |
| 21 | Output (Digital) | Brake electric vacuum pump control relay 2 drive signal | Set the electric motor switch to the ON position and depress the brake pedal firmly several times. | Battery voltage → 1 V or less |
| 22 | Power GND | Brake booster vacuum sensor power supply earth | Electric motor switch: ON position | 1 V or less |
| 23 | Power GND | Accelerator pedal position sensor (sub) power supply earth | Electric motor switch: ON position | 1 V or less |
| 24 | Power In | ECU control power supply input | Electric motor switch: ON position | Battery voltage |
| 24 | Power In | ECU control power supply input | Electric motor switch: LOCK (OFF) position | 1 V or less |
| 25 | Power GND | ECU power supply earth | Electric motor switch: ON position | 1 V or less |
| 26 | Input (Digital) | Brake electric vacuum pump main relay operation check signal | Electric motor switch: ON position | Battery voltage |
| 26 | Input (Digital) | Brake electric vacuum pump main relay operation check signal | Electric motor switch: LOCK (OFF) position | 1 V or less |
| 27 | Input (Digital) | Brake electric vacuum pump operation check signal | Set the electric motor switch to the ON position and depress the brake pedal firmly several times. | 1 V or less → Battery voltage |
| 28 | - | - | - | - |
| 29 | - | - | - | - |
| 30 | - | - | - | - |
| 31 | - | - | - | - |
| 32 | - | - | - | - |
| 33 | - | - | - | - |
| 34 | Input (Analog) | Brake electric vacuum pump vacuum 1 signal | Electric motor switch: ON position | 0.2 to 1.7 V |
| 35 | Input (Analog) | Accelerator pedal position sensor (sub) signal | Set the electric motor switch to the ON position and release the accelerator pedal. | 0.3 to 0.7 V |
| 35 | Input (Analog) | Accelerator pedal position sensor (sub) signal | Set the electric motor switch to the ON position and depress the accelerator pedal fully. | 2.0 to 2.5 V |


## Table 2 (Terminals 41–66) C-108

| Terminal No. | Type | Check item | Inspection conditions | Normal conditions |
| --- | --- | --- | --- | --- |
| 41 | - | - | - | - |
| 42 | Power Out | Accelerator pedal position sensor (main) power supply | Electric motor switch: ON position | 4.9 to 5.1 V |
| 43 | Power Out | Brake pedal stroke sensor power supply | Electric motor switch: ON position | 4.9 to 5.1 V |
| 44 | Input (Analog) | Brake pedal stroke sensor signal | Set the electric motor switch to the ON position and release the brake pedal. | 0.5 to 2.5 V |
| 44 | Input (Analog) | Brake pedal stroke sensor signal | Set the electric motor switch to the ON position and depress the brake pedal fully. | 2.5 to 4.5 V |
| 45 | Output (Digital) | Cooling fan HI relay drive signal | When the electric motor switch is set to the ON position and the radiator fan is not operated (when the coolant temperature is 52°C or less and A/C compressor switch: OFF) | Battery voltage |
| 45 | Output (Digital) | Cooling fan HI relay drive signal | When the electric motor switch is set to the ON position and the radiator fan is operated (when the coolant temperature is 52°C or more, or A/C compressor switch: ON) | 1 V or less |
| 46 | Output (Digital) | Cooling fan LO relay drive signal | When the electric motor switch is set to the ON position and the radiator fan is not operated (when the coolant temperature is 52°C or less and A/C compressor switch: OFF) | Battery voltage |
| 46 | Output (Digital) | Cooling fan LO relay drive signal | When the electric motor switch is set to the ON position and the radiator fan is operated (when the coolant temperature is 52°C or more, or A/C compressor switch: ON) | 1 V or less |
| 47 | Power GND | Brake fluid pressure sensor (power supply earth | Electric motor switch: ON position | 1 V or less |
| 48 | Power GND | Accelerator pedal position sensor (main) power supply earth | Electric motor switch: ON position | 1 V or less |
| 49 | Power GND | Brake pedal stroke sensor power supply earth | Electric motor switch: ON position | 1 V or less |
| 50 | Input (Digital) | Air bag collision signal | - | - |
| 51 | Power GND | Air bag collision signal earth | Electric motor switch: ON position | 1 V or less |
| 52 | Bus (I/O) | K-LINE communication | - | - |
| 53 | Bus (I/O) | K-LINE communication | - | - |
| 54 | - | - | - | - |
| 55 | - | - | - | - |
| 56 | Output (Digital) | Water pump relay drive signal | Electric motor switch: ON position | 1 V or less |
| 57 | Output (Digital) | Backup lamp relay drive signal | Set the electric motor switch to the ON position and the selector lever to the R position. | 1 V or less |
| 58 | - | - | - | - |
| 59 | Input (Analog) | Accelerator pedal position sensor (main) signal | Set the electric motor switch to the ON position and release the brake pedal. | 0.8 to 1.2 V |
| 59 | Input (Analog) | Accelerator pedal position sensor (main) signal | Set the electric motor switch to the ON position and depress the brake pedal fully. | 4.0 to 4.8 V |
| 60 | Input (Digital) | Parking brake lamp signal | Set the electric motor switch to the ON position and release the parking brake lever. | Battery voltage |
| 60 | Input (Digital) | Parking brake lamp signal | Set the electric motor switch to the ON position and apply the parking brake lever. | 1 V or less |
| 61 | Input (Digital) | Parking brake switch signal | - | - |
| 62 | Input (Digital) | Brake switch signal | Release the brake pedal. | 1 V or less |
| 62 | Input (Digital) | Brake switch signal | Depress the brake pedal fully. | Battery voltage |
| 63 | Input (Digital) | ST switch signal | Electric motor switch: START position | Battery voltage |
| 63 | Input (Digital) | ST switch signal | Electric motor switch: LOCK (OFF) position | 1 V or less |
| 64 | - | - | - | - |
| 65 | Output (Digital) | A/C relay drive signal | Electric motor switch: ON position | 1 V or less |
| 66 | Output (Digital) | On board charging relay drive signal | When the electric motor switch is set to the ON position and on board charging cable is disconnected | Battery voltage |
| 66 | Output (Digital) | On board charging relay drive signal | When the electric motor switch is set to the ON position and on board charging cable is connected | 1 V or less |


## Table 3 (Terminals 71–98) C-110

| Terminal No. | Type | Check item | Inspection conditions | Normal conditions |
| --- | --- | --- | --- | --- |
| 71 | - | - | - | - |
| 72 | Input (Analog) | Coolant temperature sensor signal | When the coolant temperature is -20°C with the electric motor switch set to the ON position | 3.9 to 4.5 V |
| 72 | Input (Analog) | Coolant temperature sensor signal | When the coolant temperature is 0°C with the electric motor switch set to the ON position | 3.2 to 3.8 V |
| 72 | Input (Analog) | Coolant temperature sensor signal | When the coolant temperature is 20°C with the electric motor switch set to the ON position | 2.3 to 2.9 V |
| 72 | Input (Analog) | Coolant temperature sensor signal | When the coolant temperature is 40°C with the electric motor switch set to the ON position | 1.4 to 2.0 V |
| 72 | Input (Analog) | Coolant temperature sensor signal | When the coolant temperature is 60°C with the electric motor switch set to the ON position | 0.7 to 1.3 V |
| 72 | Input (Analog) | Coolant temperature sensor signal | When the coolant temperature is 80°C with the electric motor switch set to the ON position | 0.3 to 0.9 V |
| 73 | - | - | - | - |
| 74 | Input (Digital) | Service plug switch signal | When the electric motor switch is set to the ON position, deactivate the interlock. | 1 V or less |
| 74 | Input (Digital) | Service plug switch signal | When the electric motor switch is set to the ON position, activate the interlock. | Battery voltage |
| 75 | - | - | - | - |
| 76 | Bus (I/O) | K-LINE signal | - | - |
| 77 | - | - | - | - |
| 78 | - | - | - | - |
| 79 | Power GND | Coolant temperature sensor signal earth | Electric motor switch: ON position | 1 V or less |
| 80 | - | - | - | - |
| 81 | Input (Digital) | Shift position B2 range signal (sub) | Set the electric motor switch to the ON position and the selector lever to the C position. | Battery voltage |
| 81 | Input (Digital) | Shift position B2 range signal (sub) | Set the electric motor switch to the ON position and the selector lever to a position other than the C position. | 1 V or less |
| 82 | Input (Digital) | Shift position B2-range signal (main) | Set the electric motor switch to the ON position and the selector lever to the C position. | Battery voltage |
| 82 | Input (Digital) | Shift position B2-range signal (main) | Set the electric motor switch to the ON position and the selector lever to a position other than the C position. | 1 V or less |
| 83 | Input (Digital) | Shift position B1-range signal (main) | Set the electric motor switch to the ON position and the selector lever to the B-position. | Battery voltage |
| 83 | Input (Digital) | Shift position B1-range signal (main) | Set the electric motor switch to the ON position and the selector lever to a position other than the B-position. | 1 V or less |
| 84 | Input (Digital) | Shift position D-range signal (main) | Set the electric motor switch to the ON position and the selector lever to the D position. | Battery voltage |
| 84 | Input (Digital) | Shift position D-range signal (main) | Set the electric motor switch to the ON position and the selector lever to a position other than the D position. | 1 V or less |
| 85 | Input (Digital) | Shift position N-range signal (main) | Set the electric motor switch to the ON position and the selector lever to the N position. | Battery voltage |
| 85 | Input (Digital) | Shift position N-range signal (main) | Set the electric motor switch to the ON position and the selector lever to a position other than the N position. | 1 V or less |
| 86 | Input (Digital) | Shift position R-range signal (main) | Set the electric motor switch to the ON position and the selector lever to the R position. | Battery voltage |
| 86 | Input (Digital) | Shift position R-range signal (main) | Set the electric motor switch to the ON position and the selector lever to a position other than the R position. | 1 V or less |
| 87 | Input (Digital) | Shift position P-range signal (main) | Set the electric motor switch to the ON position and the selector lever to the P position. | Battery voltage |
| 87 | Input (Digital) | Shift position P-range signal (main) | Set the electric motor switch to the ON position and the selector lever to a position other than the P position. | 1 V or less |
| 88 | Power Out | Shift position switch (main) power supply | Electric motor switch: ON position | Battery voltage |
| 89 | Power Out | Shift position switch (sub) power supply | Electric motor switch: ON position | Battery voltage |
| 90 | - | - | - | - |
| 91 | - | - | - | - |
| 92 | Input (Digital) | Shift position B1 range signal (sub) | Set the electric motor switch to the ON position and the selector lever to the B-position. | Battery voltage |
| 92 | Input (Digital) | Shift position B1 range signal (sub) | Set the electric motor switch to the ON position and the selector lever to a position other than the B-position. | 1 V or less |
| 93 | Input (Digital) | Shift position D range signal (sub) | Set the electric motor switch to the ON position and the selector lever to the D position. | Battery voltage |
| 93 | Input (Digital) | Shift position D range signal (sub) | Set the electric motor switch to the ON position and the selector lever to a position other than the D position. | 1 V or less |
| 94 | Input (Digital) | Shift position N range signal (sub) | Set the electric motor switch to the ON position and the selector lever to the N position. | Battery voltage |
| 94 | Input (Digital) | Shift position N range signal (sub) | Set the electric motor switch to the ON position and the selector lever to a position other than the N position. | 1 V or less |
| 95 | Input (Digital) | Shift position R range signal (sub) | Set the electric motor switch to the ON position and the selector lever to the R position. | Battery voltage |
| 95 | Input (Digital) | Shift position R range signal (sub) | Set the electric motor switch to the ON position and the selector lever to a position other than the R position. | 1 V or less |
| 96 | Input (Digital) | Shift position P range signal (sub) | Set the electric motor switch to the ON position and the selector lever to the P position. | Battery voltage |
| 96 | Input (Digital) | Shift position P range signal (sub) | Set the electric motor switch to the ON position and the selector lever to a position other than the P position. | 1 V or less |
| 97 | - | - | - | - |
| 98 | - | - | - | - |


## Table 4 (Terminals 101–130) C-111

| Terminal No. | Type | Check item | Inspection conditions | Normal conditions |
| --- | --- | --- | --- | --- |
| 101 | - | - | - | - |
| 102 | - | - | - | - |
| 103 | Input (Analog) | On board battery charging connection signal | When the electric motor switch is set to the ON position and on board charging cable is not connected | 4.2 to 4.8 V |
| 103 | Input (Analog) | On board battery charging connection signal | When the electric motor switch is set to the ON position, the release button is set to ON with the on board charging cable connected | 2.4 to 3.2 V |
| 103 | Input (Analog) | On board battery charging connection signal | When the electric motor switch is set to the ON position, the release button is set to OFF with the on board charging cable connected | 1.2 to 2.0 V |
| 104 | Input (Digital) | Quick charging connection signal | When the electric motor switch is set to the ON position and on board charging cable is not connected | Battery voltage |
| 104 | Input (Digital) | Quick charging connection signal | When the electric motor switch is set to the ON position and on board charging cable is connected | 1 V or less |
| 105 | Output (Digital) | Charging contactor drive signal | Electric motor switch: ON → START position | Battery voltage (intermittently) → 1 V or less |
| 106 | Output (Digital) | Main contactor (-) drive signal | Electric motor switch: ON (before electric motor unit start) | Battery voltage |
| 106 | Output (Digital) | Main contactor (-) drive signal | Electric motor switch: ON → START position (after electric motor unit start) | 1 V or less |
| 107 | Output (Digital) | Main contactor (+) E signal | Electric motor switch: ON (before electric motor unit start) | Battery voltage |
| 107 | Output (Digital) | Main contactor (+) E signal | Electric motor switch: ON → START position (after electric motor unit start) | 1 V or less |
| 108 | Power GND | On board battery charging connection earth | Electric motor switch: ON position | 1 V or less |
| 109 | - | - | - | - |
| 110 | - | - | - | - |
| 111 | Input (Digital) | Quick charging approval signal | When quick charging is in progress | 10 to 16 V |
| 112 | Output (Digital) | Quick charger relay drive signal | Electric motor switch: ON position | Battery voltage |
| 112 | Output (Digital) | Quick charger relay drive signal | When quick charging is in progress | 1 V or less |
| 113 | Output (Digital) | Water pump relay drive signal | Electric motor switch: ON (before electric motor unit start) | 1 V or less |
| 113 | Output (Digital) | Water pump relay drive signal | Electric motor switch: ON → START position (after electric motor unit start, coolant temperature is 45°C or more) | Battery voltage |
| 114 | - | - | - | - |
| 115 | - | - | - | - |
| 116 | Output (Digital) | 12 V DCDC converter shutdown signal | Electric motor switch: ON → START position (after electric motor unit start) | Battery voltage |
| 117 | Output (Digital) | Inverter shutdown signal | Electric motor switch: ON → START position (after electric motor unit start) | Battery voltage |
| 118 | - | - | - | - |
| 119 | - | - | - | - |
| 120 | Power In | Contactor power supply input | Electric motor switch: ON position | Battery voltage |
| 121 | - | - | - | - |
| 122 | - | - | - | - |
| 123 | - | - | - | - |
| 124 | Input (Digital) | Quick charging start signal | Electric motor switch: ON position | Battery voltage |
| 124 | Input (Digital) | Quick charging start signal | When quick charging is in progress | 1 V or less |
| 125 | Input (Digital) | Water pump rotation signal | Electric motor switch: ON → START position (after electric motor unit start, coolant temperature is 45°C or more) | Pulse signal |
| 126 | - | - | - | - |
| 127 | Output (Digital) | 12-V converter drive signal | Electric motor switch: ON (before electric motor unit start) | 1 V or less |
| 127 | Output (Digital) | 12-V converter drive signal | Electric motor switch: ON → START position (after electric motor unit start) | Battery voltage |
| 128 | - | - | - | - |
| 129 | - | - | - | - |
| 130 | - | - | - | - |
