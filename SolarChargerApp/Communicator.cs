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
        public List<byte> PacketsToRequest { get; set; }
        private List<UsbCommand> PendingCommands;
        public bool WaitingForDevice { get; private set; }
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
        public Int16 CalibrationInputVoltageOffset { get; private set; }
        public float CalibrationInputVoltageSlopeCorrection { get; private set; }
        public Int16 CalibrationOutputVoltageOffset { get; private set; }
        public float CalibrationOutputVoltageSlopeCorrection { get; private set; }
        public Int16 CalibrationInputCurrentOffset { get; private set; }
        public float CalibrationInputCurrentSlopeCorrection { get; private set; }
        public Int16 CalibrationOutputCurrentOffset { get; private set; }
        public float CalibrationOutputCurrentSlopeCorrection { get; private set; }
        public Int16 CalibrationOnboardTemperatureOffset { get; private set; }
        public float CalibrationOnboardTemperatureSlopeCorrection { get; private set; }
        public Int16 CalibrationExternalTemperature1Offset { get; private set; }
        public float CalibrationExternalTemperature1SlopeCorrection { get; private set; }
        public Int16 CalibrationExternalTemperature2Offset { get; private set; }
        public float CalibrationExternalTemperature2SlopeCorrection { get; private set; }
        public Int16 CalibrationRealTimeClock { get; private set; }

        public Calibration InputVoltageCalibration { get; private set; }
        public Calibration OutputVoltageCalibration { get; private set; }
        public Calibration InputCurrentCalibration { get; private set; }
        public Calibration OutputCurrentCalibration { get; private set; }
        public Calibration OnboardTemperatureCalibration { get; private set; }
        public Calibration ExternalTemperature1Calibration { get; private set; }
        public Calibration ExternalTemperature2Calibration { get; private set; }

        public string DebugString { get; private set; }

        public class UsbCommand
        {
            public byte command { get; set; }
            public List<byte> data { get; set; }

            public UsbCommand(byte command)
            {
                this.command = command;
                this.data = new List<byte>();
            }

            public UsbCommand(byte command, List<byte> data)
            {
                this.command = command;
                this.data = data;
            }

            public UsbCommand(byte command, byte d1, byte d2, byte d3, byte d4)
            {
                this.command = command;
                this.data = new List<byte>();
                this.data.Add(d1);
                this.data.Add(d2);
                this.data.Add(d3);
                this.data.Add(d4);
            }

            public UsbCommand(byte command, byte data)
            {
                this.command = command;
                this.data = new List<byte>();
                this.data.Add(data);
            }

            public UsbCommand(byte command, Int16 data)
            {
                this.command = command;
                this.data = new List<byte>();
                foreach (byte b in BitConverter.GetBytes(data))
                {
                    this.data.Add(b);
                }
            }

            public UsbCommand(byte command, float data)
            {
                this.command = command;
                this.data = new List<byte>();
                foreach (byte b in BitConverter.GetBytes(data))
                {
                    this.data.Add(b);
                }   
            }

            public List<byte> GetByteList()
            {
                switch(command & 0xF0)
                {
                    case 0x30:
                        return this.GetByteList(1);
                    case 0x40:
                        return this.GetByteList(2);
                    case 0x50:
                        return this.GetByteList(2);
                    case 0x60:
                        return this.GetByteList(5);
                    default:
                        return this.GetByteList(this.data.Count + 1);
                }
            }

            public List<byte> GetByteList(int length)
            {
                
                List<byte> ByteList = new List<byte>();
                ByteList.Add(command);
                foreach(byte b in data)
                {
                    ByteList.Add(b);
                }
                while(ByteList.Count<length)
                {
                    ByteList.Add(0x00);
                }
                return ByteList;
            
            }
        } // End of UsbCommand

        public class Calibration
        {
            public Int16 NeutralOffset { get; private set; }
            public Int16 NeutralMultiplier { get; private set; }
            public byte NeutralShift { get; private set; }
            public Int16 Offset { get; set; }
            public Int16 Multiplier { get; set; }
            public byte Shift { get; set; }
            public Int16 AutoCalibration { get; private set; }

            public Calibration(byte[] ByteArray, int StartIndex)
            {
                this.NeutralOffset = BitConverter.ToInt16(ByteArray, StartIndex);
                this.NeutralMultiplier = BitConverter.ToInt16(ByteArray, StartIndex + 2);
                this.NeutralShift = ByteArray[StartIndex + 4];
                this.Offset = BitConverter.ToInt16(ByteArray, StartIndex + 5);
                this.Multiplier = BitConverter.ToInt16(ByteArray, StartIndex + 7);
                this.Shift = ByteArray[StartIndex + 9];
                this.AutoCalibration = BitConverter.ToInt16(ByteArray, StartIndex + 10);
            }

            private struct ScaleParameters
            {
                public Int16 Multiplier;
                public byte Shift;

                public ScaleParameters(Int16 multiplier, byte shift)
                {
                    Multiplier = multiplier;
                    Shift = shift;
                }
            }

            private ScaleParameters scale(float factor)
            {
                Int16 multiplier;
                byte shift = 0;
                while (Math.Abs(2*factor) < Int16.MaxValue)
                {
                    ++shift;
                    factor *= 2;
                }
                multiplier = (Int16) Math.Round(factor);
                while (multiplier % 2 == 0)
                {
                    --shift;
                    multiplier /= 2;
                }
                return new ScaleParameters(multiplier, shift);
            }

            public float NeutralSlope
            {
                get
                {
                    return (float) NeutralMultiplier / (float) Math.Pow(2,NeutralShift);
                }
            }

            public float Slope
            {
                get
                {
                    return (float) Multiplier / (float) Math.Pow(2, Shift);
                }
                set
                {
                    ScaleParameters parameters = scale(value);
                    Multiplier = parameters.Multiplier;
                    Shift = parameters.Shift;
                }
            }

            public Int16 OffsetCorrection
            {
                get
                {
                    return (Int16) (this.Offset - this.NeutralOffset);
                }
                set
                {
                    if (value < -100)
                        value = -100;
                    if (value > 100)
                        value = 100;
                    Offset = (Int16) (value + this.NeutralOffset);
                }
            }

            public float SlopeCorrection
            {
                get
                {
                    return this.Slope / this.NeutralSlope;
                }
                set
                {
                    if (value < 0.95)
                        value = (float) 0.95;
                    if (value > 1.05)
                        value = (float) 1.05;
                    this.Slope = value * this.NeutralSlope;
                }
            }
        } // End of Calibration


        //Others
        private bool _NewStatusAvailable;
        private bool _NewDisplay1Available;
        private bool _NewDisplay2Available;
        private bool _NewCalibration1Available;
        private bool _NewCalibration2Available;

        public Communicator()
        {
            // Initialize variables
            TxCount = 0;
            TxFailedCount = 0;
            RxCount = 0;
            RxFailedCount = 0;
            PendingCommands = new List<UsbCommand>();
            PacketsToRequest = new List<byte>();
            PacketsToRequest.Add(0x10);
            PacketsToRequest.Add(0x11);
            PacketsToRequest.Add(0x12);
            PacketsToRequest.Add(0x13);
            PacketsToRequest.Add(0x14);
            WaitingForDevice = false;
            _NewStatusAvailable = false;
            _NewDisplay1Available = false;
            _NewDisplay2Available = false;
            _NewCalibration1Available = false;
            _NewCalibration2Available = false;

            // Obtain and initialize an instance of HidUtility
            HidUtil = new HidUtility();  

            // Subscribe to HidUtility events
            HidUtil.RaiseConnectionStatusChangedEvent += ConnectionStatusChangedHandler;
            HidUtil.RaiseSendPacketEvent += SendPacketHandler;
            HidUtil.RaisePacketSentEvent += PacketSentHandler;
            HidUtil.RaiseReceivePacketEvent += ReceivePacketHandler;
            HidUtil.RaisePacketReceivedEvent += PacketReceivedHandler;

            //Set the device to look for / connect with
            //HidUtil.SelectDevice(new Device(_Vid, _Pid));
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

        public bool NewCalibrationAvailable
        {
            get
            {
                if (_NewCalibration1Available && _NewCalibration2Available)
                {
                    _NewCalibration1Available = false;
                    _NewCalibration2Available = false;
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

        //Function to parse packet received over USB
        private void ParseCalibration1(ref UsbBuffer InBuffer)
        {
            InputVoltageCalibration = new Calibration(InBuffer.buffer, 2);
            OutputVoltageCalibration = new Calibration(InBuffer.buffer, 14);
            InputCurrentCalibration = new Calibration(InBuffer.buffer, 26);
            OutputCurrentCalibration = new Calibration(InBuffer.buffer, 38);
            //New calibration is now available
            _NewCalibration1Available = true;
        }

        //Function to parse packet received over USB
        private void ParseCalibration2(ref UsbBuffer InBuffer)
        {
            OnboardTemperatureCalibration = new Calibration(InBuffer.buffer, 2);
            ExternalTemperature1Calibration = new Calibration(InBuffer.buffer, 14);
            ExternalTemperature2Calibration = new Calibration(InBuffer.buffer, 26);
            //New calibration is now available
            _NewCalibration2Available = true;
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
                _NewStatusAvailable = false;
                _NewDisplay1Available = false;
                _NewDisplay2Available = false;
                _NewCalibration1Available = false;
                _NewCalibration2Available = false;
                TxCount = 0;
                TxFailedCount = 0;
                RxCount = 0;
                RxFailedCount = 0;
                PendingCommands = new List<UsbCommand>();
                PacketsToRequest = new List<byte>();
                PacketsToRequest.Add(0x10);
                PacketsToRequest.Add(0x11);
                PacketsToRequest.Add(0x12);
                PacketsToRequest.Add(0x13);
                PacketsToRequest.Add(0x14);
                WaitingForDevice = false;
            }
        }

        // HidUtility asks if a packet should be sent to the device
        // Prepare the buffer and request a transfer
        public void SendPacketHandler(object sender, UsbBuffer OutBuffer)
        {
            DebugString = "Start SendPacketHandler";
            // Fill entire buffer with 0xFF
            OutBuffer.clear();

            // The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
            OutBuffer.buffer[0] = 0x00;

            //Prepare data to send
            byte NextPacket;
            if (PacketsToRequest.Count >= 1)
            {
                NextPacket = PacketsToRequest[0];
                PacketsToRequest.RemoveAt(0);
            }     
            else
            {
                NextPacket = 0x10;
            }       
            OutBuffer.buffer[1] = NextPacket;
            PacketsToRequest.Add(NextPacket);

            int position = 2;
            while ((position<=64) && (PendingCommands.Count>0))
            {
                List<byte> CommandBytes = PendingCommands[0].GetByteList();

                //Check if entire command fits into current buffer
                if ((64-position) >= CommandBytes.Count)
                {
                    foreach (byte b in CommandBytes)
                    {
                        OutBuffer.buffer[position] = b;
                        ++position;
                    }
                    PendingCommands.RemoveAt(0);
                }
                else
                {
                    position += CommandBytes.Count;
                    break;
                }
            }

            //Request the packet to be sent over the bus
            OutBuffer.RequestTransfer = true;
            DebugString = "End SendPacketHandler";
        }

        // HidUtility informs us if the requested transfer was successful
        // Schedule to request a packet if the transfer was successful
        public void PacketSentHandler(object sender, UsbBuffer OutBuffer)
        {
            DebugString = "Start PacketSentHandler";
            WaitingForDevice = OutBuffer.TransferSuccessful;
            if (OutBuffer.TransferSuccessful)
            {
                ++TxCount;
            }
            else
            {
                ++TxFailedCount;
            }
            DebugString = "End PacketSentHandler";
        }

        // HidUtility asks if a packet should be requested from the device
        // Request a packet if a packet has been successfully sent to the device before
        public void ReceivePacketHandler(object sender, UsbBuffer InBuffer)
        {
            DebugString = "Start ReceivePacketHandler";
            InBuffer.RequestTransfer = WaitingForDevice;
            DebugString = "End ReceivePacketHandler";
        }

        // HidUtility informs us if the requested transfer was successful and provides us with the received packet
        public void PacketReceivedHandler(object sender, UsbBuffer InBuffer)
        {
            DebugString = "Start PacketReceivedHandler";
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
                case 0x13:
                    ParseCalibration1(ref InBuffer);
                    break;
                case 0x14:
                    ParseCalibration2(ref InBuffer);
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
            DebugString = "End PacketReceivedHandler";
        }


        public bool RequestValid()
        {
            return true;
        }

        public enum PowerOutput: byte
        {
            PowerOutput1 = 0x30,
            PowerOutput2 = 0x32,
            PowerOutput3 = 0x34,
            PowerOutput4 = 0x36,
            PowerOutputUsb = 0x38
        }

        public enum PowerOutputAction: byte
        {
            Off = 0x00,
            On = 0x01,
            Toggle
        }

        public void RequestPowerOutputMode(PowerOutput output, PowerOutputAction action)
        {
            byte command = 0x00;
            if (action == PowerOutputAction.Toggle)
            {
                switch(output)
                {
                    case PowerOutput.PowerOutput1:
                        if(PowerOutput1)
                            command = (byte)((byte)PowerOutput.PowerOutput1 | (byte)PowerOutputAction.Off);
                        else
                            command = (byte)((byte)PowerOutput.PowerOutput1 | (byte)PowerOutputAction.On);
                        break;
                    case PowerOutput.PowerOutput2:
                        if (PowerOutput2)
                            command = (byte)((byte)PowerOutput.PowerOutput2 | (byte)PowerOutputAction.Off);
                        else
                            command = (byte)((byte)PowerOutput.PowerOutput2 | (byte)PowerOutputAction.On);
                        break;
                    case PowerOutput.PowerOutput3:
                        if (PowerOutput3)
                            command = (byte)((byte)PowerOutput.PowerOutput3 | (byte)PowerOutputAction.Off);
                        else
                            command = (byte)((byte)PowerOutput.PowerOutput3 | (byte)PowerOutputAction.On);
                        break;
                    case PowerOutput.PowerOutput4:
                        if (PowerOutput4)
                            command = (byte)((byte)PowerOutput.PowerOutput4 | (byte)PowerOutputAction.Off);
                        else
                            command = (byte)((byte)PowerOutput.PowerOutput4| (byte)PowerOutputAction.On);
                        break;
                    case PowerOutput.PowerOutputUsb:
                        if (PowerOutputUsb)
                            command = (byte)((byte)PowerOutput.PowerOutputUsb | (byte)PowerOutputAction.Off);
                        else
                            command = (byte)((byte)PowerOutput.PowerOutputUsb | (byte)PowerOutputAction.On);
                        break;
                }
                PendingCommands.Add(new UsbCommand(command));
            }
            else
            {
                command = (byte) ((byte)output | (byte)action);
                PendingCommands.Add(new UsbCommand(command));
            }
        }
        
        public enum FanMode: byte
        {
            Off = 0x3A,
            On = 0x3B,
            Toggle
        }

        public void RequestFanMode(FanMode action)
        {
            if(action==FanMode.Toggle)
            {
                if (FanOutput)
                {
                    PendingCommands.Add(new UsbCommand((byte) FanMode.Off));
                }
                else
                {
                    PendingCommands.Add(new UsbCommand((byte) FanMode.On));
                }
            }
            else
            {
                PendingCommands.Add(new UsbCommand((byte) action));
            } 
        }

        public enum RotaryEncoder: byte
        {
            TurnLeft = 0x3C,
            TurnRight = 0x3D,
            ButtonPress = 0x3E
        }

        public void RequestEncoder(RotaryEncoder action)
        {
            PendingCommands.Add(new UsbCommand((byte) action));
        }

        public enum DateTimeElement: byte
        {
            Year = 0x40,
            Month = 0x41,
            Day = 0x42,
            Hour = 0x43,
            Minute = 0x44,
            Second = 0x45,
            EEPROM_WRITE_REQUEST = 0x3F
        }

        public void SetDateTime(DateTimeElement element, uint value)
        {
            UsbCommand cmd;
            if(element== DateTimeElement.EEPROM_WRITE_REQUEST)
            {
                cmd = new UsbCommand((byte)element);
            }
            else
            {
                cmd = new UsbCommand((byte)element, UintToBcd(value));
            }
            PendingCommands.Add(cmd);
        }

        public enum ChargerControl: byte
        {
            Remote = 0x46,
            Local = 0x47
        }

        public void RequestChargerControl(ChargerControl control)
        {
            PendingCommands.Add(new UsbCommand((byte) control));
        }

        public enum ChargerOnOff: byte
        {
            On = 0x48,
            Off = 0x49
        }

        public void RequestChargerOnOff(ChargerOnOff mode)
        {
            PendingCommands.Add(new UsbCommand((byte) mode));
        }

        public enum ChargerMode: byte
        {
            AsynchronousMode = 0x4A,
            SynchronousMode = 0x4B
        }

        public void RequestChargerMode(ChargerMode mode)
        {
            PendingCommands.Add(new UsbCommand((byte) mode));
        }

        public void RequestDecreaseDutycycle()
        {
            PendingCommands.Add(new UsbCommand(0x4C));
        }

        public void RequestIncreaseDutycycle()
        {
            PendingCommands.Add(new UsbCommand(0x4D));
        }

        public void SetDutycycle(byte dutycycle)
        {
            UsbCommand cmd = new UsbCommand(0x4E, dutycycle);
            PendingCommands.Add(cmd);
        }

        public enum CalibrationItem: byte
        {
            InputVoltageOffset = 0x00,
            InputVoltageSlope = 0x01,
            OutputVoltageOffset = 0x10,
            OutputVoltageSlope = 0x11,
            InputCurrentOffset = 0x20,
            InputCurrentSlope = 0x21,
            OutputCurrentOffset = 0x30,
            OutputCurrentSlope = 0x31,
            OnboardTemperatureOffset = 0x40,
            OnboardTemperatureSlope = 0x41,
            ExternalTemperature1Offset = 0x50,
            ExternalTemperature1Slope = 0x51,
            ExternalTemperature2Offset = 0x60,
            ExternalTemperature2Slope = 0x61
        }

        //Calibration in general
        public void SetCalibration(CalibrationItem item)
        {
            Calibration cal;
            UsbCommand cmd = new UsbCommand(0xFF);
            //Get calibration object
            switch ((byte)item & 0xF0)
            {
                case 0x00:
                    cal = this.InputVoltageCalibration;
                    break;
                case 0x10:
                    cal = this.OutputVoltageCalibration;
                    break;
                case 0x20:
                    cal = this.InputCurrentCalibration;
                    break;
                case 0x30:
                    cal = this.OutputCurrentCalibration;
                    break;
                case 0x40:
                    cal = this.OnboardTemperatureCalibration;
                    break;
                case 0x50:
                    cal = this.ExternalTemperature1Calibration;
                    break;
                case 0x60:
                    cal = this.ExternalTemperature2Calibration;
                    break;
                default:
                    cal = this.InputVoltageCalibration;
                    break;
            }
            //Assemble command
            switch ((byte) item & 0x0F)
            {
                case 0x00:
                    byte[] offset = BitConverter.GetBytes(cal.Offset);
                    cmd = new UsbCommand(0x60, (byte) item, offset[1], offset[0], 0x00);
                    break;
                case 0x01:
                    byte[] multiplier = BitConverter.GetBytes(cal.Multiplier);
                    cmd = new UsbCommand(0x60, (byte)item, multiplier[1], multiplier[0], cal.Shift);
                    break;
            }
            //Add command to cue
            PendingCommands.Add(cmd);          
        }

        /*
        //Calibrating slope
        public void SetCalibration(CalibrationItem item, float value)
        {
            UsbCommand cmd = new UsbCommand((byte) item, value);
            PendingCommands.Add(cmd);
        }

        //Calibrating offset
        public void SetCalibration(CalibrationItem item, Int16 value)
        {
            UsbCommand cmd = new UsbCommand((byte) item, value);
            PendingCommands.Add(cmd);
        }
        */

        public void SetRealTimeClockCalibration(Int16 calibration)
        {
            UsbCommand cmd = new UsbCommand(0x52, (byte) calibration);
            PendingCommands.Add(cmd);
        }

        public enum DisplayProperty: byte
        {
            Timeout = 0x51,
            Brightness = 0x50
        }

        public void SetDisplayProperty(DisplayProperty property, byte value)
        {
            UsbCommand cmd = new UsbCommand((byte)property, value);
            PendingCommands.Add(cmd);
        }

    } // Communicator

}