using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Management;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using HidUtilityNuget;
using ConfigurationFile;


namespace SolarChargerApp
{


    /*
     *  The ViewModel 
     */
    public class CommunicatorViewModel : INotifyPropertyChanged
    {
        private Communicator communicator;
        private ConfigFile config;
        DispatcherTimer timer;
        private DateTime ConnectedTimestamp = DateTime.Now;
        public string ActivityLogTxt { get; private set; }
        private byte DutycycleInput;
        private bool _ManualControl = false;
        private bool _SynchronousMode = false;
        private int _WindowPositionX;
        private int _WindowPositionY;
        public event PropertyChangedEventHandler PropertyChanged;

        public CommunicatorViewModel()
        {
            config = new ConfigFile("config.xml");
            _WindowPositionX = config.PositionX;
            _WindowPositionY = config.PositionY;

            communicator = new Communicator();
            communicator.HidUtil.RaiseDeviceAddedEvent += DeviceAddedEventHandler;
            communicator.HidUtil.RaiseDeviceRemovedEvent += DeviceRemovedEventHandler;
            communicator.HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;
            communicator.Vid = config.VendorId;
            communicator.Pid = config.ProductId;

            //Configure and start timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += TimerTickHandler;
            timer.Start();

            WriteLog("Program started", true);      
        }

        //Destructor
        ~CommunicatorViewModel()
        {
            //Save data to config file
            config.PositionX = _WindowPositionX;
            config.PositionY = _WindowPositionY;
        }

        /*
         * Local function definitions
         */

