namespace Sample.Contracts;

public record SequencedPing
{
    public SequencedPing(int sentSeqNumber)
    {
        SentSeqNumber = sentSeqNumber;
    }

    public int SentSeqNumber { get; }
}