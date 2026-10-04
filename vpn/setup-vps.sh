#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
  echo "Run as root: sudo bash setup-vps.sh"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

if [[ ! -f .env ]]; then
  PUBLIC_IP="$(curl -4 -fsS https://ifconfig.me || true)"
  if [[ -z "${PUBLIC_IP}" ]]; then
    PUBLIC_IP="PASTE_PUBLIC_VPS_IP_HERE"
  fi
  sed "s/PASTE_PUBLIC_VPS_IP_HERE/${PUBLIC_IP}/" .env.example > .env
  echo "Created .env with WG_HOST=${PUBLIC_IP}"
fi

export DEBIAN_FRONTEND=noninteractive
apt-get update -y
apt-get install -y ca-certificates curl ufw

if ! command -v docker >/dev/null 2>&1; then
  curl -fsSL https://get.docker.com | sh
fi

systemctl enable --now docker

ufw allow OpenSSH
ufw allow 51820/udp comment 'WireGuard'
ufw --force enable

docker compose pull
docker compose up -d

echo
echo "VPN server is up."
echo "Open the admin UI from your PC:"
echo "  ssh -L 51821:127.0.0.1:51821 root@$(grep '^WG_HOST=' .env | cut -d= -f2)"
echo "Then open: http://127.0.0.1:51821"
echo "On first login create an admin account and confirm the host address."
