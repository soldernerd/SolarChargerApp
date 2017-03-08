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
     *  The Model 
     */
    public class Communicator
    {
        // Instance variables
        public HidUtility HidUtil { get; set; }
        private ushort _Vid;
        private ushort _Pid;
        public bool LedTogglePending { get; private set; }
        public bool WaitingForDevice { get; private set; }
        private byte LastCommand;
        public uint AdcValue { get; private set; }
        public bool PushbuttonPressed { get; private set; }
        public uint TxCount { get; private set; }
        public uint TxFailedCount { get; private set; }
        public uint RxCount { get; private set; }
        public uint RxFailedCount { get; private set; }
        //Information obtained from the solar charger
        public Int16 InputVoltage { get; private set; }
        public Int16 OutputVoltage { get; private set; }
        public Int16 InputCurrent { get; private set; }
        public Int16 OutputCurrent { get; private set; }
        public Int16 TemperatureOnboard { get; private set; }
        public Int16 TemperatureExternal1 { get; private set; }
        public Int16 TemperatureExternal2 { get; private set; }
        public bool PowerOutput1 { get; private set; }
        public bool PowerOutput2 { get; private set; }
        public bool PowerOutput3 { get; private set; }
        public bool PowerOutput4 { get; private set; }
        public bool PowerOutputUsb { get; private set; }
        public bool FanOutput { get; private set; }
        public byte DisplayMode { get; private set; }
        public DateTime SystemTime { get; private set; }

    public Communicator()
        {
            // Initialize variables
            _Vid = 0x04D8;
            _Pid = 0xF08E;
            TxCount = 0;
            TxFailedCount = 0;
            RxCount = 0;
            RxFailedCount = 0;
            LedTogglePending = false;
            LastCommand = 0x81;

            // Obtain and initialize an instance of HidUtility
            HidUtil = new HidUtility();
            HidUtil.SelectDevice(new Device(_Vid, _Pid));

            // Subscribe to HidUtility events
            HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;
            HidUtil.RaiseSendPacketEvent += SendPacketHandler;
            HidUtil.RaisePacketSentEvent += PacketSentHandler;
            HidUtil.RaiseReceivePacketEvent += ReceivePacketHandler;
            HidUtil.RaisePacketReceivedEvent += PacketReceivedHandler;
        }

        //Function to parse packet received over USB
        private void ParseStatusData(ref UsbBuffer InBuffer)
        {
            AdcValue = (uint)(InBuffer.buffer[5] << 8) + InBuffer.buffer[4]; //output voltage

            InputVoltage = (Int16) ((InBuffer.buffer[3] << 8) + InBuffer.buffer[2]);
            OutputVoltage = (Int16)((InBuffer.buffer[5] << 8) + InBuffer.buffer[4]);
            InputCurrent = (Int16)((InBuffer.buffer[7] << 8) + InBuffer.buffer[6]);
            OutputCurrent = (Int16)((InBuffer.buffer[9] << 8) + InBuffer.buffer[8]);
            TemperatureOnboard = (Int16)((InBuffer.buffer[11] << 8) + InBuffer.buffer[10]);
            TemperatureExternal1 = (Int16)((InBuffer.buffer[13] << 8) + InBuffer.buffer[12]);
            TemperatureExternal2 = (Int16)((InBuffer.buffer[15] << 8) + InBuffer.buffer[14]);
            PowerOutput1 = ((InBuffer.buffer[16] & 1) == 1);
            PowerOutput2 = ((InBuffer.buffer[16] & 2) == 2);
            PowerOutput3 = ((InBuffer.buffer[16] & 4) == 4);
            PowerOutput4 = ((InBuffer.buffer[16] & 8) == 8);
            PowerOutputUsb = ((InBuffer.buffer[16] & 16) == 16);
            FanOutput = ((InBuffer.buffer[16] & 32) == 32);
            DisplayMode = InBuffer.buffer[16];
            SystemTime = DateTime.Now;
        }

        // Accessor for _Vid
        // Only update selected device if the value has actually changed
        public ushort Vid
        {
            get
            {
                return _Vid;
            }
            set
            {
                if(value!=_Vid)
                {
                    _Vid = value;
                    HidUtil.SelectDevice(new Device(_Vid, _Pid));
                }
            }
        }

        // Accessor for _Pid
        // Only update selected device if the value has actually changed
        public ushort Pid
        {
            get
            {
                return _Pid;
            }
            set
            {
                if (value != _Pid)
                {
                    _Pid = value;
                    HidUtil.SelectDevice(new Device(_Vid, _Pid));
                }
            }
        }

        /*
         * HidUtility callback functions
         */

        public void ConnectionStatusChangedHandler(object sender, HidUtility.ConnectionStatusEventArgs e)
        {
            if (e.ConnectionStatus != HidUtility.UsbConnectionStatus.Connected)
            {
                // Reset variables
                TxCount = 0;
                TxFailedCount = 0;
                RxCount = 0;
                RxFailedCount = 0;
                LedTogglePending = false;
                LastCommand = 0x81;
            }
        }

        // HidUtility asks if a packet should be sent to the device
        // Prepare the buffer and request a transfer
        public void SendPacketHandler(object sender, UsbBuffer OutBuffer)
        {
            // Fill entire buffer with 0xFF
            OutBuffer.clear();
            if (LedTogglePending == true)
            {
                // The first byte is the "Report ID" and does not get sent over the USB bus. Always set = 0.
                OutBuffer.buffer[0] = 0;
                // 0x80 is the "Toggle LED" command in the firmware
                OutBuffer.buffer[1] = 0x80;
                LedTogglePending = false;
                LastCommand = 0x80;
            }
            else if (LastCommand == 0x81)
            {
                // The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                OutBuffer.buffer[0] = 0x00;
                // READ_POT command (see the firmware source code), gets 10-bit ADC Value
                OutBuffer.buffer[1] = 0x37;
                LastCommand = 0x37;
            }
            else
            {
                // The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                OutBuffer.buffer[0] = 0x00;
                // 0x81 is the "Get Pushbutton State" command in the firmware
                //OutBuffer.buffer[1] = 0x81;
                //LastCommand = 0x81;
                // 0X10 is GET_STATUS
                OutBuffer.buffer[1] = 0x10;
                LastCommand = 0x10;
            }
            // Request that this buffer be sent
            OutBuffer.RequestTransfer = true;
        }

        // HidUtility informs us if the requested transfer was successful
        // Schedule to request a packet if the transfer was successful
        public void PacketSentHandler(object sender, UsbBuffer OutBuffer)
        {
            if (LastCommand == 0x80)
            {
                WaitingForDevice = false;
            }
            else
            {
                WaitingForDevice = OutBuffer.TransferSuccessful;
            }
            if (OutBuffer.TransferSuccessful)
            {
                ++TxCount;
            }
            else
            {
                ++TxFailedCount;
            }
        }

        // HidUtility asks if a packet should be requested from the device
        // Request a packet if a packet has been successfully sent to the device before
        public void ReceivePacketHandler(object sender, UsbBuffer InBuffer)
        {
            InBuffer.RequestTransfer = WaitingForDevice;
        }

        // HidUtility informs us if the requested transfer was successful and provides us with the received packet
        public void PacketReceivedHandler(object sender, UsbBuffer InBuffer)
        {
            //WriteLog(string.Format("PacketReceivedHandler: {0:X2}", InBuffer.buffer[1]), false);
            WaitingForDevice = false;
            if (InBuffer.buffer[1] == 0x10)
            {
                ParseStatusData(ref InBuffer);
                //Need to reformat the data from two unsigned chars into one unsigned int.
                //AdcValue = (uint)(InBuffer.buffer[5] << 8) + InBuffer.buffer[4]; //output voltage
            }
            if (InBuffer.buffer[1] == 0x37)
            {
                //Need to reformat the data from two unsigned chars into one unsigned int.
                AdcValue = (uint)(InBuffer.buffer[3] << 8) + InBuffer.buffer[2];
            }
            if (InBuffer.buffer[1] == 0x81)
            {
                if (InBuffer.buffer[2] == 0x01)
                {
                    PushbuttonPressed = false;
                }
                if (InBuffer.buffer[2] == 0x00)
                {
                    PushbuttonPressed = true;
                }
            }
            if (InBuffer.TransferSuccessful)
            {
                ++RxCount;
            }
            else
            {
                ++RxFailedCount;
            }
        }

        public bool RequestLedToggleValid()
        {
            return !LedTogglePending;
        }

        public void RequestLedToggle()
        {
            LedTogglePending = true;
        }
    } // Communicator

    /*
     *  The Command Class
     */

    public class UiCommand : ICommand
    {
        private Action _Execute;
        private Func<bool> _CanExecute;
        public event EventHandler CanExecuteChanged;

        public UiCommand(Action Execute, Func<bool> CanExecute)
        {
            _Execute = Execute;
            _CanExecute = CanExecute;
        }
        public bool CanExecute(object parameter)
        {
            return _CanExecute();
        }
        public void Execute(object parameter)
        {
            _Execute();
        }
    }

    /*
     *  The ViewModel 
     */
    public class CommunicatorViewModel : INotifyPropertyChanged
    {
        private Communicator communicator;
        DispatcherTimer timer;
        private UiCommand buttonCommand;
        private DateTime ConnectedTimestamp = DateTime.Now;
        public string ActivityLogTxt { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CommunicatorViewModel()
        {
            communicator = new Communicator();
            communicator.HidUtil.RaiseDeviceAddedEvent += DeviceAddedEventHandler;
            communicator.HidUtil.RaiseDeviceRemovedEvent += DeviceRemovedEventHandler;
            communicator.HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;

            buttonCommand = new UiCommand(this.RequestLedToggle, communicator.RequestLedToggleValid);

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

        public void RequestLedToggle()
        {
            WriteLog("Toggle LED button clicked", false);
            communicator.RequestLedToggle();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("LedToggleActive"));
                PropertyChanged(this, new PropertyChangedEventArgs("PushbuttonContentTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("LedTogglePendingTxt"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public ICommand ToggleClick
        {
            get
            {
                return buttonCommand;
            }
        }

        public bool LedToggleActive
        {
            get
            {
                if(communicator.HidUtil.ConnectionStatus==HidUtility.UsbConnectionStatus.Connected)
                {
                    return communicator.RequestLedToggleValid();
                }
                return false;
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


        public string LedTogglePendingTxt
        {
            get
            {
                if (communicator.LedTogglePending)
                    return "Toggle pending";
                else
                    return "No action pending";
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

        public string PushbuttonContentTxt
        {
            get
            {
                if (communicator.LedTogglePending)
                    return "Toggle pending...";
                else
                    return "Toggle LED";
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
                if(communicator.HidUtil.ConnectionStatus==HidUtility.UsbConnectionStatus.Connected)
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

        public Int16 InputVoltage
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
                return string.Format("{0:0.000}V", (float) communicator.InputVoltage/1000.0);
            }
        }

        public Int16 OutputVoltage
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
                return string.Format("{0:0.000}V", (float) communicator.OutputVoltage/ 1000.0);
            }
        }

        public Int16 InputCurrent
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
                return string.Format("{0:0.000}A", (float) communicator.InputCurrent / 1000.0);
            }
        }

        public Int16 OutputCurrent
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
                return string.Format("{0:0.000}A", (float) communicator.OutputCurrent / 1000.0);
            }
        }

        public int InputPower
        {
            get
            {
                return (communicator.InputVoltage * communicator.InputCurrent) / 1000;
            }
        }

        public string InputPowerTxt
        {
            get
            {
                return string.Format("Input: {0:0.000}W", (float) ((communicator.InputVoltage * communicator.InputCurrent) / 1000000.0));
            }
        }

        public int OutputPower
        {
            get
            {
                return (communicator.OutputVoltage * communicator.OutputCurrent) / 1000;
            }
        }

        public string OutputPowerTxt
        {
            get
            {
                return string.Format("Output: {0:0.000}W", (float)((communicator.OutputVoltage * communicator.OutputCurrent) / 1000000.0));
            }
        }

        public int Loss
        {
            get
            {
                return ((communicator.InputVoltage * communicator.InputCurrent) - (communicator.OutputVoltage * communicator.OutputCurrent)) / 1000;
            }
        }

        public string LossTxt
        {
            get
            {
                return string.Format("Loss: {0:0.000}W", (float)(((communicator.InputVoltage * communicator.InputCurrent) - (communicator.OutputVoltage * communicator.OutputCurrent)) / 1000000.0));
            }
        }

        public int Efficiency
        {
            get
            {
                return (100 * (communicator.OutputVoltage * communicator.OutputCurrent) / (communicator.InputVoltage * communicator.InputCurrent));
            }
        }

        public string EfficiencyTxt
        {
            get
            {
                return string.Format("Efficiency: {0:0.00}%", (float)(100 * (communicator.OutputVoltage * communicator.OutputCurrent) / (communicator.InputVoltage * communicator.InputCurrent)));
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
                return string.Format("Onboard: {0:0.0}°C", (float) communicator.TemperatureOnboard/100);
            }
        }

        public string TemperatureExternal1Txt
        {
            get
            {
                return string.Format("External 1: {0:0.0}°C", (float)communicator.TemperatureExternal1 / 100);
            }
        }

        public string TemperatureExternal2Txt
        {
            get
            {
                return string.Format("External 2: {0:0.0}°C", (float)communicator.TemperatureExternal2 / 100);
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


    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

    }
}
