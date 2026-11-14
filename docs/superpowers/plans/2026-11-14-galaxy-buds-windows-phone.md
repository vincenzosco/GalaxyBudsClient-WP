# Galaxy Buds Client for Windows Phone 8.1 Implementation Plan

> **For agentic workers:** Execute this plan inline, one task at a time, with a verification checkpoint after each task. Do not delegate work. The user's request authorizes autonomous implementation and publication after the design approval.

**Goal:** Turn the existing Windows Phone Silverlight starter into an independently branded, unofficial WP8.1 Silverlight Galaxy Buds client with model-aware device controls, English/Italian localization, verified upstream feature parity where technically possible, accurate licensing, and a public GitHub repository.

**Architecture:** Keep the existing WP Silverlight application, isolate Bluetooth RFCOMM transport from the protocol/session layer, and let model capability metadata gate every screen and command. Port features as testable vertical slices; stop at the compatibility gate if the WP8.1 Silverlight APIs cannot actually connect to the target RFCOMM service.

**Tech Stack:** C#, XAML, Silverlight for Windows Phone 8.1, WP8.1 WinRT Bluetooth APIs where supported by the Silverlight app model, `.resx` resources, historical Visual Studio/Windows Phone SDK, GitHub CLI. Avoid modern .NET-only dependencies in the device app.

**Spec:** `docs/superpowers/specs/2026-11-14-galaxy-buds-windows-phone-design.md`

## Global Constraints

- Target Windows Phone 8.1 Silverlight, not WP8.0, desktop UWP, or a disconnected mockup.
- English is the neutral/default language. Italian applies to `it` and `it-*`; all other device cultures use English.
- Keep code comments and user-visible strings in English or localized resource values; code comments are English only.
- Preserve upstream GPL-3.0 terms, source notices, and protocol-note attribution for all derivative work.
- Do not reuse an icon or device illustration unless its license and required credit are established; use an original neutral tile as fallback.
- No fake device readings or controls. Gate every control by model and tested capability.
- Firmware and reset actions need explicit confirmation and precondition checks; never silently retry destructive commands.
- The app is unofficial and must not impersonate Samsung or upstream author Tim Schneeberger.
- Do not claim hardware behavior without a physical Buds test. Do not claim a build without the legacy target build actually passing.
- Keep commit subjects short and human. Never add AI attribution trailers.
- Do not install software system-wide, check in credentials/signing material, or publish before license and README checks pass.

---

## Repository map and file ownership

Existing project files are under `Galaxy Buds App/`; the project is an almost-empty WP8.0 Silverlight starter, with no existing application tests or remote configured.

| Path | Responsibility |
|---|---|
| `Galaxy Buds App.sln` | WP app solution and any compatible test solution entries. |
| `Galaxy Buds App/Galaxy Buds App.csproj` | WP Silverlight target version, resource/content includes, compatible source files. |
| `Galaxy Buds App/Properties/WMAppManifest.xml` | WP8.1 target identity, localization default, capabilities and tile metadata. |
| `Galaxy Buds App/App.xaml` and `App.xaml.cs` | Theme, resource registration, device-language selection and startup error handling. |
| `Galaxy Buds App/LocalizedStrings.cs` | Existing localized resource binding adapter. |
| `Galaxy Buds App/Resources/AppResources.resx`, `.Designer.cs`, `AppResources.it-IT.resx`, generated designer if required | English/Italian resource catalog and WP Silverlight bindings. |
| `Galaxy Buds App/Protocol/` | Protocol frame model, CRC, codecs, command/session coordinator; no UI or Bluetooth dependency. |
| `Galaxy Buds App/Bluetooth/` | WP8.1 Silverlight discovery/RFCOMM/SPP transport adapter and lifecycle. |
| `Galaxy Buds App/Models/` | Device identity, status, feature capability catalog, protocol state. |
| `Galaxy Buds App/Services/` | Device manager, command dispatcher, local preferences/history and diagnostic services. |
| `Galaxy Buds App/ViewModels/` | Page state, supported command actions, busy/error/disconnected states. |
| `Galaxy Buds App/Pages/` | Devices, overview, sound, controls, device tools, diagnostics, settings/about pages. |
| `Galaxy Buds App/Assets/` | Original or rights-cleared WP app icon and tile images. |
| `tests/GalaxyBuds.Protocol.Tests/` | Protocol and capability regression tests that can run independently of Buds hardware. |
| `docs/FEATURE-PARITY.md` | Upstream-to-phone feature inventory with per-model support and evidence. |
| `docs/PROTOCOL-CREDITS.md` | Upstream protocol-source references, license and author attribution. |
| `README.md`, `README.it.md`, `LICENSE` | English and Italian user/developer docs, GPL-3.0 terms. |
| `AGENTS.md`, `.agents/skills/galaxybuds-wp-port/SKILL.md` | Repository contribution and port-quality rules, scoped to this repo. |

