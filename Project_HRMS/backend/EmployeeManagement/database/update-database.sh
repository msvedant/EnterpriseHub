#!/usr/bin/env bash
# Applies pending migrations. Set EMS_CONNECTION to target a specific server.
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet ef database update \
  --project src/EmployeeManagement.Infrastructure \
  --startup-project src/EmployeeManagement.API
