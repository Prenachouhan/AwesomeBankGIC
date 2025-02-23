using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBank.Dtos
{
    public enum TransactionType
    {
        Withdrawal = 'W',
        Deposit = 'D',
        Interest = 'I'
    }
}
