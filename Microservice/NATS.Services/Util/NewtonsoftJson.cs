using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System.IO;

namespace NATS.Services.Util
{
    /// <summary>
    /// Newtonsoft JSON Extensions
    /// </summary>
    public static class NewtonsoftJson
    {
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static readonly JsonConverter[] Converters = new JsonConverter[] { new IsoDateTimeConverter { DateTimeFormat = DateTimeFormat } };

        /// <summary>
        /// Override with Custom Newtonsoft JSON Handler
        /// </summary>
        /// <param name="client"></param>
        public static void AddJsonHandler(this RestClient client)
        {
            client.AddHandler("application/json", () => NewtonsoftJsonSerializer.Default);
            client.AddHandler("text/json", () => NewtonsoftJsonSerializer.Default);
            client.AddHandler("text/x-json", () => NewtonsoftJsonSerializer.Default);
            client.AddHandler("text/javascript", () => NewtonsoftJsonSerializer.Default);
            client.AddHandler("*+json", () => NewtonsoftJsonSerializer.Default);
        }

        /// <summary>
        /// Override with Custom Newtonsoft JSON Handler
        /// </summary>
        /// <param name="request"></param>
        /// <param name="obj"></param>
        public static void AddCustomJsonBody(this RestRequest request, object obj)
        {
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = NewtonsoftJsonSerializer.Default;
            request.AddJsonBody(obj);
        }
    }

    /// <summary>
    /// Custom Newtonsoft JSON Handler
    /// </summary>
    public class NewtonsoftJsonSerializer : ISerializer, IDeserializer
    {
        private readonly JsonSerializer serializer;

        public NewtonsoftJsonSerializer(JsonSerializer serializer)
        {
            this.serializer = serializer;
        }

        public string ContentType
        {
            get { return "application/json"; }
            set { }
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    serializer.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }

        public T Deserialize<T>(IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public static NewtonsoftJsonSerializer Default
        {
            get
            {
                return new NewtonsoftJsonSerializer(new JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = NewtonsoftJson.DateTimeFormat,
                })
                {
                    DateFormat = NewtonsoftJson.DateTimeFormat,
                };
            }
        }
    }
}
