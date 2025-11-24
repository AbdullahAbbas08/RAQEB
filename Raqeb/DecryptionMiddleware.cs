using Raqeb.Shared.Encryption;
using System.Buffers;
using System.Security.Cryptography;

namespace Raqeb
{
    public static class AppSettingsSimulation
    {
        public static string EncryptKey { get; set; } = "1203199320052021";
        public static string EncryptIV { get; set; } = "1203199320052021";
    }
    public class DecryptionMiddleware
    {
        List<string> IncludedURLList_Request = new List<string> {
               
            };


        List<string> IncludedURLList_Response = new List<string> {
               
            };


        private readonly RequestDelegate _next;

        public DecryptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            //var userAgent = httpContext.Request.Headers["User-Agent"].ToString().ToLower();
            //if (!(userAgent.Contains("ios") || userAgent.Contains("android")))
            //{
            #region Request Decryption
            if (httpContext.Request.Method.ToLower() == "get" || httpContext.Request.Method.ToLower() == "post" || httpContext.Request.Method.ToLower() == "delete")
            {
                string ContentType = httpContext.Request.ContentType ?? "";
                if (IncludedURLList_Request.Any(item => httpContext.Request.Path.Value.Contains(item)) && !(ContentType.ToLower().Contains("multipart")))
                {
                    httpContext.Request.Body = DecryptStream(httpContext.Request.Body);
                    if (httpContext.Request.QueryString.HasValue)
                    {
                        string decryptedString = DecryptString(httpContext.Request.QueryString.Value.Substring(1));
                        httpContext.Request.QueryString = new QueryString($"?{decryptedString}");
                    }
                }
            }
            else
            {
                await _next(httpContext);
            }
            #endregion


            if (IncludedURLList_Response.Any(item => httpContext.Request.Path.Value.Contains(item)))
            {
                try
                {
                    var originalBodyStream = httpContext.Response.Body;

                    using (var encryptedBodyStream = new MemoryStream())
                    {
                        GenericRsponse customResponse = null;
                        string jsonResponse = null;

                        using (var memoryStream = new MemoryStream())
                        {
                            var originalResponseBody = httpContext.Response.Body;
                            httpContext.Response.Body = memoryStream;
                            await _next(httpContext);
                            httpContext.Response.Body = originalResponseBody;
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
                            {
                                var responseData = reader.ReadToEnd();
                                customResponse = new GenericRsponse
                                {
                                    bDlTwThDpe8K = Encription.EncriptStringAES(responseData)
                                };
                                jsonResponse = JsonConvert.SerializeObject(customResponse);
                            }
                        }

                        httpContext.Response.ContentType = "application/json";
                        var contentLength = Encoding.UTF8.GetByteCount(jsonResponse);
                        httpContext.Response.ContentLength = contentLength;
                        await httpContext.Response.WriteAsync(jsonResponse);
                        await httpContext.Request.Body.DisposeAsync();
                        await httpContext.Response.Body.DisposeAsync();

                    }

                }
                catch (Exception ex)
                {

                }

            }
            else
            {
                await _next(httpContext);
            }
        }
        private Stream DecryptStream(Stream cipherStream)
        {
            Aes aes = GetEncryptionAlgorithm();
            FromBase64Transform base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
            CryptoStream base64DecodedStream = new CryptoStream(cipherStream, base64Transform, CryptoStreamMode.Read);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream decryptedStream = new CryptoStream(base64DecodedStream, decryptor, CryptoStreamMode.Read);
            return decryptedStream;
        }
        private string DecryptString(string cipherText)
        {


            Aes aes = GetEncryptionAlgorithm();

            //if(success)
            //{
            byte[] buffer = Convert.FromBase64String(cipherText);
            MemoryStream memoryStream = new MemoryStream(buffer);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();

        }

        public string DecodeUtf8Base64(string value)
        {
            var length = ((value.Length * 3) + 3) / 4;
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                if (Convert.TryFromBase64String(value, buffer, out var bytesWritten))
                {
                    return Encoding.UTF8.GetString(buffer, 0, bytesWritten);
                }
                throw new FormatException("Invalid base-64 sequence.");
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private Aes GetEncryptionAlgorithm()
        {
            Aes aes = Aes.Create();
            var secret_key = Encoding.UTF8.GetBytes(AppSettingsSimulation.EncryptKey);
            var initialization_vector = Encoding.UTF8.GetBytes(AppSettingsSimulation.EncryptIV);
            aes.Key = secret_key;
            aes.IV = initialization_vector;
            return aes;
        }


    }


    public class GenericRsponse
    {
        public string bDlTwThDpe8K { get; set; }
    }

}