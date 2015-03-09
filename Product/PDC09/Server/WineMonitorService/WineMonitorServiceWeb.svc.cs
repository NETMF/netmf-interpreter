using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.IO;
using System.Web.UI;
using System.ServiceModel.Activation;

namespace WineMonitorService
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WineMonitorServiceWeb : IWineMonitorWeb
    {
        #region IWineMonitorWeb Members

        const string c_ErrHtml = "<html><head><title>Wine Monitor Service</title><Body>{0}</Body></head></html>";

        public Stream GetHtmlPage(string cabinetId)
        {
            if(!WineMonitorService.s_wineCabinets.ContainsKey(cabinetId))
            {
                return new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(string.Format(c_ErrHtml, string.Format("Cabinet '{0}' is not registered!", cabinetId))));
                //return string.Format(c_ErrHtml, string.Format("Cabinet '{0}' is not registered!", cabinetId));
            }

            if(WineMonitorService.s_wineCabinets[cabinetId].SensorData.Count == 0)
            {
                return new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(string.Format(c_ErrHtml, string.Format("Cabinet '{0}' has no data!", cabinetId))));

                //return string.Format(c_ErrHtml, string.Format("Cabinet '{0}' has no data!", cabinetId));
            }

            WineSensorData wsd = WineMonitorService.s_wineCabinets[cabinetId].SensorData[0];

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.UTF8);
            Html32TextWriter hw = new Html32TextWriter(sw);

            if (hw.Encoding != Encoding.UTF8)
            {
                System.Diagnostics.Debugger.Break();
            }

            hw.WriteFullBeginTag("html");
            hw.WriteFullBeginTag("head");
            hw.WriteFullBeginTag("title");
            hw.Write(".Net Micro Framework Wine Cabinet Service");
            hw.WriteEndTag("title");
            hw.WriteEndTag("head");
            hw.WriteFullBeginTag("body");
            hw.WriteFullBeginTag("p");
            hw.Write(string.Format("'{0}' Sensor Data:", cabinetId));
            hw.WriteEndTag("p");
            hw.WriteBeginTag("table");
            hw.WriteAttribute("border", "");
            hw.WriteAttribute("cellpadding", "5");
            hw.Write(">");
            hw.WriteBeginTag("col");
            hw.WriteAttribute("width", "100");
            hw.Write("/>");
            hw.WriteFullBeginTag("tr");
            hw.WriteFullBeginTag("td");
            hw.Write("Temperature");
            hw.WriteEndTag("td");
            hw.WriteFullBeginTag("td");
            hw.Write(wsd.Temperature);
            hw.WriteEndTag("td");
            hw.WriteEndTag("tr");
            hw.WriteFullBeginTag("tr");
            hw.WriteFullBeginTag("td");
            hw.Write("Humidity");
            hw.WriteEndTag("td");
            hw.WriteFullBeginTag("td");
            hw.Write(wsd.Humidity);
            hw.WriteEndTag("td");
            hw.WriteEndTag("tr");
            hw.WriteFullBeginTag("tr");
            hw.WriteFullBeginTag("td");
            hw.Write("TimeStamp");
            hw.WriteEndTag("td");
            hw.WriteFullBeginTag("td");
            hw.Write(wsd.TimeStamp);
            hw.WriteEndTag("td");
            hw.WriteEndTag("tr");
            hw.WriteEndTag("table");
            hw.WriteEndTag("body");
            hw.WriteEndTag("html");

            hw.Flush();


            MemoryStream ret = new MemoryStream(ms.ToArray());

            //string ret = new string(UTF8Encoding.UTF8.GetChars(ms.ToArray()));

            hw.Close();
            hw.Dispose();

            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            return ret; //new MemoryStream(UTF8Encoding.UTF8.GetBytes(ret.ToCharArray()));
        }

        #endregion
    }
}
