#!/usr/bin/env bash
set -euo pipefail

put_env() {
  local service_id="$1"
  local key="$2"
  local value="$3"
  local encoded_key
  encoded_key="$(jq -rn --arg k "$key" '$k|@uri')"

  curl -fsS -X PUT \
    "https://api.render.com/v1/services/${service_id}/env-vars/${encoded_key}" \
    -H "Authorization: Bearer ${RENDER_API_KEY}" \
    -H "Content-Type: application/json" \
    -d "$(jq -n --arg v "$value" '{value: $v}')"
}
