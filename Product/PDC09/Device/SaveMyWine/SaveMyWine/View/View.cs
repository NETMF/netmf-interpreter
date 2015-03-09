using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Samples.SaveMyWine
{
    public delegate void UserRequestEventHandler(object sender, UserRequestEvent e);

    //--//

    public class UserRequestEvent
    {
        public enum UserRequest
        {
            Reset,
            ChangeSettings,
            SaveSettings,
            UpdateSensorData,
            SilenceAlarm,
        }

        public UserRequest Request;
        public object Data;

        public UserRequestEvent(UserRequest req, object data)
        {
            Request = req;
            Data = data;
        }

        public UserRequestEvent(UserRequest req)
            : this(req, null)
        {
        }
    }

    //--//

    public class View : ContentControl
    {
        protected object SyncRoot = new object();

        protected WineDataModel Model;

        //--//

        public View(WineDataModel model)
        {
            Model = model;
        }

        public event UserRequestEventHandler OnUserRequest;

        //--//

        protected void InvokeUserRequest(object sender, UserRequestEvent e)
        {
            if (OnUserRequest != null)
            {
                OnUserRequest(sender, e);
            }
        }
    }
}
