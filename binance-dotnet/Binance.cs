using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using binance_dotnet.enums;
using binance_dotnet.objects;
using System.Security.Cryptography;
using System.Web;
using System.Net;
using System.IO;
using System.Net.WebSockets;
using System.Timers;

namespace binance_dotnet
{
    public class Binance
    {
        
        public Binance(string apiKey = null, string secretKey = null)
        {
            ReceiveWindow = DEFAULT_RecieveWindow;
            UseReceiveWindow = DEFAULT_UseReceiveWindow;
            TimeStampSource = DEFAULT_TimeStampSources;
            APIKey = apiKey;
            APISecret = secretKey;

            
        }

        #region Default Values
        private const string APIBaseURL = @"https://www.binance.com/api/";
        private const string WAPIBaseURL = @"https://www.binance.com/wapi/";
        private const string WebSocketBaseURL = @"wss://stream.binance.com:9443/ws/";
        private const long DEFAULT_RecieveWindow = 6000000;
        private const bool DEFAULT_UseReceiveWindow = true;
        private const TimeStampSources DEFAULT_TimeStampSources = TimeStampSources.APIServer;
        #endregion

        #region Properties

        public TimeStampSources TimeStampSource { get; set; }
        public long ReceiveWindow { get; set; }
        public bool UseReceiveWindow { get; set; }
        public string APIKey { private get; set; }
        public string APISecret { private get; set; }
        #endregion

        #region Helper Functions

        /// <summary>
        /// If your API key / secret is stored in a json file, this function will import them.
        /// That json file should be formated the following way:
        /// { "key":"", "secret":"" }
        /// </summary>
        /// <param name="filename">Name of the json file containing key/secret values.</param>
        public void ImportKeyFile(string filename)
        {
            if (File.Exists(filename))
            {
                string fileText = File.ReadAllText(filename);
                Keys keys = APIResponseHandler.DeserializeKeyFile(fileText);
                this.APIKey = keys.key;
                this.APISecret = keys.secret;
            }
            else
                throw new FileNotFoundException("Key file could not be found.");
        }

        private static string FormatSymbol(string symbol)
        {
            if (!string.IsNullOrEmpty(symbol))
                return symbol.ToUpper();
            else
                return null;
        }
        private static string FormatSymbol(string baseAsset, string quoteAsset)
        {
            return baseAsset.ToUpper() + quoteAsset.ToUpper();
        }

