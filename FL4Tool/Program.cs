using System.Text;

namespace FL4Tool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("AaruSystem FL4.0 Tool");
                Console.WriteLine("  created by Crsky");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  Extract : FL4Tool -e -in [input.fl4] -out [output] -cp [codepage]");
                Console.WriteLine("  Create  : FL4Tool -c -in [folder] -out [output.fl4] -cp [codepage]");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");

                Environment.ExitCode = 1;
                Console.ReadKey();

                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var parsedArgs = CommandLineParser.ParseArguments(args);

            CommandLineParser.EnsureArguments(parsedArgs, "-in", "-out", "-cp");

            var inputPath = Path.GetFullPath(parsedArgs["-in"]);
            var outputPath = Path.GetFullPath(parsedArgs["-out"]);
            var encoding = Encoding.GetEncoding(parsedArgs["-cp"]);

            if (parsedArgs.ContainsKey("-e"))
            {
                FL4.Extract(inputPath, outputPath, encoding);
                return;
            }

            if (parsedArgs.ContainsKey("-c"))
            {
                FL4.Create(outputPath, inputPath, encoding);
                return;
            }
        }
    }
}
