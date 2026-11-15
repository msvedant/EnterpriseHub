# Database

The schema is owned by EF Core (Fluent API configurations in
`src/EmployeeManagement.Infrastructure/Persistence/Configurations`).

| Script | Purpose |
|---|---|
| `add-migration.sh <Name>` | Create a migration in `Persistence/Migrations` |
| `update-database.sh` | Apply migrations to the database in `EMS_CONNECTION` |
| `script-migrations.sh` | Emit an idempotent `migrations.sql` for review / CI deployment |

First-time setup: run `./database/add-migration.sh InitialCreate`, then commit the generated `Migrations` folder.
Until a migration exists, the API (with `Database:ApplyMigrationsOnStartup=true`) falls back to `EnsureCreated`,
which is suitable for local development only.

Master data (departments, designations, employee types, relationships, countries, states, cities) and the first admin
account are seeded idempotently by `DbInitializer` on startup (`Database:SeedOnStartup`).
