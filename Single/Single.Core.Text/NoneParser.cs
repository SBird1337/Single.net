namespace Single.Core.Text
{
    internal class NoneParser : IParseInformationProvider
    {
        #region Functions

        public string getReadableFormat(string input)
        {
            return input;
        }

        public string getTableFormat(string input)
        {
            return input;
        }

        #endregion
    }
}