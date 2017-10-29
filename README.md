# binance-dotnet
Binance Dotnet is a C# .Net class library to assist in utilizing the Binance Web API.
It currently only implements calls to the public API endpoints of the initial release.  It does not, as of yet, include wrappers for the following:
* Withdraw API
* Websockets API

These will be added as I have time to add them.
For more documentation on the Binance API, visit https://www.binance.com/restapipub.html

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
public async Task<string> CheckServerTime()
{
    var result = await Binance.Time();
    if (result.hasErrors)
        throw new Exception(result.code + ":  " + result.msg);
    else
        return result.serverTime.ToString();
}

## Examples

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

    Console.WriteLine(String.Format("| {0,-7} | {1,-5} | {2,-10} | {3,-12} | {4,-12} |", "OrderID", "Asser", "Price", "Orig Qty", "Exec Qty"));
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
