using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitTestGeneratorLibrary
{
    public class CSharpFile
    {
        public string Filename;
        public string Text;
        public CSharpFile(string filename, string text)
        {
            Filename = filename;
            Text = text;
        }
    }
}
