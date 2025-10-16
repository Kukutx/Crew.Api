# Crew API

This repository contains the backend services for the Crew application. The API is built with ASP.NET Core 9 and Entity Framework Core.

## Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/)
* PostgreSQL 14 or later

You can start a local PostgreSQL instance with Docker:

```bash
docker run --name crew-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -d postgres:16
```

## Database initialization

On application start-up the API now checks whether the configured PostgreSQL database exists and creates it automatically when the user has sufficient privileges. Make sure the connection string points to the server you started. For example:

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=crew_db;Username=postgres;Password=postgres"
}
```

If your database user cannot create databases, create the database manually before starting the API:

```sql
CREATE DATABASE crew_db;
```

## Running the API

```bash
dotnet restore
dotnet run --project Crew.Api/Crew.Api.csproj
```

The API will be available at [http://localhost:5096](http://localhost:5096) with Swagger UI enabled in development mode.

