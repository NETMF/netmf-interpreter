using System;
using System.Threading;
using Ws.Services;
using Ws.Services.Binding;
using System.Ext;
using System.Net;
using System.Xml;
using tempuri.org;

namespace Microsoft.SPOT.Sample
{
    public class MFSimpleServiceClient
    {
        public static void Main()
        {
            // Turn console messages on
            Console.Verbose = true;

            // Create a test application thread
            TestApplication testApp = new TestApplication();
            Thread testAppThread = new Thread(new ThreadStart(testApp.Run));
            testAppThread.Start();

            System.Ext.Console.Write("Application started...");
        }
    }

    /// <summary>
    /// SimpleService test application class.
    /// </summary>
    public class TestApplication
    {
        IDataAccessServiceClientProxy m_proxy;

        public void Run()
        {
            
            TestMtomService();
        }

        private void TestMtomService()
        {
            try
            {
                WS2007HttpBinding binding = new WS2007HttpBinding(
                    new HttpTransportBindingConfig(new Uri("http://localhost:51026/Service1.svc")));

                m_proxy = new IDataAccessServiceClientProxy(binding, new ProtocolVersion11());

                SetData req = new SetData();
                req.data = new schemas.datacontract.org.WcfMtomService.MtomData();

                req.data.Data =
                    System.Text.UTF8Encoding.UTF8.GetBytes(
@"A bedtime story is a traditional form of storytelling, where a story is told to a child at bedtime to prepare them for sleep.
Bedtime stories have many advantages, for parents/adults and children alike. The fixed routine of a bedtime story before sleeping has a relaxing effect, and the soothing voice of a person telling a story makes the child fall asleep more easily. The emotional aspect creates a bond between the storyteller and the listener, often a parent and child.
Bedtime stories can be read from a book, or rather, fictional stories made up by the storyteller. The stories are mostly rather short, between one and five minutes, and have a happy ending. A different form of bedtime reading is using longer stories, but dividing them up, thus creating cliffhangers. Children will look forward to their bedtime story, and a fixed routine is installed.
");
                m_proxy.SetData(req);

                GetData req2 = new GetData();
                // get bytes < 1000 to test Base64
                req2.value = 0;
                GetDataResponse resp = m_proxy.GetData(req2);
                Debug.Print(new string(System.Text.UTF8Encoding.UTF8.GetChars(resp.GetDataResult.Data)));
                
                // to get data > 1000 bytes to test mtom
                req2.value = 1;
                resp = m_proxy.GetData(req2);
                Debug.Print(new string(System.Text.UTF8Encoding.UTF8.GetChars(resp.GetDataResult.Data)));

                // to get data > 1000 (non-char) bytes to test mtom
                req2.value = 2;
                resp = m_proxy.GetData(req2);
                for (int i = 0; i < resp.GetDataResult.Data.Length; i++)
                {
                    if ((byte)i != resp.GetDataResult.Data[i])
                    {
                        Debug.Print("FAILURE!!!!! expected incrementing data byte values");
                        break;
                    }
                }

                SetNestedData req3 = new SetNestedData();
                req3.data = new schemas.datacontract.org.WcfMtomService.NestedClass();
                req3.data.MTData = new schemas.datacontract.org.WcfMtomService.MtomData();
                req3.data.MTData.Data = System.Text.UTF8Encoding.UTF8.GetBytes( @"A bedtime story is a traditional form of storytelling, where a story is told to a child at bedtime to prepare them for sleep." );
                req3.data.MyData = System.Text.UTF8Encoding.UTF8.GetBytes(@"Bedtime stories have many advantages, for parents/adults and children alike. The fixed routine of a bedtime story before sleeping has a relaxing effect, and the soothing voice of a person telling a story makes the child fall asleep more easily. The emotional aspect creates a bond between the storyteller and the listener, often a parent and child.");
                m_proxy.SetNestedData(req3);

                GetNestedData req4 = new GetNestedData();

                GetNestedDataResponse resp8 = m_proxy.GetNestedData(req4);
                Debug.Print(new string(System.Text.UTF8Encoding.UTF8.GetChars(resp8.GetNestedDataResult.MyData)));
                for (int i = 0; i < resp8.GetNestedDataResult.MTData.Data.Length; i++)
                {
                    if ((byte)i != resp8.GetNestedDataResult.MTData.Data[i])
                    {
                        Debug.Print("FAILURE!!!!! expected incrementing data byte values");
                        break;
                    }
                }


                SetFileInfo sfi = new SetFileInfo();

                sfi.files = new schemas.datacontract.org.WcfMtomService.ArrayOfFileInfo();

                sfi.files.FileInfo = new schemas.datacontract.org.WcfMtomService.FileInfo[2];

                sfi.files.FileInfo[0] = new schemas.datacontract.org.WcfMtomService.FileInfo();
                sfi.files.FileInfo[0].Name = "MyBook";
                sfi.files.FileInfo[0].Size = 340;
                sfi.files.FileInfo[1] = new schemas.datacontract.org.WcfMtomService.FileInfo();
                sfi.files.FileInfo[1].Name = "MyBook2";
                sfi.files.FileInfo[1].Size = 341;

                m_proxy.SetFileInfo(sfi);
            }
            catch (System.IO.IOException)
            {
                // Is the web service down???
                Debug.Print("Warning: could not connect to stocks web service");
            }
            finally
            {
                if (m_proxy != null) m_proxy.Dispose();
            }
        }
    }
}
