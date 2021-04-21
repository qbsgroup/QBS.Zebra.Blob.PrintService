using AzureBlobService.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Web;

namespace AzureBlobService
{
    public class AzureStorage
    {
        private CloudStorageAccount cloudStorageAccount;
        private CloudBlobContainer blobContainer;
        private string connectionString = "DefaultEndpointsProtocol=https;AccountName=YOURACCOUNT;AccountKey=YOURKEY;EndpointSuffix=core.windows.net";
        private Log log;

        public AzureStorage()
        {
            cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            log = new Log();
            /*var connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                docParameters.AzureBlobStorageURL,
                docParameters.AzureSharedAccessKey);*/
        }

        public void DeleteAllFromContainer(string containerName)
        {
            int i = 0;
            if (AssertBlobContainer(containerName))
            {
                foreach (var item in blobContainer.ListBlobs())
                {
                    string name = ((CloudBlockBlob)item).Name; CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);
                    blockBlob.DeleteIfExists();
                }
            }
            if (i > 0)
                log.Add(i.ToString() + " files deleted from Azure");
        }

        public void DownloadAll(string containerName, string downloadTo)
        {
            int i = 0;
            if (AssertBlobContainer(containerName))
            {
                foreach (var item in blobContainer.ListBlobs())
                {
                    string name = ((CloudBlockBlob)item).Name;
                    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);
                    string path = downloadTo + @"\" + name;
                    blockBlob.DownloadToFile(path, FileMode.OpenOrCreate);
                    i++;
                }
            }
            if (i > 0)
                log.Add(i.ToString() + " files downloaded from Azure");
        }

        public void PrintAll(string containerName, string printerName)
        {
            int i = 0;
            if (AssertBlobContainer(containerName))
            {
                foreach (var item in blobContainer.ListBlobs())
                {
                    string name = ((CloudBlockBlob)item).Name;
                    try
                    {
                        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);
                        //byte[] zplFile = new byte[blockBlob.StreamWriteSizeInBytes];
                        //var fileSize = blockBlob.DownloadToByteArray(zplFile, 0);
                        var fileContent = blockBlob.DownloadText();
                        log.Add("Sending " + name + " to printer " + printerName);
                        //PrintService.PrintZPL(zplFile, printerIP);
                        //PrintService.PrintJob(zplFile, name, printerName);
                        RawPrinterService.SendToPrinter(name, fileContent, printerName);
                    }
                    catch(Exception ex)
                    {
                        log.Add("Couldn't print file " + name + ". Exception: " + ex.Message);
                    }
                    i++;
                }
            }
            log.Add(i.ToString() + " files printed from Azure");
        }

        public void UploadAll(string containerName, string localFolder)
        {
            int i = 0;
            if (AssertBlobContainer(containerName))
            {
                foreach (string file in Directory.EnumerateFiles(localFolder, "*.*", SearchOption.AllDirectories))
                {
                    string cloudFileName = file.Substring(3).Replace('\\', '/').Replace('#','_');
                    CloudBlockBlob blob = blobContainer.GetBlockBlobReference(cloudFileName);
                    try
                    {
                        blob.Properties.ContentType = MimeMapping.GetMimeMapping(file);
                    }
                    catch (Exception e)
                    { }
                    //if (!blob.Exists())
                    //{
                    try
                    {
                        blob.UploadFromFile(file, FileMode.Open);
                        log.Add("It appears that file " + file + "(uploaded as " + cloudFileName + ") has been uploaded. No exception thrown. URI: " + blob.Uri);

                        blob.FetchAttributes();
                        if (blob.Properties.Length > 0)
                        {
                            log.Add("Blob length: " + blob.Properties.Length);
                            File.Delete(file);
                            i++;
                        }
                        else
                        {
                            log.Add("Couldn't upload file " + file + "(uploaded as " + cloudFileName + "). No exception thrown");
                        }
                    }
                    catch (Exception e)
                    {
                        log.Add("Couldn't upload file " + file + "(uploaded as " + cloudFileName + "). Exception: " + e.Message);
                    }
                    //}
                }
            }
            if (i > 0)
                log.Add(i.ToString() + " files uploaded to Azure");
        }

        private bool AssertBlobContainer(string containerName)
        {
            // only do once
            /*if (blobContainer == null)
            {
                lock (this.blobContainerLockObj)
                {
                    if (this.blobContainer == null)
                    {*/

            var client = cloudStorageAccount.CreateCloudBlobClient();
            blobContainer = client.GetContainerReference(containerName.ToLowerInvariant());
            
            //if (!blobContainer.Exists())
            //{
            //    log.Add("Container " + containerName + " does not exist in Azure");
            //}
            //      }
            //    }
            //}
            return (blobContainer.Exists());
        }
    }
}
 