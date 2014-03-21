using System.IO;

namespace Single.Core.Text
{
    public interface IEscapeSequence
    {
        char Sequence { get;}
        byte[] Resolve(StringReader reader);
    }
}
