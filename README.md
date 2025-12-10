# WHRID (Wallet Health & Risk Intelligence Dashboard)

WHRID is a **Cardano Risk Intelligence** platform that analyzes wallet health, detects anomalous behavior, and visualizes fund flows using real-time blockchain data. It combines on-chain data ingestion (via Oura) with AI-driven risk scoring.

![WHRID Dashboard](docs/dashboard-preview.png)

## 🚀 Features

- **Live Activity Feed**: Real-time stream of blocks and transactions from the Cardano Preview network.
- **Wallet Analysis**: Enter any address to see:
  - **Risk Score**: 0-100 assessment based on transaction patterns.
  - **Classification**: Automatic tagging (Whale, Trader, Bot, Normal User).
  - **Transaction History**: Full history pulled from a local Argus indexer.
- **Glassmorphic UI**: Modern, dark-themed interface with parallax effects and responsive design.
- **Oura Integration**: Custom webhook sink that persists blockchain events into a structured PostgreSQL database (`argus`).

## 🛠 Tech Stack

- **Frontend/Backend**: .NET 8, Blazor Server (Interactive Server Mode)
- **Database**: PostgreSQL (Two contexts: `whrid` for app data, `argus` for blockchain index)
- **Indexer**: [Oura](https://github.com/txpipe/oura) (Rust-based pipeline)
- **Infrastructure**: Docker Compose (Postgres, Cardano Node, Oura)

## 📦 Prerequisites

- **.NET 8 SDK**
- **Docker Desktop**

## 🏁 Quick Start

1. **Start Infrastructure**
   ```bash
   docker-compose up -d
   ```
   This spins up:
   - `postgres` (Port 5433)
   - `cardano-node` (Preview Network, Port 3001)
   - `oura` (Daemon mode, syncing from node to app webhook)

2. **Run Application**
   ```bash
   dotnet run --project src/Presentation/WHRID.Presentation.csproj
   ```
   The app will be available at `http://localhost:5213`.

3. **Explore**
   - Open `http://localhost:5213`.
   - Watch the live feed populate as Oura syncs.
   - Analyze a wallet (e.g., `addr1...`) to see risk scores.

## 🏗 Architecture

- **Oura Webhook**: The app exposes `POST /api/webhook/oura`. Oura streams `Block` and `Transaction` events here.
- **Argus Context**: The app ingests these events into the `argus` database schema (`block`, `tx`, `tx_out`).
- **Analysis Engine**: When a wallet is analyzed, the service:
  1. Fetches raw txs from `argus`.
  2. Aggregates stats (volume, frequency).
  3. Calculates a risk score and classification.
  4. Caches the result in the `whrid` database.

## 🧪 Testing

Run unit tests:
```bash
dotnet test
```

## 📝 Configuration

- **Database Connection**: `src/Presentation/appsettings.json`
- **Oura Pipeline**: `config/oura/daemon.toml` (Configured to filter Block/Transaction events and send to webhook)

## 📄 License

MIT
