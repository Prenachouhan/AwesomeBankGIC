using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using AwesomeGICBank;
using AwesomeGICBank.Dtos;
using System.Globalization;

namespace AwesomeGICBank.Tests
{
    public class BankTests
    {
        private Account testAccount;
        private List<InterestRule> interestRules;

        public BankTests()
        {
            testAccount = new Account("Test123");
            interestRules = new List<InterestRule>();
        }

        // 1. Validating Date Format
        [Fact]
        public void InvalidDateFormat_ShouldThrowException()
        {
            string invalidDate = "20251331"; // Invalid date format

            Assert.Throws<FormatException>(() =>
                DateTime.ParseExact(invalidDate, "yyyyMMdd", null)
            );
        }

        // 2. Validating Transaction Type
        [Fact]
        public void InvalidTransactionType_ShouldThrowException()
        {
            // Assigning an invalid integer value (assuming TransactionType is an enum)
            TransactionType invalidType = (TransactionType)999; // Out of enum range 

            Assert.Throws<ArgumentException>(() =>
                new Transaction(DateTime.UtcNow, "TXN009", invalidType, 500m)
            );
        }

        // 3. Validating Transaction Amount
        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void InvalidTransactionAmount_ShouldThrowException(decimal amount)
        {
            Assert.Throws<ArgumentException>(() =>
                new Transaction(DateTime.UtcNow, "TXN010", TransactionType.Deposit, amount)
            );
        }

        // 4. Validating Interest Rate
        [Theory]
        [InlineData(-5)]
        [InlineData(105)]
        public void InvalidInterestRate_ShouldThrowException(decimal rate)
        {
            DateTime date = DateTime.UtcNow;

            Assert.Throws<ArgumentException>(() =>
                new InterestRule(date, "IR003", rate)
            );
        }

        // 5. Generating Statement for Non-Existent Account
        [Fact]
        public void GenerateStatementForNonExistentAccount_ShouldReturnError()
        {
            string nonExistentAccount = "XYZ999";

            Assert.Throws<KeyNotFoundException>(() =>
                Program.PrintAccountStatement(nonExistentAccount)
            );
        }

        // 6. Multiple Transactions on the Same Date
        [Fact]
        public void MultipleTransactionsSameDate_ShouldGenerateUniqueIds()
        {
            testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN012", TransactionType.Deposit, 1000m));
            testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN013", TransactionType.Withdrawal, 500m));

            var transactions = testAccount.Transactions;

            Assert.Equal(2, transactions.Count);
            Assert.NotEqual(transactions[0].TxnId, transactions[1].TxnId);
        }

        // 7. Deposit Transaction
        [Fact]
        public void DepositTransaction_ShouldIncreaseBalance()
        {
            decimal initialBalance = testAccount.Balance;
            decimal depositAmount = 1000m;

            testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN001", TransactionType.Deposit, depositAmount));

            Assert.Equal(initialBalance + depositAmount, testAccount.Balance);
        }

        // 8. Withdrawal Transaction
        [Fact]
        public void WithdrawalTransaction_ShouldDecreaseBalance()
        {
            testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN002", TransactionType.Deposit, 1000m)); // Deposit first
            decimal initialBalance = testAccount.Balance;
            decimal withdrawalAmount = 500m;

            testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN003", TransactionType.Withdrawal, withdrawalAmount));

            Assert.Equal(initialBalance - withdrawalAmount, testAccount.Balance);
        }

        // 9. Withdrawal More Than Balance
        [Fact]
        public void WithdrawalMoreThanBalance_ShouldThrowException()
        {
            decimal withdrawalAmount = 500m;

            Assert.Throws<InsufficientFundsException>(() =>
                testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN004", TransactionType.Withdrawal, withdrawalAmount))
            );
        }

        // 10. Adding Interest Rule
        [Fact]
        public void AddingInterestRule_ShouldIncreaseRuleCount()
        {
            int initialRuleCount = interestRules.Count;
            InterestRule rule = new InterestRule(DateTime.UtcNow, "IR001", 5m);

            interestRules.Add(rule);

            Assert.Equal(initialRuleCount + 1, interestRules.Count);
        }

        // 11. Applying Interest to Account
        [Fact]
        public void ApplyingInterest_ShouldIncreaseBalance()
        {
            DateTime transactionDate = DateTime.ParseExact("20250101", "yyyyMMdd", CultureInfo.InvariantCulture);

            // Add transaction to the test account
            testAccount.AddTransaction(new Transaction(transactionDate, "TXN005", TransactionType.Deposit, 1000m));

            // Create an interest rule
            InterestRule rule = new InterestRule(transactionDate, "IR002", 5m);
            interestRules.Add(rule);

            testAccount.ApplyMonthlyInterest(interestRules, transactionDate.ToString("yyyyMM"));

            decimal expectedBalance = Math.Round(1000m + (1000m * 5 / 100)/365 * 31, 2); // Interest calculation
            Assert.Equal(expectedBalance, testAccount.Balance);
        }

        // 12. Account Statement Validation
        [Fact]
        public void AccountStatement_ShouldShowCorrectTransactions()
        {
            testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN006", TransactionType.Deposit, 2000m));
            testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN007", TransactionType.Withdrawal, 500m));

            var transactions = testAccount.Transactions;

            Assert.Equal(2, transactions.Count);
            Assert.Equal("TXN006", transactions[0].TxnId);
            Assert.Equal("TXN007", transactions[1].TxnId);
        }

        // 13. Loading Data from File (Uncomment if method is implemented in Program)
        //[Fact]
        //public void LoadData_ShouldPopulateAccounts()
        //{
        //    Program.LoadData();
        //    Assert.NotEmpty(Program.accounts);
        //}

        // 14. Saving Data to File (Uncomment if method is implemented in Program)
        //[Fact]
        //public void SaveData_ShouldWriteToFile()
        //{
        //    testAccount.AddTransaction(new Transaction(DateTime.UtcNow, "TXN008", "D", 3000m));
        //    Program.accounts[testAccount.AccountId] = testAccount;
        //
        //    Program.SaveData();
        //
        //    Assert.True(File.Exists("transactions.txt"));
        //}
    }
}