Only create a file after the relevant task demonstrates the need. Keep the original solution layout where possible; do not add files/folders for desktop-only scaffolding.

## Tasks

### Task 1: Establish baseline and prove the WP8.1 Silverlight Bluetooth gate

**Files:** No source changes unless a disposable probe is necessary; any probe stays outside the deliverable and is deleted afterwards.

- [ ] Record `git status --short --branch`, current commit, remotes, tracked app files, and the existing project/manifest target.
- [ ] Inspect available Visual Studio, MSBuild, Windows Phone 8.1 SDK/reference assemblies, emulator and test tools. Check known installation paths and `PATH`; do not install them system-wide.
- [ ] Using installed WP8.1 Silverlight reference assemblies or the exact historical API documentation, prove whether the target may use device discovery, `RfcommDeviceService`/stream sockets, SPP service identification, and `bluetooth.rfcomm` manifest capabilities. Verify a Silverlight 8.1 app (not just UWP/Windows Store) is in the documented supported app types.
- [ ] If the SDK exists, create a throwaway compile probe with the smallest discovery/connect/read/write API calls in the actual Silverlight target and build it. Do not copy APIs from current UWP docs without checking target references.
- [ ] If target support is plausible but no toolchain exists, mark the gate **unverified** and search installed historical docs/references. Do not report functional transport compatibility as proven and do not start an elaborate fake UI implementation.
- [ ] If the target/API combination is disproven, stop app implementation and report the exact evidence and required target change; preserve the spec and plan. No alternative platform or WP8.0 mock app is silently substituted.
- [ ] Record exact build command and output, supported device families/API references, required pairing flow, and remaining physical-device check in the task report.

**Exit gate:** Continue only when WP8.1 Silverlight RFCOMM support is backed by target SDK/reference evidence. A compile probe without a physical Buds connection proves API access only, not usable Buds connectivity.

### Task 2: Establish rights, provenance and repository rules

**Files:** `LICENSE`, `AGENTS.md`, `.agents/skills/galaxybuds-wp-port/SKILL.md`, `docs/PROTOCOL-CREDITS.md`, `.gitignore` only if required.

- [ ] Read the complete upstream GPL-3.0 license, root README credit section, protocol notes, and icon-specific attribution/source metadata. Record exact upstream commit SHA and URLs for each reused component.
- [ ] Determine whether `/GalaxyBudsClient/Resources/icon.png` may be copied/distributed under the upstream project license and whether its author/attribution requirements are clear. If unclear, create an original app tile and credit only upstream protocol/source; do not include the uncertain image.
- [ ] Copy the unmodified GPL-3.0 text into root `LICENSE` if this derivative project is distributed under GPLv3. Add source file copyright/license notices when porting GPL source, preserving existing notices verbatim.
- [ ] Create `docs/PROTOCOL-CREDITS.md` naming the upstream author, project, protocol-note URL, upstream commit/date, license, what was adapted, and which parts are new. Clearly state the protocol notes are incomplete reverse-engineering documentation and the phone client is independent/unofficial.
- [ ] Create `AGENTS.md` with only repo-specific enforceable rules: target WP8.1 Silverlight; don't fake unsupported device controls; test protocol; localize all UI with EN default/IT only; English comments; preserve licensing/credits; no Samsung impersonation; verify old toolchain; short human commits without AI trailers.
- [ ] Read the writing-skills skill before writing the scoped skill. Create a concise skill with valid frontmatter and concrete triggers/instructions for making or reviewing WP ports; no global skill changes and no duplicated generic advice.
- [ ] Validate skill markdown/frontmatter and repository instructions; check secret patterns and `.gitignore` before commit.
- [ ] Commit as `docs: add porting and attribution rules`.

### Task 3: Create feature inventory and parity contract

**Files:** `docs/FEATURE-PARITY.md`; as-needed updates to `docs/PROTOCOL-CREDITS.md`.

