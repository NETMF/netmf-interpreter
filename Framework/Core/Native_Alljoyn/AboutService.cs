using System;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace Microsoft.SPOT.AllJoyn
{       
    public partial class AJ
    {            
        public bool doAnnounce = false;                        
    
        public delegate AJ_Status AboutPropGetter(AJ_Message msg, string arg);
    
        public AboutPropGetter AboutPropGetterCB = null;
    
        public AJ_Status Initialize_AboutService()
        {
            return AJ_Status.AJ_OK;
        }        
        
        public AJ_Status AboutAnnounce(UInt32 bus, UInt32 busAboutPort)
        {
            AJ_Status status = AJ_Status.AJ_OK;
            AJ_Message announcement = new AJ_Message();

            if (!doAnnounce) {
                return status;
            }
            doAnnounce = false;
            if (busAboutPort == 0) {
                return status;
            }

            status = MarshalSignal(bus, announcement, AJ_SIGNAL_ABOUT_ANNOUNCE, 0, 0, (byte)ALLJOYN_FLAG_SESSIONLESS, 0);
            if (status != AJ_Status.AJ_OK) {
                goto ErrorExit;
            }
            status = MarshalArg(announcement, "q", ABOUT_VERSION);
            if (status != AJ_Status.AJ_OK) {
                goto ErrorExit;
            }
            status = MarshalArg(announcement, "q", busAboutPort);
            if (status != AJ_Status.AJ_OK) {
                goto ErrorExit;
            }
            status = MarshalObjectDescriptions(announcement);
            if (status != AJ_Status.AJ_OK) {
                goto ErrorExit;
            }

            if (AboutPropGetterCB != null) {
                //status = propStoreGetterCB(announcement, "", this);
                AboutPropGetterCB(announcement, "");
            } else {
                status = MarshalDefaultProps(announcement);
            }
            
            if (status != AJ_Status.AJ_OK) {
                goto ErrorExit;
            }
            
            return DeliverMsg(announcement);

        ErrorExit:
            return status;
        }
        
        static AJ_Status AboutGetPropCB(AJ_Message reply, AJ_Message msg, uint propId, AJ aj)
        {
            if (propId == AJ_PROPERTY_ABOUT_VERSION) {
                return aj.MarshalArg(reply, "q", ABOUT_VERSION);
            } else {
                return AJ_Status.AJ_ERR_UNEXPECTED;
            }
        }
        
        public AJ_Status AboutHandleGetProp(AJ_Message msg)
        {
            return BusGetProp(msg, AboutGetPropCB);
        }
        
        static AJ_Status AboutIconGetPropCB(AJ_Message reply, AJ_Message msg, uint propId, AJ aj)
        {
            AJ_Status status = AJ_Status.AJ_ERR_UNEXPECTED;

            if (propId == AJ_PROPERTY_ABOUT_ICON_VERSION_PROP) {
                status = aj.MarshalArg(reply, "q", ABOUT_ICON_VERSION);
            } else if (propId == AJ_PROPERTY_ABOUT_ICON_MIMETYPE_PROP) {
                status = aj.MarshalArg(reply, "s", aj.AboutIconMime);
            } else if (propId == AJ_PROPERTY_ABOUT_ICON_SIZE_PROP) {
                status = aj.MarshalArg(reply, "u", aj.AboutIconSize);
            }
            return status;
        }
        
        public AJ_Status AboutIconHandleGetProp(AJ_Message msg)
        {
            return BusGetProp(msg, AboutIconGetPropCB);
        }
        
        AJ_Status AboutIconHandleGetURL(AJ_Message msg, AJ_Message reply)
        {
            AJ_Status status;

            status = MarshalReplyMsg(msg, reply);
            if (status != AJ_Status.AJ_OK) {
                goto ErrorExit;
            }
            return MarshalArg(reply, "s", AboutIconURL);

        ErrorExit:
            return status;
        }
        
        public AJ_Status AboutHandleGetAboutData(AJ_Message msg, AJ_Message reply)
        {
            AJ_Status status = AJ_Status.AJ_OK;
            string language = String.Empty;

            language = UnmarshalArgs(msg, "s");
            if (status == AJ_Status.AJ_OK) {
                MarshalReplyMsg(msg, reply);
                if (AboutPropGetterCB != null) {
                    status = AboutPropGetterCB(reply, language);
                } else {
                    status = MarshalDefaultProps(reply);
                }
                if (status != AJ_Status.AJ_OK) {
                    //status = AJ_MarshalErrorMsg(msg, reply, AJ_ErrLanguageNotSuppored);
                }
            }
            return status;
        }
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalObjectDescriptions(AJ_Message msg);              
    
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalDefaultProps(AJ_Message msg);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern UInt32 GetArgPtr(int idx);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern string GetArgString(int idx);
    }
}