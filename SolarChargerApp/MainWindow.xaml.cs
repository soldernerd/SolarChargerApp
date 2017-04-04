using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Management;
using System.Text.RegularExpressions;

namespace SolarChargerApp
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CalibrationWindow CalibrationWin;
        DeviceWindow DeviceWin;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowClose(object sender, EventArgs e)
        {
            if(DeviceWin != null)
            {
                DeviceWin.Close();
            }
            if(CalibrationWin != null)
            {
                CalibrationWin.Close();
            }
        }

        private void FreezeWindowSize(object sender, EventArgs e)
        {
            this.MinWidth = this.ActualWidth;
            this.MinHeight = this.ActualHeight;
            this.MaxWidth = this.ActualWidth;
            this.MaxHeight = this.ActualHeight;
        }

        private void ResizeToContent(object sender, EventArgs e)
        {
            this.MinWidth = 0;
            this.MinHeight = 0;
            this.MaxWidth = 10000;
            this.MaxHeight = 10000;
            this.SizeToContent = SizeToContent.Manual;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void menu_view_connection(object sender, EventArgs e)
        {
            if (Communication_GroupBox.Visibility == Visibility.Visible)
            {
                Communication_GroupBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                Communication_GroupBox.Visibility = Visibility.Visible;
            }
            ResizeToContent(sender, e);
        }

        private void ExpandCollapseGroupBox(GroupBox box)
        {
            if (box.Visibility == Visibility.Visible)
            {
                box.Visibility = Visibility.Collapsed;
            }
            else
            {
                box.Visibility = Visibility.Visible;
            }
        }

        private void ExpandCollapse(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            switch (item.Name)
            {
                case "MenuItem_Outputs":
                    ExpandCollapseGroupBox(this.PowerOut_GroupBox);
                    ExpandCollapseGroupBox(this.UsbCharging_GroupBox);
                    ExpandCollapseGroupBox(this.Temperature_GroupBox);
                    ExpandCollapseGroupBox(this.DateTime_GroupBox);
                    break;
                case "MenuItem_ChargerDisplay":
                    ExpandCollapseGroupBox(this.ChargerDetails_GroupBox);
                    ExpandCollapseGroupBox(this.UserInterface_GroupBox);
                    break;
                case "MenuItem_ActivityLog":
                    ExpandCollapseGroupBox(this.ActivityLogging_GroupBox);
                    break;
                case "MenuItem_ConnectionDetail":
                    ExpandCollapseGroupBox(this.Communication_GroupBox);
                    break;
            }
            //Resize window
            ResizeToContent(sender, e);
        }



        // Update when focus is lost
        public void FocusLostHandler(object sender, EventArgs e)
        {
            try
            {
                TextBox tb = (TextBox)sender;
                tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            catch
            {
                //nothin to do
            }
        }

        // Update if ENTER key has been pressed
        public void KeyUpHander(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox tb = (TextBox)sender;
                if (e.Key == Key.Enter)
                {
                    tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                }
            }
            catch
            {
                //nothin to do
            }
        }

        //Scroll to bottom when text is changed
        public void ActivityLogTextChangedHandler(object sender, EventArgs e)
        {
            ActivityLogging_TextBox.ScrollToEnd();
            //ActivityLogScrollViewer.ScrollToBottom();
        }

        private void menu_window_about(object sender, EventArgs e)
        {
            MessageBox.Show("Lukas Fässler, 2017\n\nlfaessler@gmx.net\nVisit soldernerd.com for more information", "About Solar Charger App");
        }

        private void menu_window_device(object sender, EventArgs e)
        {
            if (DeviceWin == null)
            {
                DeviceWin = new DeviceWindow(this);
            }
            DeviceWin.Show();
            DeviceWin.WindowState = WindowState.Normal;
            DeviceWin.Focus();
        }

        private void menu_window_calibration(object sender, EventArgs e)
        {
            if (CalibrationWin == null)
            {
                CalibrationWin = new CalibrationWindow(this);
            }
            CalibrationWin.Show();
            CalibrationWin.WindowState = WindowState.Normal;
            CalibrationWin.Focus();
        }
    }
}
