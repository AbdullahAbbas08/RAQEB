using Microsoft.AspNetCore.SpaServices.AngularCli;
using NSwag.CodeGeneration.TypeScript;
using NSwag;
using System.Net;
using NSwag.CodeGeneration.OperationNameGenerators;
using System.Text.RegularExpressions;

namespace Raqeb.Middlewares.Middlewares 
{
    public static partial class _Pipeline
    {
        public static void SinglePageApp(this WebApplication app, Microsoft.Extensions.Hosting.IHostingEnvironment env, IConfiguration Configuration)
        {
            try
            {
                app.UseSpaLoadingPage();
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                    //spa.Options.StartupTimeout = TimeSpan.FromMinutes(10);
                });

            }
            catch (Exception ex)
            {
            }


            if (env.IsDevelopment())
            {
                Task.Run(() =>
                {
                    try
                    {
                        var handler = new HttpClientHandler() { Credentials = CredentialCache.DefaultCredentials };
                        HttpClient httpClient = new HttpClient(handler);
                        string SourceDocumentAbsoluteUrl = Configuration["SwaggerToTypeScriptClientGeneratorSettings:SourceDocumentAbsoluteUrl"];
                        string OutputDocumentRelativePath = Configuration["SwaggerToTypeScriptClientGeneratorSettings:OutputDocumentRelativePath"];
                        using (Stream contentStream = httpClient.GetStreamAsync(SourceDocumentAbsoluteUrl).Result)
                        using (StreamReader streamReader = new StreamReader(contentStream))
                        {
                            string json = streamReader.ReadToEnd();
                            var document = OpenApiDocument.FromJsonAsync(json).Result;
                            var settings = new TypeScriptClientGeneratorSettings
                            {
                                OperationNameGenerator = new SwaggerClientOperationNameGeneratorViewer(),
                                ClassName = "SwaggerClient",
                                Template = TypeScriptTemplate.Angular,
                                RxJsVersion = 7.0M,
                                HttpClass = HttpClass.HttpClient,
                                InjectionTokenType = InjectionTokenType.InjectionToken,
                                BaseUrlTokenName = "API_BASE_URL",
                                WrapDtoExceptions = true,
                                TypeScriptGeneratorSettings = { TypeScriptVersion = 4.6M }
                            };

                            var generator = new TypeScriptClientGenerator(document, settings);
                            string code = generator.GenerateFile();
                            new FileInfo(OutputDocumentRelativePath).Directory.Create();
                            try
                            {
                                File.WriteAllText(OutputDocumentRelativePath, code);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                });

            }

        }
    }

    public class SwaggerClientOperationNameGeneratorViewer : IOperationNameGenerator
    {
        public bool SupportsMultipleClients { get; } = true;

        /// <summary>Converts the path to an operation name.</summary>
        /// <param name="path">The HTTP path.</param>
        /// <returns>The operation name.</returns>
        public static string ConvertPathToName(string path)
        {
            var name = Regex.Replace(path, @"\{.*?\}", "")
                .Split('/', '-', '_')
                .Where(part => !part.Contains("{") && !string.IsNullOrWhiteSpace(part))
                .Aggregate("", (current, part) =>
                {
                    part.Replace("get", "Get");
                    part.Replace("post", "Post");
                    part.Replace("delete", "Delete");
                    part.Replace("update", "Update");
                    return (current + CapitalizeFirst(part));
                });

            if (string.IsNullOrEmpty(name))
            {
                name = "Index"; // Root path based operation?
            }

            return name;
        }

        /// <summary>Capitalizes first letter.</summary>
        /// <param name="name">The name to capitalize.</param>
        /// <returns>Capitalized name.</returns>
        internal static string CapitalizeFirst(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var capitalized = name;
            return char.ToUpperInvariant(capitalized[0]) + (capitalized.Length > 1 ? capitalized.Substring(1) : "");
        }



        public string GetClientName(NSwag.OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
        {
            return string.Empty;
        }

        public string GetOperationName(NSwag.OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
        {
            var operationName = ConvertPathToName(path);
            //var hasNameConflict = document.Paths
            //    .SelectMany(pair => pair.Value.Select(p => new { Path = pair.Key.Trim('/'), HttpMethod = p.Key, Operation = p.Value }))
            //    .Where(op =>
            //        GetClientName(document, op.Path, op.HttpMethod, op.Operation) == GetClientName(document, path, httpMethod, operation) &&
            //        ConvertPathToName(op.Path) == operationName
            //    )
            //    .ToList().Count > 1;

            //if (hasNameConflict)
            //{
            operationName += CapitalizeFirst(httpMethod.ToString());
            //}
            if (string.IsNullOrEmpty(httpMethod.ToString()))
            {
                Console.WriteLine("egtrer");
            }

            return operationName;
        }
    }
}
