////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Xml;
using System.IO;
using ProfilerXmlTests;
using System.Text;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_ProfilerXmlTests
    {
        public static void Main()
        {
            Master_ProfilerXmlTests tests = new Master_ProfilerXmlTests();
            tests.ParseXmlTest();
        }

        public void ParseXmlTest()
        {
            ProfilerXmlTests.Resources.StringResources[] Xmls = { Resources.StringResources.MeshXml, };
            TextWriter writer = new DoNothingWriter();

            for (int i = 0; i < Xmls.Length; i++)
            {
                ParseXml(new MemoryStream(Encoding.UTF8.GetBytes(Resources.GetString(Xmls[i]))), writer);
            }

            writer.Flush();
        }

        private void ParseXml(Stream input, TextWriter output)
        {
            XmlReader reader = new XmlTextReader(input);

            reader.MoveToContent();
            int previousDepth = reader.Depth;
            int originalDepth = previousDepth;

            PrintNode(reader, output);

            // Walk the document
            while (true)
            {
                reader.Read();
                reader.MoveToContent();

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        // We are decending the branch
                        while (reader.Depth < previousDepth)
                        {
                            --previousDepth;
                        }

                        // We are accending the branch
                        if (reader.Depth > previousDepth)
                        {
                            ++previousDepth;
                        }

                        PrintNode(reader, output);
                        break;
                    case XmlNodeType.Text:
                        output.WriteLine(new String(' ', reader.Depth * 2) + reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        output.WriteLine(new String(' ', reader.Depth * 2) + "</" + reader.Name + ">");
                        break;
                }

                if (reader.Depth <= originalDepth)
                    break;
            }

            reader.Close();
        }

        private void PrintNode(XmlReader reader, TextWriter output)
        {
            output.Write(new String(' ', reader.Depth * 2) + "<" + reader.Name);

            if (reader.HasAttributes)
            {
                reader.MoveToFirstAttribute();
                do
                {
                    output.Write(" " + reader.Name + "=\"" + reader.Value + "\"");
                }
                while (reader.MoveToNextAttribute());

                reader.MoveToElement();
            }

            output.WriteLine(">");
        }
    }

    public class DebugPrintWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        String buffer = String.Empty;

        public override void Write(string value)
        {
            buffer += value;
        }

        public override void WriteLine(string value)
        {
            Debug.Print(buffer + value);
            buffer = String.Empty;
        }

        public override void WriteLine()
        {
            WriteLine("");
        }
    }

    public class DoNothingWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}