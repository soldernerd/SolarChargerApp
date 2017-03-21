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
using hid;


namespace SolarChargerApp
{


    /*
     *  The ViewModel 
     */
    public class CommunicatorViewModel : INotifyPropertyChanged
    {
        private Communicator communicator;
        DispatcherTimer timer;
        private DateTime ConnectedTimestamp = DateTime.Now;
        public string ActivityLogTxt { get; private set; }
        private byte DutycycleInput;
        private bool _ManualControl = false;
        private bool _SynchronousMode = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public CommunicatorViewModel()
        {
            communicator = new Communicator();
            communicator.HidUtil.RaiseDeviceAddedEvent += DeviceAddedEventHandler;
            communicator.HidUtil.RaiseDeviceRemovedEvent += DeviceRemovedEventHandler;
            communicator.HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;



            WriteLog("Program started", true);

            //Configure and start timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += TimerTickHandler;
            timer.Start();
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
            communicator.RequestOut1Toggle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }


        public void RequestOut2Toggle()
        {
            WriteLog("Toggle Out2 button clicked", false);
            communicator.RequestOut2Toggle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestOut3Toggle()
        {
            WriteLog("Toggle Out3 button clicked", false);
            communicator.RequestOut3Toggle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestOut4Toggle()
        {
            WriteLog("Toggle Out4 button clicked", false);
            communicator.RequestOut4Toggle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestUsbToggle()
        {
            WriteLog("Toggle Usb button clicked", false);
            communicator.RequestUsbToggle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestTurnLeft()
        {
            WriteLog("Turn left button clicked", false);
            communicator.RequestTurnLeft();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestTurnRight()
        {
            WriteLog("Turn right button clicked", false);
            communicator.RequestTurnRight();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void RequestButtonPress()
        {
            WriteLog("Press button clicked", false);
            communicator.RequestButtonPress();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void UseSystemTime()
        {
            WriteLog("Date and time set to system time", false);
            communicator.SetYear((uint)(DateTime.Now.Year - 2000));
            communicator.SetMonth((uint)DateTime.Now.Month);
            communicator.SetDay((uint)DateTime.Now.Day);
            communicator.SetHour((uint)DateTime.Now.Hour);
            communicator.SetMinute((uint)DateTime.Now.Minute);
            communicator.SetSecond((uint)DateTime.Now.Second);
            communicator.RequestDatetimeWrite();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void ChargerOnOff()
        {
            if (OnOffButtonTxt == "Turn charger on")
            {
                WriteLog("Charger on button clicked", false);
                communicator.RequestChargerOn();
            }
            else
            {
                WriteLog("Charger off button clicked", false);
                communicator.RequestChargerOff();
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
                    PropertyChanged(this, new PropertyChangedEventArgs("ConnectionStatusTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("UptimeTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("TxSuccessfulTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("TxFailedTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("RxSuccessfulTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("RxFailedTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("TxSpeedTxt"));
                    PropertyChanged(this, new PropertyChangedEventArgs("RxSpeedTxt"));
                }
                if (communicator.NewDisplayAvailable)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayTxt"));
                }
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

        public double InutPowerp
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
                return string.Format("Loss: {0:0.000}W", (float)(((communicator.InputVoltage * communicator.InputCurrent) - (communicator.OutputVoltage * communicator.OutputCurrent)) / 1000000.0));
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
                    communicator.EnableManualControl();
                }
                else
                {
                    WriteLog("Manual control disabled", false);
                    communicator.DisableManualControl();
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
                    communicator.RequestSynchronousMode();
                }
                else
                {
                    WriteLog("Asynchronous mode entered", false);
                    communicator.RequestAsynchronousMode();
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

    }

}