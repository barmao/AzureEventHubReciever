using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using EventsHubReciever;
using System.Text;
using System.Text.Json;
using static Azure.Core.HttpHeader;

// Create a blob container client that the event processor will use 
BlobContainerClient storageClient = new BlobContainerClient(
    "<connection string>", 
    "<blob storage name>");

// Create an event processor client to process events in the event hub
var processor = new EventProcessorClient(
    storageClient,
    EventHubConsumerClient.DefaultConsumerGroupName,
     "<connection string>",
    "<event hub name>");

// Register handlers for processing events and handling errors
processor.ProcessEventAsync += ProcessEventHandler;
processor.ProcessErrorAsync += ProcessErrorHandler;

// Start the processing
await processor.StartProcessingAsync();

// Wait for 30 seconds for the events to be processed
await Task.Delay(TimeSpan.FromSeconds(10));

// Stop the processing
await processor.StopProcessingAsync();

async Task ProcessEventHandler(ProcessEventArgs eventArgs)
{
    // Write the body of the event to the console window
    Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
    var attribs = JsonSerializer.Deserialize<StandardCommon>(eventArgs.Data.Body.ToString());

    // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
    await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
}

Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
{
    // Write details about the error to the console window
    Console.WriteLine($"\tPartition '{eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
    Console.WriteLine(eventArgs.Exception.Message);
    return Task.CompletedTask;
}