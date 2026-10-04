# Lab notes (Mindaugas)

Short samples from a home lab: a personal VPN, a process-lifecycle wrapper, and a config-review checklist.

This is **not** a full product and **not** a game client or third-party engine. Only original notes and scripts are here.

## What is in this repo

| Folder | What it shows |
| --- | --- |
| `vpn/` | WireGuard on a VPS, admin UI bound to localhost, SSH tunnel only |
| `process-lifecycle/` | Windows console close → graceful child shutdown over localhost admin |
| `config-review/` | How I review bind addresses, default passwords, fail-closed settings |

## Skills

- Linux VPS, Docker Compose, UFW
- Least-privilege network exposure (no public admin UI)
- Windows process control and graceful shutdown
- Config review: defaults, empty-data fail-closed, dual-purpose admin channels

## What I did not publish

- Secrets, API keys, database passwords
- Third-party binaries / game assets
- Exploit proof-of-concept code
