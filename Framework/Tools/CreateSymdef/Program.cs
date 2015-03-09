using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CreateSymdef
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
            }
            else
            {
                String inFileName = args[0];
                String outFileName;

                if (args.Length < 2)
                {
                    outFileName = inFileName + ".symdefs";
                }
                else
                {
                    outFileName = args[1];
                }

                using(StreamReader reader = new StreamReader(inFileName))
                using(StreamWriter writer = new StreamWriter(outFileName, false))
                {
                    String line;

                    do
                    {
                        line = reader.ReadLine();

                        if (line.StartsWith("    ") == false)
                        {
                            line = line.Remove(9, 1);
                            line = line.Insert(9, "A");
                            writer.WriteLine("0x" + line);
                        }
                    } while (reader.EndOfStream == false);
                }

                Console.WriteLine("Done !");
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("");
            Console.WriteLine("Wrong Arguments");
            Console.WriteLine("");
            Console.WriteLine("CreateSymdef - ADENEO 2008");
            Console.WriteLine("Convert a gcc dump file (generated with 'nm') to an ARM Symdefs compliant file"); 
            Console.WriteLine("USAGE :");
            Console.WriteLine("\tCreateSymdef <dump file> <symdef file>");

        }
    }
}
