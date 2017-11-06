
![alt text](https://raw.githubusercontent.com/adhd-champion/binance-dotnet/master/logo.png "binance-dotnet")
# binance-dotnet
Binance Dotnet is a C# .Net class library to assist in utilizing the Binance Web API.  For more documentation on the Binance API, visit https://www.binance.com/restapipub.html


## Using binance-dotnet
To use the binance-dotnet library, add the binance-dotnet project to your solution and add it as a reference into the desired project.

### Using directives  
To reference the full binance_dotnet library in your class, use the following directives.
```csharp
using binance_dotnet;
using binance_dotnet.enums;
using binance_dotnet.objects;
```

### Declare new object
This will create a new instance of the Binance class.  
```csharp
string APIKey="****************************"; // Your Binance API Key
string APISecret="*************************"; // Your Binance Secret
Binance binance = new Binance(APIKey, APISecret);
```
You may also instantiate without your key/secret, but you will only be able to access public endpoints.
```csharp
Binance binance = new Binance();
```

### Using Binance object endpoint methods
Each of the endpoint methods in the api object are asynchronous / awaitable methods.
They each return a custom object based on the response it will receive from the api call.  
If you want to access the raw json response, you may do so by referencing the '.raw' property of any response.
```csharp
public async Task<string> TestConnectivity()
{
    var response = await Binance.Ping();
    return response.raw;
}
```
To check if an error has occurred, use the 'hasErrors' property in combination with the 'code' and 'msg' properties.
```csharp
public async Task<string> CheckServerTime()
{
    var result = await Binance.Time();
    if (result.hasErrors)
        throw new Exception(result.code + ":  " + result.msg);
    else
        return result.serverTime.ToString();
}
```

## API Endpoint Examples

### Getting latest price of a symbol
```csharp
public async Task<string> GetLatestPrice()
{
    string symbol = "BNBBTC";
	
    var response = await Binance.Ticker_24Hr(symbol);
    return latestPrice.lastPrice;
}
```

### Getting the order book for a symbol
```csharp
public async void PrintOrderBook()
{
    string symbol = "BNBBTC";
	
    var result = await Binance.Depth(symbol);
    Console.WriteLine(String.Format("| {0,-29} | {0,-29} |", "Asks", "Bids"));
    Console.WriteLine(String.Format("| {0,-16} | {1,-10} | {1,-10} | {0,-16} |", "Qty", "Price", "Price", "Qty"));
    for (int a = 0; a < result.asks.Count(); a++)
        Console.WriteLine(String.Format("| {0,16} | {1,10} | {1,10} | {0,16} |", result.asks[a][1], result.asks[a][0], result.bids[a][0], result.bids[a][1]));
}
```

### Placing a LIMIT order
```csharp
public async Task<int> PlaceOrder_LIMIT()
{
    string symbol = "LTCBTC";
    double quantity = 1;
    double price = 0.001;     
	
    var result = await Binance.BuyLimit(symbol, quantity, price);
    return result.orderId; 
}
```


### Placing a MARKET order
```csharp
public async Task<int> PlaceOrder_MARKET()
{
    string symbol = "LTCBTC";
    double quantity = 1;
	
    var result = await Binance.BuyMarket(symbol, quantity);
    return result.orderId;
}
```

### Placing a custom order
```csharp
public async Task<int> PlaceCustomOrder()
{
    string symbol = "LTCBTC";
    enums.OrderSides side = enums.OrderSides.SELL;
    enums.OrderTypes type = enums.OrderTypes.LIMIT;
    double quantity = 1;
    double price = 1;
    enums.InForceTimes timeInForce = enums.InForceTimes.GTC;
    string newClientOrderID = "customid1111111";
	
    var result = await Binance.Order(symbol, side, type, quantity, price, timeInForce, newClientOrderID);
    return result.orderId;
}
```

### Checking an order’s status
```csharp
public async Task<string> CheckOrderStatus_byOrderID()
{
    string symbol = "LTCBTC";
    long orderId = 123456;

    var result = await Binance.Get_Order(symbol, orderId);
    return result.status;
}
public async Task<string> CheckOrderStatus_byClientOrderID()
{
    string symbol = "LTCBTC";
    string clientOrderId = "customid1111111";
	
    var result = await Binance.Get_Order(symbol, clientOrderId);
    return result.status;
}
```

### Cancelling an order
```csharp
public async Task<string> CancelOrder_byOrderID()
{
    string symbol = "LTCBTC";
    long orderId = 123456;

    var result = await Binance.Delete_Order(symbol, orderId);
    return result.raw;
}
public async Task<string> CancelOrder_byClientOrderID()
{
    string symbol = "LTCBTC";
    string clientOrderId = "customid1111111";

    var result = await Binance.Delete_Order(symbol, clientOrderId);
    return result.raw;
}
```

### Getting list of open orders
```csharp
public async void PrintOpenOrders()
{
    string symbol = "LTCBTC";
    var result = await Binance.Open_Orders(symbol);

    Console.WriteLine(String.Format("| {0,-7} | {1,-5} | {2,-10} | {3,-12} | {4,-12} |", "OrderID", "Asset", "Price", "Orig Qty", "Exec Qty"));
    foreach (var order in result.orders)
        Console.WriteLine(String.Format("| {0,7} | {1,5} | {2,10} | {3,12} | {4,12} |", order.orderId, order.symbol, order.price, order.origQty, order.executedQty));
}
```

### Getting list of current position
```csharp
public async void PrintCurrentPositions()
{
    var result = await Binance.Account();

    Console.WriteLine(String.Format("| {0,-5} | {1,-16} | {2,-16} |", "Asset", "Free", "Locked"));
    foreach(var balance in result.balances)
        Console.WriteLine(String.Format("| {0,5} | {1,16} | {2,16} |", balance.asset, balance.free, balance.locked));
}
```

## Websockets API
There are two primary pieces to accessing the websockets api:
1. User Data Stream - This is the connection initiated by the client that enables communication to the websocket endpoints.  It has three sub-methods:
   1. Connect - Initiate a tunnel between the client and server.
   1. Keep Alive - An echo request that must be sent at a regular interval to keep the server from closing the tunnel.
   1. Disconnect - Cleanly closes the tunnel.
1. Websocket Endpoints - These are the actual endpoints that are websocket-enabled.  Current version includes the following endpoints:
   1. Depth - Buy/sell wall updates for a symbol
   1. Kline - Candlestick updates for a symbol
   1. Trades - Trade updates for a symbol
   1. User Data - Updates to you're account

### Things to keep in mind...
There are a few things to keep in mind when using the websockets API:
* User data stream keep alive interval must be smaller than 60 seconds or the connection will timeout.
* The listen key received in a user data stream will stop working after an hour or so and therefore it has to be reset.
* When resetting the listen key, all but the User Data endpoint can remain running.  You will have to temporarily close the user data stream and reopen using your new listen key.

### Examples
To use the websockets implementation in binance-dotnet, you will need to utilize the 'WebSocketUpdateReceived' event as shown below:
```csharp
static void Main()
{
    Binance.WebSocketUpdateReceived += WebSocketsUpdateReceived;
}
private void WebSocketsUpdateReceived(Object sender, WebSocketUpdateReceivedEventArgs e)
{
    if(e.UpdateType == WebSocketUpdateTypes.EndpointDataReceived)
    {
        string message = string.Format("{0,-16} {1,-20} | {3,-30}", "[WebSocketUpdate]", e.Timestamp.ToString(), e.Message);
        Console.WriteLine(message);
    }
}
```
To change your keep-alive and connection reset intervals, set the value of the 'UDS_KeepAliveInterval' and 'UDS_ResetInterval' properties in miliseconds.
```csharp
public void ConfigureConnectionIntervals()
{
    Binance.UDS_KeepAliveInterval = 30000;//<--- 30 Seconds
    Binance.UDS_ResetInterval = 3000000;//<----- 50 Minutes
}
```

When you execute a websocket method (any method prefaced with 'WS_'), it will automatically start a user data stream for you if one isn't already open.  If you prefer, you can open a websocket stream manually using the 'OpenUserDataStream' method:
```csharp
public void OpenUserDataStream()
{
    Binance.OpenUserDataStream();
}
```
In order to close user data stream and all active websocket connections, use the 'CloseUserDataStream' method:
```csharp
public void CloseUserDataStream()
{
    Binance.CloseUserDataStream();
}
```

#### Getting order book via websockets
```csharp
public void PrintOrderBook_WebSocket()
{
    string symbol="ltcbtc";
    Binance.WS_Depth(symbol);
}
```

#### Closing websocket endpoints
To close particular socket endpoint connections, you have a couple of options:
```csharp
public void CloseSocket_Option1()
{
	// Closes all "Depth" sockets
    var activeSockets = Binance.GetActiveSockets();
	foreach(var socket in activeSockets)
	{
		if(socket.Name == WebSocketEndpoints.Depth)
			Binance.CloseSocket(socket);
	}
    Binance.WS_Depth(symbol);
}
```
```csharp
public void CloseSocket_Option2()
{
	// Closes "Depth" socket for only "ltcbtc"
    string symbol="ltcbtc";
    Binance.Close_WS_Depth(symbol);
}
```
If you would like to close all active sockets, but keep your user data stream alive, use the 'CloseActiveSockets' method:
```csharp
public void CloseActiveSockets()
{
    Binance.CloseActiveSockets();
}
```
