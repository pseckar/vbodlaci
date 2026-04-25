#!/usr/bin/env bash
set -euo pipefail

APP_USER="${1:-vbodlaci}"
SSH_DIR="/home/${APP_USER}/.ssh"
KEY_PATH="${SSH_DIR}/github_actions"
AUTH_KEYS="${SSH_DIR}/authorized_keys"

sudo -u "${APP_USER}" mkdir -p "${SSH_DIR}"
sudo chmod 700 "${SSH_DIR}"
sudo -u "${APP_USER}" touch "${AUTH_KEYS}"
sudo -u "${APP_USER}" chmod 600 "${AUTH_KEYS}"

if [[ ! -f "${KEY_PATH}" ]]; then
  sudo -u "${APP_USER}" ssh-keygen -t ed25519 -q -f "${KEY_PATH}" -N "" -C "github-actions-staging"
fi

PUB_KEY="$(sudo -u "${APP_USER}" cat "${KEY_PATH}.pub")"
if ! sudo -u "${APP_USER}" grep -qxF "${PUB_KEY}" "${AUTH_KEYS}"; then
  echo "${PUB_KEY}" | sudo -u "${APP_USER}" tee -a "${AUTH_KEYS}" >/dev/null
fi

sudo -u "${APP_USER}" ssh-keygen -lf "${KEY_PATH}.pub"
