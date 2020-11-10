﻿using System;
using System.Configuration;
using System.IO.Abstractions;
using Blaise.Case.Nisra.Processor.CloudStorage;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.Core;
using Blaise.Case.Nisra.Processor.Core.Cases;
using Blaise.Case.Nisra.Processor.Core.Configuration;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Data;
using Blaise.Case.Nisra.Processor.Data.Interfaces;
using Blaise.Case.Nisra.Processor.Data.Mappers;
using Blaise.Case.Nisra.Processor.MessageBroker;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Mappers;
using Blaise.Case.Nisra.Processor.WindowsService.Interfaces;
using Blaise.Nuget.Api;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;
using Unity;

namespace Blaise.Case.Nisra.Processor.WindowsService.Ioc
{
    public class UnityProvider
    {
        private readonly IUnityContainer _unityContainer;

        public UnityProvider()
        {
            _unityContainer = new UnityContainer();
            //blaise services
            _unityContainer.RegisterType<IBlaiseApi, BlaiseApi>();

            //system abstractions
            _unityContainer.RegisterType<IFileSystem, FileSystem>();

            // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
            // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
            // When running in Release, the service will be running as compute account which will have access to all buckets. In test we need to get credentials
            var credentialKey = ConfigurationManager.AppSettings["GOOGLE_APPLICATION_CREDENTIALS"];
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialKey);

            _unityContainer.RegisterType<IConfigurationProvider, LocalConfigurationProvider>();
#else
            _unityContainer.RegisterType<IConfigurationProvider, ConfigurationProvider>();
#endif
            _unityContainer.RegisterType<IStorageClientProvider, CloudStorageClientProvider>();
            _unityContainer.RegisterSingleton<IFluentQueueApi, FluentQueueApi>();
            _unityContainer.RegisterFactory<ILog>(f => LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType));

            //mappers
            _unityContainer.RegisterType<IMessageModelMapper, MessageModelMapper>();
            _unityContainer.RegisterType<IDataRecordMapper, DataRecordMapper>();

            //handlers
            _unityContainer.RegisterType<IMessageHandler, MessageHandler>();

            //services   
            _unityContainer.RegisterType<IBlaiseApiService, BlaiseApiService>();
            _unityContainer.RegisterType<IUpdateCaseService, UpdateCaseService>();
            _unityContainer.RegisterType<IStorageService, StorageService>();
            _unityContainer.RegisterType<IProcessNisraCasesService, ProcessNisraCasesService>();
            _unityContainer.RegisterType<IProcessNisraFilesService, ProcessNisraFilesService>();

            //queue service
            _unityContainer.RegisterType<IMessageBrokerService, MessageBrokerService>();

            //main service
            _unityContainer.RegisterType<IInitialiseWindowsService, InitialiseWindowsService>();
        }

        public T Resolve<T>()
        {
            return _unityContainer.Resolve<T>();
        }
    }
}
