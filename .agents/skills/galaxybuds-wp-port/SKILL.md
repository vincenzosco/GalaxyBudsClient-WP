---
name: galaxybuds-wp-port
description: Use when changing or reviewing this repository's Windows Phone 8.1 Silverlight Bluetooth client, Galaxy Buds RFCOMM/SPP protocol, device feature support, localization, or upstream attribution.
---

# Galaxy Buds WP8.1 port

## Overview

This repo is an independent, unofficial Windows Phone 8.1 Silverlight client. Device capability and real target behavior matter more than desktop feature-name parity.

## Required checks

1. Read `AGENTS.md`, the design spec and plan under `docs/superpowers/`, and `docs/PROTOCOL-CREDITS.md` before broad changes.
2. Confirm every Bluetooth API against the installed WP8.1 Silverlight SDK/reference assemblies. UWP documentation alone does not prove Silverlight app-model support.
3. Add a failing regression test before protocol or product behavior code. Cover CRC/framing bounds and malformed input for protocol changes.
4. Trace each visible device control through its model gate, encoder, transport, response/readback and user-facing failure state. Never use fake live values.
5. Put all UI strings in English default resources and Italian resources. Select Italian for `it` or `it-*`; use English for every other device language.
6. Keep comments in English, retain GPL-3.0 notices for derivative code, cite upstream protocol sources, and verify third-party artwork rights before reuse.
7. Require explicit confirmation and verified preconditions for reset or firmware operations. Do not silently retry non-idempotent commands.
8. Build with the historical Windows Phone toolchain when present; distinguish compilation from emulator and physical-Buds evidence.
9. Keep changes scoped, avoid generated output/secrets in commits, and use brief human commit subjects without AI trailers.

## Common mistakes

- Implementing the mockup without a real RFCOMM session.
- Assuming all models share the same commands or feature set.
- Treating desktop tray, hotkey, CLI, IPC, or desktop audio features as mobile equivalents.
- Calling a UWP-only API from Silverlight because a recent sample compiles elsewhere.
- Copying an upstream logo whose asset-specific reuse terms are unclear.
- Claiming a feature or build is verified without the corresponding test output or device evidence.
