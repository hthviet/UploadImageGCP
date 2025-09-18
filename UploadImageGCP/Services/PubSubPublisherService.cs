using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using System.Threading.Tasks;

namespace UploadImageGCP.Services
{
    public class PubSubPublisherService
    {
        private readonly PublisherServiceApiClient _publisherClient;
        private readonly string _projectId;
        private readonly string _topicId;

        public PubSubPublisherService(string projectId, string topicId)
        {
            _projectId = projectId;
            _topicId = topicId;
            _publisherClient = PublisherServiceApiClient.Create();
        }

        public async Task<string> PublishMessageAsync(string message)
        {
            var topicName = TopicName.FromProjectTopic(_projectId, _topicId);
            var pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(message)
            };
            var response = await _publisherClient.PublishAsync(topicName, new[] { pubsubMessage });
            return response.MessageIds[0];
        }
    }
}
