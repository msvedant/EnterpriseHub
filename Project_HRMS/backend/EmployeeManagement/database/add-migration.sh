#!/usr/bin/env bash
# Usage: ./database/add-migration.sh <MigrationName>
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet tool restore >/dev/null 2>&1 || true
dotnet ef migrations add "${1:?Migration name required}" \
  --project src/EmployeeManagement.Infrastructure \
  --startup-project src/EmployeeManagement.API \
  --output-dir Persistence/Migrations
