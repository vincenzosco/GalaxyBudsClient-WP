# Galaxy Buds Client for Windows Phone 8.1: design

## Status

Approved scope: Windows Phone 8.1 Silverlight, with implementation proceeding autonomously after the user approved the target and requested autonomous execution. This is an independent, unofficial client and must never imply that it was written, endorsed, or maintained by Samsung or the upstream author.

## Goal

Turn the existing Windows Phone 8 Silverlight starter into a practical Windows Phone 8.1 Silverlight Galaxy Buds manager, porting the upstream GalaxyBudsClient features that can be made meaningful and technically supported on the phone, while documenting unsupported desktop-only or platform-incompatible features instead of presenting mock controls as working functionality.

The upstream project is https://github.com/timschneeb/GalaxyBudsClient. Its current README identifies the project as an unofficial Galaxy Buds manager, and its license is GPL-3.0. The upstream protocol notes describe proprietary Samsung RFCOMM messages and an incomplete, evolving reverse-engineering effort. This project must preserve GPL-3.0 obligations for derivative code and retain accurate upstream and protocol attribution. Reused logos or device artwork must be checked for their individual rights and attribution before redistribution; prefer a separately designed app tile if that is unclear.

## Product and visual direction

- Keep Silverlight and Windows Phone 8.1, using the period-appropriate Metro design language: large lightweight page headings, pivot/panorama-style navigation, flat surfaces, high-contrast text, restrained accent color, and native system controls.
- Build the device list and a connected-device overview first. The overview must visibly distinguish left bud, right bud, and case values, and show connection freshness/status rather than fabricated live data.
- Group supported controls into readable phone-sized pages: sound/noise controls, equalizer, touch actions, device tools, and settings/diagnostics. Only expose controls that the connected model and validated transport support. Unsupported capabilities should be clearly identified, not silently simulated.
- Reuse the upstream app logo only after confirming the asset's reuse terms. Do not copy Samsung branding or imply official status. Create WP tile assets that follow platform constraints and have correct source credits.
- The browser mockup is a visual direction only: its device names, battery readings, and control states are illustrative and are not product behavior or evidence of compatibility.

## Localization

- English is the neutral/default resource language.
- Italian is the only additional translation in this delivery.
- Choose Italian when the device UI culture is Italian (`it` and its regional variants); use English for every other device language.
- All user-visible strings, accessibility labels, validation messages, connection errors, dialogs, and settings must use resources. Keep code identifiers and comments in English.
- Verify fallback explicitly for a non-Italian culture and verify that changing the device culture takes effect on a fresh app launch.

## Architecture and boundaries

1. **Silverlight presentation**: WP8.1 XAML pages, view models or small code-behind adapters following the existing project's capabilities, and resource-backed strings. Pages must remain usable at supported phone resolutions and with touch/Back navigation.
2. **Feature catalog and device capabilities**: a model-aware map of features, upstream-supported models, protocol prerequisites, and status (`implemented`, `unsupported on WP`, or `not yet verified`). The UI consumes the catalog rather than guessing from the device name.
3. **Protocol core**: narrowly scoped framing, CRC16-CCITT, message encoding/decoding, response matching, and bounded parsing. Port only code needed for phone-supported features, preserve GPL-3.0 headers/attribution for derivative files, and add fixture-based tests against upstream protocol samples or independently verified vectors. Do not port modern .NET dependencies wholesale into the legacy project.
4. **Bluetooth transport**: an isolated Windows Phone 8.1 Silverlight RFCOMM/SPP adapter. First prove that the SDK reference assemblies and a WP8.1 Silverlight target support the needed discovery, connection, and stream APIs; verify pairing prerequisites, service identifier, connection lifecycle, disconnects, timeouts, cancellation, and device compatibility. Keep UI and protocol code independent of transport APIs.
5. **Safety and operations**: all command writes pass through a single session/command boundary with response and timeout handling. Show connection state and report errors. Require explicit, descriptive confirmation for destructive device reset and firmware operations; do not silently retry non-idempotent commands. Treat firmware transfer as a separately gated capability, with checks for model/revision, file integrity, power/connection, progress, interruption, and recovery before enabling it.
6. **Persistence**: use only local WP storage for user preferences and any explicitly opted-in battery history; bound retention, provide clear/reset behavior, and do not transmit personal/device data to a service without a separate explicit requirement.

## Feature scope and parity accounting

Maintain a checked-in feature matrix based on the upstream feature catalog and UI pages, with per-feature model support, WP8.1 API status, implementation/test evidence, and honest exclusions. Target the full set of device-management features that can operate in the foreground on the phone, including:

