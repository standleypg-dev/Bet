# Your Dream Kaching

## Installation / Getting Started

### server

Make sure to check appsettings.json for database credentials reference & editing

```
$cd server
$dotnet ef migrations <MigrationFileName>
$dotnet ef database update
$dotnet restore
$dotnet watch OR $dotnet run
```

### client

```
$cd client
$npm install
$ng serve -o
```
