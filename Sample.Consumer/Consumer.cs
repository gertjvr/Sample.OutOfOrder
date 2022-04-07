using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Sample.Consumer;

public record SequencedPing
{
    public SequencedPing(int sentSeqNumber)
    {
        SentSeqNumber = sentSeqNumber;
    }

    public int SentSeqNumber { get; }
}

public struct MessageReceived
{
    public MessageReceived(int sentSeqNumber, int receivedSeqNumber)
    {
        SentSeqNumber = sentSeqNumber;
        ReceivedSeqNumber = receivedSeqNumber;
    }

    public int SentSeqNumber { get; }
    public int ReceivedSeqNumber { get; }
}

public static class Consumer
{
    static readonly ConcurrentDictionary<string, MessageReceived> _result = new ();

    [FunctionName("Consumer")]
    [FixedDelayRetry(5, "00:00:10")]
    public static async Task RunAsync([ServiceBusTrigger("messages", 
        IsSessionsEnabled = true,
        Connection = "ServiceBusConnection")] ServiceBusReceivedMessage received, 
        ILogger log)
    {
        var message = JsonSerializer.Deserialize<SequencedPing>(received.Body.ToStream());
        if (message is null)
            throw new ArgumentException("message is null");

        var sessionCount = _result.TryGetValue(received.SessionId, out var mr) 
            ? new MessageReceived(message.SentSeqNumber, mr.ReceivedSeqNumber + 1) 
            : new MessageReceived(message.SentSeqNumber, 1);
        
        _result[received.SessionId] = sessionCount;

        await Task.Delay(1);
            
        log.LogInformation("Received: {Session}:{SequenceNumber}", received.SessionId, message.SentSeqNumber);
    }
}