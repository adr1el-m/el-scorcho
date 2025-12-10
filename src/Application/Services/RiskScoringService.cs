using System;
using WHRID.Domain.Entities;
using WHRID.Domain.Enums;

namespace WHRID.Application.Services
{
    public class RiskScoringService
    {
        public RiskProfile CalculateRisk(Wallet wallet)
        {
            var profile = new RiskProfile
            {
                WalletId = wallet.Id,
                CalculatedAt = DateTime.UtcNow
            };

            // 1. Activity Score
            profile.ActivityScore = CalculateActivityScore(wallet);

            // 2. Risk Score
            profile.RiskScore = CalculateRiskScore(wallet);

            // 3. Classification
            profile.Classification = ClassifyWallet(wallet, profile.ActivityScore, profile.RiskScore);

            return profile;
        }

        private double CalculateActivityScore(Wallet wallet)
        {
            // Simple heuristic: Max 100 based on tx count
            double score = Math.Min(100, wallet.Transactions.Count * 2);
            return score;
        }

        private double CalculateRiskScore(Wallet wallet)
        {
            // Heuristic: High frequency of small txs might be a bot/spam
            // Or large sudden movements
            double score = 10; // Base risk
            if (wallet.Transactions.Count > 1000) score += 20;
            
            // Random factor for demo diversity
            var hash = wallet.Address.GetHashCode();
            if (hash % 10 == 0) score += 50; // High risk demo

            return Math.Min(100, score);
        }

        private WalletClassification ClassifyWallet(Wallet wallet, double activity, double risk)
        {
            if (risk > 80) return WalletClassification.HighRisk;
            if (activity > 90) return WalletClassification.Whale;
            if (wallet.Transactions.Count > 50) return WalletClassification.Trader;
            return WalletClassification.NormalUser;
        }
    }
}
