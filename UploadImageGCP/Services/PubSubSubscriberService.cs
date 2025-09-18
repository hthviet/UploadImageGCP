
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UploadImageGCP.Services
{
    public class PubSubSubscriberService : BackgroundService
    {
        private readonly string _projectId;
        private readonly string _subscriptionId;

        public PubSubSubscriberService(string projectId, string subscriptionId)
        {
            _projectId = projectId;
            _subscriptionId = subscriptionId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, _subscriptionId);
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            await subscriber.StartAsync((PubsubMessage message, CancellationToken ct) =>
            {
                // Process the message
                var text = message.Data.ToStringUtf8();
                Console.WriteLine($"Received: {text}");
                // Return Ack to acknowledge
                return Task.FromResult(SubscriberClient.Reply.Ack);
            });

            // Keep running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await subscriber.StopAsync(CancellationToken.None);
        }
    }
}
