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
    public partial class DeviceWindow : Window
    {
        MainWindow parentWindow;

        public DeviceWindow(MainWindow mainWin)
        {
            InitializeComponent();
            parentWindow = mainWin;
        }

        private void DeviceWindowClose(object sender, EventArgs e)
        {
            parentWindow.DeviceWin = null;
        }

        private void PidVidSave_Click(object sender, EventArgs e)
        {
            this.Pid_TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.Vid_TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

    }
}
