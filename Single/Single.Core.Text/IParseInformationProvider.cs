namespace Single.Core.Text
{
    public interface IParseInformationProvider
    {
        string getReadableFormat(string input);
        string getTableFormat(string input);
    }
}