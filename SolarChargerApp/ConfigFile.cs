using System;
using System.Windows;
using System.IO;
using System.Xml;
using System.Collections.Generic;


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
                // Open file if present
                fileName = System.IO.Path.GetFullPath(fName);
                if (File.Exists(fileName))
                {
                    cfg.Load(fileName);
                }
                else
                {
                    //Create Root Tag
                    XmlNode rootNode = cfg.CreateElement("Config");
                    cfg.AppendChild(rootNode);
                }
                // Ensure that all tags are present. Existing items won't be overwritten
                makeNewConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class Tag
        {
            public List<string> Path { get; private set; }
            public string DefaultValue { get; private set; }

            public Tag(string PathString, string DefaultValue)
            {
                this.Path = new List<string>();
                this.Path.AddRange(PathString.Split('/'));
                this.DefaultValue = DefaultValue;
            }
        }

        private XmlNode _getNode(string path)
        {
            try
            {
                return cfg.DocumentElement.SelectSingleNode(path);
            }
            catch
            {
                return null;
            }
        }

        private void _createNodeIfNotExists(string node, string parentNode)
        {
            try
            {
                if (_getNode(parentNode + "/" + node) == null)
                {
                    XmlNode parent = _getNode(parentNode);
                    XmlNode child = cfg.CreateElement(node);
                    parent.AppendChild(child);
                }
            }
            catch
            {

                MessageBox.Show( "_createNodeIfNotExists " + parentNode + "/" + node, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void _createPathIfNotExists(Tag tag)
        {
            string parent = "";
            foreach(string child in tag.Path)
            {
                _createNodeIfNotExists(child, parent);
                parent += "/" + child;
            }
            if(_getString(parent).Length == 0)
            {
                _setString(parent, tag.DefaultValue);
            }
        }

        public void makeNewConfig()
        {
            List<Tag> tags = new List<Tag>();
            tags.Add(new Tag("Config/Device/VendorId", "0x04D8"));
            tags.Add(new Tag("Config/Device/ProductId", "0xF08E"));
            tags.Add(new Tag("Config/LogFile", "SolarCharger.csv"));
            tags.Add(new Tag("Config/Window/Maximized", "False"));
            tags.Add(new Tag("Config/Window/PositionX", "100"));
            tags.Add(new Tag("Config/Window/PositionY", "100"));
            tags.Add(new Tag("/Config/Window/Outputs", "Visible"));
            tags.Add(new Tag("/Config/Window/ChargerDisplay", "Visible"));
            tags.Add(new Tag("/Config/Window/ActivityLog", "Visible"));
            tags.Add(new Tag("/Config/Window/ConnectionDetails", "Visible"));

            foreach(Tag tag in tags)
            {
                _createPathIfNotExists(tag);
            }

            cfg.Save(fileName);
        }

        // Most basic functions: get and set string

        private string _getString(string path)
        {
            try
            {
                return cfg.DocumentElement.SelectSingleNode(path).InnerText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void _setString(string path, string value)
        {
            try
            {
                cfg.DocumentElement.SelectSingleNode(path).InnerText = value;
                cfg.Save(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Boolean functions: get and set boolean values

        private bool _getBool(string path, string TrueString)
        {
            return _getString(path) == TrueString;
        }

        private void _setBool(string path, bool value, string TrueString, string FalseString)
        {
            if (value)
                _setString(path, TrueString);
            else
                _setString(path, FalseString);
        }

        // Hex encoded unsigned integers

        private uint _getHex(string path)
        {
            return uint.Parse(_getString(path).Substring(2), System.Globalization.NumberStyles.HexNumber);
        }

        private void _setHex(string path, uint value)
        {
            _setString(path, string.Format("0x{0:X4}", value));
        }

        // Vendor and product ID

        public ushort VendorId
        {
            get { return (ushort)_getHex("/Config/Device/VendorId"); }
            set { _setHex("/Config/Device/VendorId", (uint)value); }
        }

        public ushort ProductId
        {
            get { return (ushort)_getHex("/Config/Device/ProductId"); }
            set { _setHex("/Config/Device/ProductId", (uint)value); }
        }

        // Visibility of certain elements

        private bool _getVisibility(string path)
        {
            return _getBool(path, "Visible");
        }

        private void _setVisibility(string path, bool value)
        {
            _setBool(path, value, "Visible", "Collapsed");
        }

        public bool OutputsVisible
        {
            get { return _getVisibility("/Config/Window/Outputs"); }
            set { _setVisibility("/Config/Window/Outputs", value); }
        }

        public bool ChargerDisplayVisible
        {
            get { return _getVisibility("/Config/Window/ChargerDisplay"); }
            set { _setVisibility("/Config/Window/ChargerDisplay", value); }
        }

        public bool ActivityLogVisible
        {
            get { return _getVisibility("/Config/Window/ActivityLog"); }
            set { _setVisibility("/Config/Window/ActivityLog", value); }
        }

        public bool ConnectionDetailsVisible
        {
            get { return _getVisibility("/Config/Window/ConnectionDetails"); }
            set { _setVisibility("/Config/Window/ConnectionDetails", value); }
        }

        // Decimally encoded unsigned integers

        private int _getInt(string path)
        {
            return int.Parse(_getString(path));
        }

        private void _setInt(string path, int value)
        {
            _setString(path, string.Format("{0}", value));
        }

        // Window position

        public int PositionX
        {
            get { return _getInt("/Config/Window/PositionX"); }
            set { _setInt("/Config/Window/PositionX", value); }
        }

        public int PositionY
        {
            get { return _getInt("/Config/Window/PositionY"); }
            set { _setInt("/Config/Window/PositionY", value); }
        }
    }
}
