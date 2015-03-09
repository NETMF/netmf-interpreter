using System;
using System.IO;

namespace binToSrec
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//
			// TODO: Add code to start application here
			//
			uint nBaseAddress = 0; 
			bool bBaseAddressSet = false;
			uint nEndAddress = 0; 
			bool bEndAddressSet = false;
			string szInfile = null; //compulsory to enter
			string szOutFile = null;

			try 
			{
				for (int i=0;i<args.Length;i++)
				{
					if (args[i].Trim().ToUpper() == "-I")
					{
						i = i + 1;
						szInfile = args[i];
					}

					if (args[i].Trim().ToUpper() == "-O")
					{
						i = i + 1;
						szOutFile = args[i];
					}

					if (args[i].Trim().ToUpper() == "-B")
					{
						i = i + 1;
						nBaseAddress = (uint)System.Int32.Parse(args[i], System.Globalization.NumberStyles.HexNumber);
						bBaseAddressSet = true;
					}

					if (args[i].Trim().ToUpper() == "-E")
					{
						i = i + 1;
						nEndAddress = (uint)System.Int32.Parse(args[i] , System.Globalization.NumberStyles.HexNumber);
						bEndAddressSet = true;
					}
				
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Usage();
				return;
			}

			if (szInfile == null || bBaseAddressSet == false)
			{
				Usage();
				return;
			}

			if (szOutFile == null)
			{
				szOutFile = szInfile.Trim() + ".srec"; //output file = infile name  + srec
			}

			DisplayParameters(szInfile, szOutFile, nBaseAddress, nEndAddress);
			if (File.Exists(szInfile) == false)
			{
				Console.WriteLine("InFile Does not Exists");
				return;
			}

			if (-1 == DoConversion(szInfile, szOutFile, nBaseAddress, nEndAddress, bEndAddressSet))
			{
				Console.WriteLine("We have a problem doing the conversion");
			}
			else
			{
				Console.WriteLine("Conversion is Successful");
			}
		}

		static int DoConversion(string szfile, string szOutfile, uint nBaseAddress, uint nEndaddress, bool bEndAddressSet)
		{
			FileStream objfsRead  = null;
			FileStream objfsWrite = null;
			int nChunkLength = 16;
			int nRet = 0;

			try
			{
				//First Open The file handle
				objfsRead  = new FileStream(szfile   , FileMode.Open  , FileAccess.Read, FileShare.ReadWrite);
				objfsWrite = new FileStream(szOutfile, FileMode.Create, FileAccess.Write);

				//Now read chunk of 16 bytes - 
				int nRead = 0 ;
				byte[] arrByte = new byte[nChunkLength];
				uint nCurrentOffSet = nBaseAddress;
				while ((nRead = objfsRead.Read(arrByte, 0, nChunkLength))!= 0)
				{
					char [] arrRecord = ConstructSrecRecord("S3", arrByte, nRead, nCurrentOffSet);
					if (arrRecord != null)
					{
						for (int i=0;i<arrRecord.Length;i++)
						{
							objfsWrite.WriteByte((byte)arrRecord[i]);
						}

						objfsWrite.WriteByte((byte)'\r');
						objfsWrite.WriteByte((byte)'\n');
					}
					else
						throw new Exception("Problem in Code");

					nCurrentOffSet = nCurrentOffSet + 16;
				}

				if (bEndAddressSet)
				{
					WriteS7(objfsWrite, nEndaddress);
				}
				
			}
			catch
			{
				nRet = -1;
			}
			
			if (objfsRead != null)
			{
				objfsRead.Close();
			}

			if (objfsWrite != null)
			{
				objfsWrite.Close();
			}

			return nRet;
		}


		static void WriteS7(FileStream objfsWrite, uint nEndaddress)
		{
			int nSum = 0;
			
			//Get the address
			char [] arrByte = new char[2 + 2 + 8 + 2];
			arrByte[0]= 'S';
			arrByte[1] = '7';
			arrByte[2]= '0';
			arrByte[3] = '5';

			nSum = 5;
			string sHex = String.Format( "{0:X8}", nEndaddress);

			for (int j=0;j<sHex.Length;j++)
			{
				arrByte[j + 4] = sHex[j];
			}

			//for check sum
			for (int j=0;j<4;j++)
			{
				nSum = (int)(nSum + (nEndaddress & 255));
				nEndaddress  = nEndaddress >> 8;
			}

			nSum = int.MaxValue - nSum;
			sHex = String.Format( "{0:X8}", nSum);

			arrByte[arrByte.Length -2] = sHex[sHex.Length - 2];
			arrByte[arrByte.Length -1] = sHex[sHex.Length - 1]; 

			for (int i=0;i<arrByte.Length;i++)
			{
				objfsWrite.WriteByte((byte)arrByte[i]);
			}

			objfsWrite.WriteByte((byte)'\r');
			objfsWrite.WriteByte((byte)'\n');
		}

		static char [] ConstructSrecRecord(string szType, byte [] arrByteInput , int nRead, uint nCurrentAddress)
		{
			int nBaseLength = 2 + 2 + 8 + 2;
			char[] arrByte = new char[nBaseLength + (nRead*2)];
			
			//BUG -BUG we can have an exception here
			if (szType == null || szType.Length != 2)
			{
				return null;
			}
		
			//Put in the type
			arrByte[0] = szType[0];
			arrByte[1] = szType[1];

			string sHex=null;

			//Put in the count
			byte byCount = (byte)((arrByte.Length - 4) / 2);
			sHex =  String.Format( "{0:X2}", byCount );
			arrByte[2] = sHex[0];
			arrByte[3] = sHex[1];

			int nSum = 0;
			nSum = (int) byCount;

			//Get the address
			sHex =  String.Format( "{0:X8}", nCurrentAddress );
			for (int i=0;i<sHex.Length;i++)
			{
				arrByte[4+i] = sHex[i];
			}

			//for check sum
			for (int j=0;j<4;j++)
			{
				nSum = (int)(nSum + (nCurrentAddress & 255));
				nCurrentAddress  = nCurrentAddress >> 8;
			}

			for (int s=0;s<nRead;s++)
			{
				nSum = nSum + arrByteInput[s];
			}
			nSum = int.MaxValue - nSum;
			//end check sum

			//Put in the Data
			ConvertToHexAscii(arrByte, 12, arrByteInput, nRead);

			//Check Sum Right now
			sHex =  String.Format( "{0:X8}", nSum );
			arrByte[arrByte.Length - 2] = sHex[sHex.Length - 2];
			arrByte[arrByte.Length - 1] = sHex[sHex.Length - 1];
			//Then Put in the new line thing

			return arrByte;
		}
		
		static void ConvertToHexAscii(char[] arrToWriteTo, int nIndexToStart , byte [] arrInput, int nRead)
		{
			string sHex = null;
			for (int i=0;i<nRead;i++)
			{
				sHex =  String.Format( "{0:X2}", arrInput[i] );
				arrToWriteTo[nIndexToStart] = sHex[0];
				arrToWriteTo[nIndexToStart + 1] = sHex[1];
				nIndexToStart = nIndexToStart + 2;
			}
		}

		static void DisplayParameters(string szInfile,string  szOutFile,uint nBaseAddress,uint nEndAddress)
		{	
			Console.WriteLine("InFile  : " + szInfile);
			Console.WriteLine("OutFile : " + szOutFile);
			Console.WriteLine("Base    : 0x" + nBaseAddress.ToString("x"));
			Console.WriteLine("End     : 0x" + nEndAddress.ToString("x"));
		}

		//hpleung
		//binToSrec.exe -b 0x001000  -i inFile.bin <-o outfile.srec> <-e 0x0002000>
		static void Usage()
		{
			Console.WriteLine("binToSrec.exe -b e0434f4d  -i inFile.bin <-o outfile.srec> <-e e0434f4d>");
			Console.WriteLine("DO NOT PUT 0x in front of the HEX");
			Console.WriteLine("-b Specifies the base address, mandatory");
			Console.WriteLine("-e Specified the Starting execution address : When Set will write an S7 Record");
			Console.WriteLine("-i Specified the binary file to convert");
			Console.WriteLine("-o Specified the output srec file");
		}
	}
}
