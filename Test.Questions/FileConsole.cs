using System.IO;
using System.Linq;

namespace Questions
{
    public class FileConsole : IConsole
    {
        private readonly string inputFile;
        private readonly string outputFile;

        public FileConsole(string inputFilePath, string outputFilePath)
        {
            inputFile = inputFilePath;
            outputFile = outputFilePath;
        }

        public void WriteLine(string message)
        {
            File.AppendAllLines(outputFile, new []{message});
        }

        public string ReadLine()
        {
            if (!File.Exists(inputFile))
                return "";
            var lines = File.ReadAllLines(inputFile).ToList();
            if (lines.Count == 0)
            {
                return "";
            }
            var input = lines.First();
            lines.RemoveRange(0,1);
            File.WriteAllLines(inputFile, lines);
            return input;
        }
    }
}