- [ ] Enumerate current upstream README feature claims and all applicable upstream UI pages, view models, device `Features` flags/specifications, diagnostics and firmware/reset paths from the pinned upstream SHA. Don't derive scope from README alone.
- [ ] Create a table with columns `Feature`, `Upstream evidence`, `Buds models`, `Phone API prerequisite`, `Status`, `Verification`. Initial status for unimplemented items is `planned`; do not mark any hardware feature `supported` before implementation and readback evidence.
- [ ] Cover model selection/device info; battery and history; equalizer/custom EQ; ANC/ambient/voice focus; touch lock/actions; find earbuds/mute; fit tests; spatial/head tracking; rename; firmware; factory/diagnostics/self tests; usage/system reports; settings/import/export; and every desktop-only feature in the design spec.
- [ ] Explicitly list desktop-only tray, CLI, IPC, OS media keys/hotkeys, desktop audio capture/sinks, window/OS integrations, and developer scripting features as excluded or separately adapted, with a concrete reason.
- [ ] Validate each feature claim against upstream source paths and keep a link/commit reference so future parity changes can be reviewed.
- [ ] Commit as `docs: map upstream feature parity`.

### Task 4: Add protocol regression harness and port the minimal codec

**Files:** `tests/GalaxyBuds.Protocol.Tests/*`, `Galaxy Buds App/Protocol/ProtocolFrame.cs`, `Crc16Ccitt.cs`, `ProtocolEncoder.cs`, `ProtocolDecoder.cs`, project/solution includes.

**Interfaces:**
- `ProtocolFrame`: `MessageType`, `MessageId`, `Payload`.
- `Crc16Ccitt.Compute(byte[] data, int offset, int count) -> ushort`.
- `ProtocolEncoder.Encode(ProtocolFrame frame) -> byte[]`.
- `ProtocolDecoder.TryDecode(byte[] buffer, int offset, int count, out ProtocolFrame frame, out int consumed) -> bool`.
- Keep these classes independent of Silverlight UI, device objects, and transport streams.

- [ ] Select the smallest test framework compatible with the installed toolchain. If no .NET compiler/test runner exists, still write deterministic tests in a test project and use independent executable vectors only where useful; explicitly keep target compilation as a remaining check.
- [ ] Write failing tests for valid minimal frame, request/response type, message ID/payload encoding, CRC16-CCITT known vector, bad preamble/postamble, bad CRC, truncated header/payload, invalid length, concatenated frames, fragmented/oversized length behavior and unknown message IDs.
- [ ] Run the tests and preserve observed failing output before codec implementation.
- [ ] Implement strict bounded parser/encoder compatible with the upstream protocol notes and target framework; frame layout is `FE`, type, length, message ID/payload, CRC16-CCITT, `EE` only where confirmed by upstream code. Resolve any difference by reading the pinned upstream encoder rather than relying on the abbreviated notes.
- [ ] Add tests for each discovered upstream framing edge and malformed input; do not decode bytes outside supplied bounds or accept invalid CRCs.
- [ ] Port only enum/message IDs and helpers needed by the first vertical slice, with GPL notices where derived. Do not copy modern runtime/model layers wholesale.
- [ ] Add test project references/solution entries without making the phone app depend on a modern test runtime.
- [ ] Run protocol tests and build the app/core with available tools; run `git diff --check`.
- [ ] Commit as `feat: add tested Buds protocol framing`.

### Task 5: Implement the device capability catalog and typed status/commands

**Files:** `Galaxy Buds App/Models/BudsModel.cs`, `BudsFeature.cs`, `DeviceIdentity.cs`, `DeviceStatus.cs`, `Galaxy Buds App/Protocol/Messages/`, `Galaxy Buds App/Services/CommandDispatcher.cs`, and tests.

- [ ] Add failing tests mapping every upstream model identifier/revision to a stable model enum and supported-feature set; include unknown model and missing firmware revision cases.
- [ ] Port model identifiers and feature differences from upstream device specifications. Never assume a feature on an unknown model.
- [ ] Add typed status values and command encoders for the first supported read/write vertical slice, starting with status response parsing, battery, equalizer and supported ambient/noise state only as the upstream protocol and target allow.
- [ ] Test payload lengths, range validation, model-gated command eligibility, unknown values, response ACK/rejection, and stale/disconnected state.
- [ ] Define a command/session boundary that serializes writes, correlates responses, enforces per-command timeout, cancels pending operations on disconnect, and classifies retry safety. Never retry reset, firmware, find-my-earbuds toggles, or other non-idempotent actions invisibly.
- [ ] Compare all codecs to upstream implementation and annotate derivation/source. Run tests before commit.
- [ ] Commit as `feat: add model-aware Buds commands`.

