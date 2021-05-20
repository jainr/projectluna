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
            var input = new OpenApiGeneratorConfig(
                annotationXmlDocuments: new List<XDocument>()
                {
                    XDocument.Load(Path.GetFullPath($@"..\..\..\..\..\{serviceName}\functions\Luna.{serviceName}.Functions.xml")),
                },
                assemblyPaths: new List<string>()
                {
                    Path.GetFullPath($@"..\..\..\..\..\{serviceName}\functions\bin\{config}\netcoreapp3.1\Luna.{serviceName}.Functions.dll"),
                    Path.GetFullPath($@"..\..\..\..\..\{serviceName}\public\bin\{config}\netcoreapp3.1\Luna.{serviceName}.Public.Client.dll")

                },
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

        static void Main(string[] args)
        {
            string[] services = new string[] { "gallery", "partner", "publish", "pubsub", "rbac" };
            string[] servicesWithoutSwaggerYet = new string[] { "gateway", "mockup", "provision", "routing" };
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

                GenerateSwagger(serviceName, config);
            }
            else
            {
                PrintUsage();
                return;
            }

        }



    }
}
