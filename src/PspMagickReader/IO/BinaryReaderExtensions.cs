using System;
using System.IO;
using System.Text;

namespace PspMagickReader.IO
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadAsciiString(this EndianBinaryReader br, int length, StringReadOptions options)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            Span<byte> buf = length <= 1024 ? stackalloc byte[length] : new byte[length];
            br.ReadExactly(buf);

            int actualLen = length;
            if ((options & StringReadOptions.TrimNullTerminator) != 0)
            {
                int idx = buf.IndexOf((byte)0);
                if (idx >= 0) actualLen = idx;
            }
            if ((options & StringReadOptions.TrimWhiteSpace) != 0)
            {
                // trim ASCII whitespace from end
                while (actualLen > 0)
                {
                    byte b = buf[actualLen - 1];
                    if (b == 0x09 || b == 0x0A || b == 0x0B || b == 0x0C || b == 0x0D || b == 0x20)
                        actualLen--;
                    else
                        break;
                }
            }

            return Encoding.ASCII.GetString(buf.Slice(0, actualLen));
        }
    }

    [Flags]
    internal enum StringReadOptions
    {
        None = 0,
        TrimWhiteSpace = 1,
        TrimNullTerminator = 2,
    }
}