### Task 6: Implement the WP8.1 Silverlight Bluetooth transport

**Files:** `Galaxy Buds App/Bluetooth/IBluetoothTransport.cs`, `Wp81RfcommTransport.cs`, `BluetoothDeviceInfo.cs`; `Services/DeviceSession.cs`; project and manifest; transport tests where the API permits.

**Interfaces:**
- `IBluetoothTransport`: `event EventHandler<TransportEventArgs> ConnectionChanged`; asynchronous `DiscoverAsync`, `ConnectAsync`, `ReadAsync`, `WriteAsync`, `DisconnectAsync` methods expressed using the legacy-compatible async model established by the project.
- `DeviceSession`: owns one transport, one protocol receiver/command dispatcher, and a monotonic connection generation so old reads cannot overwrite a newer session.

- [ ] Write tests for transport lifecycle state transitions, duplicate connect, cancellation, timeout, stream close, read/write failure, reconnect, and stale-session event rejection using a fake `IBluetoothTransport`.
- [ ] Implement only against the WP8.1 Silverlight APIs proven in Task 1; isolate any WinRT/Silverlight await adapters from the protocol layer.
- [ ] Enumerate paired devices/service records as supported by that target. Require system Bluetooth pairing where the app model does not permit custom pairing; explain this in UI and docs.
- [ ] Use the upstream-confirmed Buds RFCOMM/SPP service ID and the declared WP8.1 Bluetooth device capability. Verify manifest schema against the historical phone SDK.
- [ ] Bound stream reads, handle partial frames/concatenated frames, serialize writes, close sockets on app suspension/disconnect, and surface actionable errors/timeouts.
- [ ] If device API calls are unavailable in Silverlight even though present in UWP, stop and report the gate; do not use reflection, private APIs, or unexplained interop hacks to bypass the app model.
- [ ] Run transport-state tests and the WP8.1 build. Physical phone + Buds test remains required to mark hardware functionality verified.
- [ ] Commit as `feat: connect paired Buds over RFCOMM`.

### Task 7: Implement localization and app shell

**Files:** `Resources/AppResources.resx`, `Resources/AppResources.it-IT.resx`, generated `.Designer.cs` files if required; `App.xaml`, `App.xaml.cs`, `LocalizedStrings.cs`, `MainPage.xaml`, `MainPage.xaml.cs`, new `Pages/*`, app project file.

- [ ] Add localization selector tests: `en-US -> English`, `it-IT -> Italian`, `it-CH -> Italian`, `fr-FR -> English`, and empty/unknown culture -> English.
- [ ] Implement culture selection before `InitializeComponent`/page binding using only APIs supported by WP Silverlight. Respect OS language at fresh app launch; no manual language selector unless user changes requirements.
- [ ] Add all resource keys in English first, then Italian keys for every displayed label/error/dialog/accessibility string. Add a test/script that reports missing or duplicate resource keys.
- [ ] Replace the template page and Italian template comments/strings with a WP Metro pivot/navigation shell using resource binding and native Back behavior.
- [ ] Implement devices list, first-run empty state, permission/pairing instructions, loading, no device found, connection/reconnection, busy, disconnected/stale-data, protocol error states. No live-device data is hard-coded into app code.
- [ ] Test shell/resource fallback at WVGA/WXGA/HD720P layouts and project `SupportedCultures`/manifest consistency with available toolchain.
- [ ] Commit as `feat: add localized Metro app shell`.

### Task 8: Deliver live overview, battery history and supported sound controls

**Files:** `Pages/OverviewPage.xaml(.cs)`, `SoundPage.xaml(.cs)`, related view models, converters; `Services/BatteryHistoryStore.cs`; `Models/DeviceStatus.cs`, protocol codecs; parity docs; tests.

- [ ] Add failing view-model tests for freshness timestamps, left/right/case battery availability, missing case values, disconnected stale values, loading, failed read and model feature flags.
- [ ] Implement overview that reads actual session state, labels left/right/case independently, timestamps data and hides unsupported case status. Add local battery history only where the phone receives reliable history; bound retention and provide erase behavior.
- [ ] Add tested model-supported equalizer presets/custom EQ and ANC/ambient/voice-focus/volume controls only where the upstream exposes the actual command and model feature. Validate slider/preset ranges and ACK/error readback.
- [ ] Route every user action through `CommandDispatcher`; show pending result, server rejection, timeout, disconnect, and actual updated state. Prevent rapid taps from issuing overlapping commands.
- [ ] Mark every screen/feature test result in `docs/FEATURE-PARITY.md` with model and test evidence.
- [ ] Run unit tests and target build. Compare UI controls to actual encoded command and response paths.
- [ ] Commit as `feat: add live status and sound controls`.

