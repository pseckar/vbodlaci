#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
  echo "Run as root (use sudo)." >&2
  exit 1
fi

APP_USER="${APP_USER:-vbodlaci}"
APP_GROUP="${APP_GROUP:-vbodlaci}"
DEPLOY_USER="${DEPLOY_USER:-$APP_USER}"
APP_DIR="${APP_DIR:-/opt/vbodlaci}"
ENV_DIR="${ENV_DIR:-/etc/vbodlaci}"
ENV_FILE="${ENV_DIR}/staging.env"
SERVICE_NAME="${SERVICE_NAME:-vbodlaci-staging}"
DOMAIN="${DOMAIN:-35-231-76-42.nip.io}"
LETSENCRYPT_EMAIL="${LETSENCRYPT_EMAIL:-petrsec@gmail.com}"
ENABLE_TLS="${ENABLE_TLS:-1}"

DB_NAME="${DB_NAME:-vbodlaci_staging}"
DB_USER="${DB_USER:-vbodlaci_staging}"
ADMIN_EMAIL="${ADMIN_EMAIL:-petrsec@gmail.com}"

# Avoid SIGPIPE failures from tr|head under pipefail when generating random values.
set +o pipefail
DB_PASSWORD="${DB_PASSWORD:-$(tr -dc 'A-Za-z0-9' </dev/urandom | head -c 32)}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-$(tr -dc 'A-Za-z0-9' </dev/urandom | head -c 32)}"
set -o pipefail

DEPLOY_PUBLIC_KEY="${DEPLOY_PUBLIC_KEY:-}"

echo "Installing system packages..."
export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get install -y \
  apt-transport-https \
  ca-certificates \
  certbot \
  curl \
  gnupg \
  nginx \
  postgresql \
  postgresql-contrib \
  python3-certbot-nginx

if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-runtimes | grep -q "Microsoft.AspNetCore.App 10"; then
  echo "Installing ASP.NET Core Runtime 10..."
  curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -o /tmp/packages-microsoft-prod.deb
  dpkg -i /tmp/packages-microsoft-prod.deb
  rm -f /tmp/packages-microsoft-prod.deb
  apt-get update
  apt-get install -y aspnetcore-runtime-10.0
fi

echo "Configuring users and directories..."
groupadd -f "${APP_GROUP}"

if ! id -u "${APP_USER}" >/dev/null 2>&1; then
  useradd --create-home --shell /bin/bash --gid "${APP_GROUP}" "${APP_USER}"
fi

if ! id -u "${DEPLOY_USER}" >/dev/null 2>&1; then
  useradd --create-home --shell /bin/bash "${DEPLOY_USER}"
fi

usermod -a -G "${APP_GROUP}" "${DEPLOY_USER}"

if [[ -n "${DEPLOY_PUBLIC_KEY}" ]]; then
  DEPLOY_HOME="$(getent passwd "${DEPLOY_USER}" | cut -d: -f6)"
  install -d -m 0700 -o "${DEPLOY_USER}" -g "${DEPLOY_USER}" "${DEPLOY_HOME}/.ssh"
  printf '%s\n' "${DEPLOY_PUBLIC_KEY}" > "${DEPLOY_HOME}/.ssh/authorized_keys"
  chown "${DEPLOY_USER}:${DEPLOY_USER}" "${DEPLOY_HOME}/.ssh/authorized_keys"
  chmod 0600 "${DEPLOY_HOME}/.ssh/authorized_keys"
fi

install -d -m 2775 -o "${APP_USER}" -g "${APP_GROUP}" "${APP_DIR}"
install -d -m 2775 -o "${APP_USER}" -g "${APP_GROUP}" "${APP_DIR}/releases"
install -d -m 2775 -o "${APP_USER}" -g "${APP_GROUP}" "${APP_DIR}/shared"

echo "Configuring PostgreSQL..."
systemctl enable --now postgresql

sudo -u postgres psql -v ON_ERROR_STOP=1 <<SQL
DO \$\$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '${DB_USER}') THEN
        EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L', '${DB_USER}', '${DB_PASSWORD}');
    ELSE
        EXECUTE format('ALTER ROLE %I WITH LOGIN PASSWORD %L', '${DB_USER}', '${DB_PASSWORD}');
    END IF;
END
\$\$;
SQL

if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='${DB_NAME}'" | grep -q 1; then
  sudo -u postgres psql -v ON_ERROR_STOP=1 -c "CREATE DATABASE ${DB_NAME} OWNER ${DB_USER};"
