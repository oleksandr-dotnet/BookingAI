#!/usr/bin/env bash
set -euo pipefail

: "${RENDER_API_KEY:?RENDER_API_KEY is required}"
: "${1:?Service ID argument is required}"

service_id="$1"

response="$(curl -fsS -X POST \
  "https://api.render.com/v1/services/${service_id}/deploys" \
  -H "Authorization: Bearer ${RENDER_API_KEY}" \
  -H "Content-Type: application/json" \
  -d '{"clearCache":"do_not_clear"}')"

echo "Deploy triggered for service ${service_id}."
echo "${response}" | jq -r '.id // .deploy.id // "queued"' 2>/dev/null || echo "${response}"
