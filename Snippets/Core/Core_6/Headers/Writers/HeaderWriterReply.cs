﻿namespace Core6.Headers.Writers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using NServiceBus;
    using NServiceBus.MessageMutator;
    using NUnit.Framework;
    using Operations.Msmq;

    [TestFixture]
    public class HeaderWriterReply
    {
        static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);
        string endpointName = "HeaderWriterReplyV6";

        [SetUp]
        [TearDown]
        public void Setup()
        {
            QueueDeletion.DeleteQueuesForEndpoint(endpointName);
        }

        [Test]
        public async Task Write()
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            var callbackTypes = typeof(RequestResponseExtensions).Assembly.GetTypes();
            var typesToScan = TypeScanner.NestedTypes<HeaderWriterReply>(callbackTypes);
            endpointConfiguration.SetTypesToScan(typesToScan);
            endpointConfiguration.ScaleOut().InstanceDiscriminator("A");
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.RegisterComponents(c => c.ConfigureComponent<Mutator>(DependencyLifecycle.InstancePerCall));
            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            await endpointInstance.SendLocal(new MessageToSend())
                .ConfigureAwait(false);
            ManualResetEvent.WaitOne();
        }

        class MessageToSend :
            IMessage
        {
        }

        class MessageHandler :
            IHandleMessages<MessageToSend>
        {
            public Task Handle(MessageToSend message, IMessageHandlerContext context)
            {
                var messageToReply = new MessageToReply();
                return context.Reply(messageToReply);
            }
        }

        class MessageToReply :
            IMessage
        {
        }

        class Mutator :
            IMutateIncomingTransportMessages
        {

            public Task MutateIncoming(MutateIncomingTransportMessageContext context)
            {
                var headers = context.Headers;
                if (context.IsMessageOfTye<MessageToReply>())
                {
                    var headerText = HeaderWriter.ToFriendlyString<HeaderWriterReply>(headers);
                    SnippetLogger.Write(
                        text: headerText,
                        suffix: "Replying",
                        version: "6");
                    ManualResetEvent.Set();
                }
                if (context.IsMessageOfTye<MessageToSend>())
                {
                    var headerText = HeaderWriter.ToFriendlyString<HeaderWriterReply>(headers);
                    SnippetLogger.Write(
                        text: headerText,
                        suffix: "Sending",
                        version: "6");
                }
                return Task.FromResult(0);
            }
        }
    }
}