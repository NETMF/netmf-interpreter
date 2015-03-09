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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [Serializable]
    public class MFUserExitException : Exception
    {
        public override string Message { get { return Properties.Resources.ExceptionUserExit; } }
    }
    [Serializable]
    public class MFDeviceInUseException : Exception
    {
        public override string Message { get { return Properties.Resources.ExceptionDeviceInUse; } }
    }
    [Serializable]
    public class MFDeviceNoResponseException : Exception
    {
        public override string Message { get { return Properties.Resources.ExceptionNoResponse; } }
    }
    [Serializable]
    public class MFDeviceUnknownDeviceException : Exception
    {
        public override string Message { get { return Properties.Resources.ExceptionUnknownDevice; } }
    }
    [Serializable]
    public class MFInvalidNetworkAddressException : Exception
    {
        public override string Message { get { return Properties.Resources.ExceptionInvalidNetworkAddress; } }
    }
    [Serializable]
    public class MFInvalidMacAddressException : Exception
    {
        public override string Message { get { return Properties.Resources.ExceptionInvalidMacAddress; } }
    }
    [Serializable]
    public class MFInvalidFileFormatException : Exception
    {
        string m_file;

        public MFInvalidFileFormatException(string file)
        {
            m_file = file;
        }
        
        public override string Message { get { return string.Format(Properties.Resources.ExceptionInvalidFileFormat, m_file); } }
    }
    [Serializable]
    public class MFSignatureFailureException : Exception
    {
        string m_file;

        public MFSignatureFailureException(string file)
        {
            FileInfo fi = new FileInfo(file);
            m_file = fi.Name;
        }
        
        public override string Message { get { return string.Format(Properties.Resources.ExceptionSignatureFailure, m_file); } }
    }
}
