////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace CLRProfiler
{
    internal class Helpers
    {
        static internal long ReadNumber(BitReader br)
        {
            ulong tmp;
            int quadsToRead = 0;

            tmp = br.ReadBits(1);
            if(tmp == 0)
            {
                quadsToRead = 0;
            }
            else
            {
                tmp = br.ReadBits(2);
                if(tmp == 0)
                {
                    tmp = br.ReadBits(1);
                    if(tmp == 0)
                    {
                        quadsToRead = 1;
                    }
                    else
                    {
                        quadsToRead = 9;
                    }
                }
                else if(tmp == 1)
                {
                    quadsToRead = 8;
                }
                else if(tmp == 2)
                {
                    quadsToRead = 5;
                }
                else
                {
                    tmp = br.ReadBits(2);
                    if(tmp == 0)
                    {
                        quadsToRead = 2;
                    }
                    else if(tmp == 1)
                    {
                        quadsToRead = 4;
                    }
                    else if(tmp == 2)
                    {
                        tmp = br.ReadBits(1);
                        if(tmp == 0)
                        {
                            quadsToRead = 7;
                        }
                        else if(tmp == 1)
                        {
                            tmp = br.ReadBits(1);
                            if(tmp == 0)
                            {
                                quadsToRead = 3;
                            }
                            else
                            {
                                tmp = br.ReadBits(1);
                                if(tmp == 0)
                                {
                                    quadsToRead = 11;
                                }
                                else
                                {
                                    tmp = br.ReadBits(1);
                                    if(tmp == 0)
                                    {
                                        quadsToRead = 6;
                                    }
                                    else
                                    {
                                        tmp = br.ReadBits(2);
                                        if(tmp == 0)
                                        {
                                            tmp = br.ReadBits(1);
                                            if(tmp == 0)
                                            {
                                                quadsToRead = 16;
                                            }
                                            else
                                            {
                                                quadsToRead = 15;
                                            }
                                        }
                                        else if(tmp == 1)
                                        {
                                            quadsToRead = 14;
                                        }
                                        else if(tmp == 2)
                                        {
                                            quadsToRead = 13;
                                        }
                                        else
                                        {
                                            quadsToRead = 12;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        quadsToRead = 10;
                    }
                }
            }

            return (long)br.ReadBits(4 * quadsToRead);
        }

        static internal void WriteNumber(BitWriter bw, long n)
        {
            ulong number = (ulong)n;
            if(number < 1)
            {
                bw.WriteBits(0, 1);
            }
            else if(number < 16)
            {
                bw.WriteBits(128 + number, 8);
            }
            else if(number < 256)
            {
                bw.WriteBits(7168 + number, 13);
            }
            else if(number < 4096)
            {
                bw.WriteBits(499712 + number, 19);
            }
            else if(number < 65536)
            {
                bw.WriteBits(1900544 + number, 21);
            }
            else if(number < 1048576)
            {
                bw.WriteBits(6291456 + number, 23);
            }
            else if(number < 16777216)
            {
                bw.WriteBits(8287944704 + number, 33);
            }
            else if(number < 268435456)
            {
                bw.WriteBits(16106127360 + number, 34);
            }
            else if(number < 4294967296)
            {
                bw.WriteBits(21474836480 + number, 35);
            }
            else if(number < 68719476736)
            {
                bw.WriteBits(618475290624 + number, 40);
            }
            else if(number < 1099511627776)
            {
                bw.WriteBits(34084860461056 + number, 45);
            }
            else if(number < 17592186044416)
            {
                bw.WriteBits(4327677766926336 + number, 52);
            }
            else if(number < 281474976710656)
            {
                bw.WriteBits(558164878817230848 + number, 59);
            }
            else if(number < 4503599627370496)
            {
                bw.WriteBits(8926134461448323072 + number, 63);
            }
            else if(number < 72057594037927936)
            {
                bw.WriteBits(1981, 11);
                bw.WriteBits(number, 56);
            }
            else if(number < 1152921504606846976)
            {
                bw.WriteBits(3961, 12);
                bw.WriteBits(number, 60);
            }
            else
            {
                bw.WriteBits(3960, 12);
                bw.WriteBits(number, 64);
            }
        }
    }
}
