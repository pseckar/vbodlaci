# Staging CI/CD Runbook

This runbook configures staging deployment for `master` using GitHub Actions, SSH, `systemd`, `nginx`, and local PostgreSQL.

## 1) One-time VM bootstrap

1. Ensure you are authenticated in gcloud:
   - `gcloud auth login`
2. Run bootstrap script from repository root:
   - `.\scripts\staging\bootstrap-vm.ps1 -ProjectId "<PROJECT_ID>" -Zone "<ZONE>" -InstanceName "<INSTANCE_NAME>"`
3. Save the generated output values from the script:
   - `DB_PASSWORD`
   - `ADMIN_PASSWORD`
4. Generate deploy SSH key on VM:
   - `gcloud compute scp .\scripts\staging\setup-deploy-key.sh <INSTANCE_NAME>:/tmp/setup-deploy-key.sh --project "<PROJECT_ID>" --zone "<ZONE>"`
   - `gcloud compute ssh <INSTANCE_NAME> --project "<PROJECT_ID>" --zone "<ZONE>" --command "chmod +x /tmp/setup-deploy-key.sh && /tmp/setup-deploy-key.sh vbodlaci"`
   - `gcloud compute ssh <INSTANCE_NAME> --project "<PROJECT_ID>" --zone "<ZONE>" --command "sudo cat /home/vbodlaci/.ssh/github_actions"`

The script provisions:
- `vbodlaci-staging.service`
- `/opt/vbodlaci/{releases,shared,current}`
- `/etc/vbodlaci/staging.env`
- local PostgreSQL role/database
- nginx reverse proxy
- TLS certificate for `35-231-76-42.nip.io` (default)

## 2) GitHub environment setup

Create GitHub Environment `staging` and set required reviewer to your account.  
Then create these **Environment Secrets**:

- `STAGING_SSH_HOST` (`35.231.76.42`)
- `STAGING_SSH_PORT` (`22`)
- `STAGING_SSH_USER` (bootstrap deploy user, default `vbodlaci`)
- `STAGING_SSH_PRIVATE_KEY` (private key from `/home/vbodlaci/.ssh/github_actions`)
- `ConnectionStrings__DefaultConnection`
- `Admin__Email`
- `Admin__Password`
- `Site__SiteUrl`
- `Site__ContactInboxEmail`
- `Site__RegistrationInboxEmail`
- `Site__FacebookUrl`
- `Site__InstagramUrl`
- `LegalIdentity__BusinessName`
- `LegalIdentity__Address`
- `LegalIdentity__CompanyId`
- `LegalIdentity__ContactEmail`
- `LegalIdentity__ContactPhone`
- `Email__Smtp__Enabled`
- `Email__Smtp__Host`
- `Email__Smtp__Port`
- `Email__Smtp__UserName`
- `Email__Smtp__Password`
- `Email__Smtp__EnableSsl`
- `Email__Smtp__From`

SMTP can stay disabled in staging for now (`Email__Smtp__Enabled=false`).

## 3) Deploy flow

- Trigger: push to `master` (or manual `workflow_dispatch`)
- Pipeline:
  1. waits for successful `ci.yml` run on the same commit,
  2. publishes app artifact (`tar.gz`),
  3. uploads to VM over SSH,
  4. rewrites `/etc/vbodlaci/staging.env`,
  5. switches symlink `/opt/vbodlaci/current`,
  6. restarts `vbodlaci-staging.service`,
  7. checks `http://127.0.0.1:5000/healthz`,
  8. rolls back to previous symlink on failure,
  9. keeps only latest two releases.

## 4) Validation

- `https://35-231-76-42.nip.io`
- `sudo systemctl status vbodlaci-staging.service`
- `curl -fsS http://127.0.0.1:5000/healthz`