        // Add a line to the activity log text box
        void WriteLog(string message, bool clear)
        {
            // Replace content
            if (clear)
            {
                ActivityLogTxt = string.Format("{0}: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            }
            // Add new line
            else
            {
                ActivityLogTxt += Environment.NewLine + string.Format("{0}: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            }
        }

        public void RequestOut1Toggle()
        {
            WriteLog("Toggle Out1 button clicked", false);
            communicator.RequestPowerOutputMode(Communicator.PowerOutput.PowerOutput1, Communicator.PowerOutputAction.Toggle);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }


        public void RequestOut2Toggle()
        {
            WriteLog("Toggle Out2 button clicked", false);
            communicator.RequestPowerOutputMode(Communicator.PowerOutput.PowerOutput2, Communicator.PowerOutputAction.Toggle);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestOut3Toggle()
        {
            WriteLog("Toggle Out3 button clicked", false);
            communicator.RequestPowerOutputMode(Communicator.PowerOutput.PowerOutput3, Communicator.PowerOutputAction.Toggle);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestOut4Toggle()
        {
            WriteLog("Toggle Out4 button clicked", false);
            communicator.RequestPowerOutputMode(Communicator.PowerOutput.PowerOutput4, Communicator.PowerOutputAction.Toggle);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }


        public void RequestUsbToggle()
        {
            WriteLog("Toggle Usb button clicked", false);
            communicator.RequestPowerOutputMode(Communicator.PowerOutput.PowerOutputUsb, Communicator.PowerOutputAction.Toggle);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestTurnLeft()
        {
            WriteLog("Turn left button clicked", false);
            communicator.RequestEncoder(Communicator.RotaryEncoder.TurnLeft);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestTurnRight()
        {
            WriteLog("Turn right button clicked", false);
            communicator.RequestEncoder(Communicator.RotaryEncoder.TurnRight);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestButtonPress()
        {
            WriteLog("Press button clicked", false);
            communicator.RequestEncoder(Communicator.RotaryEncoder.ButtonPress);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void UseSystemTime()
        {
            WriteLog("Date and time set to system time", false);
            communicator.SetDateTime(Communicator.DateTimeElement.Year, (uint) (DateTime.Now.Year - 2000));
            communicator.SetDateTime(Communicator.DateTimeElement.Month, (uint) DateTime.Now.Month);
            communicator.SetDateTime(Communicator.DateTimeElement.Day, (uint) DateTime.Now.Day);
            communicator.SetDateTime(Communicator.DateTimeElement.Hour, (uint) DateTime.Now.Hour);
            communicator.SetDateTime(Communicator.DateTimeElement.Minute, (uint) DateTime.Now.Minute);
            communicator.SetDateTime(Communicator.DateTimeElement.Second, (uint) DateTime.Now.Second);
            communicator.SetDateTime(Communicator.DateTimeElement.EEPROM_WRITE_REQUEST, 0x00);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        /*
        public void SavePidVid()
        {
            WriteLog("Save button clicked", false);
            communicator.RequestPowerOutputMode(Communicator.PowerOutput.PowerOutput1, Communicator.PowerOutputAction.Toggle);
            //TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }
        */

        public void ResetPidVid()
        {
            WriteLog("Reset Pid/Vid button clicked", false);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("VidTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("PidTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void ResetCalibration()
        {
            WriteLog("Reset calibration button clicked", false);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputVoltageOffsetCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputVoltageSlopeCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputVoltageOffsetCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputVoltageSlopeCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputCurrentOffsetCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputCurrentSlopeCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputCurrentOffsetCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputCurrentSlopeCorrectionTxt"));

                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOnboardTemperatureOffsetCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOnboardTemperatureSlopeCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature1OffsetCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature1SlopeCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature2OffsetCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature2SlopeCorrectionTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrationRealTimeClockTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void ChargerOnOff()
        {
            if (OnOffButtonTxt == "Turn charger on")
            {
                WriteLog("Charger on button clicked", false);
                communicator.RequestChargerOnOff(Communicator.ChargerOnOff.On);
            }
            else
            {
                WriteLog("Charger off button clicked", false);
                communicator.RequestChargerOnOff(Communicator.ChargerOnOff.Off);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestIncreaseDutycycle()
        {
            WriteLog("Increase dutycycle button clicked", false);
            communicator.RequestIncreaseDutycycle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestDecreaseDutycycle()
        {
            WriteLog("Decrease dutycycle button clicked", false);
            communicator.RequestDecreaseDutycycle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void MenuDebuggingOutputToggle()
        {
            WriteLog("Menu item clicked", false);
            config.ActivityLogVisible = !config.ActivityLogVisible;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogVisibility"));
            }
        }

        public void SetDutycycle()
        {
            WriteLog(string.Format("Set dutycycle to {0}", DutycycleInput.ToString()), false);
            communicator.SetDutycycle(DutycycleInput);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public ICommand Out1ToggleClick
        {
            get
            {
                return new UiCommand(this.RequestOut1Toggle, communicator.RequestValid);
            }
        }

        public ICommand Out2ToggleClick
        {
            get
            {
                return new UiCommand(this.RequestOut2Toggle, communicator.RequestValid);
            }
        }

        public ICommand Out3ToggleClick
        {
            get
            {
                return new UiCommand(this.RequestOut3Toggle, communicator.RequestValid);
            }
        }

        public ICommand Out4ToggleClick
        {
            get
            {
                return new UiCommand(this.RequestOut4Toggle, communicator.RequestValid);
            }
        }

        public ICommand UsbToggleClick
        {
            get
            {
                return new UiCommand(this.RequestUsbToggle, communicator.RequestValid);
            }
        }

        public ICommand TurnLeftClick
        {
            get
            {
                return new UiCommand(this.RequestTurnLeft, communicator.RequestValid);
            }
        }

        public ICommand TurnRightClick
        {
            get
            {
                return new UiCommand(this.RequestTurnRight, communicator.RequestValid);
            }
        }

        public ICommand ButtonPressClick
        {
            get
            {
                return new UiCommand(this.RequestButtonPress, communicator.RequestValid);
            }
        }


        public ICommand UseSystemTimeClick
        {
            get
            {
                return new UiCommand(this.UseSystemTime, communicator.RequestValid);
            }
        }

        public ICommand DecreaseDutycycleClick
        {
            get
            {
                return new UiCommand(this.RequestDecreaseDutycycle, communicator.RequestValid);
            }
        }

        public ICommand IncreaseDutycycleClick
        {
            get
            {
                return new UiCommand(this.RequestIncreaseDutycycle, communicator.RequestValid);
            }
        }

        public ICommand SetDutycycleClick
        {
            get
            {
                return new UiCommand(this.SetDutycycle, communicator.RequestValid);
            }
        }

        public ICommand ChargerOnOffClick
        {
            get
            {
                return new UiCommand(this.ChargerOnOff, communicator.RequestValid);
            }
        }
        
        public ICommand MenuDebuggingOutputClick
        {
            get
            {
                return new UiCommand(this.MenuDebuggingOutputToggle, communicator.RequestValid);
            }
        }

        /*
        public ICommand SavePidVidClick
        {
            get
            {
                return new UiCommand(this.SavePidVid, communicator.RequestValid);
            }
        }
        */

        public ICommand ResetPidVidClick
        {
            get
            {
                return new UiCommand(this.ResetPidVid, communicator.RequestValid);
            }
        }

        public ICommand ResetCalibrationClick
        {
            get
            {
                return new UiCommand(this.ResetCalibration, communicator.RequestValid);
            }
        }

        public bool UserInterfaceActive
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                    return true;
                else
                    return false;
            }
        }

        public string UserInterfaceColor
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                    return "Black";
                else
                    return "Gray";
            }
        }

        public void TimerTickHandler(object sender, EventArgs e)
        {
            //WriteLog(communicator.DebugString, false);
            if (PropertyChanged != null)
            {
                if (communicator.NewStatusAvailable)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("InputVoltage"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputVoltageTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputVoltage"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputVoltageTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputCurrent"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputCurrentTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputCurrent"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputCurrentTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputPower"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputPowerTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputPower"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputPowerTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Loss"));
                    PropertyChanged(this, new PropertyChangedEventArgs("LossTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Efficiency"));
                    PropertyChanged(this, new PropertyChangedEventArgs("EfficiencyTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Output1Txt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Output2Txt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Output3Txt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Output4Txt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("UsbChargingTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("TemperatureOnboardTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("TemperatureExternal1Txt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("TemperatureExternal2Txt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FanTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("DateTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("TimeTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BuckModeTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("DutyCycleTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OnOffButtonTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("UserInterfaceEnabled"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputVoltageBarColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputVoltageBarColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputCurrentBarColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputCurrentBarColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("InputPowerBarColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputPowerBarColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("LossBarColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("EfficiencyBarColor"));
                }
                if (communicator.NewDisplayAvailable)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayTxt"));
                }
                if (communicator.NewCalibrationAvailable)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputVoltageNeutralTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputVoltageActualTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputVoltageNeutralTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputVoltageActualTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputCurrentNeutralTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationInputCurrentActualTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputCurrentNeutralTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOutputCurrentActualTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOnboardTemperatureNeutralTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationOnboardTemperatureActualTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature1NeutralTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature1ActualTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature2NeutralTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CalibrationExternalTemperature2ActualTxt"));
                }

                //Update these in any case
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ConnectionStatusTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("UptimeTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("TxSuccessfulTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("TxFailedTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("RxSuccessfulTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("RxFailedTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("TxSpeedTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("RxSpeedTxt"));

                PropertyChanged(this, new PropertyChangedEventArgs("WindowPositionX"));
                PropertyChanged(this, new PropertyChangedEventArgs("WindowPositionY"));
            }
        }

        public void DeviceAddedEventHandler(object sender, Device dev)
        {
            WriteLog("Device added: " + dev.ToString(), false);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DeviceListTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void DeviceRemovedEventHandler(object sender, Device dev)
        {
            WriteLog("Device removed: " + dev.ToString(), false);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("DeviceListTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }

        }

        public void ConnectionStatusChangedHandler(object sender, HidUtility.ConnectionStatusEventArgs e)
        {
            WriteLog("Connection status changed to: " + e.ToString(), false);
            switch (e.ConnectionStatus)
            {
                case HidUtility.UsbConnectionStatus.Connected:
                    ConnectedTimestamp = DateTime.Now;
                    break;
                case HidUtility.UsbConnectionStatus.Disconnected:
                    break;
                case HidUtility.UsbConnectionStatus.NotWorking:
                    break;
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ConnectionStatusTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("UptimeTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("UserInterfaceActive"));
                PropertyChanged(this, new PropertyChangedEventArgs("UserInterfaceColor"));
                PropertyChanged(this, new PropertyChangedEventArgs("LedToggleActive"));
                PropertyChanged(this, new PropertyChangedEventArgs("PushbuttonContentTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("AdcValue"));
            }
        }

        public string DeviceListTxt
        {
            get
            {
                string txt = "";
                foreach (Device dev in communicator.HidUtil.DeviceList)
                {
                    string devString = string.Format("VID=0x{0:X4} PID=0x{1:X4}: {2} ({3})", dev.Vid, dev.Pid, dev.Caption, dev.Manufacturer);
                    txt += devString + Environment.NewLine;
                }
                return txt.TrimEnd();
            }
        }

        public string PushbuttonStatusTxt
        {
            get
            {
                if (communicator.PushbuttonPressed)
                    return "Pushbutton pressed";
                else
                    return "Pushbutton not pressed";
            }
        }

        public uint AdcValue
        {
            get
            {
                return communicator.AdcValue;
            }
        }

        // Try to convert a (hexadecimal) string to an unsigned 16-bit integer
        // Return 0 if the conversion fails
        // This function is used to parse the PID and VID text boxes
        private ushort ParseHex(string input)
        {
            input = input.ToLower();
            if (input.Length >= 2)
            {
                if (input.Substring(0, 2) == "0x")
                {
                    input = input.Substring(2);
                }
            }
            try
            {
                ushort value = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
                return value;
            }
            catch
            {
                return 0;
            }
        }

        public string VidTxt
        {
            get
            {
                return string.Format("0x{0:X4}", communicator.Vid);
            }
            set
            {
                communicator.Vid = ParseHex(value);
                string log = string.Format("Trying to connect (VID=0x{0:X4} PID=0x{1:X4})", communicator.Vid, communicator.Pid);
                WriteLog(log, false);
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                config.VendorId = communicator.Vid;
            }
        }

        public string PidTxt
        {
            get
            {
                return string.Format("0x{0:X4}", communicator.Pid);
            }
            set
            {
                communicator.Pid = ParseHex(value);
                string log = string.Format("Trying to connect (VID=0x{0:X4} PID=0x{1:X4})", communicator.Vid, communicator.Pid);
                WriteLog(log, false);
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                config.ProductId = communicator.Pid;
            }
        }

        public string ConnectionStatusTxt
        {
            get
            {
                return string.Format("Connection Status: {0}", communicator.HidUtil.ConnectionStatus.ToString());
            }

        }

        public string UptimeTxt
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                {
                    //Save time elapsed since the device was connected
                    TimeSpan uptime = DateTime.Now - ConnectedTimestamp;
                    //Return uptime as string
                    return string.Format("Uptime: {0}", uptime.ToString(@"hh\:mm\:ss\.f"));
                }
                else
                {
                    return "Uptime: -";
                }
            }
        }

        public string TxSuccessfulTxt
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                    return string.Format("Sent: {0}", communicator.TxCount);
                else
                    return "Sent: -";
            }
        }



        public string TxFailedTxt
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                    return string.Format("Sending failed: {0}", communicator.TxFailedCount);
                else
                    return "Sending failed: -";
            }
        }

        public string RxSuccessfulTxt
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                    return string.Format("Received: {0}", communicator.RxCount);
                else
                    return "Receied: -";
            }
        }

        public string RxFailedTxt
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                    return string.Format("Reception failed: {0}", communicator.RxFailedCount);
                else
                    return "Reception failed: -";
            }
        }

        public string TxSpeedTxt
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                {
                    if (communicator.TxCount != 0)
                    {
                        return string.Format("TX Speed: {0:0.00} packets per second", communicator.TxCount / (DateTime.Now - ConnectedTimestamp).TotalSeconds);
                    }
                }
                return "TX Speed: n/a";
            }
        }

        public string RxSpeedTxt
        {
            get
            {
                if (communicator.HidUtil.ConnectionStatus == HidUtility.UsbConnectionStatus.Connected)
                {
                    if (communicator.TxCount != 0)
                    {
                        return string.Format("RX Speed: {0:0.00} packets per second", communicator.TxCount / (DateTime.Now - ConnectedTimestamp).TotalSeconds);
                    }
                }
                return "RX Speed: n/a";
            }
        }

        // New bindings

        public double InputVoltage
        {
            get
            {
                return communicator.InputVoltage;
            }
        }

        public string InputVoltageTxt
        {
            get
            {
                return string.Format("{0:0.000}V", communicator.InputVoltage);
            }
        }

        public double OutputVoltage
        {
            get
            {
                return communicator.OutputVoltage;
            }
        }

        public string OutputVoltageTxt
        {
            get
            {
                return string.Format("{0:0.000}V", communicator.OutputVoltage);
            }
        }

        public double InputCurrent
        {
            get
            {
                return communicator.InputCurrent;
            }
        }

        public string InputCurrentTxt
        {
            get
            {
                return string.Format("{0:0.000}A", communicator.InputCurrent);
            }
        }

        public double OutputCurrent
        {
            get
            {
                return communicator.OutputCurrent;
            }
        }

        public string OutputCurrentTxt
        {
            get
            {
                return string.Format("{0:0.000}A", communicator.OutputCurrent);
            }
        }

        public double InputPower
        {
            get
            {
                return communicator.InputPower;
            }
        }

        public string InputPowerTxt
        {
            get
            {
                return string.Format("Input: {0:0.000}W", communicator.InputPower);
            }
        }

        public double OutputPower
        {
            get
            {
                return communicator.OutputPower;
            }
        }

        public string OutputPowerTxt
        {
            get
            {
                return string.Format("Output: {0:0.000}W", communicator.OutputPower);
            }
        }

        public double Loss
        {
            get
            {
                return communicator.Loss;
            }
        }

        public string LossTxt
        {
            get
            {
                return string.Format("Loss: {0:0.000}W", communicator.Loss);
            }
        }

        public double Efficiency
        {
            get
            {
                return communicator.Efficiency;
            }
        }

        public string EfficiencyTxt
        {
            get
            {
                return string.Format("Efficiency: {0:0.00}%", 100 * communicator.Efficiency);
            }
        }

        public string Output1Txt
        {
            get
            {
                if (communicator.PowerOutput1)
                    return "Output 1 on";
                else
                    return "Output 1 off";
            }
        }

        public string Output2Txt
        {
            get
            {
                if (communicator.PowerOutput2)
                    return "Output 2 on";
                else
                    return "Output 2 off";
            }
        }

        public string Output3Txt
        {
            get
            {
                if (communicator.PowerOutput3)
                    return "Output 3 on";
                else
                    return "Output 3 off";
            }
        }

        public string Output4Txt
        {
            get
            {
                if (communicator.PowerOutput4)
                    return "Output 4 on";
                else
                    return "Output 4 off";
            }
        }

        public string UsbChargingTxt
        {
            get
            {
                if (communicator.PowerOutputUsb)
                    return "USB Charger on";
                else
                    return "USB Charger off";
            }
        }

        public string TemperatureOnboardTxt
        {
            get
            {
                return string.Format("Onboard: {0:0.0}°C", communicator.TemperatureOnboard);
            }
        }

        public string TemperatureExternal1Txt
        {
            get
            {
                return string.Format("External 1: {0:0.0}°C", communicator.TemperatureExternal1);
            }
        }

        public string TemperatureExternal2Txt
        {
            get
            {
                return string.Format("External 2: {0:0.0}°C", communicator.TemperatureExternal2);
            }
        }

        public string FanTxt
        {
            get
            {
                if (communicator.FanOutput)
                    return "Fan on";
                else
                    return "Fan off";
            }
        }

        public string DateTxt
        {
            get
            {
                return string.Format("{0:yyyy-MM-dd}", communicator.SystemTime);
            }
        }

        public string TimeTxt
        {
            get
            {
                return string.Format("{0:HH:mm:ss}", communicator.SystemTime);
            }
        }

        public string BuckModeTxt
        {
            get
            {
                switch (communicator.BuckMode)
                {
                    case 0x00:
                        return "Buck status: off";
                    case 0x01:
                        return "Buck status: starup";
                    case 0x02:
                        return "Buck status: asynchronous";
                    case 0x03:
                        return "Buck status: synchronous";
                    case 0x80:
                        return "Buck status: remote off";
                    case 0x81:
                        return "Buck status: remote starup";
                    case 0x82:
                        return "Buck status: remote asynchronous";
                    case 0x83:
                        return "Buck status: remote synchronous";
                    default:
                        return "Buck status: UNKNOWN";

                }
            }
        }

        public string DutyCycleTxt
        {
            get
            {
                if (communicator.BuckMode == 0x00 || communicator.BuckMode == 0x80)
                    return "Dutycycle: -";
                else
                    return string.Format("Dutycycle: {0} ({1:0.0}%)", communicator.BuckDutyCycle.ToString(), (double)communicator.BuckDutyCycle / 2.55);
            }
        }

        public string DisplayTxt
        {
            get
            {
                string txt = communicator.Display[0];
                txt += Environment.NewLine + communicator.Display[1];
                txt += Environment.NewLine + communicator.Display[2];
                txt += Environment.NewLine + communicator.Display[3];
                return txt;
            }
        }

        public string DutyCycleInputTxt
        {
            set
            {
                try
                {
                    uint tmp = uint.Parse(value);
                    DutycycleInput = (byte)tmp;
                }
                catch
                {
                    ;
                }
            }

            get
            {
                return DutycycleInput.ToString();
            }
        }

        public bool ManualControl
        {
            set
            {
                _ManualControl = value;
                if (_ManualControl)
                {
                    WriteLog("Manual control enabled", false);
                    communicator.RequestChargerControl(Communicator.ChargerControl.Remote);
                }
                else
                {
                    WriteLog("Manual control disabled", false);
                    communicator.RequestChargerControl(Communicator.ChargerControl.Local);
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                }
            }
            get
            {
                return _ManualControl;
            }
        }

        public bool SynchronousMode
        {
            set
            {
                _SynchronousMode = value;
                if (_SynchronousMode)
                {
                    WriteLog("Synchronous mode entered", false);
                    communicator.RequestChargerMode(Communicator.ChargerMode.SynchronousMode);
                }
                else
                {
                    WriteLog("Asynchronous mode entered", false);
                    communicator.RequestChargerMode(Communicator.ChargerMode.AsynchronousMode);
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
                }
            }
            get
            {
                return _SynchronousMode;
            }
        }

        public string OnOffButtonTxt
        {
            get
            {
                if (communicator.BuckMode == 0x00 || communicator.BuckMode == 0x80)
                {
                    return "Turn charger on";
                }
                else
                {
                    return "Turn charger off";
                }
            }
        }

        public bool UserInterfaceEnabled
        {
            get
            {
                return communicator.DisplayStatus>1;
            }
        }

        public string InputVoltageBarColor
        {
            get
            {
                if (communicator.InputVoltage < 17.0)
                    return "OliveDrab";
                else if (communicator.InputVoltage < 19.0)
                    return "DarkOrange";
                else if (communicator.InputVoltage < 21.0)
                    return "OrangeRed";
                else
                    return "Red";

            }
        }

        public string OutputVoltageBarColor
        {
            get
            {
                if (communicator.OutputVoltage < 11.0)
                    return "Red";
                else if (communicator.OutputVoltage < 11.5)
                    return "OrangeRed";
                else if (communicator.OutputVoltage < 12.0)
                    return "DarkOrange";
                else if (communicator.OutputVoltage < 13.3)
                    return "OliveDrab";
                else if (communicator.OutputVoltage < 13.5)
                    return "DarkOrange";
                else if (communicator.OutputVoltage < 14.0)
                    return "OrangeRed";
                else
                    return "Red";
            }
        }

        public string InputCurrentBarColor
        {
            get
            {
                if (communicator.InputCurrent < 4.0)
                    return "OliveDrab";
                else if (communicator.InputCurrent < 5.0)
                    return "DarkOrange";
                else if (communicator.InputCurrent < 6.0)
                    return "OrangeRed";
                else
                    return "Red";
            }
        }

        public string OutputCurrentBarColor
        {
            get
            {
                if (communicator.OutputCurrent < 5.0)
                    return "OliveDrab";
                else if (communicator.OutputCurrent < 6.0)
                    return "DarkOrange";
                else if (communicator.OutputCurrent < 7.0)
                    return "OrangeRed";
                else
                    return "Red";
            }
        }

        public string InputPowerBarColor
        {
            get
            {
                if (communicator.InputPower < 60.0)
                    return "OliveDrab";
                else if (communicator.InputPower < 75.0)
                    return "DarkOrange";
                else if (communicator.InputPower < 80.0)
                    return "OrangeRed";
                else
                    return "Red";
            }
        }

        public string OutputPowerBarColor
        {
            get
            {
                if (communicator.OutputPower < 60.0)
                    return "OliveDrab";
                else if (communicator.OutputPower < 75.0)
                    return "DarkOrange";
                else if (communicator.OutputPower < 80.0)
                    return "OrangeRed";
                else
                    return "Red";
            }
        }

        public string LossBarColor
        {
            get
            {
                if (communicator.Loss < 1.5)
                    return "OliveDrab";
                else if (communicator.Loss < 2.5)
                    return "DarkOrange";
                else if (communicator.Loss < 4.0)
                    return "OrangeRed";
                else
                    return "Red";
            }
        }

        public string EfficiencyBarColor
        {
            get
            {
                if (communicator.Efficiency > 0.97)
                    return "OliveDrab";
                else if (communicator.Efficiency < 96.0)
                    return "DarkOrange";
                else if (communicator.Efficiency < 95)
                    return "OrangeRed";
                else
                    return "Red";
            }
        }

        public string ActivityLogVisibility
        {
            get
            {
                if (config.ActivityLogVisible)
                    return "Visible";
                else
                    return "Collapsed";
            }
            set
            {
                if (value == "Visible")
                    config.ActivityLogVisible = true;
                else
                    config.ActivityLogVisible = false;
            }
        }

        public string OutputsVisibility
        {
            get
            {
                if (config.OutputsVisible)
                    return "Visible";
                else
                    return "Collapsed";
            }
            set
            {
                if (value == "Visible")
                    config.OutputsVisible = true;
                else
                    config.OutputsVisible = false;
            }
        }

        public string ChargerDisplayVisibility
        {
            get
            {
                if (config.ChargerDisplayVisible)
                    return "Visible";
                else
                    return "Collapsed";
            }
            set
            {
                if (value == "Visible")
                    config.ChargerDisplayVisible = true;
                else
                    config.ChargerDisplayVisible = false;
            }
        }

        public string CommunicationVisibility
        {
            get
            {
                if (config.ConnectionDetailsVisible)
                    return "Visible";
                else
                    return "Collapsed";
            }
            set
            {
                if (value == "Visible")
                    config.ConnectionDetailsVisible = true;
                else
                    config.ConnectionDetailsVisible = false;
            }
        }

        public int WindowPositionX
        {
            get
            {
                return _WindowPositionX;
            }
            set
            {
                _WindowPositionX = value;
            }
        }

        public int WindowPositionY
        {
            get
            {
                return _WindowPositionY;
            }
            set
            {
                _WindowPositionY = value;
            }
        }

        private string GetNeutralCalibrationString(Communicator.Calibration cal, string unit, bool autocal)
        {
            string s = "Neutral: Offset={0:D}, Slope={1:F6} {4}\n(Multiplier={2:D}, Shift={3:D})";
            if(autocal)
                s = "Neutral: Offset={0:D}, Slope={1:F6} {4}\n(Multiplier={2:D}, Shift={3:D}, AutoCalibration={5:D})";
            s = string.Format(s,
                cal.NeutralOffset,
                cal.NeutralSlope,
                cal.NeutralMultiplier,
                cal.NeutralShift,
                unit,
                cal.AutoCalibration);
            return s;
        }

        private string GetActualCalibrationString(Communicator.Calibration cal, string unit)
        {
            string s = string.Format("Actual: Offset={0:D}, Slope={1:F6} {4}\n(Multiplier={2:D}, Shift={3:D})",
                cal.Offset,
                cal.Slope,
                cal.Multiplier,
                cal.Shift,
                unit);
            return s;
        }

        public string CalibrationInputVoltageNeutralTxt
        {
            get { return GetNeutralCalibrationString(communicator.InputVoltageCalibration, "[mV/count]", false); }
        }

        public string CalibrationInputVoltageActualTxt
        {
            get { return GetActualCalibrationString(communicator.InputVoltageCalibration, "[mV/count]"); }
        }

        public string CalibrationInputVoltageOffsetCorrectionTxt
        {
            get { return string.Format("{0:D}", communicator.InputVoltageCalibration.OffsetCorrection); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.InputVoltageCalibration.OffsetCorrection)
                {
                    communicator.InputVoltageCalibration.OffsetCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.InputVoltageOffset);
                    WriteLog(string.Format("InputVoltageOffsetCorrection set to : {0:D}", new_value), false);
                }
            }
        }

        public string CalibrationInputVoltageSlopeCorrectionTxt
        {
            get { return string.Format("{0:F6}", communicator.InputVoltageCalibration.SlopeCorrection); }
            set
            {
                float new_value = float.Parse(value);
                if (new_value != communicator.InputVoltageCalibration.SlopeCorrection)
                {
                    communicator.InputVoltageCalibration.SlopeCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.InputVoltageSlope);
                    WriteLog(string.Format("InputVoltageSlopeCorrection set to : {0:F6}", new_value), false);
                }
            }
        }

        public string CalibrationOutputVoltageNeutralTxt
        {
            get { return GetNeutralCalibrationString(communicator.OutputVoltageCalibration, "[mV/count]", false); }
        }

        public string CalibrationOutputVoltageActualTxt
        {
            get { return GetActualCalibrationString(communicator.OutputVoltageCalibration, "[mV/count]"); }
        }

        public string CalibrationOutputVoltageOffsetCorrectionTxt
        {
            get { return string.Format("{0:D}", communicator.OutputVoltageCalibration.OffsetCorrection); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.OutputVoltageCalibration.OffsetCorrection)
                {
                    communicator.OutputVoltageCalibration.OffsetCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.OutputVoltageOffset);
                    WriteLog(string.Format("OutputVoltageOffsetCorrection set to : {0:D}", new_value), false);
                }
            }
        }

        public string CalibrationOutputVoltageSlopeCorrectionTxt
        {
            get { return string.Format("{0:F6}", communicator.OutputVoltageCalibration.SlopeCorrection); }
            set
            {
                float new_value = float.Parse(value);
                if (new_value != communicator.OutputVoltageCalibration.SlopeCorrection)
                {
                    communicator.OutputVoltageCalibration.SlopeCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.OutputVoltageSlope);
                    WriteLog(string.Format("OutputVoltageSlopeCorrection set to : {0:F6}", new_value), false);
                }
            }
        }

        public string CalibrationInputCurrentNeutralTxt
        {
            get { return GetNeutralCalibrationString(communicator.InputCurrentCalibration, "[mA/count]", true); }
        }

        public string CalibrationInputCurrentActualTxt
        {
            get { return GetActualCalibrationString(communicator.InputCurrentCalibration, "[mA/count]"); }
        }

        public string CalibrationInputCurrentOffsetCorrectionTxt
        {
            get { return string.Format("{0:D}", communicator.InputCurrentCalibration.OffsetCorrection); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.InputCurrentCalibration.OffsetCorrection)
                {
                    communicator.InputCurrentCalibration.OffsetCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.InputCurrentOffset);
                    WriteLog(string.Format("InputCurrentOffsetCorrection set to : {0:D}", new_value), false);
                }
            }
        }

        public string CalibrationInputCurrentSlopeCorrectionTxt
        {
            get { return string.Format("{0:F6}", communicator.InputCurrentCalibration.SlopeCorrection); }
            set
            {
                float new_value = float.Parse(value);
                if (new_value != communicator.InputCurrentCalibration.SlopeCorrection)
                {
                    communicator.InputCurrentCalibration.SlopeCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.InputCurrentSlope);
                    WriteLog(string.Format("InputCurrentSlopeCorrection set to : {0:F6}", new_value), false);
                }
            }
        }

        public string CalibrationOutputCurrentNeutralTxt
        {
            get { return GetNeutralCalibrationString(communicator.OutputCurrentCalibration, "[mA/count]", true); }
        }

        public string CalibrationOutputCurrentActualTxt
        {
            get { return GetActualCalibrationString(communicator.OutputCurrentCalibration, "[mA/count]"); }
        }

        public string CalibrationOutputCurrentOffsetCorrectionTxt
        {
            get { return string.Format("{0:D}", communicator.OutputCurrentCalibration.OffsetCorrection); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.OutputCurrentCalibration.OffsetCorrection)
                {
                    communicator.OutputCurrentCalibration.OffsetCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.OutputCurrentOffset);
                    WriteLog(string.Format("OutputCurrentOffsetCorrection set to : {0:D}", new_value), false);
                }
            }
        }

