using PspMagickReader;
using System;
using System.IO;

if (args.Length < 2)
{
    Console.WriteLine("Usage: psp2png <input.psp> <output.png>");
    return 1;
}

string input = args[0];
string output = args[1];

try
{
    using var fs = File.OpenRead(input);
    var img = PspReader.LoadSingle(fs);
    img.Write(output);
    Console.WriteLine($"Wrote {output}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 2;
}
