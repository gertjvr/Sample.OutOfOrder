using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Sample.Contracts;

var client = new ServiceBusClient("Endpoint=sb://jtrack-gertjvr-sbn.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=tc7mQDdlqnHYdYtPqHk7KZkXe09oD1Y0F/3UW54DV1Q=");

var sender = client.CreateSender("messages");

var SessionCount = 10;
var MessageCount = 50;

async Task Send(string session)
{
    for (var i = 1; i <= MessageCount; i++)
    {
        var message = new SequencedPing(i);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        
        await sender!.SendMessageAsync(new ServiceBusMessage(body)
        {
            SessionId = session
        });
    }
}

var sendTasks = (from id in Enumerable.Range(1, SessionCount + 1)
    let session = $"Session{id}"
    select Send(session)).ToList(); 

await Task.WhenAll(sendTasks);