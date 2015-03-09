//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;
using System.Runtime.Serialization;

namespace Microsoft.SPOT.Debugger
{
    public class SRecordFile
    {
        public class Block
        {
            public uint         address;
            public MemoryStream data;
            public byte[]       signature;
            public bool         executable;
        }

        static public uint Parse( string file, ArrayList blocks, string signatureFile )
        {
            uint entrypoint = 0;

            if(System.IO.File.Exists( file ) == false)
            {
                throw new System.IO.FileNotFoundException( String.Format( "Cannot find {0}", file ) );
            }

            using(System.IO.StreamReader reader = new StreamReader( file ))
            {
                string line;
                int    lineNum = 0;

                while((line = reader.ReadLine()) != null)
                {
                    char[] lineBytes = line.ToCharArray();
                    int    len       = lineBytes.Length;
                    int    i;

                    lineNum++; if(len == 0) continue;

                    // we only accept S0, S3 and S7 records (header, memory loadable data, execution address)
                    if(
                        (Char.ToLower( lineBytes[0] ) != 's') ||
                        (lineBytes[1] != '0' && lineBytes[1] != '3' && lineBytes[1] != '7')
                      )
                    {
                        throw new System.ArgumentException( String.Format( "Unknown format at line {0} of {1}:\n {2}", lineNum, file, line ) );
                    }

                    // we discard S0 records
                    if( (Char.ToLower( lineBytes[0] ) == 's') && (lineBytes[1] == '0') )
                    {
                        continue;
                    }                      

                    int num = Byte.Parse( new string( lineBytes, 2, 2 ),  System.Globalization.NumberStyles.HexNumber );
                    if(num != ((len / 2) - 2))
                    {
                        throw new System.ArgumentException( String.Format( "Incorrect length at line {0} of {1}: {2}", lineNum, file, num ) );
                    }

                    byte crc = (byte)num;

                    for(i = 4; i<len - 2; i += 2)
                    {
                        crc += Byte.Parse( new string( lineBytes, i, 2 ), System.Globalization.NumberStyles.HexNumber );
                    }

                    byte checksum = Byte.Parse( new string( lineBytes, len - 2, 2 ), System.Globalization.NumberStyles.HexNumber );

                    if((checksum ^ crc) != 0xFF)
                    {
                        throw new System.ArgumentException( String.Format( "Incorrect crc at line {0} of {1}: got {2:X2}, expected {3:X2}", lineNum, file, crc, checksum ) );
                    }

                    num -= 5;

                    uint address = UInt32.Parse( new string( lineBytes, 4, 8 ), System.Globalization.NumberStyles.HexNumber );

                    if(lineBytes[1] == '7')
                    {
                        entrypoint = address;
                        for(i=0; i<blocks.Count; i++)
                        {
                            Block bl  = (Block)blocks[i];
                            if(bl.address == entrypoint)
                            {
                                bl.executable = true;
                            }
                        }
                        break;
                    }
                    else
                    {
                        Block bl = new Block();

                        bl.address = address;
                        bl.data    = new MemoryStream();
                        bl.executable = false;

                        for(i=0; i<num; i++)
                        {
                            bl.data.WriteByte( Byte.Parse( new string( lineBytes, 12 + i * 2, 2 ), System.Globalization.NumberStyles.HexNumber ) );
                        }

                        for(i=0; i<blocks.Count; i++)
                        {
                            Block bl2  = (Block)blocks[i];
                            int   num2 = (int)bl2.data.Length;

                            if(bl2.address + num2 == bl.address)
                            {
                                byte[] data = bl.data.ToArray();

                                bl2.data.Write( data, 0, data.Length );

                                bl = null;
                                break;
                            }

                            if(bl.address + num == bl2.address)
                            {
                                byte[] data = bl2.data.ToArray();

                                bl.data.Write( data, 0, data.Length );

                                bl2.address = bl.address;
                                bl2.data    = bl.data;

                                bl = null;
                                break;
                            }

                            if(bl.address < bl2.address)
                            {
                                blocks.Insert( i, bl );

                                bl = null;
                                break;
                            }
                        }

                        if(bl != null) 
                        {
                            if(signatureFile != null)
                            {
                                using(System.IO.FileStream fs = new FileStream( signatureFile, System.IO.FileMode.Open, System.IO.FileAccess.Read ))
                                {
                                    if(fs.Length != 128)
                                    {
                                        throw new ArgumentOutOfRangeException( String.Format( "Signature is not 128 bytes long; it is {0} bytes long", fs.Length ) );
                                    }

                                    byte[] signature = new byte[128];

                                    if(128 != fs.Read( signature, 0, 128 ))
                                    {
                                        throw new ArgumentOutOfRangeException( "Could not read 128 bytes from signature file" );
                                    }

                                    bl.signature = signature;
                                }
                            }
                            else
                            {
                                bl.signature = new byte[0];
                            }
                            
                            blocks.Add( bl );
                        }
                    }
                }
            }

            return entrypoint;
        }

        static public void Encode( Stream stream, byte[] buf, uint address )
        {
            StreamWriter writer = new StreamWriter( stream, Encoding.ASCII );

            uint len    = (uint)buf.Length;
            int  offset = 0;

            while(len > 0)
            {
                uint size = len > 16 ? 16 : len;
                byte crc  = (byte)(size + 5);

                writer.Write( "S3{0:X2}{1:X8}", size + 5, address );

                crc += (byte)(address >>  0);
                crc += (byte)(address >>  8);
                crc += (byte)(address >> 16);
                crc += (byte)(address >> 24);

                for(uint i=0; i<size; i++)
                {
                    byte v = buf[offset++];

                    writer.Write( "{0:X2}", v );

                    crc += v;
                }

                address += size;
                len     -= size;

                writer.WriteLine( "{0:X2}", (byte)~crc );
            }

            writer.Flush();
        }
    }
}
