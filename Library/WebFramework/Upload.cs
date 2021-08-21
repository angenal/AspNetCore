using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebFramework
{
    /// <summary>
    /// Attribute for File Upload Controller
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DisableFormModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var formValueProviderFactory = context.ValueProviderFactories.OfType<FormValueProviderFactory>().FirstOrDefault();
            if (formValueProviderFactory != null) context.ValueProviderFactories.Remove(formValueProviderFactory);

            var formFileValueProviderFactory = context.ValueProviderFactories.OfType<FormFileValueProviderFactory>().FirstOrDefault();
            if (formFileValueProviderFactory != null) context.ValueProviderFactories.Remove(formFileValueProviderFactory);

            var jqueryFormValueProviderFactory = context.ValueProviderFactories.OfType<JQueryFormValueProviderFactory>().FirstOrDefault();
            if (jqueryFormValueProviderFactory != null) context.ValueProviderFactories.Remove(jqueryFormValueProviderFactory);
        }

        public void OnResourceExecuted(ResourceExecutedContext context) { }
    }

    /// <summary>
    /// HttpRequest Extensions for File Upload Action
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// File Upload Action
        /// </summary>
        public static async Task<FormValueProvider> UploadFile(this HttpRequest request, Func<IFormFile, Task> func)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");

            // Used to accumulate all the form url encoded key value pairs in the request.
            var formOptions = new FormOptions();
            var formAccumulator = new KeyValueAccumulator();
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), formOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);

            // section may have already been read (if for example model binding is not disabled), please use [DisableFormModelBinding]
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDispositionHeader);

                if (hasContentDispositionHeader)
                {
                    if (contentDispositionHeader.IsFileDisposition())
                    {
                        FileMultipartSection fileSection = section.AsFileSection();

                        // process file stream
                        await func(new MultipartFile(fileSection.FileStream, fileSection.Name, fileSection.FileName)
                        {
                            ContentType = fileSection.Section.ContentType,
                            ContentDisposition = fileSection.Section.ContentDisposition
                        });
                    }
                    else if (contentDispositionHeader.IsFormDisposition())
                    {
                        // Content-Disposition: form-data; name="key"
                        // Do not limit the key name length here because the multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes(contentDispositionHeader.Name);
                        var encoding = section.GetEncoding();
                        if (encoding == null)
                            throw new NullReferenceException($"Null encoding");

                        using var streamReader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
                        // The value length limit is enforced by MultipartBodyLengthLimit
                        var value = await streamReader.ReadToEndAsync();
                        if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            value = string.Empty;

                        formAccumulator.Append(key.Value, value);

                        if (formAccumulator.ValueCount > formOptions.ValueCountLimit)
                            throw new InvalidDataException($"Form key count limit {formOptions.ValueCountLimit} exceeded.");
                    }
                }

                // Drains any remaining section body that has not been consumed and reads the headers for the next section.
                //section = request.Body.CanSeek && request.Body.Position == request.Body.Length ? null : await reader.ReadNextSectionAsync();
                section = await reader.ReadNextSectionAsync();
            }

            // Bind form data to a model
            return new FormValueProvider(BindingSource.Form, new FormCollection(formAccumulator.GetResults()), CultureInfo.CurrentCulture);
        }
    }

    public class MultipartFile : IFormFile
    {
        // Stream.CopyTo method uses 128KB as the default buffer size.
        const int DefaultBufferSize = 128 * 1024;
        readonly Stream Stream;

        public string ContentType
        {
            get { return Headers["Content-Type"]; }
            set { Headers["Content-Type"] = value; }
        }

        public string ContentDisposition
        {
            get { return Headers["Content-Disposition"]; }
            set { Headers["Content-Disposition"] = value; }
        }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public long Length => Stream.Length;
        public string Name { get; set; }
        public string FileName { get; set; }

        public MultipartFile(Stream stream, string name, string filename)
        {
            Stream = stream;
            Name = name;
            FileName = filename;
        }

        public void CopyTo(Stream target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            Stream.CopyTo(target);
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return Stream.CopyToAsync(target, DefaultBufferSize, cancellationToken);
        }

        public Stream OpenReadStream()
        {
            return Stream;
        }
    }

    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec says 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary))
                throw new InvalidDataException("Missing content-type boundary.");

            if (boundary.Length > lengthLimit)
                throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return contentType != null && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class MultipartSectionExtensions
    {
        public static Encoding GetEncoding(this MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out MediaTypeHeaderValue mediaType);
            return !hasMediaTypeHeader ? Encoding.UTF8 : mediaType.Encoding;
        }
    }
}
