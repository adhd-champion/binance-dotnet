using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.enums
{
    public enum OrderStatuses
    {
        NEW,
        PARTIALLY_FILLED,
        FILLED,
        CANCELLED,
        PENDING_CANCEL,
        REJECTED,
        EXPIRED
    }
}
