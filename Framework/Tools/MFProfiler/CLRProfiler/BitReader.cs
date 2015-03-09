////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace CLRProfiler
{
	internal class BitReader
	{
		private BinaryReader fp;

		private int incurr;
		private ulong current;

		internal ulong ReadBits(int numBits)
		{
			ulong r;
			int haveBits = 64 - incurr;
			int remains = (numBits - haveBits);
			if(haveBits >= numBits)
			{
				incurr += numBits;
				r = (current >> (-remains)) & ((1ul << numBits) - 1);
			}
			else
			{
				r = (current & ((1ul << haveBits) - 1)) << remains;
				current = (ulong)fp.ReadInt64();
				r |= (current >> (64 - remains)) & ((1ul << remains) - 1);
				incurr = remains;
			}

			return r;
		}

		internal BitReader(Stream s)
		{
			fp = new BinaryReader(s);
			current = 0;
			if (fp.BaseStream.Length != 0)
			{
				current = (ulong)fp.ReadInt64();
			}
			incurr = 0;
		}

		internal long Length
		{
			get
			{
				return fp.BaseStream.Length * 8;
			}
		}

		internal long Position
		{
			get
			{
				return fp.BaseStream.Position * 8 + incurr;
			}
			set
			{
				fp.BaseStream.Position = 8 * (value / 64);
				current = (ulong)fp.ReadInt64();
				incurr = 0;
				if(value % 64 != 0)
				{
					ReadBits((int)(value % 64));
				}
			}
		}

		internal void Close()
		{
			fp.Close();
			fp = null;
		}
	}
}
