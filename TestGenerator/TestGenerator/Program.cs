using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnitTestGeneratorLibrary;

namespace TestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter list of file to generate tests for them separating them by spaces");
            List<string> Files = Console.ReadLine().Split(' ').ToList();
            List<string> FilesPath = new List<string>();
            foreach (string File in Files)
            {
                FilesPath.Add(Path.GetFullPath(File));
            }
            Console.WriteLine("Enter test folder");
            string Folder = Path.GetFullPath(Console.ReadLine());
            Console.WriteLine("Enter max amount of threads for generating tests");
            int Threads = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter max amount of threads for reading files");
            int FilesToRead = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter max amount of threads for writing files");
            int FilesToWrite = int.Parse(Console.ReadLine());
            Generator generator = new Generator(FilesPath, Folder, Threads, FilesToRead, FilesToWrite);
            generator.GenerateAsync().Wait();
            Console.WriteLine("Completed.");
        
        }
    }
}
