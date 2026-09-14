# ECU Firmware Timeline (from binary strings and ECU identification)

## Release Timeline

| Timeline date | Software Part No | Primary firmware marker(s) | Notes |
|---|---|---|---|
| Dec 13 2010 | 9499B131 | `MAB_MMC_3F45E_EV_9.05a` |  |
| May 12 2014 | 9499A43902 | `MAB_MMC_3F45E_EV_8.16a` | From a Citroen C-Zero 2011. Also verfied from a Mitsubishi |
| May 27 2014 | 9499A18204 | `6.09a (20-May-2014)` |  |
| May 27 2014 | 9499B13102 | `MAB_MMC_3F45E_EV_9.50a` (with `9.05.0018:20:01` also present) | An update to 9499B131 |
| Oct 14 2015 | 9499A18206 | `6.09a (20-May-2014)` (same branch marker as A18204) | An update to 9499A108204 |


## Component Matrix

Using the initial consecutive 4-line component block in each ECU `strings.txt`, this matrix shows the component version string and component date at each component/ECU intersection.

| Component | 9499B131 | 9499A43902 | 9499A18204 | 9499B13102 | 9499A18206 |
|---|---|---|---|---|---|
| `std_s(GAIO)` | `V02.08`<br>`Nov 10 2009` | `V02.08`<br>`Nov 10 2009` | `xassv_LNG`<br>`Aug 2 2007` | `V02.08`<br>`Nov 10 2009` | `xassv_LNG`<br>`Aug 2 2007` |
| `math_s(GAIO)` | `V02.08`<br>`Nov 10 2009` | `V02.08`<br>`Nov 10 2009` | `xassv_LNG`<br>`Aug 2 2007` | `V02.08`<br>`Nov 10 2009` | `xassv_LNG`<br>`Aug 2 2007` |
| `cs(GAIO)` | `V01.01`<br>`Nov 10 2009` | `V01.01`<br>`Nov 10 2009` | `xassv_LNG`<br>`Aug 2 2007` | `V01.01`<br>`Nov 10 2009` | `xassv_LNG`<br>`Aug 2 2007` |
| `ert(MATLAB)` | `2007a/7.4.0.287`<br>`Apr 15 2010` | `2007a/7.4.0.287`<br>`Apr 15 2010` |  | `2007a/7.4.0.287`<br>`Apr 15 2010` |  |
| `Can Reproggram` | `00.06.00`<br>`2009/02/10(Tue)` | `00.06.00`<br>`2009/02/10(Tue)` | `00.06.00.00`<br>`2009/02/10(Tue)` | `00.06.00`<br>`2009/02/10(Tue)` | `00.06.00.00`<br>`2009/02/10(Tue)` |
| `BOOT` | `00.07.07`<br>`2009/08/03(Mon)` | `00.07.07`<br>`2009/08/03(Mon)` | `1.31`<br>`May 22 2014` | `00.07.07`<br>`2009/08/03(Mon)` | `1.31`<br>`May 22 2014` |
| `BACKUP RAM` | `00.07.07`<br>`2009/08/03(Mon)` | `00.07.07`<br>`2009/08/03(Mon)` | `00.05.00`<br>`2009/01/08(Thu)` | `00.07.07`<br>`2009/08/03(Mon)` | `00.05.00`<br>`2009/01/08(Thu)` |
| `EEPROM` | `00.08.11`<br>`2010/10/12(Tue)` | `00.08.11`<br>`2010/10/12(Tue)` | `5.03`<br>`2009/01/31(Sat)` | `00.08.11`<br>`2010/10/12(Tue)` | `5.03`<br>`2009/01/31(Sat)` |
| `TASK ciNAFWH` | `00.01.30`<br>`2009/05/20(Wed)` | `00.01.30`<br>`2009/05/20(Wed)` | `1.30`<br>`May 22 2014` | `00.01.30`<br>`2009/05/20(Wed)` | `1.30`<br>`May 22 2014` |
| `PORT` | `00.05.01`<br>`2009/01/29(Thu)` | `00.05.01`<br>`2009/01/29(Thu)` | `00.05.01`<br>`2009/01/29(Thu)` | `00.05.01`<br>`2009/01/29(Thu)` | `00.05.01`<br>`2009/01/29(Thu)` |
| `RAM MONITOR` | `00.01.03`<br>`2008/12/04(Thu)` | `00.01.03`<br>`2008/12/04(Thu)` | `1.03`<br>`2008/12/04(Thu)` | `00.01.03`<br>`2008/12/04(Thu)` | `1.03`<br>`2008/12/04(Thu)` |
| `Watch CPU` | `00.07.06`<br>`2009/07/07(Tue)` | `00.07.06`<br>`2009/07/07(Tue)` | `5.04`<br>`2009/02/02(Mon)` | `00.07.06`<br>`2009/07/07(Tue)` | `5.04`<br>`2009/02/02(Mon)` |
| `CHECKER` | `00.07.07`<br>`2009/07/23(Wed)` | `00.07.07`<br>`2009/07/23(Wed)` | `00.03.00`<br>`2008/12/16(Thu)` | `00.07.07`<br>`2009/07/23(Wed)` | `00.03.00`<br>`2008/12/16(Thu)` |
| `DTC` | `00.09.00`<br>`Jul 5 2010` | `00.08.10`<br>`Apr 15 2010` | `00.05.01`<br>`2009/01/08(Thu)` | `00.09.00`<br>`Jul 5 2010` | `00.05.01`<br>`2009/01/08(Thu)` |
| `KWP2000` | `00.09.04`<br>`Oct 08 2010` | `00.08.10`<br>`Apr 15 2010` | `00.06.02.00`<br>`2009/06/25(Thr)` | `00.09.04`<br>`Oct 08 2010` | `00.06.02.00`<br>`2009/06/25(Thr)` |
| `ARBS-COMM` | `00.01.01`<br>`2008/11/18(Tue)` | `00.01.01`<br>`2008/11/18(Tue)` | `1.01`<br>`2008/11/18(Tue)` | `00.01.01`<br>`2008/11/18(Tue)` | `1.01`<br>`2008/11/18(Tue)` |
| `IMMOBI` | `00.08.08`<br>`2010/02/26(Fri)` | `00.08.08`<br>`2010/02/26(Fri)` |  | `00.08.08`<br>`2010/02/26(Fri)` |  |
| `SPOOFING` |  |  | `3.00`<br>`2008/10/27(MON)` |  | `3.00`<br>`2008/10/27(MON)` |
| `DBKOM PJ` |  |  | `00.01.08`<br>`May 22 2014` |  | `00.01.08`<br>`May 22 2014` |
| `DBKOM 12MY` | `00.02.05`<br>`Oct 8 2010` | `00.02.05`<br>`Jun 13 2011` |  | `00.02.05`<br>`Oct 8 2010` |  |
| `CHG-CAN 12MY` | `00.02.01`<br>`Oct 8 2010` |  | `00.02.01`<br>`May 22 2014` | `00.02.01`<br>`Oct 8 2010` | `00.02.01`<br>`May 22 2014` |
| `CHG-CAN EU` |  | `00.01.05`<br>`Apr 20 2010` |  |  |  |

