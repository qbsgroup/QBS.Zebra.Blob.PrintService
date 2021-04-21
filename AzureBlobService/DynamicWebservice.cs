using System;
using System.CodeDom;
using System.Web.Services.Description;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Net;
using System.Xml;

namespace AzureBlobService
{
    static class DynamicWebservice
    {
        public static void Consume(string wsdl, Log log)
        {
            // create an assembly from the web service description
            Assembly webServiceAssembly = NAVPageDynamicWebReference.BuildAssemblyFromWSDL(new Uri(wsdl));

            // Create Service Reference
            Type[] types = webServiceAssembly.GetExportedTypes();
            //foreach (Type type in types)
            //    Console.WriteLine(type.ToString());
            Type serviceType = webServiceAssembly.GetType("Scansys");
            object service = Activator.CreateInstance(serviceType);
            PropertyInfo useDefaultCredentials = service.GetType().GetProperty("UseDefaultCredentials");
            useDefaultCredentials.SetValue(service, (object)true, new object[] { });

            log.Add("Start Invoking ProcessBuffer: " + wsdl);
            try
            {
                MethodInfo processBuffer = service.GetType().GetMethod("ProcessBuffer");
                bool result = (bool)processBuffer.Invoke(service, null);
                log.Add("Done Invoking ProcessBuffer with result: " + result.ToString());
            }
            catch (Exception e)
            {
                log.Add("ERROR in ProcessBuffer result ... \n- Exception:" + e.Message);
                if (e.InnerException != null)
                    log.Add("- Innerexception: " + e.InnerException.Message);
            }
        }

        public static class NAVPageDynamicWebReference
        {
            /// <summary>
            /// Builds the web service description importer, which allows us to generate a proxy class based on the 
            /// content of the WSDL described by the XmlTextReader.
            /// </summary>
            /// <param name="xmlreader">The WSDL content, described by XML.</param>
            /// <returns>A ServiceDescriptionImporter that can be used to create a proxy class.</returns>
            private static ServiceDescriptionImporter BuildServiceDescriptionImporter(XmlTextReader xmlreader)
            {
                // make sure xml describes a valid wsdl
                if (!ServiceDescription.CanRead(xmlreader))
                    throw new Exception("Invalid Web Service Description");

                // parse wsdl
                ServiceDescription serviceDescription = ServiceDescription.Read(xmlreader);

                // build an importer, that assumes the SOAP protocol, client binding, and generates properties
                ServiceDescriptionImporter descriptionImporter = new ServiceDescriptionImporter();
                descriptionImporter.ProtocolName = "Soap";
                descriptionImporter.AddServiceDescription(serviceDescription, null, null);
                descriptionImporter.Style = ServiceDescriptionImportStyle.Client;
                descriptionImporter.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

                return descriptionImporter;
            }


            /// <summary>
            /// Compiles an assembly from the proxy class provided by the ServiceDescriptionImporter.
            /// </summary>
            /// <param name="descriptionImporter"></param>
            /// <returns>An assembly that can be used to execute the web service methods.</returns>
            private static Assembly CompileAssembly(ServiceDescriptionImporter descriptionImporter)
            {
                // a namespace and compile unit are needed by importer
                CodeNamespace codeNamespace = new CodeNamespace();
                CodeCompileUnit codeUnit = new CodeCompileUnit();

                codeUnit.Namespaces.Add(codeNamespace);

                ServiceDescriptionImportWarnings importWarnings = descriptionImporter.Import(codeNamespace, codeUnit);

                if (importWarnings == 0) // no warnings
                {
                    // create a c# compiler
                    CodeDomProvider compiler = CodeDomProvider.CreateProvider("CSharp");

                    // include the assembly references needed to compile
                    string[] references = new string[2] { "System.Web.Services.dll", "System.Xml.dll" };

                    CompilerParameters parameters = new CompilerParameters(references);

                    // compile into assembly
                    CompilerResults results = compiler.CompileAssemblyFromDom(parameters, codeUnit);

                    foreach (CompilerError oops in results.Errors)
                    {
                        // trap these errors and make them available to exception object
                        throw new Exception("Compilation Error Creating Assembly");
                    }

                    // all done....
                    return results.CompiledAssembly;
                }
                else
                {
                    // warnings issued from importers, something wrong with WSDL
                    throw new Exception("Invalid WSDL");
                }
            }

            /// <summary>
            /// Builds an assembly from a web service description.
            /// The assembly can be used to execute the web service methods.
            /// </summary>
            /// <param name="webServiceUri">Location of WSDL.</param>
            /// <returns>A web service assembly.</returns>
            public static Assembly BuildAssemblyFromWSDL(Uri webServiceUri)
            {
                if (String.IsNullOrEmpty(webServiceUri.ToString()))
                    throw new Exception("Web Service Not Found");

                WebRequest request = WebRequest.Create(webServiceUri.ToString());
                request.Method = "GET";
                request.ContentType = "text/xml";
                request.Timeout = 10000;
                request.UseDefaultCredentials = true;
                WebResponse response = request.GetResponse();
                XmlTextReader xmlreader = new XmlTextReader(response.GetResponseStream());
                ServiceDescriptionImporter descriptionImporter = BuildServiceDescriptionImporter(xmlreader);
                return CompileAssembly(descriptionImporter);
            }
        }
    }
}
