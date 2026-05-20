#!/usr/bin/env bash
set -euo pipefail

: "${RENDER_API_KEY:?RENDER_API_KEY is required}"
: "${RENDER_UI_SERVICE_ID:?RENDER_UI_SERVICE_ID is required}"
: "${TEST_API_URL:?TEST_API_URL is required}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=render-put-env.sh
source "${SCRIPT_DIR}/render-put-env.sh"

put_env "${RENDER_UI_SERVICE_ID}" "VITE_API_URL" "${TEST_API_URL}"

echo "UI environment variables synced to Render."
