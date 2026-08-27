*** Begin Patch
*** Update File: src/PspMagickReader/PspReader.cs
@@
-            if (!PSPFile.CheckSignature(input))
+            if (!PSPFile.CheckSignature(input))
             {
                 throw new FormatException("The file is not a valid Paint Shop Pro (PSP) image.");
             }
 
-            // TODO: Implement full PSP parsing and conversion to MagickImage.
-            // For now return a 1x1 transparent placeholder image so consumers can validate integration.
-            var img = new MagickImage(MagickColors.Transparent, 1, 1);
-            img.Comment = "PSP parser: placeholder image. Full parser not implemented yet.";
-            return img;
+            // Try to extract a composite JPEG embedded in the PSP file (fast path used by many PSP files).
+            var jpegImg = PspMagickReader.PSPSections.PSPFile.TryExtractCompositeJpeg(input);
+            if (jpegImg != null)
+            {
+                return jpegImg;
+            }
+
+            // TODO: Implement full PSP parsing and conversion to MagickImage when no embedded JPEG is available.
+            // For now return a 1x1 transparent placeholder image so consumers can validate integration.
+            var img = new MagickImage(MagickColors.Transparent, 1, 1);
+            img.Comment = "PSP parser: placeholder image. Composite JPEG not found and full parser not yet implemented.";
+            return img;
         }
*** End Patch
