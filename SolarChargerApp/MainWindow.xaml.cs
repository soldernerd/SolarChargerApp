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
        public MainWindow()
        {
            InitializeComponent();
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

        private void menu_about(object sender, EventArgs e)
        {
            MessageBox.Show("Lukas Fässler, 2017\nlfaessler@gmx.net\n\nVisit soldernerd.com for more information", "About Solar Charger App");
        }

        /*
        private void menu_view_connection(object sender, EventArgs e)
        {
            if (gb_connection.Visibility == Visibility.Visible)
            {
                gb_connection.Visibility = Visibility.Collapsed;
                this.Height -= gb_connection.Height;
            }
            else
            {
                gb_connection.Visibility = Visibility.Visible;
                this.Height += gb_connection.Height;
            }
        }

        private void menu_view_control(object sender, EventArgs e)
        {
            if (gb_control.Visibility == Visibility.Visible)
            {
                gb_control.Visibility = Visibility.Collapsed;
                this.Height -= gb_control.Height;
            }
            else
            {
                gb_control.Visibility = Visibility.Visible;
                this.Height += gb_control.Height;
            }
        }

        private void menu_view_logging(object sender, EventArgs e)
        {
            if (gb_logging.Visibility == Visibility.Visible)
            {
                gb_logging.Visibility = Visibility.Collapsed;
                this.Height -= gb_logging.Height;
            }
            else
            {
                gb_logging.Visibility = Visibility.Visible;
                this.Height += gb_logging.Height;
            }
        }
        */

        private void menu_view_debugging(object sender, EventArgs e)
        {
            if (ActivityLogging_GroupBox.Visibility == Visibility.Visible)
            {
                ActivityLogging_GroupBox.Visibility = Visibility.Collapsed;
                this.ActivityLog_Row.Height = new GridLength(0);
                this.Height -= 150;
            }
            else
            {
                ActivityLogging_GroupBox.Visibility = Visibility.Visible;
                this.ActivityLog_Row.Height = new GridLength(150);
                this.Height += 150;
            }
        }

        private void menu_view_connection(object sender, EventArgs e)
        {
            if (Communication_GroupBox.Visibility == Visibility.Visible)
            {
                Communication_GroupBox.Visibility = Visibility.Collapsed;
                this.Connection_Row.Height = new GridLength(0);
                this.Height -= 150;
            }
            else
            {
                Communication_GroupBox.Visibility = Visibility.Visible;
                this.Connection_Row.Height = new GridLength(150);
                this.Height += 150;
            }
        }

    }
}
