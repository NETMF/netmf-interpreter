using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WineMonitorDevice
{
    public class WineMonitorDevice : IWineMonitorRequest
    {
        #region IWineMonitorRequest Members

        public void SetCabinetId(int cabinetId)
        {
            throw new NotImplementedException();
        }

        public void RequestUpdate()
        {
            throw new NotImplementedException();
        }

        public void SetThresholds(WineSensorThresholdReq alert)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
