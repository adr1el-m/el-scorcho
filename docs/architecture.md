# Architecture Overview

WHRID follows the **Clean Architecture** pattern to ensure separation of concerns and testability.

## Layers

### 1. Domain
The core of the application. It contains:
- **Entities**: `Wallet`, `Transaction`, `UTxO`, `RiskAssessment`.
- **Enums**: `WalletType`, `RiskLevel`.
- **Value Objects**: `Address`, `TokenAmount`.
- **Interfaces**: Repository interfaces (e.g., `IWalletRepository`).

*Dependencies: None.*

### 2. Application
Contains business logic and use cases.
- **Services**: `WalletAnalysisService`, `RiskScoringService`.
- **DTOs**: Data Transfer Objects for UI.
- **Interfaces**: Service contracts.

*Dependencies: Domain.*

### 3. Infrastructure
Implements external concerns.
- **Database**: `ArgusDbContext` (Entity Framework Core).
- **Repositories**: `WalletRepository`, `ArgusRepository`.
- **Argus Integration**: Logic to connect to Argus Indexer DB and Cardano Node.

*Dependencies: Application, Domain.*

### 4. Presentation
The User Interface.
- **Blazor Server**: `WHRID.Presentation`.
- **Components**: Reusable UI widgets (Charts, Search Bar).
- **Pages**: Dashboard, Analytics, Details.

*Dependencies: Application, Infrastructure (for DI).*

## Data Flow

1.  **User** searches for a wallet address in the UI.
2.  **Presentation** layer calls `IWalletService.AnalyzeWallet(address)`.
3.  **Application** layer orchestrates:
    *   Fetches raw data via `IArgusRepository` (Infrastructure).
    *   Computes scores using `RiskScoringService`.
    *   Returns a `WalletProfileDTO`.
4.  **Infrastructure** queries the PostgreSQL database populated by Argus.

## Database Schema

We utilize the schema provided by Argus, plus our own tables for caching analysis results:
*   `AnalysisResults`: Stores computed scores to avoid re-calculation.
*   `WalletTags`: User-defined or system-defined tags for wallets.
