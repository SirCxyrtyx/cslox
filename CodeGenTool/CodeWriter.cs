﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTool
{
    internal class CodeWriter : IDisposable
    {
        private readonly StreamWriter writer;
        private byte indent;
        private bool writingLine;
        private readonly string indentString;

        public CodeWriter(Stream stream, string indentString = "    ")
        {
            writer = new StreamWriter(stream);
            this.indentString = indentString;
        }

        public void IncreaseIndent(byte amount = 1)
        {
            indent += amount;
        }

        public void DecreaseIndent(byte amount = 1)
        {
            if (amount > indent)
            {
                throw new InvalidOperationException("Cannot have a negative indent!");
            }
            indent -= amount;
        }

        public void WriteIndent()
        {
            for (int i = 0; i < indent; i++)
            {
                writer.Write(indentString);
            }
        }

        public void Write(string text)
        {
            if (!writingLine)
            {
                WriteIndent();
                writingLine = true;
            }
            writer.Write(text);
        }

        public void WriteLine(string line)
        {
            if (!writingLine)
            {
                WriteIndent();
            }
            writingLine = false;
            writer.WriteLine(line);
        }

        public void WriteLine()
        {
            writingLine = false;
            writer.WriteLine();
        }

        public void WriteBlock(string header, Action contents = null)
        {
            WriteLine(header);
            WriteLine("{");
            IncreaseIndent();
            contents?.Invoke();
            DecreaseIndent();
            WriteLine("}");
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
