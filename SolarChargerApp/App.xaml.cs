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
        private List<byte> PendingCommands;
        public bool WaitingForDevice { get; private set; }
        private byte LastCommand;
        public uint AdcValue { get; private set; }
        public bool PushbuttonPressed { get; private set; }
        public uint TxCount { get; private set; }
        public uint TxFailedCount { get; private set; }
        public uint RxCount { get; private set; }
        public uint RxFailedCount { get; private set; }
        //Information obtained from the solar charger
        public double InputVoltage { get; private set; }
        public double OutputVoltage { get; private set; }
        public double InputCurrent { get; private set; }
        public double OutputCurrent { get; private set; }
        public double InputPower { get; set; }
        public double OutputPower { get; set; }
        public double Loss { get; set; }
        public double Efficiency { get; set; }
        public double TemperatureOnboard { get; private set; }
        public double TemperatureExternal1 { get; private set; }
        public double TemperatureExternal2 { get; private set; }
        public bool PowerOutput1 { get; private set; }
        public bool PowerOutput2 { get; private set; }
        public bool PowerOutput3 { get; private set; }
        public bool PowerOutput4 { get; private set; }
        public bool PowerOutputUsb { get; private set; }
        public bool FanOutput { get; private set; }
        public byte DisplayMode { get; private set; }
        public DateTime SystemTime { get; private set; }
        public byte BuckMode { get; private set; }
        public byte BuckDutyCycle { get; private set; }
        public string[] Display { get; private set; } = new string[4];
        //Others
        private bool _NewStatusAvailable;
        private bool _NewDisplay1Available;
        private bool _NewDisplay2Available;

        public Communicator()
        {
            // Initialize variables
            _Vid = 0x04D8;
            _Pid = 0xF08E;
            TxCount = 0;
            TxFailedCount = 0;
            RxCount = 0;
            RxFailedCount = 0;
            PendingCommands = new List<byte>();
            LastCommand = 0x12;
            _NewStatusAvailable = false;
            _NewDisplay1Available = false;
            _NewDisplay2Available = false;

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

        //Convert binary coded decimal byte to integer
        private uint BcdToUint(byte bcd)
        {
            uint lower = (uint) (bcd & 0x0F);
            uint upper = (uint) (bcd >> 4);
            return (10 * upper) + lower;
        }

        //Convert integer to binary encoded decimal byte
        private byte UintToBcd(uint val)
        {
            uint lower = val % 10;
            uint upper = val / 10;
            byte retval = (byte) upper;
            retval <<= 4;
            retval |= (byte)lower;
            return retval;
        }

        //Accessors for the UI to call
        public bool NewStatusAvailable
        {
            get
            {
                if (_NewStatusAvailable)
                {
                    _NewStatusAvailable = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool NewDisplayAvailable
        {
            get
            {
                if (_NewDisplay1Available && _NewDisplay2Available)
                {
                    _NewDisplay1Available = false;
                    _NewDisplay2Available = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //Function to parse packet received over USB
        private void ParseStatusData(ref UsbBuffer InBuffer)
        {
            //Input values are mainly encoded as Int16
            Int16 tmp;

            tmp = (Int16) ((InBuffer.buffer[3] << 8) + InBuffer.buffer[2]);
            InputVoltage = (double) tmp / 1000.0;
            tmp = (Int16)((InBuffer.buffer[5] << 8) + InBuffer.buffer[4]);
            OutputVoltage = (double)tmp / 1000.0;
            tmp = (Int16)((InBuffer.buffer[7] << 8) + InBuffer.buffer[6]);
            InputCurrent = (double)tmp / 1000.0;
            tmp = (Int16)((InBuffer.buffer[9] << 8) + InBuffer.buffer[8]);
            OutputCurrent = (double)tmp / 1000.0;
            tmp = (Int16)((InBuffer.buffer[11] << 8) + InBuffer.buffer[10]);
            TemperatureOnboard = (double)tmp / 100.0;
            tmp = (Int16)((InBuffer.buffer[13] << 8) + InBuffer.buffer[12]);
            TemperatureExternal1 = (double)tmp / 100.0;
            tmp = (Int16)((InBuffer.buffer[15] << 8) + InBuffer.buffer[14]);
            TemperatureExternal2 = (double)tmp / 100.0;
            InputPower = InputVoltage * InputCurrent;
            OutputPower = OutputVoltage * OutputCurrent;
            Loss = InputPower - OutputPower;
            Efficiency = OutputPower / InputPower;
            PowerOutput1 = ((InBuffer.buffer[16] & 1) == 1);
            PowerOutput2 = ((InBuffer.buffer[16] & 2) == 2);
            PowerOutput3 = ((InBuffer.buffer[16] & 4) == 4);
            PowerOutput4 = ((InBuffer.buffer[16] & 8) == 8);
            PowerOutputUsb = ((InBuffer.buffer[16] & 16) == 16);
            FanOutput = ((InBuffer.buffer[16] & 32) == 32);
            DisplayMode = InBuffer.buffer[17];
            uint Year = 2000 + BcdToUint(InBuffer.buffer[18]);
            uint Month = BcdToUint(InBuffer.buffer[19]);
            uint Day = BcdToUint(InBuffer.buffer[20]);
            uint Hour = BcdToUint(InBuffer.buffer[21]);
            uint Minute = BcdToUint(InBuffer.buffer[22]);
            uint Second = BcdToUint(InBuffer.buffer[23]);
            SystemTime = new DateTime((int) Year, (int) Month, (int) Day, (int) Hour, (int) Minute, (int) Second);
            BuckMode = InBuffer.buffer[24];
            BuckDutyCycle = InBuffer.buffer[25];
            //New status data is now available
            _NewStatusAvailable = true;
        }

        //Function to parse packet received over USB
        private void ParseDisplay1(ref UsbBuffer InBuffer)
        {
            for(int line=0; line<2; ++line)
            {
                Display[line] = "";
                for (int c = 0; c < 20; ++c)
                {
                    char character = (char) InBuffer.buffer[2 + 20 * line + c];
                    Display[line] += character.ToString();
                }
            }
            //New display1 data is now available
            _NewDisplay1Available = true;
        }

        //Function to parse packet received over USB
        private void ParseDisplay2(ref UsbBuffer InBuffer)
        {
            for (int line = 2; line < 4; ++line)
            {
                Display[line] = "";
                for (int c = 0; c < 20; ++c)
                {
                    char character = (char) InBuffer.buffer[2 + 20*(line-2) + c];
                    Display[line] += character.ToString();
                }
            }
            //New display2 data is now available
            _NewDisplay2Available = true;
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
                PendingCommands = new List<byte>();
                LastCommand = 0x81;
            }
        }

        // HidUtility asks if a packet should be sent to the device
        // Prepare the buffer and request a transfer
        public void SendPacketHandler(object sender, UsbBuffer OutBuffer)
        {
            // Fill entire buffer with 0xFF
            OutBuffer.clear();

            // The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
            OutBuffer.buffer[0] = 0x00;

            //Prepare data to send
            switch (LastCommand)
            {
                case 0x10:
                    OutBuffer.buffer[1] = 0x11;
                    LastCommand = 0x11;
                    break;
                case 0x11:
                    OutBuffer.buffer[1] = 0x12;
                    LastCommand = 0x12;
                    break;
                case 0x12:
                    OutBuffer.buffer[1] = 0x10;
                    LastCommand = 0x10;
                    break;
                default:
                    OutBuffer.buffer[1] = 0x10;
                    LastCommand = 0x10;
                    break;
            };

            for(int i=2; i<64; ++i)
            {
                if(PendingCommands.Count!=0)
                {
                    // 2-byte commands
                    // Make sure both bytes are transmitted in the same packet
                    if((PendingCommands[0] & 0xF0)==0x40)
                    {
                        OutBuffer.buffer[i] = PendingCommands[0];
                        PendingCommands.RemoveAt(0);
                        ++i;
                        OutBuffer.buffer[i] = PendingCommands[0];
                        PendingCommands.RemoveAt(0);
                    }
                    else // Regular 1-byte command
                    {
                        OutBuffer.buffer[i] = PendingCommands[0];
                        PendingCommands.RemoveAt(0);
                    }
                }
                else
                {
                    break;
                }
            }

            //Request the packet to be sent over the bus
            OutBuffer.RequestTransfer = true;
        }

        // HidUtility informs us if the requested transfer was successful
        // Schedule to request a packet if the transfer was successful
        public void PacketSentHandler(object sender, UsbBuffer OutBuffer)
        {
            WaitingForDevice = OutBuffer.TransferSuccessful;
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
            WaitingForDevice = false;

            //Parse received data
            switch(InBuffer.buffer[1])
            {
                case 0x10:
                    ParseStatusData(ref InBuffer);
                    break;
                case 0x11:
                    ParseDisplay1(ref InBuffer);
                    break;
                case 0x12:
                    ParseDisplay2(ref InBuffer);
                    break;
            };

            //Some statistics
            if (InBuffer.TransferSuccessful)
            {
                ++RxCount;
            }
            else
            {
                ++RxFailedCount;
            }
        }


        public bool RequestValid()
        {
            return true;
        }

        public void RequestOut1Toggle()
        {
            if(PowerOutput1)
            {
                PendingCommands.Add(0x30);
            }
            else
            {
                PendingCommands.Add(0x31);
            }
        }

        public void RequestOut2Toggle()
        {
            if (PowerOutput2)
            {
                PendingCommands.Add(0x32);
            }
            else
            {
                PendingCommands.Add(0x33);
            }
        }

        public void RequestOut3Toggle()
        {
            if (PowerOutput3)
            {
                PendingCommands.Add(0x34);
            }
            else
            {
                PendingCommands.Add(0x35);
            }
        }

        public void RequestOut4Toggle()
        {
            if (PowerOutput4)
            {
                PendingCommands.Add(0x36);
            }
            else
            {
                PendingCommands.Add(0x37);
            }
        }

        public void RequestUsbToggle()
        {
            if (PowerOutputUsb)
            {
                PendingCommands.Add(0x38);
            }
            else
            {
                PendingCommands.Add(0x39);
            }
        }

        public void RequestFanToggle()
        {
            if (FanOutput)
            {
                PendingCommands.Add(0x3A);
            }
            else
            {
                PendingCommands.Add(0x3B);
            }
        }

        public void RequestTurnLeft()
        {
            PendingCommands.Add(0x3C);
        }

        public void RequestTurnRight()
        {
            PendingCommands.Add(0x3D);
        }

        public void RequestButtonPress()
        {
            PendingCommands.Add(0x3E);
        }

        public void SetYear(uint year)
        {
            PendingCommands.Add(0x40);
            PendingCommands.Add(UintToBcd(year));
        }

        public void SetMonth(uint month)
        {
            PendingCommands.Add(0x41);
            PendingCommands.Add(UintToBcd(month));
        }

        public void SetDay(uint day)
        {
            PendingCommands.Add(0x42);
            PendingCommands.Add(UintToBcd(day));
        }

        public void SetHour(uint hour)
        {
            PendingCommands.Add(0x43);
            PendingCommands.Add(UintToBcd(hour));
        }

        public void SetMinute(uint minute)
        {
            PendingCommands.Add(0x44);
            PendingCommands.Add(UintToBcd(minute));
        }

        public void SetSecond(uint second)
        {
            PendingCommands.Add(0x45);
            PendingCommands.Add(UintToBcd(second));
        }

        public void EnableManualControl()
        {
            PendingCommands.Add(0x46);
            PendingCommands.Add(0x00);
        }

        public void DisableManualControl()
        {
            PendingCommands.Add(0x47);
            PendingCommands.Add(0x00);
        }

        public void RequestChargerOn()
        {
            PendingCommands.Add(0x48);
            PendingCommands.Add(0x00);
        }

        public void RequestChargerOff()
        {
            PendingCommands.Add(0x49);
            PendingCommands.Add(0x00);
        }

        public void RequestAsynchronousMode()
        {
            PendingCommands.Add(0x4A);
            PendingCommands.Add(0x00);
        }

        public void RequestSynchronousMode()
        {
            PendingCommands.Add(0x4B);
            PendingCommands.Add(0x00);
        }

        public void RequestIncreaseDutycycle()
        {
            PendingCommands.Add(0x4C);
            PendingCommands.Add(0x00);
        }

        public void RequestDecreaseDutycycle()
        {
            PendingCommands.Add(0x4D);
            PendingCommands.Add(0x00);
        }

        public void SetDutycycle(byte dutycycle)
        {
            PendingCommands.Add(0x4E);
            PendingCommands.Add(dutycycle);
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
            communicator.SetYear((uint)(DateTime.Now.Year-2000));
            communicator.SetMonth((uint)DateTime.Now.Month);
            communicator.SetDay((uint)DateTime.Now.Day);
            communicator.SetHour((uint)DateTime.Now.Hour);
            communicator.SetMinute((uint)DateTime.Now.Minute);
            communicator.SetSecond((uint)DateTime.Now.Second);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityLogTxt"));
            }
        }

        public void ChargerOnOff()
        {
            if(OnOffButtonTxt=="Turn charger on")
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
                    case 0x04:
                        return "Buck status: off";
                    default:
                        return "Buck status: UNKNOWN";

                }
            }
        }

        public string DutyCycleTxt
        {
            get
            {
                return string.Format("Dutycycle: {0} ({1:0.0}%)", communicator.BuckDutyCycle.ToString(), (double) communicator.BuckDutyCycle/2.55);
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
                if(_ManualControl)
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
                    communicator.EnableManualControl();
                }
                else
                {
                    WriteLog("Asynchronous mode entered", false);
                    communicator.DisableManualControl();
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
                if(communicator.BuckMode==0x00)
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

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

    }
}
