using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;

namespace DataAccess
{
    public class PubSubRepository
    {
        private Topic GetOrCreateTopic(string name)
        {
            PublisherServiceApiClient client = PublisherServiceApiClient.Create();
            try
            {
                return client.GetTopic(new TopicName("programming-for-the-cloud", name));
            }
            catch (RpcException ex)
            {
                if (ex.Status.StatusCode == StatusCode.NotFound)
                    return client.CreateTopic(new TopicName("programming-for-the-cloud", name));
            }
            return null;
        }

        public void PublishMessage(string message, string topicName)
        {
            PublisherServiceApiClient client = PublisherServiceApiClient.Create();
            Topic MyTopic = GetOrCreateTopic(topicName);

            List<PubsubMessage> myMessages = new List<PubsubMessage>();
            myMessages.Add(new PubsubMessage()
            {
                //create mailmessage object instead of string
                //serialize mailmessage and convert to binary by using binary formatter
                //and pass to the below: ByteString.CopyFrom <- array of bytes or use ByteString.FromStream
                MessageId = Guid.NewGuid().ToString(),
                PublishTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                Data = ByteString.CopyFromUtf8(message)
            });

            try
            {
                client.Publish(MyTopic.TopicName, myMessages);
            }
            catch (Exception ex)
            {

            }
        }

        private Subscription GetOrCreateSubscription(string name, string topicName)
        {
            SubscriberServiceApiClient client = SubscriberServiceApiClient.Create();
            try
            {
                return client.GetSubscription(new SubscriptionName("programming-for-the-cloud", name));
            }
            catch (RpcException ex)
            {
                if (ex.Status.StatusCode == StatusCode.NotFound)
                    return client.CreateSubscription(new SubscriptionName("programming-for-the-cloud", name), GetOrCreateTopic(topicName).TopicName, null, 60);
            }
            return null;
        }
    }
}
