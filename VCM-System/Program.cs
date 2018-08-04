//FINAL VERSION with WS
using System;
using Exchanges.Binance;
using Exchanges.BinanceWS;
using System.Threading;

namespace VCM_System_binance
{
	public class Program
	{
		public static void BinanceTest()
		{

			const string ApiKey = "urBQiDZjfKQDoIu0o970LKuODwq0oONver3pSFg83nWJ8lLUg1i5tuVrp2jqqJRA";
			const string ApiSecret = "ePlSCXCsdL0GaRK1ARrnGqdh2EcjkNj6uOLNuV9esnHSxJ4rvqyHVoOOpPFHfVTL";

			var apiClient = new ApiClient(ApiKey, ApiSecret);
			var binanceClient = new BinanceClient(apiClient);
			// REST api
			// TestConnectivity
			Console.WriteLine("Test Connectivity");
			Console.WriteLine(binanceClient.TestConnectivity().Result);

			//GetServerTime
			Console.WriteLine("GetServerTime");
			Console.WriteLine(binanceClient.GetServerTime().Result);

			///Market Data
			Console.WriteLine("GetOrderBook");
			Console.WriteLine(binanceClient.GetOrderBook("ethbtc").Result);
			Console.WriteLine("GetCandleSticks");
			Console.WriteLine(binanceClient.GetCandleSticks("ethbtc", "1h",
															new System.DateTime(2018, 01, 01),
															new System.DateTime(2018, 01, 07)).Result);
			Console.WriteLine("GetAggregateTrades");
			Console.WriteLine(binanceClient.GetAggregateTrades("ethbtc").Result);
			Console.WriteLine("GetPriceChange24H");
			Console.WriteLine(binanceClient.GetPriceChange24H("ETHBTC").Result);
			Console.WriteLine("GetPriceChange24H");
			Console.WriteLine(binanceClient.GetPriceChange24H().Result);
			Console.WriteLine("GetAllPrices");
			Console.WriteLine(binanceClient.GetAllPrices().Result);
			Console.WriteLine("GetOrderBookTicker");
			Console.WriteLine(binanceClient.GetOrderBookTicker().Result);

			///Account Information
			///Console.WriteLine(binanceClient.PostNewOrder("ethbtc", 0.01m, 0m, "BUY", "MARKET").Result);
			Console.WriteLine("PostNewOrderTest");
			Console.WriteLine(binanceClient.PostNewOrderTest("ethbtc", 1m, 0.1m, "BUY").Result);
			///Console.WriteLine(binanceClient.CancelOrder("ethbtc", 9137796).Result);
			Console.WriteLine("GetCurrentOpenOrders");
			Console.WriteLine(binanceClient.GetCurrentOpenOrders("ethbtc").Result);
			///Console.WriteLine(binanceClient.GetOrder("ethbtc", 8982811).Result);
			Console.WriteLine("GetAllOrders");
			Console.WriteLine(binanceClient.GetAllOrders("ethbtc").Result);
			Console.WriteLine("GetAccountInfo");
			Console.WriteLine(binanceClient.GetAccountInfo().Result);
			Console.WriteLine("GetTradeList");
			Console.WriteLine(binanceClient.GetTradeList("ethbtc").Result);
		}

		public static void BinanceWSTest()
		{
			const string ApiKey = "urBQiDZjfKQDoIu0o970LKuODwq0oONver3pSFg83nWJ8lLUg1i5tuVrp2jqqJRA";
			const string ApiSecret = "ePlSCXCsdL0GaRK1ARrnGqdh2EcjkNj6uOLNuV9esnHSxJ4rvqyHVoOOpPFHfVTL";
			var apiClient = new ApiClientWS(ApiKey, ApiSecret);
			var binanceClient = new BinanceClientWS(apiClient);
			
			// Websocket
			void KlineHandler(dynamic messageData)
			{
				var klineData = messageData;
			}
			Console.WriteLine("Websocket");
			binanceClient.ListenKlineEndpoint("ethbtc", "1m", KlineHandler);
			Thread.Sleep(1000000);
		}

		public static void Main()
		{
			BinanceTest();
			BinanceWSTest();
		}

	}
}