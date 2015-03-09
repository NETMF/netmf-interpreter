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
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using System.Runtime.InteropServices;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns
{
    public interface IMFDeployForm
    {
        void DumpToOutput(string text);
        void DumpToOutput(string text, bool newLine);

        ReadOnlyCollection<string> Files { get;      }

        MFPortDefinition TransportTinyBooter { get; set;}
    }

    public abstract class MFPlugInMenuItem
    {
        private object m_tag       = null;

        public abstract string Name { get; }
        public object          Tag  { get { return m_tag;  } set { m_tag  = value; } }

        public virtual ReadOnlyCollection<MFPlugInMenuItem> Submenus { get { return null; } }

        public virtual bool    RunInSeparateThread { get { return true; } }
        public virtual bool    RequiresConnection { get { return true; } }
        public virtual void    OnAction(IMFDeployForm form, MFDevice device) { }
        public override string ToString() { return Name; }
    }
}
