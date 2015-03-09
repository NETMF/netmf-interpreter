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

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [Serializable]
    public class MFInvalidConfigurationSectorException : Exception
    {
        public override string Message { get { return Properties.Resources.InvalidCfgSector; } }
    }
    [Serializable]
    public class MFInvalidKeyLengthException : Exception
    {
        public override string Message { get { return Properties.Resources.InvalidKeyLength; } }
    }
    [Serializable]
    public class MFTinyBooterConnectionFailureException : Exception
    {
        public override string Message { get { return Properties.Resources.TinyBooterConnectionFailure; } }
    }
    [Serializable]
    public class MFInvalidConfigurationDataException : Exception
    {
        public override string Message { get { return Properties.Resources.InvalidConfig; } }
    }
    [Serializable]
    public class MFConfigSectorEraseFailureException : Exception
    {
        public override string Message { get { return Properties.Resources.ConfigSectorEraseFailure; } }
    }
    [Serializable]
    public class MFConfigSectorWriteFailureException : Exception
    {
        public override string Message { get { return Properties.Resources.ConfigSectorWriteFailure; } }
    }
    [Serializable]
    public class MFConfigurationSectorOutOfMemoryException : Exception
    {
        public override string Message { get { return Properties.Resources.ConfigSectorOutOfMemory; } }
    }
}
