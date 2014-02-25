namespace Single.Core.Text
{
    public class NormalChar : PokehexCharBase
    {
        public NormalChar(object value, string readable) : base(value, readable)
        {}

        public override CharType Type
        {
            get { return CharType.Byte; }
        }
    }
}
