using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;

namespace PspMagickReader.IO
{
    /// <summary>
    /// Minimal Endian-aware binary reader used by the PSP parser.
    /// This is a lightweight replacement for the original project's EndianBinaryReader
    /// with only the members currently required by the scaffold.
    /// </summary>
    internal sealed class EndianBinaryReader : IDisposable
    {
        private readonly Stream stream;
        private readonly bool leaveOpen;

        public EndianBinaryReader(Stream stream, Endianess endianess, bool leaveOpen = true)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.Endianess = endianess;
            this.leaveOpen = leaveOpen;
        }

        public Endianess Endianess { get; }

        public long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        public long Length => stream.Length;

        public void Dispose()
        {
            if (!leaveOpen)
            {
                stream.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            int b = stream.ReadByte();
            if (b == -1) throw new EndOfStreamException();
            return (byte)b;
        }

        public void ReadExactly(Span<byte> destination)
        {
            int total = 0;
            while (total < destination.Length)
            {
                int r = stream.Read(destination.Slice(total));
                if (r == 0)
                {
                    throw new EndOfStreamException();
                }
                total += r;
            }
        }

        public ushort ReadUInt16()
        {
            Span<byte> buf = stackalloc byte[2];
            ReadExactly(buf);
            if (Endianess == Endianess.Little)
                return BinaryPrimitives.ReadUInt16LittleEndian(buf);
            else
                return BinaryPrimitives.ReadUInt16BigEndian(buf);
        }

        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        public uint ReadUInt32()
        {
            Span<byte> buf = stackalloc byte[4];
            ReadExactly(buf);
            if (Endianess == Endianess.Little)
                return BinaryPrimitives.ReadUInt32LittleEndian(buf);
            else
                return BinaryPrimitives.ReadUInt32BigEndian(buf);
        }

        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public double ReadDouble()
        {
            Span<byte> buf = stackalloc byte[8];
            ReadExactly(buf);
            if (BitConverter.IsLittleEndian != (Endianess == Endianess.Little))
            {
                buf.Reverse();
            }
            return BitConverter.ToDouble(buf);
        }
    }

    internal enum Endianess
    {
        Little,
        Big,
    }
}
