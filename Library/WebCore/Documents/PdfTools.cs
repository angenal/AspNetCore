using SignLib.Certificates;
using SignLib.Pdf;
using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebInterface;
using WebInterface.Settings;

namespace WebCore.Documents
{
    public class PdfTools : IPdfTools
    {
        public string SaveToFile(FileInfo source, string outputDirectory, string outputFileFormat, string uriString = null, string password = null, string name = null, string ts = null)
        {
            string filename = source.FullName, dirString = outputDirectory, fName, fPath;
            if (name == null) name = Path.GetFileNameWithoutExtension(filename);
            if (ts == null) ts = source.LastWriteTimeHex();
            var dir = new DirectoryInfo(dirString);

            if (outputFileFormat == "html" || outputFileFormat == ".html")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                //var doc = new PdfDocument();
                //if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                //else doc.LoadFromFile(filename, password);
                //doc.SaveToFile(fPath, FileFormat.HTML);
                //doc.Dispose();

                filename = filename.Replace('\\', '/');
                var src = filename.Contains("wwwroot")
                    ? filename.Substring(filename.IndexOf("wwwroot") + 7)
                    : source.FullName.Substring(Environment.CurrentDirectory.Length).Replace('\\', '/');
                var html = PdfTemplate(source.Name, src);
                File.WriteAllText(fPath, html, Encoding.UTF8);

                return uriString + "/" + fName;
            }

            if (outputFileFormat == "png" || outputFileFormat == ".png")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                var doc = new PdfDocument();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, password);
                var count = doc.Pages.Count;
                int width = 0, height = 0;
                var images = new Image[count];
                for (var i = 0; i < count; i++)
                {
                    var stream = doc.SaveAsImage(i);
                    if (stream == null) continue;
                    images[i] = Image.FromStream(stream);
                    width = images[i].Width;
                    height += images[i].Height;
                }
                doc.Dispose();

                // merge image
                var b = new Bitmap(width, height);
                var g = Graphics.FromImage(b);
                for (int i = 0, y = 0; i < count; i++)
                {
                    if (images[i] == null) continue;
                    g.DrawImage(images[i], new Point(0, y));
                    y += images[i].Height;
                }
                b.Save(fPath, ImageFormat.Png);
                foreach (var img in images) img.Dispose();
                b.Dispose(); g.Dispose();

                return uriString + "/" + fName;
            }

            throw new ArgumentNullException(nameof(outputFileFormat));
        }

        static string PdfTemplate(string title, string url) => $@"<!DOCTYPE html><html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <title>{title}</title>
    <script src=""../../scripts/jquery/jquery-1.7.1.min.js""></script>
    <script src=""../../scripts/pdfobject/pdfobject.min.js""></script>
</head>
<body>
    <script type=""text/javascript"">PDFObject.embed(""{url}"");</script>
</body>
</html>";


        const string CertificateFile = "PdfCert.pfx";
        const string CertificatePassword = "123456";
        const string PdfSignatureSerialNumberLicense = "a017e6fb4bedc82a85cf";

        /// <summary>
        /// Apply digital signature
        /// </summary>
        /// <param name="unsignedDocument"></param>
        /// <param name="signedDocument"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="signingReason"></param>
        /// <param name="signingLocation"></param>
        public void Sign(string unsignedDocument, string signedDocument, string hashAlgorithm = "SHA256", string signingReason = null, string signingLocation = null)
        {
            var ps = new PdfSignature(PdfSignatureSerialNumberLicense);

            // Load the PDF document
            ps.LoadPdfDocument(unsignedDocument);
            ps.SigningReason = signingReason;
            ps.SigningLocation = signingLocation;
            ps.SignaturePosition = SignaturePosition.TopLeft;
            ps.HashAlgorithm = (SignLib.HashAlgorithm)Enum.Parse(typeof(SignLib.HashAlgorithm), hashAlgorithm);

            // Load the signature certificate from a PFX or P12 file
            var certificateFile = Path.Combine(Directory.GetCurrentDirectory(), CertificateFile);
            ps.DigitalSignatureCertificate = DigitalCertificate.LoadCertificate(certificateFile, CertificatePassword);

            // Write the signed file
            File.WriteAllBytes(signedDocument, ps.ApplyDigitalSignature());
        }
        /// <summary>
        /// Gets digital signature infos
        /// </summary>
        /// <param name="signedDocument"></param>
        /// <returns></returns>
        public IEnumerable<CertInfo> GetSignInfo(string signedDocument)
        {
            var ps = new PdfSignature(PdfSignatureSerialNumberLicense);

            // Load the PDF document
            ps.LoadPdfDocument(signedDocument);

            // Verify every digital signature form the PDF document
            foreach (PdfSignatureInfo csi in ps.DocumentProperties.DigitalSignatures)
                yield return ExtractCertificateInformation(csi);
        }
        static CertInfo ExtractCertificateInformation(PdfSignatureInfo csi) => new CertInfo
        {
            Subject = csi.SignatureCertificate.Subject,
            Issuer = csi.SignatureCertificate.GetNameInfo(X509NameType.SimpleName, true),
            Status = (CertStatus)Enum.Parse(typeof(CertStatus), DigitalCertificate.VerifyDigitalCertificate(csi.SignatureCertificate, VerificationType.LocalTime).ToString()),
            HashAlgorithm = csi.HashAlgorithm,
            DigestAlgorithm = csi.DigestAlgorithm,
            SignatureAlgorithm = csi.SignatureAlgorithm,
            SigningReason = csi.SigningReason,
            SigningLocation = csi.SigningLocation,
            SignatureName = csi.SignatureName,
            SignatureTime = csi.SignatureTime,
            SignatureExpireTime = csi.SignatureCertificate.NotAfter,
            SignatureIsTimestamped = csi.SignatureIsTimestamped,
            SignatureIsValid = csi.SignatureIsValid,
            TimestampInfo = csi.SignatureIsTimestamped == false ? null : new CertTimestampInfo
            {
                HashAlgorithm = csi.TimestampInfo.HashAlgorithm.FriendlyName,
                IsTimestampAltered = csi.TimestampInfo.IsTimestampAltered,
                SerialNumber = csi.TimestampInfo.SerialNumber,
            },
        };

    }
}
