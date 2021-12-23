using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace WebCore.IO
{
    public abstract class StorageClient : IDisposable
    {
        private readonly List<HttpClient> _clients = new List<HttpClient>();
        protected readonly CancellationToken CancellationToken;
        protected readonly UploadProgress UploadProgress;
        protected const int MaxRetriesForMultiPartUpload = 5;

        protected StorageClient(UploadProgress uploadProgress, CancellationToken? cancellationToken)
        {
            UploadProgress = uploadProgress;
            CancellationToken = cancellationToken ?? CancellationToken.None;
        }

        protected HttpClient GetClient(TimeSpan? timeout = null)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.None
            };

            var client = new HttpClient(handler)
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(120)
            };

            _clients.Add(client);

            return client;
        }

        public virtual void Dispose()
        {
            var exceptions = new List<Exception>();

            foreach (var client in _clients)
            {
                try
                {
                    client.Dispose();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        public class Blob
        {
            public Blob(Stream data, Dictionary<string, string> metadata)
            {
                Data = data;
                Metadata = metadata;
            }

            public Stream Data { get; }

            public Dictionary<string, string> Metadata { get; }
        }
    }
}
