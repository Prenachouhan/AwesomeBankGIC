using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBank.Dtos
{
    public class InterestRule
    {
        public DateTime StartDate { get; }
        public string RuleId { get; }
        public decimal Rate { get; } // Percentage (e.g., 2.0 for 2%)

        public InterestRule(DateTime startDate, string ruleId, decimal rate)
        {
            if (rate <= 0 || rate >= 100)
                throw new ArgumentException("Interest rate must be between 0 and 100.");

            StartDate = startDate;
            RuleId = ruleId;
            Rate = rate;
        }
    }
}
