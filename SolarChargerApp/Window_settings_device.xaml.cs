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
    public partial class Window_settings_device : Window
    {
        MainWindow parentWindow;

        public Window_settings_device(MainWindow mainWin)
        {
            InitializeComponent();
            parentWindow = mainWin;
        }

        private void PidVidSave_Click(object sender, EventArgs e)
        {
            this.Pid_TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.Vid_TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        /*
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

        
        private void bt_save_Click(object sender, EventArgs e)
        {
            parentWindow.cfgFile.setVendorId(tblk_vendor.Text);
            parentWindow.cfgFile.setProductId(tblk_product.Text);
        }

        private void bt_undo_Click(object sender, EventArgs e)
        {
            tblk_vendor.Text = parentWindow.cfgFile.getVendorId();
            tblk_product.Text = parentWindow.cfgFile.getProductId();
        }
        */

    }
}
