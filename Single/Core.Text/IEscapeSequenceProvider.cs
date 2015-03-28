namespace Single.Core.Text
{
    public interface IEscapeSequenceProvider
    {
        IEscapeSequence[] Sequences { get; }
    }
}
