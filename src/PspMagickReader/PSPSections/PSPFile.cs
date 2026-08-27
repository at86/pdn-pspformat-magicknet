using PspMagickReader.IO;
using System;
using System.IO;
using ImageMagick;

namespace PspMagickReader.PSPSections
{
    internal static class PSPFile
    {
        private static readonly ReadOnlySpan<byte> PSPFileSig = new byte[32]
        { 0x50, 0x61, 0x69, 0x6E, 0x74, 0x20, 0x53, 0x68, 0x6F, 0x70, 0x20, 0x50,
          0x72, 0x6F, 0x20, 0x49, 0x6D, 0x61, 0x67, 0x65, 0x20, 0x46, 0x69, 0x6C, 0x65, 0x0A,
          0x1A, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static bool CheckSignature(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanSeek) throw new ArgumentException("Stream must be seekable.", nameof(input));

            long orig = input.Position;
            try
            {
                Span<byte> buf = stackalloc byte[32];
                int read = input.Read(buf);
                if (read < 27) return false;
                // compare first 27 bytes
                return buf.Slice(0, 27).SequenceEqual(PSPFileSig.Slice(0, 27));
            }
            finally
            {
                input.Position = orig;
            }
        }

        public static MagickImage? TryExtractCompositeJpeg(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanSeek) throw new ArgumentException("Stream must be seekable.", nameof(input));

            long orig = input.Position;
            try
            {
                using EndianBinaryReader br = new EndianBinaryReader(input, Endianess.Little, leaveOpen: true);

                // consume signature
                Span<byte> sig = stackalloc byte[32];
                br.ReadExactly(sig);

                // read file header
                ushort major = br.ReadUInt16();
                ushort minor = br.ReadUInt16();

                while (br.Position < br.Length)
                {
                    uint blockSig = br.ReadUInt32();
                    if (blockSig != PSPConstants.blockIdentifier)
                    {
                        // not a valid block, abort
                        break;
                    }

                    PSPBlockID blockID = (PSPBlockID)br.ReadUInt16();

                    // initialBlockLength for v5 compatibility
                    if (major <= PSPConstants.majorVersion5)
                    {
                        _ = br.ReadUInt32();
                    }

                    uint blockLength = br.ReadUInt32();

                    long blockEnd = br.Position + blockLength;

                    if (blockID == PSPBlockID.JPEGImage)
                    {
                        // JPEGCompositeInfoChunk format: chunkSize (uint), compressedSize (uint), unCompressedSize (uint), imageType (ushort), imageData (compressedSize bytes)
                        uint chunkSize = br.ReadUInt32();
                        uint compressedSize = br.ReadUInt32();
                        _ = br.ReadUInt32(); // unCompressedSize
                        _ = br.ReadUInt16(); // imageType

                        byte[] imageData = new byte[compressedSize];
                        int total = 0;
                        while (total < imageData.Length)
                        {
                            int r = input.Read(imageData, total, imageData.Length - total);
                            if (r == 0) break;
                            total += r;
                        }

                        try
                        {
                            return new MagickImage(imageData);
                        }
                        catch
                        {
                            // ignore and continue
                        }
                    }
                    else if (blockID == PSPBlockID.CompositeImageBank)
                    {
                        // parse CompositeImageBlock: first uint = blockSize, uint = attrChunkCount, then attr chunks, then child chunks
                        uint blockSize = br.ReadUInt32();
                        uint attrChunkCount = br.ReadUInt32();

                        // parse attribute chunks (skip their content)
                        for (uint i = 0; i < attrChunkCount; i++)
                        {
                            uint childSig = br.ReadUInt32();
                            if (childSig != PSPConstants.blockIdentifier) break;
                            ushort childType = br.ReadUInt16();
                            uint childLen = br.ReadUInt32();

                            // each attr chunk starts with its own chunkSize we can skip
                            // read the chunkSize then skip remaining bytes
                            uint chunkSize = br.ReadUInt32();
                            long skip = chunkSize - 24; // CompositeImageAttributesChunk header is 24 bytes when saving; but safer to skip childLen-4
                            // However childLen refers to attr chunk length in some variants; to be robust, move position by (childLen - 4) if childLen>0
                            if (childLen > 4)
                            {
                                br.Position += (childLen - 4);
                            }
                        }

                        // now parse attrChunkCount child chunks which may include JPEGImage or CompositeImage
                        for (uint i = 0; i < attrChunkCount; i++)
                        {
                            if (br.Position >= br.Length) break;
                            uint childSig = br.ReadUInt32();
                            if (childSig != PSPConstants.blockIdentifier) break;
                            PSPBlockID childType = (PSPBlockID)br.ReadUInt16();
                            uint childLen = br.ReadUInt32();

                            long childEnd = br.Position + childLen;

                            if (childType == PSPBlockID.JPEGImage)
                            {
                                uint chunkSize = br.ReadUInt32();
                                uint compressedSize = br.ReadUInt32();
                                _ = br.ReadUInt32(); // unCompressedSize
                                _ = br.ReadUInt16(); // imageType

                                byte[] imageData = new byte[compressedSize];
                                int total = 0;
                                while (total < imageData.Length)
                                {
                                    int r = input.Read(imageData, total, imageData.Length - total);
                                    if (r == 0) break;
                                    total += r;
                                }

                                try
                                {
                                    return new MagickImage(imageData);
                                }
                                catch
                                {
                                    // ignore and continue
                                }
                            }

                            // skip unknown child
                            br.Position = childEnd;
                        }

                        // skip to end of block
                        br.Position = blockEnd;
                    }
                    else
                    {
                        // skip this block
                        br.Position = blockEnd;
                    }
                }
            }
            finally
            {
                input.Position = orig;
            }

            return null;
        }
    }
}
