using System;
using System.Timers;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Xml;

namespace AzureBlobService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        Log log = new Log();

        public Service1()
        {
            InitializeComponent();

            timer.AutoReset = false;
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 10000;
        }

        protected override void OnStart(string[] args)
        {
            log.Add("Azure_Blob_Service Service up");
            timer.Enabled = true;
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            timer.Stop();
            log.Add("Azure_Blob_Service stopped");
        }
        internal void TestStartupAndStop(string[] args)
        {
            //this.OnStart(args);
            DoEvents();
            //this.OnStop();
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            log.Add("Timer elapsed, starting tasks");
            timer.Stop();
            DoEvents();
            timer.Start();
        }

        public void DoEvents()
        {
            try
            {
                AzureStorage azureStorage = new AzureStorage();
                Config config = new Config();
                XmlDocument xdConfig = new XmlDocument();

                try
                {
                    xdConfig.Load(config.GetConfigFile());
                }
                catch (Exception e)
                {
                    log.Add("ERROR; Configuratiebestand niet aanwezig (" + e.Message + ")");
                    return;
                }

                XmlNodeList xnlFtp = xdConfig.GetElementsByTagName("customer");
                log.Add(String.Format("Found {0} tasks", xnlFtp.Count));

                for (int i = 0; i < xnlFtp.Count; i++)
                {
                    XmlNode ftpNode = xnlFtp[i];

                    if (ConfigAttributeExists(ftpNode, "container_download") && ConfigAttributeExists(ftpNode, "download_folder"))
                        azureStorage.DownloadAll(ftpNode.Attributes["container_download"].Value, ftpNode.Attributes["download_folder"].Value);

                    if (ConfigAttributeExists(ftpNode, "container_upload") && ConfigAttributeExists(ftpNode, "upload_folder"))
                        azureStorage.UploadAll(ftpNode.Attributes["container_upload"].Value, ftpNode.Attributes["upload_folder"].Value);

                    /*if (ConfigAttributeExists(ftpNode, "webservice"))
                    {
                        DynamicWebservice.Consume(ftpNode.Attributes["webservice"].Value, log);
                    }*/

                    if (ConfigAttributeExists(ftpNode, "container_print"))
                        azureStorage.PrintAll(ftpNode.Attributes["container_print"].Value, ftpNode.Attributes["printer_name"].Value);

                    if (ConfigAttributeExists(ftpNode, "container_delete"))
                        azureStorage.DeleteAllFromContainer(ftpNode.Attributes["container_delete"].Value);

                }
            }
            catch (Exception e)
            {
                log.Add("Taken konden niet worden uitgevoerd, error: " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private bool ConfigAttributeExists(XmlNode ftpNode, string nodeAttribute)
        {
            try
            {
                XmlNode node = ftpNode.Attributes[nodeAttribute];
                return node != null && !string.IsNullOrEmpty(node.Value);
            }
            catch (Exception ex)
            {
                log.Add("Error in configuratiebestand bij het valideren van attribuut. Fout: " + ex.Message);
                return false;
            }
        }
    }
}
