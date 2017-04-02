using System;
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

namespace SolarChargerApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        MainWindow parentWindow;

        public CalibrationWindow(MainWindow mainWin)
        {
            InitializeComponent();
            parentWindow = mainWin;
        }

        private void SaveCalibration_Click(object sender, EventArgs e)
        {
            this.InputVoltageOffset_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.InputVoltageSlope_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.OutputVoltageOffset_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.OutputVoltageSlope_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.InputCurrentOffset_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.InputCurrentSlope_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.OutputCurrentOffset_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.OutputCurrentSlope_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.OnboardTemperatureOffset_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.OnboardTemperatureSlope_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.ExternalTemperature1Offset_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.ExternalTemperature1Slope_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.ExternalTemperature2Offset_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.ExternalTemperature2Slope_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.RealTimeClock_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

    }
}
