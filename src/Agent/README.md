# AyBorg.Agent

## Initialize database

`dotnet ef database update`

## Update migrations

`dotnet ef migrations add ProjectMetaVersion -c ProjectContext --project ../SDK/database/migrations/AyBorg.Database.Migrations.SqlLite -- --DatabaseProvider SqlLite`

`dotnet ef migrations add ProjectMetaVersion -c ProjectContext --project ../SDK/database/migrations/AyBorg.Database.Migrations.PostgreSql -- --DatabaseProvider PostgreSql`
