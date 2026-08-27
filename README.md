# pdn-pspformat-magicknet

This repository is a port-in-progress of `0xC0000054/pdn-pspformat` to a standalone .NET 10 library that reads Paint Shop Pro (PSP) files and converts them into Magick.NET `MagickImage` / `MagickImageCollection` instances.

Status
- Initial scaffold created on branch `magicknet-port`.
- Project `src/PspMagickReader` (net10.0) and a small CLI `src/psp2png` were added.
- A minimal Endian-aware binary reader and a PSP signature check have been implemented.
- Full PSP parsing (layers, pixel decoding, compression formats, palettes, etc.) is NOT implemented yet. The public API currently returns a placeholder transparent image after validating the PSP signature.

What I will do next
- Incrementally port the PSP parser code from the original repository's `src/PSPSections` and `src/IO` modules, removing Paint.NET dependencies and converting internal image/surface representations to Magick.NET pixel buffers.
- Implement `PspReader.LoadSingle(Stream)` and `PspReader.LoadAll(Stream)` to return real images.
- Add unit tests and example PSP files for validation.

How to build
- Requires .NET 10 SDK and a network connection to restore NuGet packages.

  dotnet restore
  dotnet build

Example
- Convert a PSP file to PNG using the sample CLI:

  dotnet run --project src/psp2png -- example.psp out.png

License
- This project is MIT licensed (see original project's License.txt). The original project contains code adapted from Paint.NET which carries additional licensing notes; see the original repository for details.

Original repository: https://github.com/0xC0000054/pdn-pspformat
