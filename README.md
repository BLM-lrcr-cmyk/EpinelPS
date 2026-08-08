# EpinelPS — Personal Local Server Lab

<div align="center">

![Personal fork](https://img.shields.io/badge/personal%20fork-custom%20build-7c3aed?style=flat-square)
[![Upstream](https://img.shields.io/badge/upstream-EpinelPS%2FEpinelPS-2563eb?style=flat-square)](https://github.com/EpinelPS/EpinelPS)
[![License: AGPL-3.0](https://img.shields.io/badge/license-AGPL--3.0-green?style=flat-square)](LICENSE)
[![中文说明](https://img.shields.io/badge/README-%E4%B8%AD%E6%96%87-red?style=flat-square)](README.zh-CN.md)

**A private/local server playground for a 2D anime RPG game.**

Small fixes, safer defaults, and a little more personality for a self-hosted setup.

</div>

---

## About this fork

This repository is a personal fork of [EpinelPS/EpinelPS](https://github.com/EpinelPS/EpinelPS). The upstream project aims to replicate the functionality of the official server for local/private use.

This fork currently focuses on stability and quality-of-life tweaks:

- Character level commands and server-side validation now support the client-safe cap of `1400`.
- Character levels above `1400` are clamped back to `1400` on startup/reload.
- Synchro device level responses are also capped to avoid client-side infinite loading.
- Overload equipment option rolls are biased toward offensive stats such as attack, damage, crit, charge, ammo, reload, and hit-related options.
- `db.json` saving is safer: writes go through a temporary file and keep a `db.json.bak` backup.
- If `db.json` is empty or unreadable, the server tries to recover from `db.json.bak`.

> [!IMPORTANT]
> The `1400` cap matches the currently available client static level data. Levels above `1400` do not have matching client level/stat records, so the client can hang on loading screens or when opening character/synchro pages. This fork does not modify game resources, so the server clamps levels to `1400`.

## Official source and downloads

The official upstream project is:

https://github.com/EpinelPS/EpinelPS

Official build download:

https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/Server%20and%20Server%20selector.zip

Linux build:

https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/EpinelPS_linux_x64.zip

For running the game on Linux with EpinelPS, use:

https://github.com/EpinelPS/EpinelPSLauncher

> [!CAUTION]
> EpinelPS is free/open-source software. If someone sold it to you, you were likely scammed. Prefer the official upstream source and builds unless you intentionally use a personal fork like this one.

## Usage

1. Download the official GitHub Actions build from the upstream project.
2. Run `ServerSelector.Desktop.exe` as administrator.
   - This is needed to update the hosts file and install the CA certificate.
3. Close the game and launcher first.
4. Select `Local server`, then save.
5. Start `EpinelPS.exe` to run the actual server.
6. Register a new account in the launcher. Any email verification code should work.

Admin panel:

https://127.0.0.1/admin/

The first created account becomes the admin account.

> [!NOTE]
> Before updating the game, switch back to the official server so the game can patch normally.

## Local data notes

The server automatically searches the corresponding `Unity/.../saus/saus/lss` directory. Without this path, IDs can still work, but localized names may not resolve.

For this fork, levels above `1400` are treated as unsafe. The current client does not include level/stat records for `1401+`, so values such as `4500` or `9999` can make the client hang during loading. On startup or database reload, the server clamps those values back to `1400`.

## What is implemented or missing?

See the upstream todo/project board and issues:

- https://github.com/orgs/EpinelPS/projects/1
- https://github.com/EpinelPS/EpinelPS/issues

## License

This project follows the upstream license: [AGPL-3.0](LICENSE).
