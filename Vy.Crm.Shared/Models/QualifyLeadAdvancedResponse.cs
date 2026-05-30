using System;

namespace Vy.Crm.Shared.Models
{
    public class QualifyLeadAdvancedResponse
    {
        public Guid AccountId     { get; set; }
        public Guid ContactId     { get; set; }
        public Guid OpportunityId { get; set; }
        public bool Success       { get; set; }
        public string Message     { get; set; }
    }
}
