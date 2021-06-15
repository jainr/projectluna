using Microsoft.OpenApi;
using Microsoft.OpenApi.CSharpAnnotations.DocumentGeneration;
using Microsoft.OpenApi.CSharpAnnotations.DocumentGeneration.Models;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Luna.Common.Swagger
{
    public class SwaggerGenerator
    {
        static void GenerateSwagger(string serviceName, string config)
        {
            var assemblyPathList = new List<string>();

            if (serviceName == "gateway")
            {
                assemblyPathList.Add(Path.GetFullPath($@"..\..\..\..\..\{serviceName}\functions\bin\{config}\netcoreapp3.1\Luna.{serviceName}.Functions.dll"));
                // Gateway service references all other public client libraries
                foreach (var name in services)
                {
                    if (name != "gateway")
                    {
                        assemblyPathList.Add(Path.GetFullPath($@"..\..\..\..\..\{name}\public\bin\{config}\netcoreapp3.1\Luna.{name}.Public.Client.dll"));
                    }
                }
            }
            else
            {
                assemblyPathList.Add(Path.GetFullPath($@"..\..\..\..\..\{serviceName}\functions\bin\{config}\netcoreapp3.1\Luna.{serviceName}.Functions.dll"));
                assemblyPathList.Add(Path.GetFullPath($@"..\..\..\..\..\{serviceName}\public\bin\{config}\netcoreapp3.1\Luna.{serviceName}.Public.Client.dll"));
            }

            var input = new OpenApiGeneratorConfig(
                annotationXmlDocuments: new List<XDocument>()
                {
                    XDocument.Load(Path.GetFullPath($@"..\..\..\..\..\{serviceName}\functions\Luna.{serviceName}.Functions.xml")),
                },
                assemblyPaths: assemblyPathList,
                openApiDocumentVersion: "V1",
                filterSetVersion: FilterSetVersion.V1
            );

            GenerationDiagnostic result;

            var generator = new OpenApiGenerator();


            IDictionary<DocumentVariantInfo, OpenApiDocument> openApiDocuments = generator.GenerateDocuments(
                openApiGeneratorConfig: input,
                generationDiagnostic: out result,
                openApiDocumentGenerationSettings: new OpenApiDocumentGenerationSettings(true)
            );

            foreach (var diag in result.OperationGenerationDiagnostics)
            {
                if (diag.Errors.Count > 0)
                {
                    foreach (var error in diag.Errors)
                    {
                        Console.WriteLine($"[Error][{error.ExceptionType}]: {error.Message}.");
                    }
                }
            }

            File.WriteAllText($@"..\..\..\..\..\swagger\{serviceName}_service_swagger.yaml", openApiDocuments.First().Value.SerializeAsYaml(OpenApiSpecVersion.OpenApi2_0));
        }

        static void PrintUsage()
        {
            Console.WriteLine("Invalid input arguments.");
            Console.WriteLine("Usage:");
            Console.WriteLine("swaggergenerator.exe : generate swagger for all services");
            Console.WriteLine("swaggergenerator.exe -r : generate swagger for all services for release build");
            Console.WriteLine("swaggergenerator.exe -s serviceName : generate swagger for the specified service");
            Console.WriteLine("swaggergenerator.exe -r -s serviceName : generate swagger for the specified service for release build");
        }

        private static string[] services = new string[] { "gallery", "partner", "publish", "pubsub", "rbac", "gateway" };

        static void Main(string[] args)
        {
            string[] servicesWithoutSwaggerYet = new string[] {"mockup", "provision", "routing" };
            string config = "Debug";

            if (args.Length == 0)
            {
                foreach (var name in services)
                {
                    GenerateSwagger(name, config);
                }
            }
            else if (args.Length == 1)
            {
                if (!args[0].Equals("-r"))
                {
                    PrintUsage();
                    return;
                }

                foreach (var name in services)
                {
                    GenerateSwagger(name, "Release");
                }
            }
            else if (args.Length == 2)
            {
                if (!args[0].Equals("-s"))
                {
                    PrintUsage();
                    return;
                }

                string serviceName = args[1];
                if (!services.Contains(serviceName))
                {
                    Console.WriteLine($"Invalid service name {serviceName}.");
                    Console.WriteLine("Valid service names are:");
                    foreach (var name in services)
                    {
                        Console.WriteLine(name);
                    }
                    return;
                }

                GenerateSwagger(serviceName, config);
            }
            else if (args.Length == 3)
            {
                if (!args[0].Equals("-r"))
                {
                    PrintUsage();
                    return;
                }

                if (!args[1].Equals("-s"))
                {
                    PrintUsage();
                    return;
                }

                string serviceName = args[2];
                if (!services.Contains(serviceName))
                {
                    Console.WriteLine($"Invalid service name {serviceName}.");
                    Console.WriteLine("Valid service names are:");
                    foreach (var name in services)
                    {
                        Console.WriteLine(name);
                    }
                    return;
                }

                GenerateSwagger(serviceName, "Release");
            }
            else
            {
                PrintUsage();
                return;
            }
        }
    }
}
