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
    public partial class SettingsWindow : Window
    {
        MainWindow parentWindow;

        public SettingsWindow(MainWindow mainWin)
        {
            InitializeComponent();
            parentWindow = mainWin;
            this.MaxHeight = SystemParameters.VirtualScreenHeight-50;
        }

        private void SettingsWindowClose(object sender, EventArgs e)
        {
            parentWindow.SettingsWin = null;
        }

        private void SaveSettings_Click(object sender, EventArgs e)
        {
            this.DisplayBrightness_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.DisplayTimeout_Textbox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

    }
}
