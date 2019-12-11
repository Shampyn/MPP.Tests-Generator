using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NUnitTestGeneratorLibrary
{
    public class Generator
    {
        private List<string> _filesfortest;
        private string _testfolder;
        private int _maxThreads;
        private int _maxFilesToRead;
        private int _maxFilesToWrite;

        public Generator(List<string> files, string folder, int maxThreads, int maxFilesToRead, int maxFilesToWrite)
        {
            _filesfortest = files;
            _testfolder = folder;
            _maxThreads = maxThreads;
            _maxFilesToRead = maxFilesToRead;
            _maxFilesToWrite = maxFilesToWrite;
        }

        public async Task GenerateAsync()
        {
            var readFile = new TransformBlock<string, string>(async path =>
            {
                string fileContent;
                using (StreamReader reader = File.OpenText(path))
                {
                    fileContent = await reader.ReadToEndAsync();
                }
                return fileContent;

            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _maxFilesToRead
            });

            var getTest = new TransformBlock<string, List<CSharpFile>>(source =>
            {
                NUnitTestGenerator nUnitTestGenerator = new NUnitTestGenerator();

                var generatedTests = nUnitTestGenerator.Generate(source);
                List<CSharpFile> results = new List<CSharpFile>();
                foreach (var generatedTest in generatedTests)
                {
                    string generatedTestStr = generatedTest.ToString();
                    string testName = "";
                    string[] words = generatedTestStr.Split(' ');
                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] == "class")
                        {
                            i++;
                            testName = words[i].Trim(' ', '\r', '\n', '\t');
                        }
                    }
                    results.Add(new CSharpFile(Path.GetFullPath(_testfolder) + "\\" + testName + "Test.cs", generatedTestStr));
                }
                return results;
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _maxThreads
            });


            var writeResult = new ActionBlock<List<CSharpFile>>(async csharpFiles =>
            {
                foreach (var csharpFile in csharpFiles)
                {
                    using (StreamWriter writer = File.CreateText(csharpFile.Filename))
                    {
                        await writer.WriteAsync(csharpFile.Text);
                    }
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _maxFilesToWrite
            });

            //
            // Connect the dataflow blocks to form a pipeline.
            //

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            readFile.LinkTo(getTest, linkOptions);
            getTest.LinkTo(writeResult, linkOptions);
            foreach (string file in _filesfortest)
            {
                readFile.Post(file);
            }
            readFile.Complete();

            await writeResult.Completion;
        }
    }
}
