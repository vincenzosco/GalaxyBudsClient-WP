# Upstream feature parity

This inventory is pinned to upstream commit [`754b6fdfb1c151b022cece775c8aaad4eb185afa`](https://github.com/timschneeb/GalaxyBudsClient/commit/754b6fdfb1c151b022cece775c8aaad4eb185afa). Names refer to upstream `GalaxyBudsClient/Interface/Pages`, `Model/Specifications/Features.cs`, per-model `*DeviceSpec.cs`, and `Message` encoders/decoders. No device feature is verified in this WP client yet. Statuses may change only with implementation, tests, and the listed device evidence.

**Status definitions:** `Planned` = in-scope for investigation/implementation, not implemented. `Blocked on WP` = requires a Windows API or background/desktop facility not yet established for Silverlight. `Out of scope` = host-app integration has no meaningful WP phone equivalent. `Unknown` = upstream behavior/model support needs source-level review. None of these statuses means the feature works here.

| Upstream feature or group | Upstream evidence | Model scope | WP client status | Verification required |
|---|---|---|---|---|
| Device discovery and device selection | `Interface/Pages/DevicesPage`, `Model/Specifications/DeviceSpecHelper.cs`, `Platform/DeviceManager.cs` | All identifiable models | Planned | Paired-device/service discovery on WP8.1 Silverlight; device/model identification on hardware |
| Connect, disconnect, reconnect and connection information | `Platform/BluetoothImpl.cs`, `Platform/DeviceManager.cs`; RFCOMM transport | Paired Buds with SPP | Planned | WP Silverlight socket lifecycle and real connection to each claimed model family |
| Device model, firmware/build information and color | `SystemInfoPage`, `SystemPage`, `DEBUG_BUILD_INFO`, `VERSION_INFO`, model specifications | Model-dependent | Planned | Typed response fixtures and device readback |
| Live left/right battery, placement/wearing and connection role | `StatusUpdateDecoder`, `ExtendedStatusUpdateDecoder` | All; payload differs by model/revision | Planned | Valid/truncated payload tests and live status readback |
| Case battery and charging status | status decoders, `Features.CaseBattery`, `Features.ChargingState` | Buds+ and newer, model/revision dependent | Planned | Model/revision payload tests and live case readback |
| Battery cycle, voltage, current and battery type | `BatteryHistoryPage`, `BatteryTypeDecoder`, `SocBatteryCycleDecoder`, feature flags | Model-dependent, partly diagnostic | Planned | Source/payload validation; show only actual supported values |
| Battery history and metering | `BatteryHistoryPage`, `BatteryHistoryManager`, `METERING_REPORT` | Where data is exposed | Planned | Local retention, readback, persistence and erase tests |
| Stock equalizer presets | `EqualizerPage`, `SetEqualizerEncoder`, `Features` | All supported models; payload differs | Planned | Encoder vectors and per-model command/readback |
| Custom equalizer | `EqualizerPage`, `CUSTOM_EQUALIZE_SEND/RECV`, `Features.CustomEqualizer` | Newer model-dependent devices | Planned | Model gating, bounded payload tests and readback |
| Noise control, ANC and ambient sound | `NoiseControlPage`, `NoiseControlUpdateDecoder`, command encoders | Varies; original Buds and Buds Live use different controls | Planned | Per-model command/state mapping and device readback |
| Adaptive noise control and noise reduction levels | `Features.NoiseControlAdaptive`, `AncNoiseReductionLevels`, decoder/encoder classes | Newer models/revisions | Planned | Model/revision tests and real control response |
| Ambient sound volume, voice focus, amplification and tone customization | `AmbientCustomizePage`, `AmbientSoundVolume`, `AmbientVoiceFocus`, `AmbientCustomizeEncoder` | Model/revision-specific | Planned | Value bounds, payload fixtures and real device readback |
| Ambient sidetone and call audio controls | `Features.AmbientSidetone`, `CallPathControl`, device settings | Model/revision-specific | Planned | Per-model gating and command/readback |
| Touchpad lock and advanced per-action lock | `TouchpadPage`, `LockTouchpadEncoder`, feature flags | Varies from simple lock to advanced action masks | Planned | Bit/byte encoding tests and device readback |
| Left/right touch and long-press actions | `TouchOptions`, `SetTouchOptionsEncoder`, touch maps | Buds/model-specific action mapping | Planned | Each action mapped per model, command ACK/readback |
| Swipe, outside double tap and call touch actions | `AdvancedPage`, decoder and feature flags | Model/revision-specific | Planned | Per-model command support and real test |
| Find-my-earbuds start/stop and mute | `FindMyBudsPage`, `FIND_MY_EARBUDS_*`, `MUTE_EARBUD` | Model/connection-mode-specific | Planned | Start/stop lifecycle, error recovery, device test |
| SmartThings Find and FMM configuration | `FmmConfigPage`, FMM protocol and web/service integrations | Feature- and service-dependent | Unknown | Trace upstream server/API dependencies and determine phone API/privacy limits |
| Fit test | `FitTestPage`, `FitTestDecoder`, `CHECK_THE_FIT_OF_EARBUDS` | Supported models only | Planned | Payload/results tests and a physical supported-device test |
| Rename Buds | `RenamePage`, `Features.Rename`, Bluetooth identity/service | Newer model-dependent devices | Planned | WP8.1 rename API behavior and actual reconnect/identity result |
| Firmware information and updates/downgrades | `FirmwarePage`, `FirmwareTransferManager`, `Model/Firmware`, `FOTA_*` | Explicit model/revision support varies | Blocked on WP | Safe file/download/storage flow, signatures/checksums, interruption/recovery and per-model hardware test; keep disabled until proven |
| Device reset, reboot, poweroff and pairing mode | `RESET`, `REBOOT`, `POWEROFF`, `PAIRING_MODE` | Varies | Planned, safety-gated | Exact command and response source, model preconditions, warning and device test |
| Self-test and factory/device diagnostics | `SelfTestDialog`, diagnostic decoders, `ACT_TEST_CMD` | Model-dependent | Planned | Read-only/test command audit, bounds, model gating, device test |
| Usage and health reports | `UsageReportPage`, `USAGE_REPORT`, `USAGE_REPORT_V2`, metering decoders | Model-dependent | Planned | Parser fixtures, privacy review, no upload by default |
| Water-detection, overheat, logs, trace and coredump | diagnostic pages, `READ_WATER_DETECTION_HISTORY`, `LOG_*`, `OVERHEAT` | Model-dependent | Unknown | Review risks, storage limits, exact model support and user consent |
| Spatial/head tracking state and sensor data | `SpatialAudioPage`, `SpatialSensorManager`, `SPATIAL_AUDIO_*`, device feature flags | Select Buds Live/Pro/newer models | Planned only for data/control | Prove foreground phone capture/processing; desktop audio sinks/routing remain excluded |
| Conversation detection, seamless switching and media auto-pause | device status/commands and per-model flags | Newer models/revisions | Planned | Model-specific commands/readback; identify OS-host dependencies |
| Voice wakeup, hot command, language and Bixby remap | `BixbyRemapPage`, voice and hot-command message IDs | Model/revision-specific | Unknown | Trace app-vs-device behavior and phone limitations before enabling |
| Hearing enhancements/test, neck posture, adaptive EQ/volume and lighting | upstream settings/message IDs and model flags | Newer model/revision-specific | Unknown | Source completeness, model mapping, command tests and physical device verification |
| Settings, local configuration and export/import | `SettingsPage`, `Model/Config`, import/export dialogs | App-level | Planned | Resource, persistence corruption, bounded import and erase tests |
| Desktop tray icons and tray quick actions | `Utils/Interface/TrayManager`, `TrayItemTypes`, desktop app shell | Windows/macOS/Linux desktop only | Out of scope | WP8.1 Silverlight has no desktop tray host |
| Desktop hotkeys, media-key hooks, OS integration, window placement/startup | platform-specific implementations, `HotkeyPage` | Windows/macOS/Linux desktop only | Out of scope | No mobile equivalent for host OS integration in this app |
| CLI, IPC, scripting hooks and developer plugin environment | `Cli`, `Scripting`, developer pages | Desktop/developer workflows | Out of scope | Not a phone-user feature; do not port runtime code execution |
| Desktop spatial-audio capture, sinks, routing and broadcast | `Platform/SpatialAudio` | Desktop OS audio stack | Blocked on WP | No mobile system-audio routing/capture API established; do not imply parity from UI toggle |
| Firmware archive/downloader/extractor and developer debug mode | separate upstream tools/repositories and hidden command pages | Developer workflows | Out of scope | Separate projects and risk; no bundled opaque firmware or undocumented command surface |

## Feature verification record

No Buds hardware is connected to the development environment. The WP8.1 SDK exposes RFCOMM APIs and a minimal Silverlight API probe compiled against its reference metadata, but that does not establish successful Buds SPP discovery or control. Update this record only after the relevant test and physical-device evidence exist.
