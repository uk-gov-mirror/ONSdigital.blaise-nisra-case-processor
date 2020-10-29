﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Blaise.Case.Nisra.Processor.Interfaces.Providers;
using Google.Cloud.Storage.V1;

namespace Blaise.Case.Nisra.Processor.Providers
{
    public class CloudStorageClientProvider : IStorageClientProvider
    {
        private readonly IFileSystem _fileSystem;

        private readonly string _processedFolder;

        private StorageClient _storageClient;

        public CloudStorageClientProvider(
            IConfigurationProvider configurationProvider,
            IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            _processedFolder = configurationProvider.CloudProcessedFolder;
        }

        public StorageClient GetStorageClient()
        {
            var client = _storageClient;

            if (client != null)
            {
                return client;
            }

            return (_storageClient = StorageClient.Create());
        }

        public void DisposeStorageClient()
        {
            _storageClient?.Dispose();
            _storageClient = null;
        }
        
        public IEnumerable<string> GetListOfFilesInBucket(string bucketName)
        {
            var storageClient = GetStorageClient();
            var availableObjectsInBucket = storageClient.ListObjects(bucketName, "");

            //get all objects that are not folders
            return availableObjectsInBucket.Where(f => f.Size > 0).Select(f => f.Name).ToList();
        }

        public void Download(string bucketName, string fileName, string filePath)
        {
            var storageClient = GetStorageClient();
            using (var fileStream = _fileSystem.FileStream.Create(filePath, FileMode.OpenOrCreate))
            {
                storageClient.DownloadObject(bucketName, fileName, fileStream);
            }
        }

        public void MoveFileToProcessedFolder(string bucketName, string file)
        {
            var storageClient = GetStorageClient();
            foreach (var storageObject in storageClient.ListObjects(bucketName, ""))
            {
                if (!string.Equals(storageObject.Name, file, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var fileName = _fileSystem.Path.GetFileName(file);
                var filePath = _fileSystem.Path.GetDirectoryName(file).Replace("\\", "/");
                var processedPath = $"{filePath}/{_processedFolder}/{fileName}";
                
                storageClient.CopyObject(bucketName, storageObject.Name, bucketName, processedPath);
                storageClient.DeleteObject(bucketName, storageObject.Name);

                return;
            }
        }

        public void Dispose()
        {
            DisposeStorageClient();
        }
    }
}