- Device discovery/selection, connection state, device/model/firmware identification, and live status.
- Left/right/case battery display where available; battery-history reporting only where data can be collected and stored reliably.
- Supported stock and custom equalizer controls.
- Supported ANC, ambient sound, voice-focus/ambient level, and noise-control controls.
- Touchpad lock, per-side long-press actions, and supported volume/earbud mute controls.
- Find-my-earbuds start/stop, wearing/status indicators, supported fit test, supported spatial/head-tracking data, rename, and model-dependent settings.
- Firmware information and update workflow only after protocol/API feasibility, provenance, compatibility, and interruption-safety gates pass.
- Applicable device diagnostics, self-test, usage reports, system information, and import/export functions where phone APIs and upstream licensing permit.

The parity matrix must also account for upstream-only desktop or development functions: tray integration, desktop hotkeys/media-key hooks, desktop spatial-audio sinks/capture and routing, desktop window behavior, CLI, IPC, desktop log/trace integrations, and developer scripting/hook infrastructure. Do not claim these are implemented as phone features. Where a meaningful mobile equivalent is feasible, document it as a separate implementation; otherwise mark it out of scope for the WP client, with a reason. Do not mark upstream experimental/hidden commands as ordinary supported features.

If a core blocker makes the RFCOMM transport impossible on this WP8.1 Silverlight target, stop feature implementation at that technical gate and report the exact SDK/API evidence. A disconnected UI demo is not an acceptable substitute for a functional client.

## Reliability and errors

- Validate frame delimiters, lengths, CRC, fragmentation boundaries, message IDs, and model-dependent payload lengths before acting on data.
- Handle malformed/unknown frames without crashing; surface a recoverable diagnostic and keep the session state consistent.
- Prevent overlapping writes that violate protocol ordering. Bound reads, timeouts, and accumulated diagnostic/history data.
- On disconnect, stop pending work, release transport resources, and invalidate stale status so the UI cannot show old readings as current.
- Put destructive and firmware actions behind confirmation and precondition checks. No hidden firmware download, reset, or permanent settings write.

## Validation

- Establish the current baseline and identify the available Windows Phone 8.1 SDK, Visual Studio/MSBuild, emulator, and device before implementation claims.
- Build the Silverlight solution with the historical WP8.1 toolchain when available. The present shell has no .NET SDK and no `msbuild` command, so this limitation must be rechecked and reported accurately; static checks do not equal a target build.
- Add unit tests that can run independently for framing, CRC, encoders/decoders, malformed/truncated data, and feature catalog/localization fallback. Use upstream-derived fixtures only under compatible GPL terms and preserve their provenance.
- Test on a WP8.1 emulator/device and real supported Buds models for discovery, connection, state refresh, control/readback, disconnect/reconnect, and error paths before describing hardware functionality as verified. Emulator-only evidence must not be described as physical-Buds testing.
- Audit every displayed control against a real command and observed response. No fabricated battery/status data in release builds.
- Validate resource keys and both cultures, inspect the diff and license headers, and check repository links/assets/README claims.

## Repository, documentation, and delivery

- Create a new public GitHub repository under the authenticated user's account only after implementation and license/attribution review. Use a clear name such as `GalaxyBudsClient-WP`; do not fork or overwrite the upstream repository and do not imply upstream ownership.
- Include GPL-3.0 license text and notices appropriate for derivative source; clearly name the upstream repository and protocol-note authorship in credits. Keep original upstream code attribution intact where code is ported. Confirm the logo and third-party artwork license individually before including them.
- Write the main README in English, with a separate Italian README. State supported models/features, build prerequisites, install/test method, known limitations, compatibility evidence, and that the app is unofficial. Do not claim a release/store distribution unless one exists.
- Add concise English-only repository instructions in `AGENTS.md` covering language, attribution/licensing, platform compatibility, truthfulness, tests, and commit style. Add one narrowly scoped reusable skill in the repository for preserving those porting and quality rules. Never edit the user's global skill installation.
- Keep source comments in English. Use short, human commit subjects without AI trailers. Do not commit secrets, signing material, generated build artifacts, or unverified screenshots.

## Acceptance criteria

1. The existing app's project target and manifests consistently identify Windows Phone 8.1 Silverlight; the project builds with its compatible toolchain, or the report names the precise missing toolchain blocker and does not claim a build.
2. The WP Silverlight RFCOMM/SPP support gate is evidenced using target SDK references and, if available, a physical device. No GUI-only fallback is passed off as device control.
3. English/Italian resource selection follows device language rules, with English fallback for every non-Italian culture.
4. Every supported on-device function in the parity matrix has a real transport/protocol path, model gating, error behavior, and automated or documented device test evidence; unsupported desktop-only features are plainly marked.
5. No mock values are presented as live; reset and any enabled firmware operation require safety confirmations and validated preconditions.
6. Licensing, logo/artwork provenance, unofficial status, credits, README claims, and all code comments comply with this design.
7. The public GitHub repository is created under the authenticated account, source and docs are pushed, and commits have concise human messages without AI trailers.
