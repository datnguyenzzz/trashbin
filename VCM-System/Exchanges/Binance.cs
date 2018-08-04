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

namespace Exchanges.Binance
{
	public class ApiClient
	{
		/// ctor.
		public readonly string _apiUrl = "";
		public readonly string _apiKey = "";
		public readonly string _apiSecret = "";
		public readonly HttpClient _httpClient;
		public delegate void MessageHandler<T>(T messageData);
		public ApiClient(string apiKey, string apiSecret,
						 string apiUrl = @"https://www.binance.com",
						 bool addDefaultHeaders = true)
		{
			_apiUrl = apiUrl;
			_apiKey = apiKey;
			_apiSecret = apiSecret;
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(_apiUrl)
			};

			if (addDefaultHeaders)
			{
				ConfigureHttpClient();
			}
		}
		private void ConfigureHttpClient()
		{
			_httpClient.DefaultRequestHeaders
				 .Add("X-MBX-APIKEY", _apiKey);

			_httpClient.DefaultRequestHeaders
					.Accept
					.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
		}
		/// Calls API Methods.
		public async Task<dynamic> CallAsync<T>(string method, string endpoint, bool isSigned = false, string parameters = null)
		{
			var finalEndpoint = endpoint + (string.IsNullOrWhiteSpace(parameters) ? "" : $"?{parameters}");

			if (isSigned)
			{
				// Joining provided parameters
				parameters += (!string.IsNullOrWhiteSpace(parameters) ? "&timestamp=" : "timestamp=") + Utilities.GenerateTimeStamp(DateTime.Now.ToUniversalTime());

				// Creating request signature
				var signature = Utilities.GenerateSignature(_apiSecret, parameters);
				finalEndpoint = $"{endpoint}?{parameters}&signature={signature}";
			}

			var request = new HttpRequestMessage(Utilities.CreateHttpMethod(method.ToString()), finalEndpoint);
			var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
			if (response.IsSuccessStatusCode)
			{
				// Api return is OK
				response.EnsureSuccessStatusCode();

				// Get the result
				var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

				// Serialize and return result
				return JsonConvert.DeserializeObject<T>(result);
			}

			// We received an error
			if (response.StatusCode == HttpStatusCode.GatewayTimeout)
			{
				throw new Exception("Api Request Timeout.");
			}

			// Get te error code and message
			var e = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			// Error Values
			var eCode = 0;
			string eMsg = "";
			if (e.IsValidJson())
			{
				try
				{
					var i = JObject.Parse(e);

					eCode = i["code"]?.Value<int>() ?? 0;
					eMsg = i["msg"]?.Value<string>();
				}
				catch { }
			}

			throw new Exception(string.Format("Api Error Code: {0} Message: {1}", eCode, eMsg));
		}
	}
	public class BinanceClient
	{
		/// Secret used to authenticate within the API.
		public dynamic _tradingRules;

		/// Client to be used to call the API.
		public readonly ApiClient _apiClient;

		/// Defines the constructor of the Binance client.
		public BinanceClient(ApiClient apiClient)
		{
			_apiClient = apiClient;
		}

		/// Validates that a new order is valid before posting it.

		/// Test connectivity to the Rest API.
		public async Task<dynamic> TestConnectivity()
		{
			var result = await _apiClient.CallAsync<dynamic>("GET", "/api/v1/ping", false);

			return result;
		}
		/// Test connectivity to the Rest API and get the current server time.
		public async Task<dynamic> GetServerTime()
		{
			var result = await _apiClient.CallAsync<dynamic>("GET", "/api/v1/time", false);

			return result;
		}

		/// Get order book for a particular symbol.
		public async Task<dynamic> GetOrderBook(string symbol, int limit = 100)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var result = await _apiClient.CallAsync<dynamic>("GET", "/api/v1/depth", false, $"symbol={symbol.ToUpper()}&limit={limit}");

			return result;
		}

		/// Get compressed, aggregate trades
		public async Task<IEnumerable<dynamic>> GetAggregateTrades(string symbol, int limit = 500)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var result = await _apiClient.CallAsync<IEnumerable<dynamic>>("GET", "/api/v1/aggTrades", false, $"symbol={symbol.ToUpper()}&limit={limit}");

			return result;
		}

		/// Kline/candlestick bars for a symbol
		public async Task<IEnumerable<dynamic>> GetCandleSticks(string symbol, string interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 500)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var args = $"symbol={symbol.ToUpper()}&interval={interval}"
				+ (startTime.HasValue ? $"&startTime={startTime.Value.GetUnixTimeStamp()}" : "")
				+ (endTime.HasValue ? $"&endTime={endTime.Value.GetUnixTimeStamp()}" : "")
				+ $"&limit={limit}";
			//Console.WriteLine(args);
			var result = await _apiClient.CallAsync<dynamic>("GET", "/api/v1/klines", false, args);

			return result;
		}

		/// 24 hour price change statistics.
		public async Task<IEnumerable<dynamic>> GetPriceChange24H(string symbol = "")
		{
			var args = string.IsNullOrWhiteSpace(symbol) ? "" : $"symbol={symbol.ToUpper()}";

			var result = new List<dynamic>();

			if (!string.IsNullOrEmpty(symbol))
			{
				var data = await _apiClient.CallAsync<dynamic>("GET", "/api/v1/ticker/24hr", false, args);
				result.Add(data);
			}
			else
			{
				result = await _apiClient.CallAsync<List<dynamic>>("GET", "/api/v1/ticker/24hr", false, args);
			}

			return result;
		}
		/// Latest price for all symbols.
		public async Task<IEnumerable<dynamic>> GetAllPrices()
		{
			var result = await _apiClient.CallAsync<IEnumerable<dynamic>>("GET", "/api/v1/ticker/allPrices", false);

			return result;
		}

		/// Best price/qty on the order book for all symbols.
		public async Task<IEnumerable<dynamic>> GetOrderBookTicker()
		{
			var result = await _apiClient.CallAsync<IEnumerable<dynamic>>("GET", "/api/v1/ticker/allBookTickers", false);

			return result;
		}

		/// Send in a new order.
		public async Task<dynamic> PostNewOrder(string symbol, decimal quantity, decimal price, string side, string orderType = "LIMIT", string timeInForce = "GTC", decimal icebergQty = 0m, long recvWindow = 5000)
		{
			var args = $"symbol={symbol.ToUpper()}&side={side}&type={orderType}&quantity={quantity}"
				+ (orderType == "LIMIT" ? $"&timeInForce={timeInForce}" : "")
				+ (orderType == "LIMIT" ? $"&price={price}" : "")
				+ (icebergQty > 0m ? $"&icebergQty={icebergQty}" : "")
				+ $"&recvWindow={recvWindow}";
			var result = await _apiClient.CallAsync<dynamic>("POST", "/api/v3/order", true, args);

			return result;
		}
		/// Test new order creation and signature/recvWindow long. Creates and validates a new order but does not send it into the matching engine.
		public async Task<dynamic> PostNewOrderTest(string symbol, decimal quantity, decimal price, string side, string orderType = "LIMIT", string timeInForce = "GTC", decimal icebergQty = 0m, long recvWindow = 5000)
		{
			var args = $"symbol={symbol.ToUpper()}&side={side}&type={orderType}&quantity={quantity}"
				+ (orderType == "LIMIT" ? $"&timeInForce={timeInForce}" : "")
				+ (orderType == "LIMIT" ? $"&price={price}" : "")
				+ (icebergQty > 0m ? $"&icebergQty={icebergQty}" : "")
				+ $"&recvWindow={recvWindow}";
			var result = await _apiClient.CallAsync<dynamic>("POST", "/api/v3/order/test", true, args);

			return result;
		}

		/// Check an order's status.
		public async Task<dynamic> GetOrder(string symbol, long? orderId = null, string origClientOrderId = null, long recvWindow = 5000)
		{
			var args = $"symbol={symbol.ToUpper()}&recvWindow={recvWindow}";

			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			if (orderId.HasValue)
			{
				args += $"&orderId={orderId.Value}";
			}
			else if (!string.IsNullOrWhiteSpace(origClientOrderId))
			{
				args += $"&origClientOrderId={origClientOrderId}";
			}
			else
			{
				throw new ArgumentException("Either orderId or origClientOrderId must be sent.");
			}

			var result = await _apiClient.CallAsync<dynamic>("GET", "/api/v3/order", true, args);

			return result;
		}

		/// Cancel an active order.
		public async Task<dynamic> CancelOrder(string symbol, long? orderId = null, string origClientOrderId = null, long recvWindow = 5000)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var args = $"symbol={symbol.ToUpper()}&recvWindow={recvWindow}";

			if (orderId.HasValue)
			{
				args += $"&orderId={orderId.Value}";
			}
			else if (string.IsNullOrWhiteSpace(origClientOrderId))
			{
				args += $"&origClientOrderId={origClientOrderId}";
			}
			else
			{
				throw new ArgumentException("Either orderId or origClientOrderId must be sent.");
			}

			var result = await _apiClient.CallAsync<dynamic>("DELETE", "/api/v3/order", true, args);

			return result;
		}

		/// Get all open orders on a symbol.
		public async Task<IEnumerable<dynamic>> GetCurrentOpenOrders(string symbol, long recvWindow = 5000)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var result = await _apiClient.CallAsync<IEnumerable<dynamic>>("GET", "/api/v3/openOrders", true, $"symbol={symbol.ToUpper()}&recvWindow={recvWindow}");

			return result;
		}

		/// Get all account orders; active, canceled, or filled.

		public async Task<IEnumerable<dynamic>> GetAllOrders(string symbol, long? orderId = null, int limit = 500, long recvWindow = 5000)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var result = await _apiClient.CallAsync<IEnumerable<dynamic>>("GET", "/api/v3/allOrders", true, $"symbol={symbol.ToUpper()}&limit={limit}&recvWindow={recvWindow}" + (orderId.HasValue ? $"&orderId={orderId.Value}" : ""));

			return result;
		}

		/// Get current account information.
		public async Task<dynamic> GetAccountInfo(long recvWindow = 5000)
		{
			var result = await _apiClient.CallAsync<dynamic>("GET", "/api/v3/account", true, $"recvWindow={recvWindow}");

			return result;
		}

		/// Get trades for a specific account and symbol.
		public async Task<IEnumerable<dynamic>> GetTradeList(string symbol, long recvWindow = 5000)
		{
			if (string.IsNullOrWhiteSpace(symbol))
			{
				throw new ArgumentException("symbol cannot be empty. ", "symbol");
			}

			var result = await _apiClient.CallAsync<IEnumerable<dynamic>>("GET", "/api/v3/myTrades", true, $"symbol={symbol.ToUpper()}&recvWindow={recvWindow}");

			return result;
		}

	}
}