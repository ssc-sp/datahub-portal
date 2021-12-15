# Entity Framework Setup

## Operations

1. Install EF Tools

[Documentation](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)

1. Update database

- Go into project directory (e.g. `C:\code\nrcan\portal\DatahubPBILicenseForms`)
- `dotnet ef migrations add <simple name> --context <context>`
- Project DB: `dotnet ef migrations add "ProjectUser" --context DatahubProjectDBContext`
- Option 1: Generate SQL Script `dotnet ef migrations script --context DatahubProjectDBContext`
- Option 2: Apply Migration Directly `dotnet ef database update --context DatahubProjectDBContext`
- Option 3: Package Manager: `Add-Migration -Name "PBIWorkspaces" -Context DatahubProjectDBContext -OutputDir Migrations/Core`

1. Remove Migration
`dotnet ef migrations remove --context DatahubProjectDBContext --force`

## Projects

### Datahub Projects

- Context class is Datahub.Data.Projects.DatahubProjectDBContext

### PIP Form

- Context class is Datahub.ProjectForms.Data.PIP.PIPDBContext
- Add-Migration -name InitialPip -context pipdbcontext -o Migrations/Forms/PIP


