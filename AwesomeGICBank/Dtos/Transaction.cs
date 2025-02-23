using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBank.Dtos
{
    public class Transaction
    {
        public DateTime Date { get; }
        public string TxnId { get; }
        public TransactionType Type { get; init; } // "D", "W", "I"
        public decimal Amount { get; }

        public Transaction(DateTime date, string txnId, TransactionType type, decimal amount)
        {
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Transaction date cannot be in the future.", nameof(date));

            if (!Enum.IsDefined(typeof(TransactionType), type))
                throw new ArgumentException($"Invalid transaction type: {type}.", nameof(type));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

            Date = date;
            TxnId = txnId;
            Type = type;
            Amount = amount;
        }
    }

}
