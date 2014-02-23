namespace Single.Core.Text
{
    internal class NoneParser : IParseInformationProvider
    {
        #region Functions

        public string GetReadableFormat(string input)
        {
            return input;
        }

        public string GetTableFormat(string input)
        {
            return input;
        }

        #endregion
    }
}