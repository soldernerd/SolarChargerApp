using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SolarChargerApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        MainWindow parentWindow;

        public LogWindow(MainWindow mainWin)
        {
            InitializeComponent();
            parentWindow = mainWin;
            this.MaxHeight = SystemParameters.VirtualScreenHeight - 50;
        }

        private void LogWindowClose(object sender, EventArgs e)
        {
            parentWindow.LogWin = null;
        }

        private void SelectInputFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                InputFile_Textbox.Text = openFileDialog.FileName;
        }

        private void SelectOutputFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                OutputFile_Textbox.Text = openFileDialog.FileName;
        }

        class LogEntry
        {
            DateTime dateTime;
            double inputVoltage;
            double inputCurrent;
            double outputVoltage;
            double outputCurrent;
            double inputPower;
            double outputPower;
            double inputCapacity;
            double outputCapacity;
            double temperatureOnboard;
            double temperatureExternal1;
            double temperatureExternal2;
            int chargerOnTime;
            int lowPowerTime;
            bool output1on;
            bool output2on;
            bool output3on;
            bool output4on;
            bool usbChargingOn;
            bool fanOn;
            bool usbConnected;
            bool userInterfaceActive;

            public LogEntry(Byte[] data)
            {
                /* Log structure *
                 * 32 bytes per entry, 1 entry every 7.5 minutes
                 * Bytes 0-3: Date and time
                 *      Year:       6 bits (2-digit year, i.e year - 2000)
                 *      Month       4 bits
                 *      Day         5 bits
                 *      Hours       5 bits
                 *      Minutes     6 bits
                 *      Seconds:    6 bits
                 * Bytes 4-5: Input voltage: uint16_t in millivolts
                 * Bytes 6-7: Input current: uint16_t in milliamps
                 * Bytes 8-9: Output voltage: uint16_t in millivolts
                 * Bytes 10-11: Output current: uint16_t in milliamps
                 * Bytes 12-13: Input power: uint16_t in 2 milliwatts
                 * Bytes 14-15: Output power: uint16_t in 2 milliwatts
                 * Bytes 16-17: Input capacity: uint16_t in Ws (watt-seconds)
                 * Bytes 18-19: Output capacity: uint16_t in Ws (watt-seconds)
                 * Byte 20: On-board temperature: uint8_t in 0.5 degrees centigrade above -40
                 * Byte 21: External temperature 1: uint8_t in 0.5 degrees centigrade above -40
                 * Byte 22: External temperature 2: uint8_t in 0.5 degrees centigrade above -40
                 * Byte 23: Charger on-time: uint8_t in 2 seconds
                 * Byte 24: Low-power state time: uint8_t in 2 seconds
                 * ...
                 * Byte 30: Status byte:
                 *      Bit 7: Output 1 on
                 *      Bit 6: Output 2 on
                 *      Bit 5: Output 3 on
                 *      Bit 4: Output 4 on
                 *      Bit 3: USB charging on
                 *      Bit 2: Fan on
                 *      Bit 1: USB connected
                 *      Bit 0: User interface active
                 * Byte 31: Checksum: All other bytes XOR-ed
                 * ****************************************************************************/
                this.dateTime = DecodeDateTime(BitConverter.ToInt32(data, 0));
                this.inputVoltage = 0.001 * ((double) BitConverter.ToUInt16(data, 4));
                this.inputCurrent = 0.001 * ((double) BitConverter.ToUInt16(data, 6));
                this.outputVoltage = 0.001 * ((double) BitConverter.ToUInt16(data, 8));
                this.outputCurrent = 0.001 * ((double) BitConverter.ToUInt16(data, 10));
                this.inputPower = 0.002 * ((double)BitConverter.ToUInt16(data, 12));
                this.outputPower = 0.002 * ((double)BitConverter.ToUInt16(data, 14));
                this.inputCapacity = (double) BitConverter.ToUInt16(data, 16);
                this.outputCapacity = (double) BitConverter.ToUInt16(data, 18);
                this.temperatureOnboard = -40.0 + (0.5 * ((double) data[20]));
                this.temperatureExternal1 = -40.0 + (0.5 * ((double)data[21]));
                this.temperatureExternal2 = -40.0 + (0.5 * ((double)data[22]));
                this.chargerOnTime = 2 * ((int)data[23]);
                this.lowPowerTime = 2 * ((int)data[24]);
                Byte status = data[30];
                this.output1on = (status & 0b10000000) == 0b10000000;
                this.output2on = (status & 0b01000000) == 0b01000000;
                this.output3on = (status & 0b00100000) == 0b00100000;
                this.output4on = (status & 0b00010000) == 0b00010000;
                this.usbChargingOn = (status & 0b00001000) == 0b00001000;
                this.fanOn = (status & 0b00000100) == 0b00000100;
                this.usbConnected = (status & 0b00000010) == 0b00000010;
                this.userInterfaceActive = (status & 0b00000001) == 0b00000001;
                Byte checksum = data[31];
            }

            private DateTime DecodeDateTime(Int32 timestamp)
            {
                Int32 year = (timestamp >> 26) + 2000;
                Int32 month = (timestamp >> 22) & 0b1111;
                Int32 day = (timestamp >> 17) & 0b11111;
                Int32 hour = (timestamp >> 12) & 0b11111;
                Int32 minute = (timestamp >> 6) & 0b111111;
                Int32 second = timestamp & 0b111111;
                string msg = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", timestamp, year, month, day, hour, minute, second);
                //MessageBox.Show(msg);
                try
                {
                    return new DateTime(year, month, day, hour, minute, second);
                }
                catch(Exception e)
                {
                    return new DateTime(2017, 11, 30, 22, 22, 22);
                }
            }

            override public string ToString()
            {
                string separator = ";";
                string s = dateTime.ToString() + separator;
                s += this.inputVoltage.ToString() + separator;
                s += this.inputCurrent.ToString() + separator;
                s += this.outputVoltage.ToString() + separator;
                s += this.outputCurrent.ToString() + separator;
                s += this.inputPower.ToString() + separator;
                s += this.outputPower.ToString() + separator;
                s += this.inputCapacity.ToString() + separator;
                s += this.outputCapacity.ToString() + separator;
                s += this.temperatureOnboard.ToString() + separator;
                s += this.temperatureExternal1.ToString() + separator;
                s += this.temperatureExternal2.ToString() + separator;
                s += this.chargerOnTime.ToString() + separator;
                s += this.lowPowerTime.ToString() + separator;
                s += this.output1on.ToString() + separator;
                s += this.output2on.ToString() + separator;
                s += this.output3on.ToString() + separator;
                s += this.output4on.ToString() + separator;
                s += this.usbChargingOn.ToString() + separator;
                s += this.fanOn.ToString() + separator;
                s += this.usbConnected.ToString() + separator;
                s += this.userInterfaceActive.ToString();
                return s;
            }

            public string GetHeader()
            {
                string separator = ";";
                string s = "DateTime" + separator;
                s += "InputVoltage" + separator;
                s += "InputCurrent" + separator;
                s += "OutputVoltage" + separator;
                s += "OutputCurrent" + separator;
                s += "InputPower" + separator;
                s += "OutputPower" + separator;
                s += "InputCapacity" + separator;
                s += "OutputCapacity" + separator;
                s += "TemperatureOnboard" + separator;
                s += "TemperatureExternal1" + separator;
                s += "TemperatureExternal2" + separator;
                s += "ChargerOnTime" + separator;
                s += "LowPowerTime" + separator;
                s += "Output1on" + separator;
                s += "Output2on" + separator;
                s += "Output3on" + separator;
                s += "Output4on" + separator;
                s += "USBChargingOn" + separator;
                s += "FanOn" + separator;
                s += "USBConnected" + separator;
                s += "UserInterfaceActive";
                return s;
            }
        }

        private void WriteCsv(string FileName, List<LogEntry> entries)
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine(entries.First().GetHeader());

            foreach (LogEntry entry in entries)
            {
                string line = string.Format("{0}", entry.ToString());
                csv.AppendLine(line);
            }

            File.WriteAllText(FileName, csv.ToString());
        }

        private void StartConversion_Click(object sender, EventArgs e)
        {
            List<LogEntry> entries = new List<LogEntry>();
            string inFile = InputFile_Textbox.Text;
            string outFile = OutputFile_Textbox.Text;
            int numberOfEntries = 0;
            if (File.Exists(inFile))
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(inFile, FileMode.Open)))
                {
                    while(true)
                    //for(int i=0; i<6; ++i)
                    {
                        try
                        {
                            List<Byte> record = new List<Byte>();
                            for(int b=0; b<32; ++b)
                            {
                                record.Add(reader.ReadByte());
                            }
                            entries.Add(new LogEntry(record.ToArray()));
                        }
                        catch (EndOfStreamException ex)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            break;
                        }
                    }
                }
                //We're done parsing the input file
                //Write the output file
                WriteCsv(outFile, entries);
                MessageBox.Show("Successfully converted log file.");
            }
            catch
            {
                MessageBox.Show("Can't convert input file.");
            }
        }



    }
}