## Notes and caveats
- `ecu_identification.txt` top-line timestamps are acquisition dates in 2026, not firmware build dates.
- Some embedded dates are component/library timestamps; version labels plus full build strings were weighted most heavily.
- Timeline dates in this file now use the stricter standalone `MMM DD YYYY` line that is immediately followed by a standalone time line.
- The Release Timeline `Notes` column is sourced from each ECU folder's `notes.txt`; if missing or empty, the table cell is left blank.

## Reproduction Method

Use this process to regenerate this markdown file from the ECU folders.

1. Enumerate each ECU subfolder and identify exactly one firmware `.bin` file plus `ecu_identification.txt`.
2. Run `strings -n 8` against the chosen firmware binary in each ECU folder and save the result as `strings.txt` in that same folder.
3. Read `strings.txt` and locate the single standalone date line in `MMM DD YYYY` format that is immediately followed by a standalone `HH:MM:SS` line.
4. Use that date line as the canonical timeline date for the folder.
5. Read `ecu_identification.txt` and extract the software part number from `ECUCodeIdentification`.
6. Read each ECU folder's `notes.txt` and map the content to that folder's software part number.
7. If `notes.txt` is missing or empty, leave the Release Timeline `Notes` cell empty.
8. For the Release Timeline table, extract the main firmware marker from `strings.txt` near the canonical date block.
9. Prefer the family/build marker that best identifies the release branch, for example `MAB_MMC_3F45E_EV_9.05a`, `MAB_MMC_3F45E_EV_8.16a`, `MAB_MMC_3F45E_EV_9.50a`, or `6.09a (20-May-2014)`.
10. Populate the Release Timeline table with four columns only: timeline date, software part number, primary firmware marker, and notes.
11. For the Component Matrix, parse the initial consecutive 4-line component records from each `strings.txt`.
12. Treat each component record as: component name, platform/product line, version string, and component date.
13. Stop the component-record pass when the regular 4-line pattern breaks and the file transitions into build/package metadata.
14. Pivot those parsed records into a matrix with components as rows and ECU software part numbers as columns.
15. Order the ECU software part number columns by ascending Release Timeline date; if two part numbers share the same timeline date, order those ties by software part number.
16. In each populated matrix cell, include both the version string and the component date, separated by a line break rather than a pipe character so the markdown table remains valid.
17. Keep the per-folder evidence and notes sections aligned with the final chosen canonical dates and current file state.
