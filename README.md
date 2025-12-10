# Wash Trade Detective

A Cardano blockchain analysis tool to detect wash trading activities using Argus.

## Project Structure

- **Argus.Indexer**: Console application responsible for ingesting blockchain data (UTXOs) via Argus and storing it in PostgreSQL.
- **WashTrade.Analyzer**: Background worker service that analyzes the indexed data for suspicious patterns (Circular Trading, Self-Funding).
- **WashTrade.UI**: Blazor Web App (Interactive Server) for visualizing the network and alerts.
- **WashTrade.Domain**: Shared library containing EF Core entities and DbContext.
- **WashTrade.Tests**: Unit test project verifying the analysis logic using an in-memory database.

## Prerequisites

- .NET 9 SDK
- Docker Desktop

## Setup Instructions

1.  **Start Database**:
    ```bash
    docker-compose up -d
    ```

2.  **Apply Migrations**:
    ```bash
    dotnet ef database update --project WashTrade.Domain --startup-project Argus.Indexer
    ```

3.  **Run Tests**:
    Verify the logic by running the unit tests:
    ```bash
    dotnet test
    ```

4.  **Run Projects**:
    You can run the projects individually or set up multiple startup projects in Visual Studio / VS Code.

    - Start the UI:
      ```bash
      dotnet run --project WashTrade.UI
      ```
    - Start the Analyzer:
      ```bash
      dotnet run --project WashTrade.Analyzer
      ```
    - Start the Indexer:
      ```bash
      dotnet run --project Argus.Indexer
      ```

## Features

- **Data Ingestion**: Scaffolding for Argus integration.
- **Circular Trade Detection**: Detects A -> B -> A patterns within 24 hours.
- **Self-Funding Detection**: Detects Wallet A funding Wallet B (ADA) immediately before B buys an NFT from A.
- **Dashboard**: Real-time view of wash trade alerts with visual badges.
