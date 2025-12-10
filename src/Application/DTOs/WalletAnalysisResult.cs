using System;
using WHRID.Domain.Enums;

namespace WHRID.Application.DTOs
{
    public class WalletAnalysisResult
    {
        public string Address { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public double RiskScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public double ActivityScore { get; set; }
        public WalletClassification Classification { get; set; }
    }
}
