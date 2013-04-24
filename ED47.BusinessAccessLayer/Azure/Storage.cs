using System;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ED47.BusinessAccessLayer.Azure
{
    /// <summary>
    /// 
    /// </summary>
    public class Storage
    {
        public static CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));


        public static bool CreateContainer(string containerName, BlobContainerPermissions permissions)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            if (container.CreateIfNotExists())
            {
                container.SetPermissions(permissions);
                return true;
            }
            return false;
        }


        public static Uri StoreBlob(string containerName, string blobName, Stream fileStream)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            if (container.CreateIfNotExists())
            {

                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Blob
                    });
            }

            var blockBlob = container.GetBlockBlobReference(blobName);

            using (fileStream)
            {
                blockBlob.UploadFromStream(fileStream);
            }
            return blockBlob.Uri;
        }

        public static Uri StoreFile(string containerName, string virtualFilename, Stream fileStream)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
           
            var correctedPath = String.Join(client.DefaultDelimiter, virtualFilename.Split(new[] { '/', '\\' }));

            var blockBlob = container.GetBlockBlobReference(correctedPath.ToLower());

            using (fileStream)
            {
                blockBlob.UploadFromStream(fileStream);
            }
            return blockBlob.Uri;
        }

        public static Uri StoreFile(string containerName, string virtualFilename, FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                return StoreFile(containerName, virtualFilename, fs);
            }


        }





    }
}