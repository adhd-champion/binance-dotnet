using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Binance;
using Binance.enums;
using Binance.objects.api_responses;
using Binance.objects;

namespace Binance.CMD
{
    class Program
    {
        private static Binance binance;
        private static bool ShellMode = false;
        
        static void Main(string[] args)
        {
            binance = new Binance();
            Start(args).Wait();
        }
        static async Task Start(string[] args)
        {
            try
            {
                if (args.Count() == 0)
                    await ShellExecution(PromptForInput());
                else
                    await Execute(args);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        private static async Task<bool> ShellExecution(string[] args)
        {
            bool exit = false;
            try
            {
                if (args != null && args.Count() > 0)
                    exit = await Execute(args, true);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            if (!exit)
                await ShellExecution(PromptForInput());
            return false;
        }
        static string[] PromptForInput()
        {
            Console.Write("BinanceCMD:> ");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
                return input.Split(' ');
            else
                return null;
        }
        private async static Task<bool> Execute(string[] args, bool shellExec = false)
        {
            bool exit = false;
            ShellMode = shellExec;
            try
            {
                if (!exit)
                {
                    if (args.Count() > 0)
                    {
                        string symbol = string.Empty;
                        int? limit = null;
                        decimal? quantity = null;
                        decimal? price = null;
                        object param = null;
                        

                        LogToConsole();

                        switch (args[0].ToLower())
                        {
                            case "set":
                                string varName = string.Empty; 
                                try
                                {
                                    varName = args[1];
                                    switch (varName.ToLower())
                                    {
                                        case "key":
                                        case "k":
                                            binance.APIKey = args[2];
                                            break;
                                        case "secret":
                                        case "s":
                                            binance.APISecret = args[2];
                                            break;
                                        case "timesource":
                                        case "ts":
                                            var sources = Enum.GetValues(typeof(TimeStampSources));
                                            foreach(TimeStampSources source in sources)
                                            {
                                                if (args[2].ToLower() == source.ToString().ToLower())
                                                    binance.TimeStampSource = source;
                                            }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException(varName);
                                    }
                                }
                                catch(ArgumentOutOfRangeException ex)
                                {
                                    if(!string.IsNullOrEmpty(varName))
                                        HandleException(ex, "Invalid value specified for 'set' parameter. [" + ex.ParamName + "]");
                                    else
                                        HandleException(ex, "No parameter name specified for 'set' command.");
                                }
                                break;
                            case "ping":
                                Response_Ping ping = await binance.Ping();
                                if (LogAPIIntro(ping))
                                {
                                    LogProperty("Connectivity Test Status", ping.status);
                                }
                                break;
                            case "time":
                                Response_Time time = await binance.Time();
                                if (LogAPIIntro(time))
                                {
                                    LogProperty("Server Time", time.serverTime.ToString());
                                }
                                break;
                            case "depth":
                                symbol = SetValue("Symbol", args[1]);
                                if (args.Count() > 2)
                                    limit = int.Parse(SetValue("Limit", args[2]));
                                Response_Depth depth = await binance.Depth(symbol, limit);
                                if (LogAPIIntro(depth))
                                {

                                }
                                break;
                            case "ticker_all":
                                Response_AllPrices prices = await binance.Ticker_AllPrices();
                                if (LogAPIIntro(prices))
                                {
                                    LogToConsole(String.Format("| {0,8} | {1,16} |", "Symbol", "Price"));
                                    foreach (var quote in prices.prices)
                                    {
                                        LogToConsole(String.Format("| {0,8} | {1,16} |", quote.symbol, quote.price));
                                    }
                                    
                                }
                                break;
                            case "ticker_24hr":
                                symbol = SetValue("Symbol", args[1]);
                                Response_Ticker_24hr ticker = await binance.Ticker_24Hr(symbol);
                                if (LogAPIIntro(ticker))
                                {
                                    LogToConsole("24 Hr Change:  " + ticker.priceChange + " / %" + ticker.priceChangePercent);
                                    LogToConsole();
                                    LogToConsole(String.Format("| {0,4} | {1,10} | {2,10} |", "", "Price", "Quantity"));
                                    LogToConsole(String.Format("| {0,4} | {1,10} | {2,10} |", "Last", ticker.lastPrice.Substring(0, 10), ticker.lastQty.Substring(0, 10)));
                                    LogToConsole(String.Format("| {0,4} | {1,10} | {2,10} |", "Bid", ticker.bidPrice.Substring(0, 10), ticker.bidQty.Substring(0, 10)));
                                    LogToConsole(String.Format("| {0,4} | {1,10} | {2,10} |", "Ask", ticker.askPrice.Substring(0, 10), ticker.askQty.Substring(0, 10)));
                                }
                                break;
                            case "account":
                                Response_Account acct = await binance.Account();
                                if (LogAPIIntro(acct))
                                {
                                    LogToConsole(String.Format("| {0,5} | {1,16} | {2,16} |", "Asset", "Free", "Locked"));
                                    foreach (var bal in acct.balances)
                                    {
                                        LogToConsole(String.Format("| {0,5} | {1,16} | {2,16} |", bal.asset, bal.free, bal.locked));
                                    }
                                }
                                break;
                            case "buy_limit_test":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                price = decimal.Parse(SetValue("Price", args[3]));
                                Response_NewOrder test_buylimit = await binance.BuyLimitTEST(symbol, quantity.Value, price.Value);
                                if (LogAPIIntro(test_buylimit))
                                {

                                }
                                break;
                            case "buy_market_test":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                Response_NewOrder test_buymarket = await binance.BuyMarketTEST(symbol, quantity.Value);
                                if (LogAPIIntro(test_buymarket))
                                {

                                }
                                break;
                            case "sell_limit_test":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                price = decimal.Parse(SetValue("Price", args[3]));
                                Response_NewOrder test_selllimit = await binance.SellLimitTEST(symbol, quantity.Value, price.Value);
                                if (LogAPIIntro(test_selllimit))
                                {

                                }
                                break;
                            case "sell_market_test":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                Response_NewOrder test_sellmarket = await binance.SellMarketTEST(symbol, quantity.Value);
                                if (LogAPIIntro(test_sellmarket))
                                {

                                }
                                break;
                            case "buy_limit":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                price = decimal.Parse(SetValue("Price", args[3]));
                                Response_NewOrder buylimit = await binance.BuyLimit(symbol, quantity.Value, price.Value);
                                if (LogAPIIntro(buylimit))
                                {

                                }
                                break;
                            case "buy_market":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                price = decimal.Parse(SetValue("Price", args[3]));
                                Response_NewOrder buymarket = await binance.BuyMarket(symbol, quantity.Value);
                                if (LogAPIIntro(buymarket))
                                {

                                }
                                break;
                            case "sell_limit":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                price = decimal.Parse(SetValue("Price", args[3]));
                                Response_NewOrder selllimit = await binance.SellLimit(symbol, quantity.Value, price.Value);
                                if (LogAPIIntro(selllimit))
                                {

                                }
                                break;
                            case "sell_market":
                                symbol = SetValue("Symbol", args[1]);
                                quantity = decimal.Parse(SetValue("Quantity", args[2]));
                                price = decimal.Parse(SetValue("Price", args[3]));
                                Response_NewOrder sellmarket = await binance.SellMarket(symbol, quantity.Value);
                                if (LogAPIIntro(sellmarket))
                                {

                                }
                                break;
                            case "get_order":
                                symbol = SetValue("Symbol", args[1]);
                                param = SetValue("Param", args[2]);
                                long param_long;
                                Response_QueryOrder queryorder;

                                if (long.TryParse(param.ToString(), out param_long))
                                    queryorder = await binance.Get_Order(symbol, param_long);
                                else
                                    queryorder = await binance.Get_Order(symbol, param.ToString());
                                if (LogAPIIntro(queryorder))
                                {
                                    LogProperty("Order Status", queryorder.status);
                                }
                                break;
                            case "cancel_order":
                                string co_symbol;
                                long co_orderId; string co_origClientOrderId;
                                string co_newClientOrderId;
                                Response_CancelOrder cancelorder = null;
                                SetValue<string>("Symbol", args, 1, out co_symbol);
                                SetValue<string>("NewClientOrderID", args, 3, out co_newClientOrderId);
                                if (SetValue<long>("OrderID", args, 2, out co_orderId))
                                    cancelorder = await binance.Delete_Order(symbol, co_orderId, co_newClientOrderId);
                                else if (SetValue<string>("NewClientOrderID", args, 2, out co_origClientOrderId))
                                    cancelorder = await binance.Delete_Order(symbol, co_origClientOrderId, co_newClientOrderId);
                                if (LogAPIIntro(cancelorder))
                                {
                                    LogToConsole("Order Status:  " + cancelorder.status);
                                }
                                break;
                            case "all_orders":
                                string ao_symbol;
                                SetValue<string>("Symbol", args, 1, out ao_symbol);
                                Response_AllOrders allorders = await binance.All_Orders(ao_symbol);
                                if (LogAPIIntro(allorders))
                                {
                                    LogToConsole(String.Format("| {0,5} | {1,10} | {2,10} |", "Symbol", "Price", "Quantity"));
                                    foreach (var order in allorders.orders)
                                    {
                                        LogToConsole(String.Format("| {0,5} | {1,10} | {2,10} |", order.symbol, order.price, order.executedQty));
                                    }
                                }
                                break;
                            case "open_orders":
                                string oo_symbol;
                                SetValue<string>("Symbol", args, 1, out oo_symbol);
                                Response_OpenOrders openorders = await binance.Open_Orders(oo_symbol);
                                if (LogAPIIntro(openorders))
                                {
                                    LogToConsole(String.Format("| {0,5} | {1,10} | {2,10} |", "Symbol", "Price", "Quantity"));
                                    foreach (var order in openorders.orders)
                                    {
                                        LogToConsole(String.Format("| {0,5} | {1,10} | {2,10} |", order.symbol, order.price, order.executedQty));
                                    }
                                }
                                break;
                            case "my_trades":
                                string mt_symbol = null;
                                int? mt_limit = null;
                                long? mt_fromId = null;
                                if (SetValue<string>("Symbol", args, 1, out mt_symbol))
                                    if (SetValue<int?>("Limit", args, 2, out mt_limit))
                                        SetValue<long?>("FromID", args, 3, out mt_fromId);
                                Response_MyTrades myTrades = await binance.My_Trades(mt_symbol, mt_limit, mt_fromId);
                                if (LogAPIIntro(myTrades))
                                {
                                    LogToConsole(String.Format("| {0,14} | {1,12} | {2,12} |", "Time", "Price", "Quantity"));
                                    foreach (var trade in myTrades.trades)
                                    {
                                        LogToConsole(String.Format("| {0,14} | {1,12} | {2,12} |", trade.time, trade.price, trade.qty));
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentException("Invalid action.", args[0]);
                        }
                        LogToConsole();
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                if(ex.ParamName != "input") //<---- Request failed.  Exception already thrown.
                    HandleException(ex, "Invalid argument supplied. [" + ex.ParamName + "]");
            }
            catch (ArgumentException ex)
            {
                HandleException(ex, "Invalid argument supplied. [" + ex.ParamName + "]");
            }
            catch (IndexOutOfRangeException ex)
            {
                HandleException(ex, "One or more required parameters were not supplied. (" + args.Count() + ")");
            }
            catch (Exception ex)
            {
                HandleException(ex, "An unknown error occurred.");
            }
            return exit;
        }
        private static string SetValue(string name, string value)
        {
            LogProperty(name, value);
            return value;
        }
        private static bool SetValue<T>(string name, string[] values, int index, out T value)
        {
            bool valueSet = false;
            try
            {
                if (values.Count() > index)
                {
                    string valStr = values[index];
                    value = (T)Convert.ChangeType(valStr, typeof(T));
                    valueSet = true;
                    LogProperty(name, valStr);
                }
                else
                    value = default(T);
            }
            catch
            {
                value = default(T);
            }
            return valueSet;
        }
        private static void HandleException(Exception ex, string additionalMessage=null)
        {
            if(!string.IsNullOrEmpty(additionalMessage))
            {
                LogToConsole("-----------------------------");
                LogToConsole("--> " + additionalMessage);
                LogToConsole("-----------------------------");
                LogToConsole();
            }
            LogToConsole(ex.ToString());
        }
        private static bool LogAPIIntro(APIResponse response)
        {
            bool successful = true;
            if (response != null)
            {
                LogMultilineProperty("Raw Response", response.raw);
                LogToConsole();
                if (response.hasErrors)
                {
                    LogToConsole("!ERROR OCCURRED!");
                    LogToConsole("  Server Code:  " + response.code);
                    LogToConsole("  Server Message:  " + response.msg);
                    successful = false;
                }
            }
            return successful;
        }
        public static void LogProperty(string name, string value)
        {
            LogToConsole("-" + name + ":  " + value);
        }
        private static void LogMultilineProperty(string name, string value)
        {
            LogToConsole(name + ":");
            LogToConsole("--------------------");
            LogToConsole(value);
            LogToConsole("--------------------");
        }
        public static void LogToConsole(string message = null)
        {
            if (string.IsNullOrEmpty(message))
                LogToConsole();
            else
                LogToConsole(message);
        }
    }

}
