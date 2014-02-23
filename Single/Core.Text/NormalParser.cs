namespace Single.Core.Text
{
    internal class NormalParser : IParseInformationProvider
    {
        #region Functions

        public string GetReadableFormat(string input)
        {
            input =
                input.Replace("<BR>", "\r\n")
                    .Replace("<AE>", "Ä")
                    .Replace("<OE>", "Ö")
                    .Replace("<UE>", "Ü")
                    .Replace("<ae>", "ä")
                    .Replace("<oe>", "ö")
                    .Replace("<ue>", "ü")
                    .Replace("_", " ")
                    .Replace("<e>", "é")
                    .Replace("<S>", "ß");
            return input;
        }

        public string GetTableFormat(string input)
        {
            input =
                input.Replace("\r\n", "<BR>")
                    .Replace("Ä", "<AE>")
                    .Replace("Ö", "<OE>")
                    .Replace("Ü", "<UE>")
                    .Replace("ä", "<ae>")
                    .Replace("ö", "<oe>")
                    .Replace("ü", "<ue>")
                    .Replace(" ", "_")
                    .Replace("é", "<e>")
                    .Replace("ß", "<S>");
            return input;
        }

        #endregion
    }
}