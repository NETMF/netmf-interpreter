using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

//using ChipworkX.Hardware;


namespace Microsoft.SPOT.Samples.SaveMyWine
{
    public class WineController
    {
        public WineController(Program application, WineDataModel model, View monitorView, View settingsView, WineServiceConnection dpws)
        {
            _application = application;
            _model = model;
            _monitorView = monitorView;
            _settingsView = settingsView;
            _dpws = dpws;

            _monitorView.OnUserRequest += new UserRequestEventHandler(OnUserRequest);
            _settingsView.OnUserRequest += new UserRequestEventHandler(OnUserRequest);

            Thread asyncRadioInitialization = new Thread( new ThreadStart( AsyncRadioInitializationCallback ) );
            asyncRadioInitialization.Start();

            _timeSync = new TimeSync();
            _timeSync.Start();

            _alarm = new Alarm((Cpu.Pin)53, model );

            Thread asyncServiceInitialization = new Thread(new ThreadStart( StartService ));
            asyncServiceInitialization.Start();
        }

        public void Shutdown()
        {
            _dpws.StopService();
            _radio.Dispose();
        }

        public void UpdateSensorData(double temperature, double humidity)
        {
            _model.UpdateTemperatureData(temperature);
            _model.UpdateHumidityData(humidity);
        }

        public void StartService()
        {
            _dpws.StartService();
        }

        private void AsyncRadioInitializationCallback()
        {
            _model.Radio = RadioState.Initializing;

            try
            {
                _radio = RadioClient.GetRadio(this);
            }
            catch
            {
                _model.Radio = RadioState.InitializationFailed;
            }
            finally
            {
                if (_radio != null)
                {
                    _model.Radio = RadioState.Initialized;
                }
            }
        }

        private void OnUserRequest( object sender, UserRequestEvent e)
        {
            switch(e.Request)
            {
                case UserRequestEvent.UserRequest.Reset:
                    _model.Reset();
                    break;
                case UserRequestEvent.UserRequest.ChangeSettings:
                    _application.ChangeView(_settingsView);
                    break;
                case UserRequestEvent.UserRequest.SaveSettings:
                    _model.TemperatureAlarmThresholdRange = ((Range[])e.Data)[0];
                    _model.HumidityAlarmThresholdRange = ((Range[])e.Data)[1];
                    _application.ChangeView(_monitorView);
                    break;
                case UserRequestEvent.UserRequest.UpdateSensorData:
                    _radio.Update();
                    break;
                case UserRequestEvent.UserRequest.SilenceAlarm:
                    _alarm.Stop();
                    break;
                default:
                    break;
            }
        }

        //--//

        Program _application;
        WineDataModel _model;
        View _monitorView;
        View _settingsView;
        RadioClient _radio;
        Alarm _alarm;
        WineServiceConnection _dpws;
        TimeSync _timeSync;
    }
}
