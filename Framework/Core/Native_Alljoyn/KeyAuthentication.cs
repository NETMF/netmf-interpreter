using System;
using System.Text;
using Microsoft.SPOT;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.AllJoyn
{
    public partial class AJ
    {        
        //private string m_PwdText;
        //public string PwdText
        //{
        //    set { m_PwdText = value; }
        //}
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void UsePeerAuthentication(bool val);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetPassphrase(string pass);
    }
}