        public string CalibrationOutputCurrentSlopeCorrectionTxt
        {
            get { return string.Format("{0:F6}", communicator.OutputCurrentCalibration.SlopeCorrection); }
            set
            {
                float new_value = float.Parse(value);
                if (new_value != communicator.OutputCurrentCalibration.SlopeCorrection)
                {
                    communicator.OutputCurrentCalibration.SlopeCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.OutputCurrentSlope);
                    WriteLog(string.Format("OutputCurrentSlopeCorrection set to : {0:F6}", new_value), false);
                }
            }
        }

        public string CalibrationOnboardTemperatureNeutralTxt
        {
            get { return GetNeutralCalibrationString(communicator.OnboardTemperatureCalibration, "[0.01°C/count]", false); }
        }

        public string CalibrationOnboardTemperatureActualTxt
        {
            get { return GetActualCalibrationString(communicator.OnboardTemperatureCalibration, "[0.01°C/count]"); }
        }

        public string CalibrationOnboardTemperatureOffsetCorrectionTxt
        {
            get { return string.Format("{0:D}", communicator.OnboardTemperatureCalibration.OffsetCorrection); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.OnboardTemperatureCalibration.OffsetCorrection)
                {
                    communicator.OnboardTemperatureCalibration.OffsetCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.OnboardTemperatureOffset);
                    WriteLog(string.Format("OnboardTemperatureOffsetCorrection set to : {0:D}", new_value), false);
                }
            }
        }

        public string CalibrationOnboardTemperatureSlopeCorrectionTxt
        {
            get { return string.Format("{0:F6}", communicator.OnboardTemperatureCalibration.SlopeCorrection); }
            set
            {
                float new_value = float.Parse(value);
                if (new_value != communicator.OnboardTemperatureCalibration.SlopeCorrection)
                {
                    communicator.OnboardTemperatureCalibration.SlopeCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.OnboardTemperatureSlope);
                    WriteLog(string.Format("OnboardTemperatureSlopeCorrection set to : {0:F6}", new_value), false);
                }
            }
        }

        public string CalibrationExternalTemperature1NeutralTxt
        {
            get { return GetNeutralCalibrationString(communicator.ExternalTemperature1Calibration, "[0.01°C/count]", false); }
        }

        public string CalibrationExternalTemperature1ActualTxt
        {
            get { return GetActualCalibrationString(communicator.ExternalTemperature1Calibration, "[0.01°C/count]"); }
        }

        public string CalibrationExternalTemperature1OffsetCorrectionTxt
        {
            get { return string.Format("{0:D}", communicator.ExternalTemperature1Calibration.OffsetCorrection); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.ExternalTemperature1Calibration.OffsetCorrection)
                {
                    communicator.ExternalTemperature1Calibration.OffsetCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.ExternalTemperature1Offset);
                    WriteLog(string.Format("ExternalTemperature1OffsetCorrection set to : {0:D}", new_value), false);
                }
            }
        }

        public string CalibrationExternalTemperature1SlopeCorrectionTxt
        {
            get { return string.Format("{0:F6}", communicator.ExternalTemperature1Calibration.SlopeCorrection); }
            set
            {
                float new_value = float.Parse(value);
                if (new_value != communicator.ExternalTemperature1Calibration.SlopeCorrection)
                {
                    communicator.ExternalTemperature1Calibration.SlopeCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.ExternalTemperature1Slope);
                    WriteLog(string.Format("ExternalTemperature1SlopeCorrection set to : {0:F6}", new_value), false);
                }
            }
        }

        public string CalibrationExternalTemperature2NeutralTxt
        {
            get { return GetNeutralCalibrationString(communicator.ExternalTemperature2Calibration, "[0.01°C/count]", false); }
        }

        public string CalibrationExternalTemperature2ActualTxt
        {
            get { return GetActualCalibrationString(communicator.ExternalTemperature2Calibration, "[0.01°C/count]"); }
        }

        public string CalibrationExternalTemperature2OffsetCorrectionTxt
        {
            get { return string.Format("{0:D}", communicator.ExternalTemperature2Calibration.OffsetCorrection); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.ExternalTemperature2Calibration.OffsetCorrection)
                {
                    communicator.ExternalTemperature2Calibration.OffsetCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.ExternalTemperature2Offset);
                    WriteLog(string.Format("ExternalTemperature2OffsetCorrection set to : {0:D}", new_value), false);
                }
            }
        }

        public string CalibrationExternalTemperature2SlopeCorrectionTxt
        {
            get { return string.Format("{0:F6}", communicator.ExternalTemperature2Calibration.SlopeCorrection); }
            set
            {
                float new_value = float.Parse(value);
                if (new_value != communicator.ExternalTemperature2Calibration.SlopeCorrection)
                {
                    communicator.ExternalTemperature2Calibration.SlopeCorrection = new_value;
                    communicator.SetCalibration(Communicator.CalibrationItem.ExternalTemperature2Slope);
                    WriteLog(string.Format("ExternalTemperature2SlopeCorrection set to : {0:F6}", new_value), false);
                }
            }
        }

        public string CalibrationRealTimeClockTxt
        {
            get { return string.Format("{0:D}", communicator.CalibrationRealTimeClock); }
            set
            {
                Int16 new_value = Int16.Parse(value);
                if (new_value != communicator.CalibrationRealTimeClock)
                    communicator.SetRealTimeClockCalibration(new_value);
                WriteLog(string.Format("CalibrationRealTimeClock set to : {0:D}", new_value), false);
            }
        }

        public string LogInputFileTxt
        {
            get { return config.InputLogFile; }
            set { config.InputLogFile = value; }
        }

        public string LogOutputFileTxt
        {
            get { return config.OutputLogFile; }
            set { config.OutputLogFile = value; }
        }

    }

}