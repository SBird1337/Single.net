namespace Single.Core.Text
{
    public interface IParseInformationProvider
    {
        string GetReadableFormat(string input);
        string GetTableFormat(string input);
    }
}