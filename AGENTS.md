# Repository instructions

- This repository targets Windows Phone 8.1 Silverlight. Do not silently retarget to UWP, desktop .NET, or Windows Phone 8.0.
- Read `docs/superpowers/specs/2026-11-14-galaxy-buds-windows-phone-design.md`, `docs/superpowers/plans/2026-11-14-galaxy-buds-windows-phone.md`, and `docs/PROTOCOL-CREDITS.md` before changing protocol, Bluetooth, licensing, or feature-scope code.
- Treat UI controls as real device commands: no sample battery values, pretend connected state, or enabled controls without model gating, command implementation, and response/error handling.
- Keep all code comments in English. Put every user-facing string in resources. English is the default; Italian applies to `it` and `it-*`; every other device language uses English.
- Use APIs available to Windows Phone 8.1 Silverlight, not APIs merely present in modern UWP documentation. Run the historical MSBuild target and state when build/device verification is unavailable.
- Follow test-first development for behavior changes. Protocol frame/CRC/encoder/decoder changes require regression tests for valid, malformed, and boundary inputs.
- Preserve upstream GPL-3.0 notices and accurately credit protocol research in `docs/PROTOCOL-CREDITS.md`. Verify third-party artwork licenses before copying any asset.
- Do not imply affiliation with Samsung or the upstream author. Clearly mark the app unofficial and document unsupported desktop-only upstream features.
- Destructive reset and firmware operations need explicit confirmation and verified model/recovery preconditions. Never retry non-idempotent commands automatically.
- Keep changes focused. Do not commit signing keys, credentials, `.user` files, generated `Bin`/`obj` files, or unverified screenshots.
- Use short, human commit messages with no AI attribution or trailer. Keep all commit messages and code comments in English.
