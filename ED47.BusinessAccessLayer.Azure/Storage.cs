using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using ED47.Stack.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ED47.BusinessAccessLayer.Azure
{
    public class Storage
    {
        public static CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));


        public static void DeleteBlob(string container, string blobname)
        {
            var blob = Storage.GetBlob(container, blobname);
            blob.DeleteIfExists();
        }

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

        public static void CopyContainer(string containerName, string newContainer, bool replace = false)
        {
            var client = StorageAccount.CreateCloudBlobClient();
           
            var container = client.GetContainerReference(containerName.ToLower());
             if (!container.Exists())
            {
                throw new Exception("Invalid container");
            }
            var container2 = client.GetContainerReference(newContainer.ToLower());
            foreach (var b in container.ListBlobs())
            {
                var sourceBlobReference = container.GetBlockBlobReference(b.Uri.AbsoluteUri);
                var targetBlobReference = container2.GetBlockBlobReference(sourceBlobReference.Name);

                if(targetBlobReference.Exists()) continue;

                using (Stream targetStream = targetBlobReference.OpenWrite())
                {
                    try
                    {
                        sourceBlobReference.DownloadToStream(targetStream);
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
           
           
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
                            BlobContainerPublicAccessType.Off
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

        public static CloudBlockBlob StoreFile(string containerName, string virtualFilename, Stream fileStream, StorageFileConfig config)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName.ToLower());
            if (container.CreateIfNotExists())
            {

                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Off
                    });
            }
            var correctedPath = String.Join(client.DefaultDelimiter, virtualFilename.Split(new[] { '/', '\\' }));

            var blockBlob = container.GetBlockBlobReference(correctedPath.ToLower());

            if(!config.Replace && blockBlob.Exists())
                return blockBlob;

            using (fileStream)
            {
                if (config.Gzip)
                {
                    using (var compressedData = new MemoryStream())
                    {

                        using (var gzipStream = new GZipStream(compressedData, CompressionMode.Compress))
                        {

                            var buffer = new byte[1024];
                            var c = 0;

                            while ((c = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                gzipStream.Write(buffer, 0, c);
                            }
                            gzipStream.Close();
                        }


                        using (var tmp = blockBlob.OpenWrite())
                        {
                            var data = compressedData.ToArray();
                            tmp.Write(data, 0, data.Length);
                            
                        }
                    }
                }
                else
                {
                    blockBlob.UploadFromStream(fileStream);    
                }
            }

            blockBlob.Properties.ContentType = config.ContentType;
            blockBlob.Properties.CacheControl = config.CacheControl;
            blockBlob.Properties.ContentEncoding = String.IsNullOrWhiteSpace(config.ContentEncoding) ? (config.Gzip ? "gzip" : "") : config.ContentEncoding ;
            blockBlob.Properties.ContentLanguage = config.ContentLanguage;
            blockBlob.SetProperties();
            
            return blockBlob;
        }


        public static CloudBlockBlob StoreFile(string containerName, string virtualFilename, FileInfo file, StorageFileConfig config)
        {
            using (var fs = file.OpenRead())
            {
                config.ContentType = MimeTypeHelper.GetMimeType(file.FullName);
                return StoreFile(containerName, virtualFilename, fs, config);
            }
        }

        public static CloudBlockBlob StoreFile(string containerName, string virtualFilename, FileInfo file, bool replace = true)
        {
            using (var fs = file.OpenRead())
            {
                var contenttype = MimeTypeHelper.GetMimeType(file.FullName);

                return StoreFile(containerName, virtualFilename, fs,new StorageFileConfig()
                    {
                        Replace = replace,
                        ContentType = contenttype
                    });
            }
        }

        public static bool StoreDirectory(string containerName, DirectoryInfo directory, StorageFileConfig defaultConfig,
                                          bool rootAsContainer = true)
        {
            if (!directory.Exists) return false;
            var dirName = directory.Name;
            var removePath = directory.FullName.Substring(0, directory.FullName.Length - dirName.Length);
            if (rootAsContainer)
                removePath = directory.FullName;

            var files = directory.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (var fileInfo in files)
            {
                var virtualPath = fileInfo.FullName.Substring(removePath.Length + 1).ToLower();
                StoreFile(containerName.ToLower(), virtualPath, fileInfo, defaultConfig);
            }
            return true;
        }

        public static bool RemoveDirectoryLayers(string containerName, string directoryName)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName.ToLower());

            if (!container.Exists()) return false;

            var directory = container.GetDirectoryReference(directoryName.Substring(0));

            CloudBlobDirectory dira = container.GetDirectoryReference("images");
            CloudBlobDirectory dirb = dira.GetSubdirectoryReference("layers");
            CloudBlobDirectory dirc = dirb.GetSubdirectoryReference(directoryName);

            var blobs = dirc.ListBlobs().ToList();

            foreach (var blob in blobs)
            {
                if (blob is ICloudBlob)
                    ((ICloudBlob)blob).DeleteIfExists();
                else if (blob is CloudBlobDirectory)
                {
                    var dird = blob as CloudBlobDirectory;
                    var subBlobs = dird.ListBlobs().ToList();
                    foreach (var subblob in subBlobs)
                    {
                        var cloudBlob = subblob as ICloudBlob;
                        if (cloudBlob != null)
                            cloudBlob.DeleteIfExists();
                    }
                }
                
            }
                
                

            return true;
        }

        public static bool StoreDirectory(string containerName, DirectoryInfo directory, bool replace = true, bool rootAsContainer = true)
        {
            return StoreDirectory(containerName, directory, new StorageFileConfig() {Replace = replace}, rootAsContainer);
        }

    }
}