# Setup Instructions

## 1. Prerequisites

*   **.NET 8 SDK**: [Download Here](https://dotnet.microsoft.com/download/dotnet/8.0)
*   **Docker Desktop**: Required for PostgreSQL and Cardano Node.

## 2. Database Setup

We use Docker to run the PostgreSQL database for both the application data (`whrid`) and the blockchain data (`argus`).

Start the services:

```bash
docker-compose up -d
```

**Note:** If you are migrating from the old "WashTradeDetective" project, you may need to clear the old database volume to ensure correct credentials:

```bash
docker-compose down
docker volume rm el-scorcho_postgres_data
docker-compose up -d
```

This will start:
*   Postgres (Port 5433) - User: `postgres`, Password: `password`
*   Cardano Node (Port 3001)

## 3. Configuration

The application is pre-configured to connect to the Docker database on port 5433.
Default Connection Strings (in `Program.cs`):

*   **WHRIDConnection** (App Data): `Host=localhost;Database=whrid;Username=postgres;Password=password;Port=5433`
*   **ArgusConnection** (Blockchain Data): `Host=localhost;Database=argus;Username=postgres;Password=password;Port=5433`

If you need to change these, update `src/Presentation/appsettings.json`.

## 4. Argus Indexing (Hybrid Mode)

The system runs in a **Hybrid Mode**:
1.  It attempts to connect to the `ArgusConnection`.
2.  If the connection fails or the `argus` database is empty (common in local hackathon demos), it automatically falls back to **Simulation Mode**.
3.  **Simulation Mode** generates realistic wallet activity, risk scores, and transaction history for demonstration purposes.

## 5. Running the Application

Navigate to the project root and run:

```bash
dotnet run --project src/Presentation/WHRID.Presentation.csproj
```

Open the URL displayed in the terminal (usually `http://localhost:5xxx`) in your browser.