fi

sudo -u postgres psql -v ON_ERROR_STOP=1 -c "GRANT ALL PRIVILEGES ON DATABASE ${DB_NAME} TO ${DB_USER};"

echo "Writing staging environment file..."
install -d -m 0750 -o root -g "${APP_GROUP}" "${ENV_DIR}"
if [[ ! -f "${ENV_FILE}" ]]; then
  cat > "${ENV_FILE}" <<EOF
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://127.0.0.1:5000
ConnectionStrings__DefaultConnection=Host=127.0.0.1;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
Admin__Email=${ADMIN_EMAIL}
Admin__Password=${ADMIN_PASSWORD}
Site__SiteUrl=https://${DOMAIN}
Site__ContactInboxEmail=${ADMIN_EMAIL}
Site__RegistrationInboxEmail=${ADMIN_EMAIL}
Site__FacebookUrl=#
Site__InstagramUrl=#
LegalIdentity__BusinessName=Staging Legal Name
LegalIdentity__Address=Staging Address
LegalIdentity__CompanyId=12345678
LegalIdentity__ContactEmail=${ADMIN_EMAIL}
LegalIdentity__ContactPhone=+420000000000
Email__Smtp__Enabled=false
Email__Smtp__Host=
Email__Smtp__Port=587
Email__Smtp__UserName=
Email__Smtp__Password=
Email__Smtp__EnableSsl=true
Email__Smtp__From=noreply@${DOMAIN}
EOF
fi
chown root:"${APP_GROUP}" "${ENV_FILE}"
chmod 0640 "${ENV_FILE}"

echo "Configuring systemd..."
cat > "/etc/systemd/system/${SERVICE_NAME}.service" <<EOF
[Unit]
Description=Vbodlaci staging web app
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=simple
User=${APP_USER}
Group=${APP_GROUP}
WorkingDirectory=${APP_DIR}/current
EnvironmentFile=${ENV_FILE}
ExecStart=/usr/bin/dotnet ${APP_DIR}/current/Vbodlaci.Web.dll
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=${SERVICE_NAME}

[Install]
WantedBy=multi-user.target
EOF

echo "Configuring sudo for deploy user..."
cat > "/etc/sudoers.d/${DEPLOY_USER}-${SERVICE_NAME}" <<EOF
${DEPLOY_USER} ALL=(root) NOPASSWD: /bin/systemctl restart ${SERVICE_NAME}.service, /usr/bin/systemctl restart ${SERVICE_NAME}.service, /bin/mkdir, /usr/bin/mkdir, /usr/bin/mv, /usr/bin/chown, /usr/bin/chmod
EOF
chmod 0440 "/etc/sudoers.d/${DEPLOY_USER}-${SERVICE_NAME}"

echo "Configuring nginx..."
cat > "/etc/nginx/sites-available/${SERVICE_NAME}" <<EOF
server {
    listen 80;
    listen [::]:80;
    server_name ${DOMAIN};

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header X-Forwarded-Host \$host;
        proxy_set_header X-Forwarded-Port \$server_port;
    }
}
EOF

ln -sfn "/etc/nginx/sites-available/${SERVICE_NAME}" "/etc/nginx/sites-enabled/${SERVICE_NAME}"
rm -f /etc/nginx/sites-enabled/default
nginx -t
systemctl enable --now nginx

if [[ "${ENABLE_TLS}" == "1" ]]; then
  echo "Requesting TLS certificate for ${DOMAIN}..."
  certbot --nginx --non-interactive --agree-tos --redirect -m "${LETSENCRYPT_EMAIL}" -d "${DOMAIN}"
fi

systemctl daemon-reload
systemctl enable "${SERVICE_NAME}.service"

if [[ -f "${APP_DIR}/current/Vbodlaci.Web.dll" ]]; then
  systemctl restart "${SERVICE_NAME}.service"
fi

echo
echo "Bootstrap completed."
echo "Deployment user: ${DEPLOY_USER}"
echo "Service: ${SERVICE_NAME}.service"
echo "Environment file: ${ENV_FILE}"
echo "Domain: ${DOMAIN}"
echo "Database: ${DB_NAME} (user: ${DB_USER})"
echo
echo "Generated values (store safely):"
echo "  DB_PASSWORD=${DB_PASSWORD}"
echo "  ADMIN_PASSWORD=${ADMIN_PASSWORD}"
