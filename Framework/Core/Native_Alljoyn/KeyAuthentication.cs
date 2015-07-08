using System;
using System.Text;
using Microsoft.SPOT;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.AllJoyn
{
    public partial class AJ
    {   
        // write-only properties to transfer text
        // to native layer
    
        public string PwdText
        {
            set
            { 
                SetPassphrase(value);
            }
        }
                
        public string PskHint
        {
            set
            {
                SetPskHint(value);
            }
        }
        
        public string PskString
        {
            set
            {
                SetPskString(value);
            }
        }
        
        public string PemPriv
        {
            set
            {
                SetPemPrivString(value);
            }
        }
        
        public string PemX509
        {
            set
            {
                SetPemX509String(value);
            }
        }
        
        public UInt32 KeyExpiration
        {
            set
            {
                SetKeyExpiration(value);
            }
        }
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void UsePeerAuthentication(bool val);
        
        [MethodImpl(MethodImplOptions.InternalCall)]        
        public extern void SetPassphrase(string pass);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetPskHint(string hint);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetPskString(string psk);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetPemPrivString(string pem);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetPemX509String(string pem);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetKeyExpiration(UInt32 keyExpiration);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status AuthenticatePeer(UInt32 bus, string fullServiceName);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status GetAuthStatus();
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetAuthStatus(AJ_Status status);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status EnableSecurity(UInt32 bus, int [] securitySuites);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status ClearCredentials();
    }
}