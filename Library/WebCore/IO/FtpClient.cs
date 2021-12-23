using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace WebCore.IO
{
    public class FtpClient : StorageClient
    {
        private readonly string _url;
        private readonly int? _port;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _certificateAsBase64;
        private readonly string _certificateFileName;
        private readonly bool _useSsl;
        private const int DefaultBufferSize = 81920;
        private const int DefaultFtpPort = 21;

        public FtpClient(string url, int? port, string userName, string password, string certificateAsBase64,
            string certificateFileName, UploadProgress uploadProgress = null, CancellationToken? cancellationToken = null)
            : base(uploadProgress, cancellationToken)
        {
            _url = url;
            _port = port;
            _userName = userName;
            _password = password;
            _certificateAsBase64 = certificateAsBase64;
            _certificateFileName = certificateFileName;

            if (_url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase) == false &&
                _url.StartsWith("ftps://", StringComparison.OrdinalIgnoreCase) == false)
                _url = "ftp://" + url;

            if (_url.StartsWith("ftps", StringComparison.OrdinalIgnoreCase))
            {
                _useSsl = true;
                _url = _url.Replace("ftps://", "ftp://");
            }

            if (_url.EndsWith("/") == false)
                _url += "/";

            Debug.Assert(_url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase));
        }

        public async Task UploadFile(string folderName, string fileName, Stream stream)
        {
            await TestConnection().ConfigureAwait(false);

            UploadProgress?.SetTotal(stream.Length);
            UploadProgress?.ChangeState(UploadState.PendingUpload);

            var url = await CreateNestedFoldersIfNeeded(folderName).ConfigureAwait(false);
            url += $"/{fileName}";

            var request = CreateFtpWebRequest(url, WebRequestMethods.Ftp.UploadFile, keepAlive: true);
            var readBuffer = new byte[DefaultBufferSize];

            int count;
            var requestStream = request.GetRequestStream();
            while ((count = await stream.ReadAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false)) != 0)
            {
                await requestStream.WriteAsync(readBuffer, 0, count).ConfigureAwait(false);
                UploadProgress?.UpdateUploaded(count);
            }

            requestStream.Flush();
            requestStream.Close();

            UploadProgress?.ChangeState(UploadState.PendingResponse);
            var response = await request.GetResponseAsync().ConfigureAwait(false);
            response.Dispose();

            UploadProgress?.ChangeState(UploadState.Done);
        }

        private async Task<string> CreateNestedFoldersIfNeeded(string folderName)
        {
            ExtractUrlAndDirectories(out string url, out List<string> directories);

            // create the nested folders including the new folder
            directories.Add(folderName);

            foreach (var directory in directories)
            {
                if (directory == string.Empty)
                    continue;

                url += $"/{directory}";
                var request = CreateFtpWebRequest(url, WebRequestMethods.Ftp.MakeDirectory, keepAlive: false);

                try
                {
                    var response = await request.GetResponseAsync().ConfigureAwait(false);
                    response.Close();
                }
                catch (WebException e)
                {
                    var response = (FtpWebResponse)e.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        // folder already exists
                        continue;
                    }

                    throw;
                }
            }

            return url;
        }

        private void ExtractUrlAndDirectories(out string url, out List<string> dirs)
        {
            var uri = new Uri(_url);

            dirs = uri.AbsolutePath.TrimStart('/').TrimEnd('/').Split('/').ToList();
            var port = _port ?? (uri.Port > 0 ? uri.Port : DefaultFtpPort);
            if (port < 1 || port > 65535)
                throw new ArgumentException("Port number range: 1-65535");

            url = $"{uri.Scheme}://{uri.Host}:{port}";
        }

        private FtpWebRequest CreateFtpWebRequest(string url, string method, bool keepAlive)
        {
            var request = (FtpWebRequest)WebRequest.Create(new Uri(url));
            request.Method = method;

            request.Credentials = new NetworkCredential(_userName, _password);
            request.EnableSsl = _useSsl;
            if (_useSsl)
            {
                var byteArray = Convert.FromBase64String(_certificateAsBase64);

                try
                {
                    var x509Certificate = new X509Certificate(byteArray);
                    request.ClientCertificates = new X509CertificateCollection(new[] { x509Certificate });
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"This is not a valid certificate, file name: {_certificateFileName}", e);
                }
            }

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = keepAlive;

            return request;
        }

        public async Task TestConnection()
        {
            if (_useSsl && string.IsNullOrWhiteSpace(_certificateAsBase64))
                throw new ArgumentException("Certificate must be provided when using ftp with SSL!");

            ExtractUrlAndDirectories(out string url, out List<string> _);
            var request = CreateFtpWebRequest(url, WebRequestMethods.Ftp.ListDirectory, keepAlive: false);

            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);
                response.Close();
            }
            catch (WebException e)
            {
                var response = (FtpWebResponse)e.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // folder doesn't exist
                    return;
                }

                throw;
            }
            catch (AuthenticationException e)
            {
                throw new AuthenticationException("Make sure that you provided the correct certificate and that the url matches the one that is defined in it", e);
            }
        }
    }
}