### Task 9: Deliver touch controls, find earbuds, and other model-gated device actions

**Files:** `Pages/ControlsPage.xaml(.cs)`, `FindMyBudsPage.xaml(.cs)`, applicable protocol message classes, command service, parity matrix and tests.

- [ ] Add failing tests for per-side touch-action mapping, touch lock, unsupported actions, invalid model action, mute and find start/stop command sequences/ACK behavior.
- [ ] Implement only upstream-documented, target-compatible touch lock, per-ear long-press actions, volume, find-my-earbuds, and mute actions. Check any per-model action lists against pinned upstream specs.
- [ ] Require connected device and supported feature before enabling controls. Require explicit “stop”/leave-screen handling for a find action where appropriate; on failure report that the state may remain active rather than guessing.
- [ ] Add safe command response/reconnect/error tests; update translations and parity evidence.
- [ ] Commit as `feat: add Buds touch and find controls`.

### Task 10: Port diagnostics, tests, reports, spatial features and settings

**Files:** `Pages/DiagnosticsPage.xaml(.cs)`, `DeviceToolsPage.xaml(.cs)`, `SettingsPage.xaml(.cs)`, related models/services/protocol codecs; tests and parity docs.

- [ ] Audit pinned upstream pages and protocol support to classify self-test, fit test, rename, usage report, firmware/system info, logs, spatial/head-tracking, hidden debug, import/export, history and config separately by model and platform.
- [ ] Write unit tests for each low-risk payload decoder and for `not supported`, `not connected`, parse error, protocol error, empty report, import bounds and persistence failure.
- [ ] Implement read-only identity/version/system/usage reports and official documented self-tests/fit test/rename only when their commands and WP APIs are verified.
- [ ] Add spatial/head-tracking only if real Buds models expose it, foreground phone processing is suitable, and no desktop audio sink/capture is required. Do not present a desktop spatial-audio pipeline as a mobile checkbox.
- [ ] Port settings/local persistence with versioned bounded data and corruption recovery; do not store Bluetooth credentials or transmit local data.
- [ ] Exclude developer hook/scripting infrastructure, tray/CLI/IPC, desktop hotkeys/audio capture and OS window integration; document reason in parity table.
- [ ] Keep hidden/debug commands off ordinary user pages unless upstream intended them for users and risk/readback is understood.
- [ ] Update parity status only when tests and relevant device evidence exist. Run all tests/build.
- [ ] Commit as `feat: add supported diagnostics and device tools`.

### Task 11: Gate firmware and destructive device operations

**Files:** `Pages/FirmwarePage.xaml(.cs)`, `Services/FirmwareService.cs`, protocol transfer classes, settings/manifests only as needed, tests, README and parity docs.

- [ ] Before implementing, verify actual WP8.1 Silverlight file selection/storage APIs, HTTPS download/TLS compatibility, firmware provenance, upstream update protocol, supported Buds models/revisions, integrity checks, available disk capacity, and safe interruption/recovery behavior.
- [ ] If any prerequisite cannot be verified, leave firmware updates disabled and document the exact reason; do not download unknown firmware or invent recovery behavior.
- [ ] If eligible, add tests for model/revision rejection, file-size/checksum rejection, missing required version, connection drop, cancellation, no retry/duplicate block, bounded progress, bad ACK, and firmware recovery state.
- [ ] Implement a confirmation flow that names the target Buds/model/current/new versions and warns about power, disconnect, data-loss and supported recovery; confirm once immediately before the non-idempotent transfer.
- [ ] Add a separate explicit reset confirmation and response result. Never place reset beside ordinary toggles, use one-tap action, or silently retry it.
- [ ] Run codec/service tests, build, security/licensing review, and only enable features with model-specific physical device tests and safe recovery evidence.
- [ ] Commit as `feat: gate firmware and reset operations` or `docs: explain unsupported firmware updates` based on observed feasibility.

### Task 12: Create tile assets, READMEs and release-quality contribution docs

