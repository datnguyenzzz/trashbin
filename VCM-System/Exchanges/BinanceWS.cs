using Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Exchanges.BinanceWS
{
	public class ApiClientWS
	{
		/// ctor.
		public readonly string _apiKey = "";
		public readonly string _apiSecret = "";
		public readonly string _webSocketEndpoint = "";
		public List<WebSocket> _openSockets;
		public delegate void MessageHandler<T>(T messageData);
		public ApiClientWS(string apiKey, string apiSecret,
						 string webSocketEndpoint = @"wss://stream.binance.com:9443/ws/")
		{
			_apiKey = apiKey;
			_apiSecret = apiSecret;
			_webSocketEndpoint = webSocketEndpoint;
			_openSockets = new List<WebSocket>();
		}

		public void ConnectToWebSocket(string parameters, MessageHandler<dynamic> messageHandler, bool useCustomParser = false)
		{
			var finalEndpoint = _webSocketEndpoint + parameters;
			Console.WriteLine("Connecting: " + finalEndpoint);
			var ws = new WebSocket(finalEndpoint);

			ws.OnMessage += (sender, e) =>
			{
				dynamic eventData;
				eventData = JsonConvert.DeserializeObject<dynamic>(e.Data);
				messageHandler(eventData);
				Console.WriteLine(eventData);
			};

			ws.OnClose += (sender, e) =>
			{
				_openSockets.Remove(ws);
			};

			ws.OnError += (sender, e) =>
			{
				_openSockets.Remove(ws);
			};

			ws.Connect();
			_openSockets.Add(ws);
		}
	}
	public class BinanceClientWS
	{
		/// Secret used to authenticate within the API.
		public dynamic _tradingRules;

		/// Client to be used to call the API.
		public readonly ApiClientWS _apiClient;

		/// Defines the constructor of the Binance client.
		public BinanceClientWS(ApiClientWS apiClient)
		{
			_apiClient = apiClient;
		}

		//Websocket KLine
		public void ListenKlineEndpoint(string symbol, string interval, ApiClientWS.MessageHandler<dynamic> klineHandler)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var param = symbol + $"@kline_{interval}";
			_apiClient.ConnectToWebSocket(param, klineHandler);
		}


	}
}