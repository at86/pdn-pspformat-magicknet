using ImageMagick;
using System;
using System.IO;

namespace PspMagickReader
{
    public static class PspReader
    {
        /// <summary>
        /// Load a PSP file and return a single composite MagickImage.
        /// Current implementation only validates the PSP signature and returns a placeholder image until the full parser is implemented.
        /// </summary>
        /// <param name="input">Input stream containing a PSP file.</param>
        /// <returns>MagickImage representing the composite/flattened image.</returns>
        public static MagickImage LoadSingle(Stream input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));

            if (!PSPFile.CheckSignature(input))
            {
                throw new FormatException("The file is not a valid Paint Shop Pro (PSP) image.");
            }

            // TODO: Implement full PSP parsing and conversion to MagickImage.
            // For now return a 1x1 transparent placeholder image so consumers can validate integration.
            var img = new MagickImage(MagickColors.Transparent, 1, 1);
            img.Comment = "PSP parser: placeholder image. Full parser not implemented yet.";
            return img;
        }

        /// <summary>
        /// Load all frames/layers as a MagickImageCollection.
        /// Currently returns a collection containing a single placeholder image.
        /// </summary>
        public static MagickImageCollection LoadAll(Stream input)
        {
            var coll = new MagickImageCollection();
            coll.Add(LoadSingle(input));
            return coll;
        }
    }
}
