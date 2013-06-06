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

        public static bool RemoveContainer(string containerName, BlobContainerPermissions permissions)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);

            if (container != null)
            {
                container.DeleteIfExists();
                return true;                
            }
            else
                return false;
        }

        public static CloudBlockBlob GetBlob(IBlobItem item)
        {
        
            return Storage.GetBlob(item.ContainerName, item.BlobName);
        
        }

        public static CloudBlockBlob GetBlob(string containerName, string blobName)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var name = blobName.StartsWith("/") ? blobName.Substring(1) : blobName;
            var container = client.GetContainerReference(containerName.ToLower());
            if (!container.Exists())
            {
                throw new Exception("Invalid container");
            }
            var blockBlob = container.GetBlockBlobReference(name);
            if (!blockBlob.Exists())
            {
                throw new Exception("Invalid blob");
            }

            return blockBlob;
        }

        public static Uri StoreBlob(string containerName, string blobName, Stream fileStream)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName.ToLower());
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

        public static Uri StoreBlob(string containerName, string blobName, FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                return StoreBlob(containerName, blobName, fs);
            }


        }

        public static CloudBlockBlob StoreFile(string containerName, string virtualFilename, Stream fileStream, bool replace = true)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName.ToLower());
            if (container.CreateIfNotExists())
            {

                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Blob
                    });
            }
            var correctedPath = String.Join(client.DefaultDelimiter, virtualFilename.Split(new[] { '/', '\\' }));

            var blockBlob = container.GetBlockBlobReference(correctedPath.ToLower());

            if(!replace && blockBlob.Exists())
                return blockBlob;

            using (fileStream)
            {
                blockBlob.UploadFromStream(fileStream);
              
            }
            return blockBlob;
        }

        public static CloudBlockBlob StoreFile(string containerName, string virtualFilename, FileInfo file, bool replace = true)
        {
            using (var fs = file.OpenRead())
            {
                return StoreFile(containerName, virtualFilename, fs, replace);
            }
        }

        public static bool StoreDirectory(string containerName, DirectoryInfo directory, bool replace = true, bool rootAsContainer = true)
        {
            if (!directory.Exists) return false;
            var dirName = directory.Name;
            var removePath = directory.FullName.Substring(0, directory.FullName.Length - dirName.Length);
            if (rootAsContainer)
                removePath = directory.FullName;

            var files = directory.GetFiles("*.*", SearchOption.AllDirectories);


            foreach (var fileInfo in files)
            {
                var virtualPath = fileInfo.FullName.Substring(removePath.Length+1).ToLower();
                StoreFile(containerName.ToLower(), virtualPath, fileInfo, replace);
            }
            return true;
        }

    }
}