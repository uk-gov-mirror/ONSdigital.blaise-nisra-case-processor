﻿
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IQueueService
    {
        void Subscribe(IMessageHandler messageHandler);

        void CancelAllSubscriptions();
    }
}