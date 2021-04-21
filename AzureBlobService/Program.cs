using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace AzureBlobService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                string parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                    default:
                        var service = new Service1();
                        service.TestStartupAndStop(args);
                        break;
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]{
                    new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }
            
        }
    }
}

