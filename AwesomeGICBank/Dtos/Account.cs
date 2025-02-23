using AwesomeGICBank.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AwesomeGICBank.Dtos
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException(string message) : base(message) { }
    }
 
    public class Account
    {
        public string AccountId { get; } // Unique identifier for the account
        public decimal Balance { get; private set; } // Current balance of the account
        public List<Transaction> Transactions { get; } // List of transactions associated with this account

        public Account(string accountId)
        {
            AccountId = accountId;
            Balance = 0;
            Transactions = new List<Transaction>();
        }

    public void AddTransaction(Transaction transaction)
        {
            // Adding new transaction to the list (unsorted)
            Transactions.Add(transaction);

            // Sorting transactions chronologically to ensure correct balance validation
            Transactions.Sort((x, y) => x.Date.CompareTo(y.Date));

            RecalculateBalance(transaction);

        }

        private void RecalculateBalance(Transaction transaction)
        {
            decimal newBalance = 0;

            // Validating balance at each step to ensure no overdrafts
            foreach (var txn in Transactions)
            {
                newBalance += txn.Type == TransactionType.Withdrawal ? -txn.Amount : txn.Amount;

                // Checking if transaction results in a negative balance
                if (newBalance < 0)
                {
                    Transactions.Remove(transaction);
                    throw new InsufficientFundsException("Transaction declined: Insufficient funds.");
                }
            }

            Balance = newBalance; // Updating final account balance
        }

        public decimal CalculateMonthlyInterest(List<InterestRule> interestRules, string yearMonth)
        {
            // Extract year and month from input
            var year = int.Parse(yearMonth.Substring(0, 4));
            var month = int.Parse(yearMonth.Substring(4, 2));
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var dailyBalances = new Dictionary<DateTime, decimal>(); // Storing daily balances

            decimal runningBalance = 0;
            foreach (var txn in Transactions.OrderBy(t => t.Date))
            {
                if (txn.Date < startDate)
                    runningBalance += txn.Type == TransactionType.Withdrawal ? -txn.Amount : txn.Amount;
                if (txn.Date >= startDate && txn.Date <= endDate)
                    dailyBalances[txn.Date] = runningBalance += txn.Type == TransactionType.Withdrawal ? -txn.Amount : txn.Amount;
            }

            // Fill in missing days with last known balance
            decimal lastBalance = dailyBalances.FirstOrDefault().Value;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!dailyBalances.ContainsKey(date))
                    dailyBalances[date] = lastBalance;
                else
                    lastBalance = dailyBalances[date];
            }

            // Applying interest rules based on dates
            decimal totalInterest = 0;
            decimal currentRate = 0;
            DateTime lastRuleDate = startDate;

            foreach (var rule in interestRules.OrderBy(r => r.StartDate))
            {
                if (rule.StartDate > endDate) break; // Ignoring rules outside the date range
                if (rule.StartDate >= startDate)
                {
                    int days = (rule.StartDate - lastRuleDate).Days;
                    totalInterest += (dailyBalances[lastRuleDate] * currentRate * days) / 100;
                    lastRuleDate = rule.StartDate;
                }
                currentRate = rule.Rate; // Updating to the latest interest rate
            }

            // Apply interest for remaining days of the month
            int remainingDays = (endDate - lastRuleDate).Days + 1;
            totalInterest += (dailyBalances[lastRuleDate] * currentRate * remainingDays) / 100;

            return Math.Round(totalInterest / 365, 2); // Converting to annualized interest
        }

        public void ApplyMonthlyInterest(List<InterestRule> interestRules, string yearMonth)
        {
            // Calculating the interest for the month
            decimal interest = CalculateMonthlyInterest(interestRules, yearMonth);
            if (interest > 0)
            {
                var lastDay = new DateTime(int.Parse(yearMonth.Substring(0, 4)),
                                          int.Parse(yearMonth.Substring(4, 2)),
                                          DateTime.DaysInMonth(int.Parse(yearMonth.Substring(0, 4)), int.Parse(yearMonth.Substring(4, 2))));

                // Creating a transaction for interest earned
                var interestTransaction = new Transaction(lastDay, "", TransactionType.Interest, interest);
                AddTransaction(interestTransaction);
            }
        }
    }
}
