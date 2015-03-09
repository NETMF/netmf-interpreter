using System;
using System.Collections.Generic;
//using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
//using System.ServiceModel.Web;
using System.Text;
using System.IO;

namespace WcfMtomService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IDataAccessService
    {
        [OperationContract]
        MtomData GetData(int value);

        [OperationContract]
        void SetData(MtomData data);

        [OperationContract]
        void SetFileInfo(FileInfo[] files);

        [OperationContract]
        NestedClass GetNestedData();

        [OperationContract]
        void SetNestedData(NestedClass data);


        /*
        [OperationContract]
        Stream GetData(int value);

        [OperationContract]
        void SetData(Stream value);
         */
    }

    [DataContract]
    public class NestedClass
    {
        MtomData _mData;
        byte[] _myData;

        [DataMember]
        public MtomData MTData
        {
            get { return _mData; }
            set { _mData = value; }
        }

        [DataMember]
        public byte[] MyData
        {
            get { return _myData; }
            set { _myData = value; }
        }
    }

    [DataContract]
    public class MtomData
    {
        byte[] _Data;

        [DataMember]
        public byte[] Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
    }

    [DataContract]
    public class FileInfo
    {
        string _Name;
        int _Size;

        [DataMember]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        [DataMember]
        public int Size
        {
            get { return _Size; }
            set { _Size = value; }
        }
    }
}
