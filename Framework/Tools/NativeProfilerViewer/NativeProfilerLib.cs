using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Microsoft.SPOT.Tools
{
    public class NativeProfilerUnit
    {
        private uint entryTime;
        private uint returnTime;
        private uint functionAddress;
        private uint nestingLevel;
        private uint nItem;
        public NativeProfilerUnit()
        {
            entryTime = 0xffffffff;
            returnTime = 0xffffffff;
            functionAddress = 0xffffffff;
            nestingLevel = 0xffffffff;
            nItem = 0xffffffff;
        }
        public uint FunctionAddress
        {
            set
            {
                this.functionAddress = value;
            }
            get
            {
                return this.functionAddress;
            }
        }

        public uint EntryTime
        {
            set
            {
                this.entryTime = value;
            }
            get
            {
                return this.entryTime;
            }
        }

        public uint ReturnTime
        {
            set
            {
                this.returnTime = value;
            }
            get
            {
                return this.returnTime;
            }
        }

        public uint NestingLevel
        {
            set
            {
                this.nestingLevel = value;
            }
            get
            {
                return this.nestingLevel;
            }
        }

        public uint NItem
        {
            set
            {
                this.nItem = value;
            }
            get
            {
                return this.nItem;
            }
        }

        public uint ExecutionTime
        {
            get
            {
                return (this.returnTime - this.entryTime);
            }
        }
    }
    public class NativeProfiler
    {
        private NativeProfilerUnit [] htN;
        private Hashtable htFunctionNames;
        private uint [] htInclTime;
        private uint [] htExclTime;
        uint nItems;
        private uint totalRuntime = 0;
        private uint engineTimeOffset;
        private const string c_line = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}";
        private uint minimalLevel = 0;
        public NativeProfiler(Stream m_tInput, string mapFileName, bool b_checkBlockBegin)
        {
            bool needRestart = true;
            long streamLength = m_tInput.Length;
            uint nRestarts = 0;
            while (needRestart == true && streamLength > 0)
            {
                needRestart = false;
                try
                {              
                    NativeProfilerUnit[] htN_temp = new NativeProfilerUnit[streamLength];
                    htFunctionNames = new Hashtable();
                    htN = new NativeProfilerUnit[streamLength];
                    htInclTime = new uint[streamLength];
                    htExclTime = new uint[streamLength];
                    Stack stack = new Stack(8096);
                    byte[] bArray = new byte[20];
                    m_tInput.Seek(0, SeekOrigin.Begin);
                    nItems = 0;
                    totalRuntime = 0;
                    long n = 0;
                    uint level = 0;
                    uint address = 0, time = 0;
                    uint checkValue = 0;

                    NativeProfilerUnit unit;

                    if (b_checkBlockBegin == true)
                    {
                        while (streamLength - m_tInput.Position >= 4 && checkValue != 0xbaadf00d)
                        {
                            m_tInput.Read(bArray, 0, 4);
                            
                            checkValue = BitConverter.ToUInt32(bArray, 0);
                            if (checkValue != 0xbaadf00d)
                            {
                                m_tInput.Seek(-3, SeekOrigin.Current);
                            }
                            else
                            {
                                m_tInput.Read(bArray, 0, 4);
                                engineTimeOffset = BitConverter.ToUInt32(bArray, 0);
                            }
                        }
                    }
                    
                    while (streamLength - m_tInput.Position >= 8)
                    {
                        m_tInput.Read(bArray, 0, 8);
                        address = BitConverter.ToUInt32(bArray, 0);
                        time = BitConverter.ToUInt32(bArray, 4);
                        if (address != 0xbaadf00d && address != 0xd00fdaab && time != 0xbaadf00d && time != 0xd00fdaab)
                        {
                            if ((address & 1) == 0) // if the address is even then it is a push opcode
                            {
                                unit = new NativeProfilerUnit();
                                unit.EntryTime = time;
                                unit.FunctionAddress = address;
                                unit.NestingLevel = level;
                                unit.ReturnTime = 0xffffffff;
                                htN_temp[n] = unit;
                                stack.Push(unit);
                                n++;
                                level++;
                                // Adds a reference that a key for 'address' exists in htFunctionNames.
                                htFunctionNames[address] = null;
                            }
                            else if (stack.Count > 0) // if the address is odd then it is a pop opcode
                            {
                                unit = (NativeProfilerUnit)stack.Pop();
                                unit.ReturnTime = time;
                                level--;
                            }
                        }
                        else if (time == 0xbaadf00d)
                        {
                            if (m_tInput.Length - m_tInput.Position >= 4)
                            {
                                m_tInput.Seek(4, SeekOrigin.Current);
                            }
                        }
                        else if (address == 0xd00fdaab || time == 0xd00fdaab)
                        {
                            if (time != 0xbaadf00d)
                            {
                                m_tInput.Seek(-3, SeekOrigin.Current);
                            }
                            while (streamLength - m_tInput.Position >= 4 && time != 0xbaadf00d)
                            {
                                m_tInput.Read(bArray, 0, 4);
                                time = BitConverter.ToUInt32(bArray, 0);
                                if (time != 0xbaadf00d)
                                {
                                    m_tInput.Seek(-3, SeekOrigin.Current);
                                }
                            }
                            m_tInput.Read(bArray, 0, 4);
                            
                        }
                    }

                    OpenMAPFile(mapFileName);
                    totalRuntime = 0;

                    // Do some simple data consistancy check
                    // We use time, and check that the level of the next unit - level of the previous level is <= 1;
                    uint index = 0;
                    unit = (NativeProfilerUnit)htN_temp[index];
                    minimalLevel = 0xffffffff;
                    while (unit != null)
                    {
                        if (unit.ReturnTime > unit.EntryTime && unit.ReturnTime != 0xffffffff &&
                            unit.EntryTime != 0xffffffff && htFunctionNames[unit.FunctionAddress] != null &&
                            (nItems == 0 || nItems != 0 && ((int)unit.NestingLevel - (int)htN[nItems - 1].NestingLevel <= 1)))
                        {
                            htN[nItems] = unit;
                            unit.NItem = nItems;
                            nItems++;
                            if (unit.NestingLevel < minimalLevel) minimalLevel = unit.NestingLevel;
                        }
                        else
                        {
                            if (nItems != 0)
                            {
                                // If the node after the last one is not correct then destroy the last node too.
                                htN[nItems - 1] = null;
                                nItems--;
                            }
                            break;
                        }
                        index++;
                        unit = (NativeProfilerUnit)htN_temp[index];
                    }
                    if (minimalLevel == 0xffffffff) minimalLevel = 0;


                    // The last nodes seems to be scratched sometimes.
                    // So we ignore everything from the node after the last one until the node of level minimalLevel,
                    // or the node which is null (if it is not already that one).
                    NativeProfilerUnit lastNode;
                    uint lastLevel = minimalLevel;
                    index = nItems;
                    do
                    {
                        lastNode = (NativeProfilerUnit)htN[index];
                        if (lastNode != null) lastLevel = lastNode.NestingLevel;
                        index--;
                    }
                    while (lastNode != null && lastLevel > minimalLevel);
                    if (index != nItems - 1) lastNode = (NativeProfilerUnit)htN[index];

                    unit = (NativeProfilerUnit)htN[(uint)0];

                    index = 0;
                    while (unit != null && unit != lastNode)
                    {
                        index++;
                        unit = (NativeProfilerUnit)htN[index];
                    }

                    nItems = index;
                    htN[nItems] = null;

                    // Count inclusive, exclusive time
                    for (index = 0; index < nItems; index++)
                    {
                        unit = GetUnit(index);
                        uint entryTime = unit.EntryTime;
                        uint returnTime = unit.ReturnTime;
                        level = unit.NestingLevel;
                        uint count = index + 1;
                        long exclTime = (long)unit.ExecutionTime - (long)EngineTimeOffset;
                        while (count < nItems && htN[count] != null && ((NativeProfilerUnit)htN[count]).ReturnTime < returnTime && ((NativeProfilerUnit)htN[count]).EntryTime < returnTime)
                        {
                            if (((NativeProfilerUnit)htN[count]).NestingLevel == level + 1)
                            {
                                exclTime = exclTime - ((NativeProfilerUnit)htN[count]).ExecutionTime;
                            }
                            count++;
                        }

                        if (exclTime < 0) exclTime = 0;
                        htExclTime[index] = (uint)exclTime;
                    }

                    for (index = nItems - 1; (int)index >= 0; index--)
                    {
                        unit = GetUnit(index);
                        uint entryTime = unit.EntryTime;
                        uint returnTime = unit.ReturnTime;
                        level = unit.NestingLevel;
                        uint count = index + 1;
                        long inclTime = 0;
                        while (count < nItems && htN[count] != null && ((NativeProfilerUnit)htN[count]).ReturnTime < returnTime && ((NativeProfilerUnit)htN[count]).EntryTime < returnTime)
                        {
                            if (((NativeProfilerUnit)htN[count]).NestingLevel == level + 1)
                            {
                                inclTime = inclTime + htInclTime[count];
                            }
                            count++;
                        }

                        inclTime += htExclTime[index];
                        htInclTime[index] = (uint)(inclTime);
                        if (unit.NestingLevel == minimalLevel) totalRuntime += (uint)inclTime;
                    }
                }
                catch (System.OutOfMemoryException)
                {
                    if(nRestarts == 0) System.Windows.Forms.MessageBox.Show("Out of memory. Will try to strip data until load of data is possible.");
                    needRestart = true;
                    streamLength /= 2;
                    nRestarts++;
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Unfortunately there was an error during decoding data. Exception sent: " + e.ToString());
                }
            }
        }

        public uint GetMinimalLevel()
        {
            return minimalLevel;
        }
        public NativeProfilerUnit GetUnit(uint n)
        {
                if(n < GetNMax()) return (NativeProfilerUnit)htN[n];
                return null;
        }
        public uint GetUnitFunctionAddress(uint n)
        {
            return GetUnit(n).FunctionAddress;
        }
        public uint GetUnitEntryTime(uint n)
        {
            return GetUnit(n).EntryTime;
        }
        public uint GetUnitReturnTime(uint n)
        {
            return GetUnit(n).ReturnTime;
        }
        public string GetUnitFunctionName(uint n)
        {
            return (string)htFunctionNames[GetUnitFunctionAddress(n)];
        }
        public string GetUnitFunctionName(NativeProfilerUnit unit)
        {
            return (string)htFunctionNames[unit.FunctionAddress];
        }
        public uint GetUnitNestingLevel(uint n)
        {
            return GetUnit(n).NestingLevel - GetMinimalLevel();
        }
        public uint GetNMax()
        {
            return nItems;
        }
        public uint GetUnitExecutionTime(uint n)
        {
            return GetUnit(n).ExecutionTime;
        }
        public uint GetTotalRuntime()
        {
            return totalRuntime;
        }
        public uint GetUnitInclTime(uint n)
        {
            return (uint)((object)htInclTime[n]);
        }
        public uint GetUnitExclTime(uint n)
        {
            return (uint)((object)htExclTime[n]);
        }
        private void OpenMAPFile(string fileName)
        {
            System.IO.StreamReader m_input = new System.IO.StreamReader(fileName, Encoding.Default);
            string sBuffer = null;
            while ((sBuffer = m_input.ReadLine()) != null && sBuffer != "Memory Map of the image") ; // Find the begin of a maping table.

            while ((sBuffer = m_input.ReadLine()) != null && sBuffer != "Image component sizes") // Read until the end of the maping table.
            {
                Regex rx = new Regex(@"[\s]+0x([\dabcdefABCDEF]+)[\s]+0x([\dabcdefABCDEF]+)[\s]+[\w]+[\s]+[\w]+[\s]+[\d]+[\s]+([\w\W]+)\s+([\w\W]+)", RegexOptions.None);
                MatchCollection matches = rx.Matches(sBuffer);
                foreach (Match match in matches)
                {
                    GroupCollection groups = match.Groups;
                    uint startAddress = uint.Parse(groups[1].Value, NumberStyles.AllowHexSpecifier);
                    uint range = uint.Parse(groups[2].Value, NumberStyles.AllowHexSpecifier);
                    LinkedList<uint> modifiedNodes = new LinkedList<uint>();

                    foreach (uint index in htFunctionNames.Keys)
                    {
                        if (index >= startAddress && index <= startAddress + range)
                        {
                            modifiedNodes.AddLast(index);
                        }
                    }
                    foreach (uint index in modifiedNodes)
                    {
                        htFunctionNames[index] = groups[3].Value;
                    }
                }
            }
            m_input.Close();
        }
        // readStream is the stream you need to read
        // writeStream is the stream you want to write to
        static public void ReadWriteStream(Stream readStream, Stream writeStream)
        {
            readStream.Seek(0, SeekOrigin.Begin);
            writeStream.Seek(0, SeekOrigin.Begin);
            int Length = 65536;
            Byte[] buffer = new Byte[Length];
            int bytesRead;
            // write the required bytes
            do
            {
                bytesRead = readStream.Read(buffer, 0, Length);
                writeStream.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);
        }

        public uint EngineTimeOffset
        {
            set
            {
                this.engineTimeOffset = value;
            }
            get
            {
                return this.engineTimeOffset;
            }
            
        }

        public void WriteOffViewFormat(string fileName)
        {
            StreamWriter m_sw = new StreamWriter(fileName, false, Encoding.Default);          
            m_sw.WriteLine(c_line, "Calls", "Level", "Function", "Module", "Time", "Type", "Warnings");
            m_sw.WriteLine(c_line, "1", GetMinimalLevel().ToString(), "All functions", "0", GetTotalRuntime(), " ", " ");

            uint index = 0;
            uint maxNodeN = GetNMax();
            while(index < maxNodeN)
            {
                m_sw.WriteLine(c_line, "1", (GetUnitNestingLevel(index) + 1).ToString(), GetUnitFunctionName(index), "0", GetUnitInclTime(index), " ", " ");
                index++;
            }
            WriteGibberishForOffView(m_sw);
            m_sw.WriteLine();
            m_sw.WriteLine("STATISTICS:");
            m_sw.WriteLine("Total Threads = 0");
            m_sw.WriteLine("Maximum Concurrent Threads = 1");
            m_sw.WriteLine("Total Function Nodes = " + maxNodeN + 1);
            m_sw.WriteLine("Total Function Calls = " + maxNodeN + 1);
            m_sw.WriteLine("Memory Buffer Used = 1K");
            m_sw.WriteLine("Memory Buffer Allocated = 1K");
            m_sw.WriteLine("Memory Buffer Reserved = 2K");
            m_sw.Flush();
            m_sw.Close();
        }

        public void WriteOffViewFormat(string fileName, NativeProfilerUnit unit)
        {            
            StreamWriter m_sw = new StreamWriter(fileName, false, Encoding.Default);
            m_sw.WriteLine(c_line, "Calls", "Level", "Function", "Module", "Time", "Type", "Warnings");

            uint startLevel = unit.NestingLevel;
            uint index = unit.NItem;
            uint returnTime = unit.ReturnTime;
            uint maxNodeN = GetNMax();
            while (index < maxNodeN && ((NativeProfilerUnit)htN[index]).ReturnTime <= returnTime)
            {
                m_sw.WriteLine(c_line, "1", (GetUnitNestingLevel(index) - startLevel).ToString(), GetUnitFunctionName(index), "0", GetUnitInclTime(index), " ", " ");
                index++;
            }
            WriteGibberishForOffView(m_sw);
            m_sw.WriteLine();
            m_sw.WriteLine("STATISTICS:");
            m_sw.WriteLine("Total Threads = 0");
            m_sw.WriteLine("Maximum Concurrent Threads = 1");
            m_sw.WriteLine("Total Function Nodes = {0}", (index - unit.NItem));
            m_sw.WriteLine("Total Function Calls = {0}", (index - unit.NItem));
            m_sw.WriteLine("Memory Buffer Used = 1K");
            m_sw.WriteLine("Memory Buffer Allocated = 1K");
            m_sw.WriteLine("Memory Buffer Reserved = 2K");
            m_sw.Flush();
            m_sw.Close();
        }

        public void WriteOffViewFormat(string fileName, uint n)
        {
            StreamWriter m_sw = new StreamWriter(fileName, false, Encoding.Default);
            m_sw.WriteLine(c_line, "Calls", "Level", "Function", "Module", "Time", "Type", "Warnings");

            uint startLevel = GetUnit(n).NestingLevel;
            uint index = n;
            uint returnTime = this.GetUnitReturnTime(n);
            uint maxNodeN = GetNMax();
            while (index < maxNodeN && ((NativeProfilerUnit)htN[index]).ReturnTime <= returnTime)
            {
                m_sw.WriteLine(c_line, "1", (GetUnitNestingLevel(index) - startLevel).ToString(), GetUnitFunctionName(index), "0", GetUnitInclTime(index), " ", " ");
                index++;
            }
            WriteGibberishForOffView(m_sw);
            m_sw.WriteLine();
            m_sw.WriteLine("STATISTICS:");
            m_sw.WriteLine("Total Threads = 0");
            m_sw.WriteLine("Maximum Concurrent Threads = 1");
            m_sw.WriteLine("Total Function Nodes = {0}", (index - n));
            m_sw.WriteLine("Total Function Calls = {0}", (index - n));
            m_sw.WriteLine("Memory Buffer Used = 1K");
            m_sw.WriteLine("Memory Buffer Allocated = 1K");
            m_sw.WriteLine("Memory Buffer Reserved = 2K");
            m_sw.Flush();
            m_sw.Close();
        }

        private void WriteGibberishForOffView(StreamWriter m_sw)
        {

            /**********************************************************************/
            m_sw.WriteLine();
            m_sw.WriteLine("Hong Kong Phooey, number one super guy. Hong Kong Phooey, quicker than the");
            m_sw.WriteLine("human eye. He's got style, a groovy style, and a car that just won't stop. When");
            m_sw.WriteLine("the going gets tough, he's really rough, with a Hong Kong Phooey chop (Hi-Ya!).");
            m_sw.WriteLine("Hong Kong Phooey, number one super guy. Hong Kong Phooey, quicker than the");
            m_sw.WriteLine("human eye. Hong Kong Phooey, he's fan-riffic!");
            m_sw.WriteLine();
            m_sw.WriteLine("Children of the sun, see your time has just begun, searching for your ways,");
            m_sw.WriteLine("through adventures every day. Every day and night, with the condor in flight,");
            m_sw.WriteLine("with all your friends in tow, you search for the Cities of Gold.");
            m_sw.WriteLine("Ah-ah-ah-ah-ah... wishing for The Cities of Gold.  Ah-ah-ah-ah-ah... some day");
            m_sw.WriteLine("we will find The Cities of Gold.  Do-do-do-do ah-ah-ah, do-do-do-do, Cities of");
            m_sw.WriteLine("Gold. Do-do-do-do, Cities of Gold. Ah-ah-ah-ah-ah... some day we will find The");
            m_sw.WriteLine("Cities of Gold.");
            m_sw.WriteLine();
            m_sw.WriteLine("I never spend much time in school but I taught ladies plenty. It's true I hire");
            m_sw.WriteLine("my body out for pay, hey hey. I've gotten burned over Cheryl Tiegs, blown up");
            m_sw.WriteLine("for Raquel Welch. But when I end up in the hay it's only hay, hey hey. I might");
            m_sw.WriteLine("jump an open drawbridge, or Tarzan from a vine. 'Cause I'm the unknown stuntman");
            m_sw.WriteLine("that makes Eastwood look so fine.");
            /*********************************************************************/
        }
    }
}