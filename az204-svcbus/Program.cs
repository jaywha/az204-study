using Azure.Messaging.ServiceBus;
using System;

namespace az204_svcbus
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            // connection string to your Service Bus namespace
            string connectionString = args[0];

            // name of your Service Bus topic
            string queueName = "az204-queue";

            // the client that owns the connection and can be used to create senders and receivers
            ServiceBusClient client;

            // the sender used to publish messages to the queue
            ServiceBusSender sender;

            // Create the clients that we'll use for sending and processing messages.
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(queueName);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = WaitOn<ServiceBusMessageBatch>(sender.CreateMessageBatchAsync()).Result;

            for (int i = 1; i <= 3; i++)
            {
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    // if an exception occurs
                    throw new Exception($"Exception {i} has occurred.");
                }
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of three messages has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                WaitOn(sender.DisposeAsync());
                WaitOn(client.DisposeAsync());
            }

            Console.WriteLine("Follow the directions in the exercise to review the results in the Azure portal.");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            // Test Receive Messages
            ServiceBusProcessor processor;
            client = new ServiceBusClient(connectionString);

            // create a processor that we can use to process the messages
            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                WaitOn(processor.DisposeAsync());
                WaitOn(client.DisposeAsync());
            }

            // handle received messages
            async Task MessageHandler(ProcessMessageEventArgs args)
            {
                string body = args.Message.Body.ToString();
                Console.WriteLine($"Received: {body}");

                // complete the message. messages is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message);
            }

            // handle any errors when receiving messages
            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                Console.WriteLine(args.Exception.ToString());
                return Task.CompletedTask;
            }
        }

        private static async Task<T> WaitOn<T>(ValueTask<T> valueTask) => await valueTask;
        private static async void WaitOn(ValueTask valueTask) => await valueTask;
    }
}