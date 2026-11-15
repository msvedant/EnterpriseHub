#!/usr/bin/env bash
# Generates an idempotent SQL script for DBA-reviewed deployments (e.g. Azure SQL).
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet ef migrations script --idempotent \
  --project src/EmployeeManagement.Infrastructure \
  --startup-project src/EmployeeManagement.API \
  --output database/migrations.sql
