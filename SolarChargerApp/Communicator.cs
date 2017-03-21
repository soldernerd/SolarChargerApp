using System;
using System.Collections.Generic;
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
        public bool FanOutputManual { get; private set; }
        public byte DisplayMode { get; private set; }
        public byte DisplayStatus { get; private set; }
        public DateTime SystemTime { get; private set; }
        public byte BuckMode { get; private set; }
        public byte BuckDutyCycle { get; private set; }
        public string[] Display { get; private set; } = new string[4];
        public bool BuckRemoteEnable { get; private set; }
        public bool BuckRemoteOn { get; private set; }
        public bool BuckRemoteSynchronous { get; private set; }
        public uint BuckRemoteDutycycle { get; private set; }
        public uint TemperatureOnboardAdc { get; private set; }
        public uint TemperatureExternal1Adc { get; private set; }
        public uint TemperatureExternal2Adc { get; private set; }
        public uint[] InputVoltageAdc { get; private set; } = { 0, 0, 0, 0 };
        public uint[] OutputVoltageAdc { get; private set; } = { 0, 0, 0, 0 };
        public uint[] InputCurrentAdc { get; private set; } = { 0, 0, 0, 0 };
        public uint[] OutputCurrentAdc { get; private set; } = { 0, 0, 0, 0 };
        public uint TimeSlot { get; private set; }



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
            uint lower = (uint)(bcd & 0x0F);
            uint upper = (uint)(bcd >> 4);
            return (10 * upper) + lower;
        }

        //Convert integer to binary encoded decimal byte
        private byte UintToBcd(uint val)
        {
            uint lower = val % 10;
            uint upper = val / 10;
            byte retval = (byte)upper;
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

            tmp = (Int16)((InBuffer.buffer[3] << 8) + InBuffer.buffer[2]);
            InputVoltage = (double)tmp / 1000.0;
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
            FanOutputManual = ((InBuffer.buffer[16] & 64) == 64);
            DisplayMode = InBuffer.buffer[17];
            uint Year = 2000 + BcdToUint(InBuffer.buffer[18]);
            uint Month = BcdToUint(InBuffer.buffer[19]);
            uint Day = BcdToUint(InBuffer.buffer[20]);
            uint Hour = BcdToUint(InBuffer.buffer[21]);
            uint Minute = BcdToUint(InBuffer.buffer[22]);
            uint Second = BcdToUint(InBuffer.buffer[23]);
            SystemTime = new DateTime((int)Year, (int)Month, (int)Day, (int)Hour, (int)Minute, (int)Second);
            BuckMode = InBuffer.buffer[24];
            BuckDutyCycle = InBuffer.buffer[25];
            BuckRemoteEnable = ((InBuffer.buffer[26] & 1) == 1);
            BuckRemoteOn = ((InBuffer.buffer[26] & 2) == 2);
            BuckRemoteSynchronous = ((InBuffer.buffer[26] & 4) == 4);
            BuckRemoteDutycycle = InBuffer.buffer[27];
            tmp = (Int16)((InBuffer.buffer[29] << 8) + InBuffer.buffer[28]);
            TemperatureOnboardAdc = (uint)tmp;
            tmp = (Int16)((InBuffer.buffer[31] << 8) + InBuffer.buffer[30]);
            TemperatureExternal1Adc = (uint)tmp;
            tmp = (Int16)((InBuffer.buffer[33] << 8) + InBuffer.buffer[32]);
            TemperatureExternal2Adc = (uint)tmp;
            tmp = (Int16)((InBuffer.buffer[35] << 8) + InBuffer.buffer[34]);
            InputVoltageAdc[(TimeSlot >> 4) & 0x03] = (uint)tmp;
            tmp = (Int16)((InBuffer.buffer[37] << 8) + InBuffer.buffer[36]);
            OutputVoltageAdc[(TimeSlot >> 4) & 0x03] = (uint)tmp;
            tmp = (Int16)((InBuffer.buffer[39] << 8) + InBuffer.buffer[38]);
            InputCurrentAdc[(TimeSlot >> 4) & 0x03] = (uint)tmp;
            tmp = (Int16)((InBuffer.buffer[41] << 8) + InBuffer.buffer[40]);
            OutputCurrentAdc[(TimeSlot >> 4) & 0x03] = (uint)tmp;
            DisplayStatus = InBuffer.buffer[42];
            TimeSlot = InBuffer.buffer[43];
            //New status data is now available
            _NewStatusAvailable = true;
        }

        //Function to parse packet received over USB
        private void ParseDisplay1(ref UsbBuffer InBuffer)
        {
            for (int line = 0; line < 2; ++line)
            {
                Display[line] = "";
                for (int c = 0; c < 20; ++c)
                {
                    char character = (char)InBuffer.buffer[2 + 20 * line + c];
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
                    char character = (char)InBuffer.buffer[2 + 20 * (line - 2) + c];
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
                if (value != _Vid)
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

            for (int i = 2; i < 64; ++i)
            {
                if (PendingCommands.Count != 0)
                {
                    // 2-byte commands
                    // Make sure both bytes are transmitted in the same packet
                    if ((PendingCommands[0] & 0xF0) == 0x40)
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
            switch (InBuffer.buffer[1])
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
            if (PowerOutput1)
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

        public void RequestDatetimeWrite()
        {
            PendingCommands.Add(0x3F);
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

        public void RequestDecreaseDutycycle()
        {
            PendingCommands.Add(0x4C);
            PendingCommands.Add(0x00);
        }

        public void RequestIncreaseDutycycle()
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

}