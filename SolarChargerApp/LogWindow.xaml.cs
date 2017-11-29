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

        private void StartConversion_Click(object sender, EventArgs e)
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
            string inFile = InputFile_Textbox.Text;
            string outFile = OutputFile_Textbox.Text;
            int numberOfEntries = 0;
            if (File.Exists(inFile))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(inFile, FileMode.Open)))
                {
                    while(true)
                    {
                        try
                        {
                            UInt32 datetime = reader.ReadUInt32();
                            UInt16 inputVoltage = reader.ReadUInt16();
                            UInt16 inputCurrent = reader.ReadUInt16();
                            UInt16 outputVoltage = reader.ReadUInt16();
                            UInt16 outputCurrent = reader.ReadUInt16();
                            UInt16 inputPower = reader.ReadUInt16();
                            UInt16 outputPower = reader.ReadUInt16();
                            UInt16 inputCapacity = reader.ReadUInt16();
                            UInt16 outputCapacity = reader.ReadUInt16();
                            Byte temperatureOnboard = reader.ReadByte();
                            Byte temperatureExternal1 = reader.ReadByte();
                            Byte temperatureExternal2 = reader.ReadByte();
                            Byte chargerOnTime = reader.ReadByte();
                            Byte lowPowerTime = reader.ReadByte();
                            Byte[] unused = reader.ReadBytes(5);
                            Byte status = reader.ReadByte();
                            Byte checksum = reader.ReadByte();
                            ++numberOfEntries;
                        }
                        catch (EndOfStreamException ex)
                        {
                            break;
                        }
                    }
                    MessageBox.Show(numberOfEntries.ToString());
                }
            }
        }



    }
}
