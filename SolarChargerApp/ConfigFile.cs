using System;
using System.Windows;
using System.IO;
using System.Xml;


namespace ConfigurationFile
{
    public class ConfigFile
    {
        string fileName;
        XmlDocument cfg = new XmlDocument();

        public ConfigFile(string fName)
        {
            try
            {
                fileName = System.IO.Path.GetFullPath(fName);
                if (!File.Exists(fileName))
                {
                    makeNewConfig();
                }
                else
                {
                    cfg.Load(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void makeNewConfig()
        {
            try
            {
                cfg = new XmlDocument();
                //Root
                XmlNode rootNode = cfg.CreateElement("Config");
                cfg.AppendChild(rootNode);
                //Device
                XmlNode deviceNode = cfg.CreateElement("Device");
                rootNode.AppendChild(deviceNode);
                XmlNode vendorIdNode = cfg.CreateElement("VendorId");
                vendorIdNode.InnerText = "0x04D8";
                deviceNode.AppendChild(vendorIdNode);
                XmlNode deviceIdNode = cfg.CreateElement("ProductId");
                deviceIdNode.InnerText = "0xF08E";
                deviceNode.AppendChild(deviceIdNode);
                //Log
                XmlNode logNode = cfg.CreateElement("LogFile");
                logNode.InnerText = "SolarCharger.csv";
                rootNode.AppendChild(logNode);
                //Window
                XmlNode windowNode = cfg.CreateElement("Window");
                rootNode.AppendChild(windowNode);
                //DebugVisibility
                XmlNode debugNode = cfg.CreateElement("ActivityLog");
                debugNode.InnerText = "Visible";
                windowNode.AppendChild(debugNode);
                //ConnectionVisibility
                XmlNode connectionNode = cfg.CreateElement("ConnectionDetails");
                connectionNode.InnerText = "Visible";
                windowNode.AppendChild(connectionNode);
                cfg.Save(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ushort VendorId
        {
            get
            {
                try
                {
                    XmlNode node = cfg.DocumentElement.SelectSingleNode("/Config/Device/VendorId");
                    string VendorIdString = node.InnerText.Substring(2);
                    return ushort.Parse(VendorIdString, System.Globalization.NumberStyles.HexNumber);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0x0000;
                }
            }
            set
            {
                try
                {
                    XmlNode node = cfg.DocumentElement.SelectSingleNode("/Config/Device/VendorId");
                    node.InnerText = string.Format("0x{0:X4}", VendorId);
                    cfg.Save(fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public ushort ProductId
        {
            get
            {
                try
                {
                    XmlNode node = cfg.DocumentElement.SelectSingleNode("/Config/Device/ProductId");
                    string VendorIdString = node.InnerText.Substring(2);
                    return ushort.Parse(VendorIdString, System.Globalization.NumberStyles.HexNumber);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0x0000;
                }
            }
            set
            {
                try
                {
                    XmlNode node = cfg.DocumentElement.SelectSingleNode("/Config/Device/ProductId");
                    node.InnerText = string.Format("0x{0:X4}", ProductId);
                    cfg.Save(fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public bool ActivityLogVisible
        {
            get
            {
                try
                {
                    string txt = cfg.DocumentElement.SelectSingleNode("/Config/Window/ActivityLog").InnerText;
                    if (txt == "Visible")
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }
            set
            {
                try
                {
                    if (value)
                        cfg.DocumentElement.SelectSingleNode("/Config/Window/ActivityLog").InnerText = "Visible";
                    else
                        cfg.DocumentElement.SelectSingleNode("/Config/Window/ActivityLog").InnerText = "Collapsed";
                    cfg.Save(fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
