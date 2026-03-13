using Azure.Core;
using System;

namespace LeaveOTManagement.Models.Entities
{
    public class PayrollLog
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public long OTRequestId { get; set; }

        public DateTime WorkDate { get; set; }

        public decimal Hours { get; set; }

        public decimal RateMultiplier { get; set; }

        public decimal? Amount { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; }

        public Otrequest Otrequest { get; set; }
    }
}
