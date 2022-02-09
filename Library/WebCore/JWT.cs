using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebCore
{
    public class JWT
    {
		static JWT()
		{
			var dictionary = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>();
			dictionary.Add(JwtHashAlgorithm.HS256, (byte[] key, byte[] value) => Sodium.SecretKeyAuth.SignHmacSha256(value, key));
			dictionary.Add(JwtHashAlgorithm.HS512, (byte[] key, byte[] value) => Sodium.SecretKeyAuth.SignHmacSha512(value, key));
			HashAlgorithms = dictionary;
		}

		public static string Encode(IDictionary<string, object> extraHeaders, object payload, byte[] key, JwtHashAlgorithm algorithm)
		{
			List<string> list = new List<string>();
			Dictionary<string, object> obj = new Dictionary<string, object>(extraHeaders)
			{
				{
					"typ",
					"JWT"
				},
				{
					"alg",
					algorithm.ToString()
				}
			};
			byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
			byte[] bytes2 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
			list.Add(Base64UrlEncode(bytes));
			list.Add(Base64UrlEncode(bytes2));
			string s = string.Join(".", list.ToArray());
			byte[] bytes3 = Encoding.UTF8.GetBytes(s);
			byte[] input = HashAlgorithms[algorithm](key, bytes3);
			list.Add(Base64UrlEncode(input));
			return string.Join(".", list.ToArray());
		}
		public static string Encode(object payload, byte[] key, JwtHashAlgorithm algorithm)
		{
			return Encode(new Dictionary<string, object>(), payload, key, algorithm);
		}

		public static string Encode(IDictionary<string, object> extraHeaders, object payload, string key, JwtHashAlgorithm algorithm)
		{
			return Encode(extraHeaders, payload, Encoding.UTF8.GetBytes(key), algorithm);
		}

		public static string Encode(object payload, string key, JwtHashAlgorithm algorithm)
		{
			return Encode(new Dictionary<string, object>(), payload, Encoding.UTF8.GetBytes(key), algorithm);
		}

		public static string Decode(string token, byte[] key, bool verify = true)
		{
			string[] array = token.Split(new char[]
			{
				'.'
			});
			if (array.Length != 3)
			{
				throw new ArgumentException("Token must consist from 3 delimited by dot parts");
			}
			string text = array[0];
			string text2 = array[1];
			byte[] inArray = Base64UrlDecode(array[2]);
			string @string = GetString(Base64UrlDecode(text));
			string string2 = GetString(Base64UrlDecode(text2));
			Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(@string);
			if (verify)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(text + "." + text2);
				string algorithm = (string)dictionary["alg"];
				byte[] inArray2 = HashAlgorithms[GetHashAlgorithm(algorithm)](key, bytes);
				string text3 = Convert.ToBase64String(inArray);
				string text4 = Convert.ToBase64String(inArray2);
				if (text3 != text4)
				{
					throw new Exception(string.Format("Invalid signature. Expected {0} got {1}", new object[]
					{
						text3,
						text4
					}));
				}
				Dictionary<string, object> dictionary2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(string2);
				if (dictionary2.ContainsKey("exp") && dictionary2["exp"] != null)
				{
					int num;
					try
					{
						num = Convert.ToInt32(dictionary2["exp"]);
					}
					catch (Exception)
					{
						throw new Exception("Claim 'exp' must be an integer.");
					}
					double num2 = Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
					if (num2 >= (double)num)
					{
						throw new Exception("Token has expired.");
					}
				}
			}
			return string2;
		}

		public static string Decode(string token, string key, bool verify = true)
		{
			return Decode(token, Encoding.UTF8.GetBytes(key), verify);
		}

		public static object DecodeToObject(string token, byte[] key, bool verify = true)
		{
			string json = Decode(token, key, verify);
			return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
		}

		public static object DecodeToObject(string token, string key, bool verify = true)
		{
			return DecodeToObject(token, Encoding.UTF8.GetBytes(key), verify);
		}

		public static T DecodeToObject<T>(string token, byte[] key, bool verify = true)
		{
			string json = Decode(token, key, verify);
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static T DecodeToObject<T>(string token, string key, bool verify = true)
		{
			return DecodeToObject<T>(token, Encoding.UTF8.GetBytes(key), verify);
		}

		private static JwtHashAlgorithm GetHashAlgorithm(string algorithm)
		{
			if (algorithm != null)
			{
				if (algorithm == "HS256")
				{
					return JwtHashAlgorithm.HS256;
				}
				if (algorithm == "HS512")
				{
					return JwtHashAlgorithm.HS512;
				}
			}
			throw new Exception("Algorithm '" + algorithm + "' is not supported!");
		}

		private static string GetString(byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		public static string Base64UrlEncode(byte[] input)
		{
			string text = Convert.ToBase64String(input);
			text = text.Split(new char[]
			{
				'='
			})[0];
			text = text.Replace('+', '-');
			return text.Replace('/', '_');
		}

		public static byte[] Base64UrlDecode(string input)
		{
			string text = input.Replace('-', '+');
			text = text.Replace('_', '/');
			switch (text.Length % 4)
			{
				case 0:
					goto IL_60;
				case 2:
					text += "==";
					goto IL_60;
				case 3:
					text += "=";
					goto IL_60;
			}
			throw new Exception("Illegal base64url string!");
		IL_60:
			return Convert.FromBase64String(text);
		}

		private static readonly Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms;
	}

	public enum JwtHashAlgorithm
	{
		HS256,
		HS512
	}
}
