using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.enums
{
    public enum WebSocketUpdateTypes
    {
        ConnectionStatus,
        ConnectionStatusError,
        EndpointDataReceived,
        EndpointStatus,
        EndpointStatusError
    }
}
