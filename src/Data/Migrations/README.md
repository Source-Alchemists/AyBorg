# Example migration creating

## SqlLite

`dotnet ef migrations add SqlLite --context RegistryContext --project ../migrations/AyBorg.Database.Migrations.SqlLite -- --databaseprovider SqlLite`

## PostgreSql

`dotnet ef migrations add Postgres --context RegistryContext --project ../migrations/AyBorg.Database.
Migrations.PostgreSql -- --databaseprovider PostgreSql`