**Files:** `Assets/ApplicationIcon.png`, `Assets/Tiles/*`, `README.md`, `README.it.md`, `docs/BUILDING.md` (only if needed), `.gitattributes`, solution/project resources.

- [ ] Use a rights-cleared upstream icon only if Task 2 found clear reusable licensing/credit. Otherwise create a simple original high-contrast earbud-neutral WP tile without Samsung marks or copied upstream elements.
- [ ] Validate each icon/tile dimension, color/alpha restrictions, path casing and build manifest reference for WP8.1. No mock UI screenshots or illustrated fake live values as product evidence.
- [ ] Write the English README from implemented facts only: unofficial status, supported target/known device models, real features/evidence, prerequisites, available verified build/install steps, pairing, language fallback, troubleshooting, security/privacy, parity matrix, upstream/protocol credits, GPL license and known limitations. Omit release/download badges and unsupported install claims.
- [ ] Write an Italian README translation that keeps commands/paths/UI literal names correct and identifies the English version as canonical if appropriate.
- [ ] Add exact local build/test instructions only after trying the historical toolchain. If no SDK is available, list tested prerequisites and state that a build has not been verified; do not imply a modern `dotnet build` works for WP Silverlight.
- [ ] Check every file/image/link/anchor and all user-facing claims against the repo; remove template copy and placeholder sections.
- [ ] Commit as `docs: add bilingual setup and feature guide`.

### Task 13: Full verification, review and public GitHub publication

**Files:** Only fix files found by verification. Remote repository creation/push after all local gates.

- [ ] Run all available protocol, model, localization, view-model and fake-transport tests; run full WP8.1 build with correct historical toolchain; run `git diff --check`; inspect all tracked file names and build outputs.
- [ ] Run a feature-by-feature audit of every visible UI action to its command, readback, model gate, error state, test and (if claimed) hardware evidence. Search for fake percentages/sample values/template text, hardcoded user-facing strings, missing resource keys, TODO/TBD, copied Samsung branding, secrets and disabled controls that look active.
- [ ] Verify GPL-3.0 source notices, exact protocol credit, logo/art provenance, docs links, English-comment policy, README facts, project manifests and app tile assets.
- [ ] Recheck `git status`, commit history and `git log -1 --format='%B'`; reject any trailer/AI marker. Ensure no `.user`, `bin`, `obj`, `.xap`, cert, key or temporary artifact is committed.
- [ ] Confirm GitHub CLI authentication identity and that `vincenzosco/GalaxyBudsClient-WP` does not already exist. If it exists, inspect it before taking any action; never overwrite an existing repo. The user asked for a public repo, so create a new public repository only after local review, using the intended account and a clear unofficial description.
- [ ] Create remote with `gh repo create vincenzosco/GalaxyBudsClient-WP --public --source=. --remote=origin --push` (or use `--description` after checking exact CLI syntax). Set repository description/topics without implying official status. If the name is unavailable or GitHub rejects creation, preserve local work and report the exact blocker; do not select an unrelated owner/name silently.
- [ ] Verify `git remote -v`, pushed branch HEAD equals local HEAD, `gh repo view ...`, and public README/license visibility. Do not claim release or WP device functionality not actually exercised.
- [ ] Final report lists implementation coverage, unsupported upstream features/reasons, commands run and exact build/device evidence, repository URL, and any blocked acceptance criteria.

## Plan self-review

- **Spec coverage:** WP target/RFCOMM gate: Task 1 and 6; license/attribution/identity: Tasks 2, 12, 13; feature parity: Tasks 3, 5, 8-11; protocol reliability: Tasks 4-6; EN/IT fallback: Task 7; data retention/error/destructive safeguards: Tasks 5, 6, 8, 10, 11; project rules/skill: Task 2; bilingual README/assets: Task 12; public repository/short commits: Task 13 and per-task commit gates.
- **Placeholder scan:** No TODO/TBD or generic “handle edge cases” implementation steps; feature/API availability that cannot be known until the WP8.1 SDK probe is explicitly a stop/go observation gate.
- **Type/interface consistency:** `ProtocolFrame`, CRC and codec APIs are defined in Task 4; `IBluetoothTransport` and `DeviceSession` ownership are defined in Task 6; view models route device actions through the typed dispatcher from Task 5.
- **Independent systems:** Protocol, phone transport, model parity and UI/localization are kept in separable task boundaries, but must converge in one app; firmware is explicitly optional and gated, not allowed to stall or fake the safe core client.
