# Personal WireGuard VPN

Fast personal VPN: **WireGuard** + **wg-easy**.
Runs on a Linux VPS with a public IPv4.

## Needs

1. Linux VPS (Ubuntu 22.04/24.04, 1 vCPU, 1 GB RAM is enough).
2. Public IPv4 (no CGNAT).
3. This `vpn` folder copied onto the VPS.

A Windows PC is the **client**, not a good VPN server host.

## 1. Copy files to the VPS

From Windows (PowerShell), replace the IP:

```powershell
scp -r vpn root@VPS_IP:/root/vpn
```

## 2. Start the server

On the VPS:

```bash
cd /root/vpn
chmod +x setup-vps.sh
sudo bash setup-vps.sh
```

The script installs Docker, opens **UDP 51820**, and starts VPN.
The admin panel is **not** published to the internet.

## 3. Open the admin UI from your PC

```powershell
ssh -L 51821:127.0.0.1:51821 root@VPS_IP
```

Browser: [http://127.0.0.1:51821](http://127.0.0.1:51821)

- Create an admin password.
- Host field: VPS public IP (or domain).
- Create clients, e.g. `PC` and `Phone`.
- Download `.conf` for PC; scan QR on the phone.

## 4. Windows client

1. Install [WireGuard](https://www.wireguard.com/install/).
2. Import the `.conf`.
3. Activate.
4. Check IP: [https://ifconfig.me](https://ifconfig.me) — should show the VPS IP.

Phone: official WireGuard app + QR.

## Security (already on)

- VPN tunnel: UDP 51820 (only public VPN port).
- Web UI only on `127.0.0.1` — reach it through an SSH tunnel.
- IP forwarding enabled in the container.
- Firewall (UFW): SSH + WireGuard.

Never expose `51821` on `0.0.0.0` unless you have HTTPS and a strong password.

## Useful commands on the VPS

```bash
cd /root/vpn
docker compose logs -f
docker compose ps
docker compose restart
docker compose down
```

## If handshake fails

- Open **UDP 51820** on the VPS firewall / provider panel.
- `.env` `WG_HOST` must be a real public IP, not `10.x` / `192.168.x`.
- Client and server clocks should be correct (NTP).
- Corporate firewalls sometimes block UDP.
