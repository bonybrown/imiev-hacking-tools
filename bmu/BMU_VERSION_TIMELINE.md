# BMU Firmware Timeline (from binary strings and ECU identification)

## Release Timeline

| Timeline date | Software Part No | Primary firmware marker(s) | Notes |
|---|---|---|---|
| May 10 2010 | 9499A84200 | `1.31 (02-Apr-2010)` | This was pulled from a Citroen C-Zero 2011. Software part number suggests an earlier revision to the 9499A84201 |
| Jan 21 2011 | 9499B11500 | `3.02 (20-Oct-2010)` |  |
| May 31 2011 | 9499A84201 | `3.02 (20-Oct-2010)` | The code in this is identical to 9499B11500, only the identifiers have changed. |
| Jul 10 2014 | 9479A04300 | `8.21 (07-Jul-2014)` | From a late-model BMU embedded in battery pack, not under the rear seat; contains onboard earth leakage test hardware. |

## Component Matrix

Using the initial consecutive 4-line component block in each BMU `strings.txt`, this matrix shows the component version string and component date at each component/BMU intersection.

| Component | 9499A84200 | 9499B11500 | 9499A84201 | 9479A04300 |
|---|---|---|---|---|
| `BACKUP RAM` | `2.11`<br>`Apr 5 2010` | `2.11`<br>`Nov 10 2010` | `2.11`<br>`Nov 10 2010` | `2.11`<br>`Jul 10 2014` |
| `BOOT` | `1.31`<br>`Apr 5 2010` | `1.31`<br>`Nov 10 2010` | `1.31`<br>`Nov 10 2010` | `1.31`<br>`Jul 10 2014` |
| `BMU MONITOR` | `0.01`<br>`Apr 5 2010` | `0.01`<br>`Nov 10 2010` | `0.01`<br>`Nov 10 2010` | `1.00`<br>`Jul 10 2014` |
| `CHECKER` | `1.10`<br>`Apr 5 2010` | `1.10`<br>`Nov 10 2010` | `1.10`<br>`Nov 10 2010` | `1.10`<br>`Jul 10 2014` |
| `DTC` | `2.32`<br>`Apr 5 2010` | `2.32`<br>`Nov 10 2010` | `2.32`<br>`Nov 10 2010` | `2.32`<br>`Jul 10 2014` |
| `EEPROM` | `4.02`<br>`Apr 5 2010` | `4.02`<br>`Nov 10 2010` | `4.02`<br>`Nov 10 2010` | `4.02`<br>`Jul 10 2014` |
| `KWP2000` | `2.30`<br>`Apr 5 2010` | `2.30`<br>`Nov 10 2010` | `2.30`<br>`Nov 10 2010` | `2.31`<br>`Jul 10 2014` |
| `PORT` | `2.00`<br>`Apr 5 2010` | `2.00`<br>`Nov 10 2010` | `2.00`<br>`Nov 10 2010` | `2.00`<br>`Jul 10 2014` |
| `RAM MONITOR` | `1.01`<br>`Apr 5 2010` | `1.01`<br>`Nov 10 2010` | `1.01`<br>`Nov 10 2010` | `00.01.03`<br>`2008/12/04(Thu)` |
| `TASK ciNAFWH` | `1.31`<br>`Apr 5 2010` | `1.31`<br>`Nov 10 2010` | `1.31`<br>`Nov 10 2010` | `1.31`<br>`Jul 10 2014` |
| `Watch CPU` | `3.00`<br>`Apr 5 2010` | `3.00`<br>`Nov 10 2010` | `3.00`<br>`Nov 10 2010` | `3.00`<br>`Jul 10 2014` |
| `std_s(GAIO)` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` |
| `cs(GAIO)` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` |
| `math_s(GAIO)` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` | `xassv_LNG`<br>`Aug 2 2007` |
| `DBKOM` |  |  |  | `00.04.04`<br>`Apr 3 2014` |
| `DBKOM  11MY` | `00.01.00`<br>`Nov 10 2009` |  |  |  |
| `DBKOM  12MY` |  | `00.01.00`<br>`Oct 19 2010` | `00.01.00`<br>`Oct 19 2010` |  |
| `BAT-CAN LeakBMU` |  |  |  | `00.05.05`<br>`Jul 9 2014` |
| `BAT-CAN  11MY` | `00.01.00`<br>`Nov 10 2009` | `00.01.00`<br>`Nov 10 2009` | `00.01.00`<br>`Nov 10 2009` |  |

## Notes and caveats
- `bmu_identification.txt` top-line timestamps are acquisition dates in 2026, not firmware build dates.
- Timeline dates in this file use the stricter standalone `MMM DD YYYY` line that is immediately followed by a standalone `HH:MM:SS` line.
- The Release Timeline `Notes` column is sourced directly from each BMU folder's `notes.txt`.
- Some embedded dates in component records are historical component build dates rather than overall BMU release dates.

## Reproduction Method

Use this process to regenerate this markdown file from the BMU folders.

1. Enumerate each BMU subfolder and identify exactly one firmware `.bin` file plus `bmu_identification.txt`.
2. Run `strings -n 8` against the chosen firmware binary in each BMU folder and save the result as `strings.txt` in that same folder.
3. Read `strings.txt` and locate the single standalone date line in `MMM DD YYYY` format that is immediately followed by a standalone `HH:MM:SS` line.
4. Use that date line as the canonical timeline date for the folder.
5. Read `bmu_identification.txt` and extract the software part number from `ECUCodeIdentification`.
6. Read each BMU folder's `notes.txt` and map the content to that folder's software part number.
7. If `notes.txt` is empty, leave the Release Timeline `Notes` cell empty.
8. For the Release Timeline table, extract the main firmware marker from `strings.txt` near the canonical date block.
9. Prefer the family/build marker that best identifies the release branch, for example `1.31 (02-Apr-2010)`, `3.02 (20-Oct-2010)`, or `8.21 (07-Jul-2014)`.
10. Populate the Release Timeline table with four columns only: timeline date, software part number, primary firmware marker, and notes.
11. For the Component Matrix, parse the initial consecutive 4-line component records from each `strings.txt`.
12. Treat each component record as: component name, platform/product line, version string, and component date.
13. Stop the component-record pass when the regular 4-line pattern breaks and the file transitions into build/package metadata.
14. Pivot those parsed records into a matrix with components as rows and BMU software part numbers as columns.
15. Order the BMU software part number columns by ascending Release Timeline date; if two part numbers share the same timeline date, order those ties by software part number.
16. In each populated matrix cell, include both the version string and the component date, separated by a line break rather than a pipe character so the markdown table remains valid.
17. Keep the notes section aligned with the final chosen canonical dates and current file state.
