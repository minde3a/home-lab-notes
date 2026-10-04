# Config review notes (home lab)

These are findings from reviewing a self-hosted lab. No live secrets, IPs, or third-party game files.

## 1. Bind addresses

- Public services: only what users need (e.g. VPN UDP).
- Admin UIs and remote consoles: `127.0.0.1`, then SSH tunnel.
- `hostname = *` plus a weak default DB password is a common default-install trap.

## 2. Empty data + fail-open

If a feature needs files on disk (maps, keys, geo), **do not enable it when the folder is empty**.
Fail-open here looked like “feature is on” but movement/LOS was wrong. Safer: feature off until data exists (`Force* = false`).

## 3. Dual-purpose admin channels

If a process exposes a localhost admin port (telnet-style):

- Restrict by host list (`127.0.0.1` only).
- Change the default password.
- Prefer wrapping window-close so the process can persist state instead of `TerminateProcess`.

## 4. Secrets hygiene for GitHub

Never commit:

- `.env` with real hosts/passwords
- `Password=` / JDBC URLs from production
- Game clients, engines, datapacks you do not own

Commit:

- `.env.example` with placeholders
- Original scripts and review notes
- Short README that explains *why*, not just *what*

## 5. Checklist I actually used

- [ ] Admin UI not on `0.0.0.0`
- [ ] Firewall: SSH + VPN UDP only
- [ ] Default passwords rotated
- [ ] Empty-folder features disabled
- [ ] Close-window path saves state
- [ ] Repo has `.gitignore` for `.env` / `*.exe` / logs
