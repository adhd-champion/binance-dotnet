using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.enums
{
    public enum WithdrawHistoryStatuses
    {
        EmailSent = 0,
        Cancelled = 1,
        AwaitingApproval=2,
        Rejected=3, 
        Processing=4,
        Failure=5,
        Completed=6
    }
}
