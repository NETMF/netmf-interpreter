using System;
using System.Collections.Generic;
//using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
//using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace WcfMtomService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class DataAccessService : IDataAccessService
    {
        public void SetFileInfo(FileInfo[] files)
        {
            foreach (FileInfo fi in files)
            {
                Debug.WriteLine(fi.Name);
                Debug.WriteLine(fi.Size);
            }

        }

        #region IDataAccessService Members

        public MtomData GetData(int value)
        {
            MtomData ret = new MtomData();

            if (value == 0)
            {
                // less than 1000 bytes uses Base64 encoding
                ret.Data = UTF8Encoding.UTF8.GetBytes(
                    @"Understanding how to name your services on the Service Bus is of central importance.");
            }
            else if (value == 1)
            {
                ret.Data = UTF8Encoding.UTF8.GetBytes(
@"The M777 began as the Ultralight-weight Field Howitzer (UFH), developed by VSEL's armaments division in Barrow-in-Furness, United Kingdom. In 1999, after acquisition by BAE, VSEL was merged into the new BAE Systems RO Defence. This unit became part of BAE Systems Land Systems in 2004. Although developed by a British company, final assembly is in the US. BAE System's original US partner was United Defense. However in 2005, BAE acquired United Defense and hence is responsible for design, construction and assembly (through its US-based Land and Armaments group). The M777 uses about 70% US built parts including the gun barrel manufactured at the Watervliet Arsenal.
The M777 is smaller and 42% lighter, at under 4,100 kg (9,000 lb), than the M198 it replaces. Most of the weight reduction is due to the use of titanium. The lighter weight and smaller size allows the M777 to be transported by USMC MV-22 Osprey, CH-47 helicopter or truck with ease, so that it can be moved in and out of the battlefield more quickly than the M198. The smaller size also improves storage and transport efficiency in military warehouses and Air/Naval Transport. The gun crew required is an Operational Minimum of five, compared to a previous size of nine.[1]
The M777 uses a digital fire-control system similar to that found on self propelled howitzers such as the M109A6 Paladin to provide navigation, pointing and self-location, allowing it to be put into action more quickly than earlier towed and air-transported howitzers. The Canadian M777 in conjunction with the traditional 'glass and iron sights/mounts' also uses a digital fire control system called Digital Gun Management System (DGMS) produced by SELEX. This system has been proven on the British Army Artillery's L118 Light Gun over the past three to four years."
);

            }
            else
            {
                ret.Data = new byte[2048];

                for (int i = 0; i < ret.Data.Length; i++)
                {
                    ret.Data[i] = (byte)i;
                }
            }
            return ret;
        }

        public void SetData(MtomData data)
        {
            Debug.WriteLine(new string(UTF8Encoding.UTF8.GetChars(data.Data)));
        }

        #endregion

        #region IDataAccessService Members


        public NestedClass GetNestedData()
        {
            NestedClass ret = new NestedClass();

            ret.MTData = GetData(3);

            ret.MyData = GetData(1).Data;

            return ret;
        }

        public void SetNestedData(NestedClass data)
        {
            Debug.WriteLine(new string(UTF8Encoding.UTF8.GetChars(data.MyData)));

            Debug.WriteLine(new string(UTF8Encoding.UTF8.GetChars(data.MTData.Data)));
        }

        #endregion
    }
}
