# API Contracts & Data Structures

## Overview
This document defines the data structures (DTOs), service contracts, and data flow for the WHRID system.

## Data Flow
1. **User Input**: User enters a wallet address in the Dashboard.
2. **Analysis Service**: `WalletAnalysisService` receives the request.
   - Checks `IWalletRepository` for existing cached analysis.
   - If not found or stale:
     - Queries `IArgusRepository` for raw blockchain data (UTxOs, Transactions).
     - Passes raw data to `RiskScoringService`.
3. **Scoring Engine**: `RiskScoringService` computes:
   - Activity Score (0-100)
   - Risk Score (0-100)
   - Classification (e.g., Whale, Trader, Bot)
4. **Persistence**: Result is saved to `WHRIDDbContext`.
5. **Presentation**: Data is returned to the UI for rendering.

## Service Contracts

### IWalletAnalysisService
Primary entry point for wallet intelligence.
```csharp
public interface IWalletAnalysisService
{
    Task<WalletAnalysisResult> AnalyzeWalletAsync(string address);
    Task<WalletGraph> GetInteractionGraphAsync(string address, int depth);
}
```

### IRiskScoringService
Heuristic engine for risk assessment.
```csharp
public interface IRiskScoringService
{
    RiskProfile CalculateRisk(Wallet walletData);
    double CalculateActivityScore(IEnumerable<Transaction> history);
    double CalculateTokenDiversity(IEnumerable<Token> tokens);
}
```

### IArgusRepository
Abstraction over the Argus Blockchain Indexer (PostgreSQL).
```csharp
public interface IArgusRepository
{
    Task<List<ArgusTransaction>> GetTransactionsByAddressAsync(string address, int limit = 50);
    Task<List<ArgusTxOut>> GetUtxosByAddressAsync(string address);
    Task<decimal> GetBalanceAsync(string address);
}
```

## Data Transfer Objects (DTOs)

### WalletAnalysisResult
Complete snapshot of a wallet's health.
```json
{
  "address": "addr1...",
  "balance": 15000.50,
  "riskProfile": {
    "score": 85,
    "level": "High",
    "factors": ["Rapid Cycling", "Layering"]
  },
  "activity": {
    "dailyTxCount": 45,
    "lastActive": "2023-10-27T10:00:00Z"
  },
  "classification": "HighFrequencyTrader"
}
```

### WalletGraph
Force-directed graph structure.
```json
{
  "nodes": [
    { "id": "addr1...", "type": "Wallet", "risk": 85 },
    { "id": "addr2...", "type": "Exchange", "risk": 10 }
  ],
  "links": [
    { "source": "addr1...", "target": "addr2...", "value": 5000 }
  ]
}
```
