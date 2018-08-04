using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace Utils
{
	/// Class to define extension methods.
	public static class ExtensionMethods
	{
		/// Gets a timestamp in milliseconds.
		public static string GetUnixTimeStamp(this DateTime baseDateTime)
		{
			var dtOffset = new DateTimeOffset(baseDateTime);
			return dtOffset.ToUnixTimeMilliseconds().ToString();
		}

		/// Validates if string is a valid JSON
		public static bool IsValidJson(this string stringValue)
		{
			if (string.IsNullOrWhiteSpace(stringValue))
			{
				return false;
			}

			var value = stringValue.Trim();

			if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
				(value.StartsWith("[") && value.EndsWith("]"))) //For array
			{
				try
				{
					var obj = JToken.Parse(value);
					return true;
				}
				catch (JsonReaderException)
				{
					return false;
				}
			}

			return false;
		}
	}
}