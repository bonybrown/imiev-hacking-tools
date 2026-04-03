## Plan: Build J2534 ECU ID Console App

Create a new Windows .NET 10 console app that uses `J2534-Sharp.Core` to discover installed J2534 APIs, prompt for API selection when multiple are available, open the chosen interface, connect on CAN (11-bit), send `10 92` to enter session, then send `1A 90` to request ECU identification using TX ID `0x751` and RX ID `0x752`.

**Steps**
1. Phase 1 - Scaffold project (*blocks all later steps*): create a new console project at `c:\Users\bonyb\source\get-miev-dump`, target `net10.0`, add `J2534-Sharp.Core`, and set Windows-specific target metadata in the project file.
2. Phase 2 - Build API discovery and selection flow (*depends on 1*): in `Program.cs`, call `J2534APIFactory.DiscoverAPIs()`, print index + name + DLL path, handle zero APIs with an actionable error, and prompt the user for a valid index if more than one API is present.
3. Phase 3 - Load selected API and open transport (*depends on 2*): use `J2534APIFactory.LoadAPI(selected.FileName)` and result-pattern checks; open device (`api.OpenDevice()`), then CAN channel with 500k (`device.OpenChannel(Protocol.CAN, Baud.CAN_500000, ConnectFlag.NONE)`) and strict failure handling.
4. Phase 4 - Configure CAN IDs and receive filtering (*depends on 3*): apply channel config/filtering so outgoing requests use CAN ID `0x751` and incoming frames are read from `0x752` (or use pass-all first, then enforce `0x752` filter if API-specific config constraints exist). Log actual RX IDs during bring-up.
5. Phase 5 - Implement diagnostic exchange (*depends on 4*): send session request bytes `10 92`, read and validate a positive response (`50 92`), then send `1A 90`, collect responses with timeout/retry handling, and decode positive response (`5A 90 ...`) and negative response (`7F 1A NRC`) paths.
6. Phase 6 - UX and robustness (*parallel with 5 after basic send/receive works*): add consistent console logging (TX/RX hex + CAN IDs), graceful timeout messages, deterministic `using` disposal for API/device/channel, and clear non-zero exit codes for failures.
7. Phase 7 - Verification (*depends on 5 and 6*): run static build checks, dry-run API enumeration without vehicle, then hardware validation sequence (enumeration -> open -> session -> ID read) and capture one successful transcript.

**Relevant files**
- `c:\Users\bonyb\source\get-miev-dump\get-miev-dump.csproj` - target framework, package reference, Windows runtime settings.
- `c:\Users\bonyb\source\get-miev-dump\Program.cs` - full app flow: discovery, selection, channel setup, diagnostic requests, response parsing.
- `c:\Users\bonyb\source\get-miev-dump\README.md` - operator instructions (driver setup, required adapter install, expected TX/RX IDs, sample output).

**Verification**
1. Run `dotnet restore` and `dotnet build` in `c:\Users\bonyb\source\get-miev-dump` and confirm zero compile errors.
2. Run app with no adapter installed and verify graceful "no J2534 APIs found" handling.
3. Run app with adapter installed and verify API list appears and selection prompt accepts only valid indices.
4. Confirm open/connect path succeeds on CAN 500k and logs configured TX/RX IDs (`0x751`/`0x752`).
5. Confirm `10 92` yields expected positive response (`50 92`) before proceeding.
6. Confirm `1A 90` yields positive response (`5A 90 ...`) or decoded negative response (`7F 1A xx`) with NRC displayed.
7. Validate timeout behavior by disconnecting ECU/ignition state and confirming user-friendly timeout output.

**Decisions**
- Included scope: single-ECU query flow over CAN using fixed IDs (`TX 0x751`, `RX 0x752`) and one ID request (`1A 90`).
- Included scope: interactive API selection only when more than one API is discovered.
- Excluded scope: multi-ECU scans, ISO-TP reassembly customization beyond what the library provides, GUI, logging to external files, and long-running keepalive scheduling.
- Assumption: `J2534-Sharp.Core` supports required CAN ID/filter configuration directly; if adapter/API limits this, fallback is pass-all receive plus software filtering on `0x752`.

**Further Considerations**
1. If `1A 90` returns multi-frame payloads, add explicit frame aggregation display (raw + decoded ASCII) as a follow-up enhancement.
2. If ECU requires periodic tester-present, add optional `3E 00` heartbeat toggle after successful session entry.
3. If your adapter exposes multiple devices per API, add a second prompt for device selection before channel open.
