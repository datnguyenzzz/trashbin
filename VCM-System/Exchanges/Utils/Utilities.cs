using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{
	/// Utility class for common processes. 
	public static class Utilities
	{
		/// Gets a HMACSHA256 signature based on the API Secret.
		public static string GenerateSignature(string apiSecret, string message)
		{
			var key = Encoding.UTF8.GetBytes(apiSecret);
			string stringHash;
			using (var hmac = new HMACSHA256(key))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
				stringHash = BitConverter.ToString(hash).Replace("-", "");
			}

			return stringHash;
		}

		/// Gets a timestamp in milliseconds.
		public static string GenerateTimeStamp(DateTime baseDateTime)
		{
			var dtOffset = new DateTimeOffset(baseDateTime);
			return dtOffset.ToUnixTimeMilliseconds().ToString();
		}

		/// Gets an HttpMethod object based on a string.

		public static HttpMethod CreateHttpMethod(string method)
		{
			switch (method.ToUpper())
			{
				case "DELETE":
					return HttpMethod.Delete;
				case "POST":
					return HttpMethod.Post;
				case "PUT":
					return HttpMethod.Put;
				case "GET":
					return HttpMethod.Get;
				default:
					throw new NotImplementedException();
			}
		}
	}
}