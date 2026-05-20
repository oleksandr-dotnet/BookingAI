#!/usr/bin/env bash
set -euo pipefail

: "${RENDER_API_KEY:?RENDER_API_KEY is required}"
: "${RENDER_API_SERVICE_ID:?RENDER_API_SERVICE_ID is required}"
: "${NEON_CONNECTION_STRING:?NEON_CONNECTION_STRING is required}"
: "${JWT_KEY:?JWT_KEY is required}"
: "${TEST_UI_URL:?TEST_UI_URL is required (UI origin for CORS)}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=render-put-env.sh
source "${SCRIPT_DIR}/render-put-env.sh"

put_env "${RENDER_API_SERVICE_ID}" "ASPNETCORE_ENVIRONMENT" "Staging"
put_env "${RENDER_API_SERVICE_ID}" "ConnectionStrings__DefaultConnection" "${NEON_CONNECTION_STRING}"
put_env "${RENDER_API_SERVICE_ID}" "Jwt__Key" "${JWT_KEY}"
put_env "${RENDER_API_SERVICE_ID}" "Cors__AllowedOrigins" "${TEST_UI_URL}"

if [[ -n "${ADMIN_EMAIL:-}" ]]; then
  put_env "${RENDER_API_SERVICE_ID}" "Admin__Email" "${ADMIN_EMAIL}"
fi

if [[ -n "${ADMIN_PASSWORD:-}" ]]; then
  put_env "${RENDER_API_SERVICE_ID}" "Admin__Password" "${ADMIN_PASSWORD}"
fi

echo "API environment variables synced to Render."