        private Dictionary<string, object> CreateParamDict(params object[] parameterList)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int a = 0; a < parameterList.Count(); a += 2)
                if(parameterList.Count() >= a+2)
                    if(parameterList[a + 1] != null)
                        dict.Add(parameterList[a].ToString(), parameterList[a + 1]);
            return dict;
        }

        private async Task<string> GetTimestampForRequest()
        {
            switch(this.TimeStampSource)
            {
                case TimeStampSources.APIServer:
                    Response_Time time = await Time();
                    return time.serverTime.ToString();
                case TimeStampSources.Local:
                    return ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds).ToString();
            }
            return null;
        }

        #endregion

        #region API Requests

        #region Requests

        #region API
        private async Task<string> PUBLIC_Request_API(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET)
        {
            return await PUBLIC_Request(APIBaseURL + url, parameters, verb);
        }
        private async Task<string> APIKEY_Request_API(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET)
        {
            return await APIKEY_Request(APIBaseURL + url, parameters, verb);
        }
        private async Task<string> SIGNED_Request_API(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET, long? receiveWindow = null)
        {
            return await SIGNED_Request(APIBaseURL + url, parameters, verb, receiveWindow);
        }
        #endregion

        #region WAPI
        private async Task<string> PUBLIC_Request_WAPI(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET)
        {
            return await PUBLIC_Request(WAPIBaseURL + url, parameters, verb);
        }
        private async Task<string> APIKEY_Requestt_WAPI(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET)
        {
            return await APIKEY_Request(WAPIBaseURL + url, parameters, verb);
        }
        private async Task<string> SIGNED_Requestt_WAPI(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.POST, long? receiveWindow = null)
        {
            return await SIGNED_Request(WAPIBaseURL + url, parameters, verb, receiveWindow);
        }
        #endregion

        private async Task<string> PUBLIC_Request(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET)
        {
            return await ExecuteRequest(url, parameters, verb, EndpointSecurityTypes.NONE);
        }
        private async Task<string> APIKEY_Request(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET)
        {
            if (string.IsNullOrEmpty(this.APIKey))
                throw new Exception("Null APIKey and/or APISecret encountered.  Cannot execute 'apikey' requests.");
            return await ExecuteRequest(url, parameters, verb, EndpointSecurityTypes.API_KEY);
        }
        private async Task<string> SIGNED_Request(string url, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET, long? receiveWindow = null)
        {
            if (string.IsNullOrEmpty(this.APIKey)
                || string.IsNullOrEmpty(this.APISecret))
                throw new Exception("Null APIKey and/or APISecret encountered.  Cannot execute 'signed' requests.");
            return await ExecuteRequest(url, parameters, verb, EndpointSecurityTypes.SIGNED, receiveWindow);
        }

        #endregion

        #region [FN] ExecuteRequest

        private async Task<string> ExecuteRequest(string endpointUrl, Dictionary<string, object> parameters = null, HTTPVerbs verb = HTTPVerbs.GET, EndpointSecurityTypes securityType = EndpointSecurityTypes.NONE, long? receiveWindow = null)
        {
            string response = null;

            HttpWebRequest request = null;
            WebResponse resp = null;

            try
            {
                // Create URI
                var builder = new UriBuilder(endpointUrl);

                // Parse parameters
                var qs = HttpUtility.ParseQueryString(string.Empty);
                if (parameters != null && parameters.Count > 0)
                    foreach (string key in parameters.Keys)
                    {
                        if (!string.IsNullOrEmpty(parameters[key].ToString()))
                            qs[key] = parameters[key].ToString();
                    }
                
                // Sign request
                if (securityType == EndpointSecurityTypes.SIGNED)
                {
                    // Add receiveWindow & timestamp
                    if(this.UseReceiveWindow)
                    {
                        if (receiveWindow.HasValue)
                            qs["recvWindow"] = receiveWindow.ToString();
                        else
                            qs["recvWindow"] = this.ReceiveWindow.ToString();
                    }
                    qs["timestamp"] = await GetTimestampForRequest();

                    // Create signature
                    string signature = string.Empty;
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    byte[] secret_bytes = ascii.GetBytes(this.APISecret);
                    HMACSHA256 sha256 = new HMACSHA256(secret_bytes);
                    byte[] qs_Bytes = ascii.GetBytes(qs.ToString());
                    byte[] hash_bytes = sha256.ComputeHash(qs_Bytes);
                    for (int i = 0; i < hash_bytes.Length; i++)
                        signature += hash_bytes[i].ToString("X2");

                    // Append signature to QueryString
                    qs["signature"] = signature;
                }
                builder.Query = qs.ToString();

                // Create web request object
                request = (HttpWebRequest)WebRequest.Create(builder.Uri);
                request.Method = verb.ToString();

                // Add content type header for non-GET requests
                if (verb == HTTPVerbs.POST
                    || verb == HTTPVerbs.PUT
                    || verb == HTTPVerbs.DELETE)
                    request.Headers.Add("ContentType", "application/x-www-form-urlencoded");

                // Add APIKey to header for API-KEY & SIGNED security types
                if (securityType == EndpointSecurityTypes.API_KEY
                    || securityType == EndpointSecurityTypes.SIGNED)
                    request.Headers.Add("X-MBX-APIKEY", this.APIKey);

                // Send request
                resp = await request.GetResponseAsync();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response = sr.ReadToEnd();
            }
            catch (WebException ex)
            { // Gathers Binance-specific errors
                if (ex.Response != null)
                    using (var errorResponse = (HttpWebResponse)ex.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            response = reader.ReadToEnd();
                        }
                    }
            }
            catch (Exception ex)
            {
                response = "{ \"msg\": \"" + ex.Message + "\", \"code\": \"" + ex.HResult + "\" }";
            }

            return response;
        }

        #endregion

        #endregion

        #region Endpoints

        #region API

        #region ...General

        /// <summary>
        /// Test connectivity to the Rest API.
        /// </summary>
        public async Task<Response_Ping> Ping()
        {
            string url = "v1/ping";
            string response = await PUBLIC_Request_API(url);
            return APIResponseHandler.CreateAPIResponseObject<Response_Ping>(response);
        }

        /// <summary>
        /// Test connectivity to the Rest API and get the current server time.
        /// </summary>
        public async Task<Response_Time> Time()
        {
            string url = "v1/time";
            string response = await PUBLIC_Request_API(url);
            return APIResponseHandler.CreateAPIResponseObject<Response_Time>(response);
        }

        #endregion

        #region ...Market

        /// <summary>
        /// Order book
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="limit">[Optional] Default 100; max 100.</param>
        public async Task<Response_Depth> Depth(string symbol, int? limit = null)
        {
            string url = "v1/depth";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "limit", limit);
            string response = await PUBLIC_Request_API(url, paramList);
            return APIResponseHandler.CreateAPIResponseObject<Response_Depth>(response);
        }

        /// <summary>
        /// Get compressed, aggregate trades. Trades that fill at the time, from the same order, with the same price will have the quantity aggregated.
        /// *If both startTime and endTime are sent, limit should not be sent AND the distance between startTime and endTime must be less than 24 hours.
        /// *If frondId, startTime, and endTime are not sent, the most recent aggregate trades will be returned.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="fromId">[Optional] ID to get aggregate trades from INCLUSIVE.</param>
        /// <param name="startTime">[Optional] Timestamp in ms to get aggregate trades from INCLUSIVE.</param>
        /// <param name="endTime">[Optional] Timestamp in ms to get aggregate trades until INCLUSIVE.</param>
        /// <param name="limit">[Optional] Default 500; max 500.</param>
        public async Task<Response_AggTrades> AggTrades(string symbol, long? fromId = null, long? startTime = null, long? endTime = null, int? limit = null)
        {
            string url = "v1/aggTrades";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "fromId", fromId,
                                            "startTime", startTime,
                                            "endTime", endTime,
                                            "limit", limit);
            string response = await PUBLIC_Request_API(url, paramList);
            return APIResponseHandler.CreateAPIResponseObject<Response_AggTrades>(response, "aggtrades");
        }

        /// <summary>
        /// Kline/candlestick bars for a symbol. Klines are uniquely identified by their open time.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="intervals">[Optional]</param>
        /// <param name="startTime">[Optional]</param>
        /// <param name="endTime">[Optional]</param>
        /// <param name="limit">[Optional] Default 500; max 500.</param>
        public async Task<Response_Klines> Klines(string symbol, KlineIntervals interval = KlineIntervals._30m, long? startTime = null, long? endTime = null, int? limit = null)
        {
            string url = "v1/klines";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "interval", interval.ToString().Substring(1),
                                            "startTime", startTime,
                                            "endTime", endTime,
                                            "limit", limit);
            string response = await PUBLIC_Request_API(url, paramList);
            return APIResponseHandler.CreateAPIResponseObject<Response_Klines>(response, "klines");
        }

        /// <summary>
        /// 24 hour price change statistics.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        public async Task<Response_Ticker_24hr> Ticker_24Hr(string symbol)
        {
            string url = "v1/ticker/24hr";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol));
            string response = await PUBLIC_Request_API(url, paramList);
            return APIResponseHandler.CreateAPIResponseObject<Response_Ticker_24hr>(response);
        }

        /// <summary>
        /// Latest price for all symbols.
        /// </summary>
        public async Task<Response_AllPrices> Ticker_AllPrices()
        {
            string url = "v1/ticker/allPrices";
            string response = await PUBLIC_Request_API(url);
            return APIResponseHandler.CreateAPIResponseObject<Response_AllPrices>(response, "prices");
        }

        /// <summary>
        /// Best price/qty on the order book for all symbols.
        /// </summary>
        public async Task<Response_AllBookPrices> Ticker_AllBookTickers()
        {
            string url = "v1/ticker/allBookTickers";
            string response = await PUBLIC_Request_API(url);
            return APIResponseHandler.CreateAPIResponseObject<Response_AllBookPrices>(response, "bookprices");
        }

        #endregion

        #region ...Account

        #region Orders - New

        /// <summary>
        /// Send in a new order
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="side">[Required]</param>
        /// <param name="type">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="price">[Required]</param>
        /// <param name="timeInForce">[Optional]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        /// <param name="stopPrice">[Optional] Used with STOP orders</param>
        /// <param name="icebergQty">[Optional] Used with icebergOrders</param>
        public async Task<Response_NewOrder> Order(string symbol, OrderSides side, OrderTypes type, double quantity, double? price, InForceTimes? timeInForce = null, string newClientOrderId = null, double? stopPrice = null, double? icebergQty = null)
        {
            string url = "v3/order";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "side", side,
                                            "type", type,
                                            "quantity", quantity,
                                            "price", price,
                                            "timeInForce", timeInForce ?? null,
                                            "newClientOrderId", newClientOrderId,
                                            "stopPrice", stopPrice ?? null,
                                            "icebergQty", icebergQty ?? null);
            string response = await SIGNED_Request_API(url, paramList, HTTPVerbs.POST);
            return APIResponseHandler.CreateAPIResponseObject<Response_NewOrder>(response);
        }

        /// <summary>
        /// Send in a new limit buy order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="price">[Required]</param>
        /// <param name="timeInForce">[Optional]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> BuyLimit(string symbol, double quantity, double price, InForceTimes timeInForce = InForceTimes.GTC, string newClientOrderId = null)
        {
            return await Order(symbol, OrderSides.BUY, OrderTypes.LIMIT, quantity, price, timeInForce, newClientOrderId);
        }
        /// <summary>
        /// Send in a new market buy order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> BuyMarket(string symbol, double quantity, string newClientOrderId = null)
        {
            return await Order(symbol, OrderSides.BUY, OrderTypes.LIMIT, quantity, null, null, newClientOrderId);
        }

        /// <summary>
        /// Send in a new limit sell order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="price">[Required]</param>
        /// <param name="timeInForce">[Optional]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> SellLimit(string symbol, double quantity, double price, InForceTimes timeInForce = InForceTimes.GTC, string newClientOrderId = null)
        {
            return await Order(symbol, OrderSides.SELL, OrderTypes.LIMIT, quantity, price, timeInForce, newClientOrderId);
        }
        /// <summary>
        /// Send in a new market sell order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> SellMarket(string symbol, double quantity, string newClientOrderId = null)
        {
            return await Order(symbol, OrderSides.SELL, OrderTypes.LIMIT, quantity, null, null, newClientOrderId);
        }

        #endregion

        #region Orders - New (TEST)

        /// <summary>
        /// Test new order creation and signature/recvWindow long. Creates and validates a new order but does not send it into the matching engine.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="side">[Required]</param>
        /// <param name="type">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="price">[Required]</param>
        /// <param name="timeInForce">[Optional]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        /// <param name="stopPrice">[Optional] Used with STOP orders</param>
        /// <param name="icebergQty">[Optional] Used with icebergOrders</param>
        public async Task<Response_NewOrder> Order_Test(string symbol, OrderSides side, OrderTypes type, double quantity, double? price, InForceTimes? timeInForce = null, string newClientOrderId = null, double? stopPrice = null, double? icebergQty = null)
        {
            string url = "v3/order/test";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "side", side,
                                            "type", type,
                                            "quantity", quantity,
                                            "price", price,
                                            "timeInForce", timeInForce??null,
                                            "newClientOrderId", newClientOrderId,
                                            "stopPrice", stopPrice ?? null,
                                            "icebergQty", icebergQty ?? null);
            string response = await SIGNED_Request_API(url, paramList, HTTPVerbs.POST);
            return APIResponseHandler.CreateAPIResponseObject<Response_NewOrder>(response);
        }

        /// <summary>
        /// Send in a new TEST limit buy order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="price">[Required]</param>
        /// <param name="timeInForce">[Optional]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> BuyLimitTEST(string symbol, double quantity, double price, InForceTimes timeInForce = InForceTimes.GTC, string newClientOrderId = null)
        {
            return await Order_Test(symbol, OrderSides.BUY, OrderTypes.LIMIT, quantity, price, timeInForce, newClientOrderId);
        }
        /// <summary>
        /// Send in a new TEST market buy order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> BuyMarketTEST(string symbol, double quantity, string newClientOrderId = null)
        {
            return await Order_Test(symbol, OrderSides.BUY, OrderTypes.MARKET, quantity, null, null, newClientOrderId);
        }

        /// <summary>
        /// Send in a new TEST limit sell order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="price">[Required]</param>
        /// <param name="timeInForce">[Optional]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> SellLimitTEST(string symbol, double quantity, double price, InForceTimes timeInForce = InForceTimes.GTC, string newClientOrderId = null)
        {
            return await Order_Test(symbol, OrderSides.SELL, OrderTypes.LIMIT, quantity, price, timeInForce, newClientOrderId);
        }
        /// <summary>
        /// Send in a new TEST market sell order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="quantity">[Required]</param>
        /// <param name="newClientOrderId">[Optional] A unique id for the order. Automatically generated by default.</param>
        public async Task<Response_NewOrder> SellMarketTEST(string symbol, double quantity, string newClientOrderId = null)
        {
            return await Order_Test(symbol, OrderSides.SELL, OrderTypes.MARKET, quantity, null, null, newClientOrderId);
        }
        
        #endregion

        #region Orders - Query

        /// <summary>
        /// Check an order's status.
        /// *Either orderId or origClientOrderId must be sent.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="orderId">[Optional]*</param>
        /// <param name="origClientOrderId">[Optional]*</param>
        private async Task<Response_QueryOrder> Get_Order(string symbol, long? orderId = null, string origClientOrderId = null)
        {
            string url = "v3/order";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "orderId", orderId ?? null,
                                            "origClientOrderId", origClientOrderId);
            string response = await SIGNED_Request_API(url, paramList, HTTPVerbs.GET);
            return APIResponseHandler.CreateAPIResponseObject<Response_QueryOrder>(response);
        }
        /// <summary>
        /// Check an order's status.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="orderId">[Required]</param>
        public async Task<Response_QueryOrder> Get_Order(string symbol, long orderId)
        {
            return await Get_Order(symbol, orderId, null);
        }
        /// <summary>
        /// Check an order's status.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="origClientOrderId">[Required]</param>
        public async Task<Response_QueryOrder> Get_Order(string symbol, string origClientOrderId)
        {
            return await Get_Order(symbol, null, origClientOrderId);
        }

        #endregion

        #region Orders - Cancel

        /// <summary>
        /// Cancel an active order.
        /// *Either orderId or origClientOrderId must be sent.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="orderId">[Optional]*</param>
        /// <param name="origClientOrderId">[Optional]*</param>
        /// <param name="newClientOrderId">[Optional] Used to uniquely identify this cancel. Automatically generated by default.</param>
        /// <returns></returns>
        private async Task<Response_CancelOrder> Delete_Order(string symbol, long? orderId = null, string origClientOrderId = null, string newClientOrderId = null)
        {
            string url = "v3/order";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "orderId", orderId ?? null,
                                            "origClientOrderId", origClientOrderId,
                                            "newClientOrderId", newClientOrderId);
            string response = await SIGNED_Request_API(url, paramList, HTTPVerbs.DELETE);
            return APIResponseHandler.CreateAPIResponseObject<Response_CancelOrder>(response);
        }

        /// <summary>
        /// Cancel an active order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="orderId">[Required]</param>
        /// <param name="newClientOrderId">[Optional] Used to uniquely identify this cancel. Automatically generated by default.</param>
        /// <returns></returns>
        public async Task<Response_CancelOrder> Delete_Order(string symbol, long orderId, string newClientOrderId = null)
        {
            return await Delete_Order(symbol, orderId, null, newClientOrderId);
        }

        /// <summary>
        /// Cancel an active order.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="origClientOrderId">[Required]</param>
        /// <param name="newClientOrderId">[Optional] Used to uniquely identify this cancel. Automatically generated by default.</param>
        /// <returns></returns>
        public async Task<Response_CancelOrder> Delete_Order(string symbol, string origClientOrderId, string newClientOrderId = null)
        {
            return await Delete_Order(symbol, null, origClientOrderId, newClientOrderId);
        }

        #endregion

        #region Orders - Bulk

        /// <summary>
        /// Get all open orders on a symbol.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        public async Task<Response_OpenOrders> Open_Orders(string symbol)
        {
            string url = "v3/openOrders";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol));
            string response = await SIGNED_Request_API(url, paramList, HTTPVerbs.GET);
            return APIResponseHandler.CreateAPIResponseObject<Response_OpenOrders>(response, "orders");
        }

        /// <summary>
        /// Get all account orders; active, canceled, or filled.
        /// If orderId is set, it will get orders >= that orderId. Otherwise most recent orders are returned.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="orderId">[Optional]</param>
        /// <param name="limit">[Optional] Default 500; max 500.</param>
        public async Task<Response_AllOrders> All_Orders(string symbol, long? orderId = null, int? limit = null)
        {
            string url = "v3/allOrders";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol));
            string response = await SIGNED_Request_API(url, paramList, HTTPVerbs.GET);
            return APIResponseHandler.CreateAPIResponseObject<Response_AllOrders>(response, "orders");
        }

        #endregion

        /// <summary>
        /// Get current account information.
        /// </summary>
        public async Task<Response_Account> Account()
        {
            string url = "v3/account";
            string response = await SIGNED_Request_API(url);
            return APIResponseHandler.CreateAPIResponseObject<Response_Account>(response);
        }

        /// <summary>
        /// Get trades for a specific account and symbol.
        /// </summary>
        /// <param name="symbol">[Required]</param>
        /// <param name="limit">[Optional] Default 500; max 500.</param>
        /// <param name="fromId">[Optional] TradeId to fetch from. Default gets most recent trades.</param>
        public async Task<Response_MyTrades> My_Trades(string symbol, int? limit = null, long? fromId = null)
        {
            string url = "v3/myTrades";
            var paramList = CreateParamDict("symbol", FormatSymbol(symbol),
                                            "limit", limit ?? null,
                                            "fromId", fromId ?? null);
            string response = await SIGNED_Request_API(url, paramList, HTTPVerbs.GET);
            return APIResponseHandler.CreateAPIResponseObject<Response_MyTrades>(response, "trades");
        }

        #endregion

        #endregion

        #region WAPI

        public async Task<Response_Withdraw> Withdraw(string asset, string address, double amount, string name=null, long? recvWindow = null)
        {
            string url = "v1/withdraw.html";
            var paramList = CreateParamDict("asset", FormatSymbol(asset),
                                            "address", address,
                                            "amount", amount,
                                            "name", name,
                                            "recvWindow", recvWindow ?? null);
            string response = await SIGNED_Requestt_WAPI(url, paramList, HTTPVerbs.POST);
            return APIResponseHandler.CreateAPIResponseObject<Response_Withdraw>(response);
        }

        public async Task<Response_DepositHistory> GetDepositHistory(string asset = null, DepositHistoryStatuses? status = null, long? startTime = null, long? endTime = null, long? recvWindow = null)
        {
            string url = "v1/getDepositHistory.html";

            int? statusCode = null;
            if (status.HasValue)
                statusCode = (int)status.Value;

            var paramList = CreateParamDict("asset", FormatSymbol(asset),
                                            "status", statusCode ?? null,
                                            "startTime", startTime ?? null,
                                            "name", endTime ?? null,
                                            "recvWindow", recvWindow ?? null);
            string response = await SIGNED_Requestt_WAPI(url, paramList, HTTPVerbs.POST);
            return APIResponseHandler.CreateAPIResponseObject<Response_DepositHistory>(response, "depositList");
        }

        public async Task<Response_WithdrawHistory> GetWithdrawHistory(string asset = null, WithdrawHistoryStatuses? status = null, long? startTime = null, long? endTime = null, long? recvWindow = null)
        {
            string url = "v1/getWithdrawHistory.html";

            int? statusCode = null;
            if (status.HasValue)
                statusCode = (int)status.Value;

            var paramList = CreateParamDict("asset", FormatSymbol(asset),
                                            "status", statusCode ?? null,
                                            "startTime", startTime ?? null,
                                            "name", endTime ?? null,
                                            "recvWindow", recvWindow ?? null);
            string response = await SIGNED_Requestt_WAPI(url, paramList, HTTPVerbs.POST);
            return APIResponseHandler.CreateAPIResponseObject<Response_WithdrawHistory>(response);
        }

        #endregion

        #endregion
    }
}
