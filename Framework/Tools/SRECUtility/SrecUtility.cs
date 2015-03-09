using System;
using System.IO;

namespace Microsoft.NetMicroFramework.Tools
{
    /// <summary>
    /// Summary description for SrecUtility
    /// </summary>
    public class SrecUtility
    {
        public static bool CreateSrec(string szfile, string szOutfile, uint nBaseAddress)
        {
            return CreateSrec(szfile, szOutfile, nBaseAddress, uint.MaxValue);
        }


        public static bool CreateSrec(string szfile, string szOutfile, uint nBaseAddress, uint nExecuteAddress)
        {
            FileStream objfsRead  = null;
            FileStream objfsWrite = null;
            int nChunkLength = 16;
            bool fRet = true;

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

                if (nExecuteAddress != uint.MaxValue)
                {
                    WriteS7(objfsWrite, nExecuteAddress);
                }
                
            }
            catch
            {
                fRet = false;
            }
            
            if (objfsRead != null)
            {
                objfsRead.Close();
            }

            if (objfsWrite != null)
            {
                objfsWrite.Close();
            }

            return fRet;
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
    }
}
