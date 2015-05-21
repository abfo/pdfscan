using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catfood.Utils;
using System.Xml;
using System.Globalization;

namespace PdfScan
{
    /// <summary>
    /// User Settings
    /// </summary>
    sealed class UserSettings : CatfoodSettingsBase
    {
        private static UserSettings _settings = new UserSettings();

        private int _initialRunCount;
        private const string ElementInitialRunCount = "InitialRunCount";
        private string _selectedPaperSize;
        private const string ElementSelectedPaperSize = "SelectedPaperSize";
        private bool _closeOnSave;
        private const string ElementCloseOnSave = "CloseOnSave";
        private bool _settingsUpgraded;
        private const string ElementSettingsUpgraded = "SettingsUpgraded";
        private bool _useAdf;
        private const string ElementUseAdf = "UseAdf";

        /// <summary>
        /// The number of times that PdfScan has been run (prior to nagging the user to register)
        /// </summary>
        public int InitialRunCount
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _initialRunCount;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_initialRunCount != value)
                    {
                        propertyChanged = true;
                        _initialRunCount = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("InitialRunCount"); }
            }
        }

        /// <summary>
        /// The most recently selected paper size
        /// </summary>
        public string SelectedPaperSize
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _selectedPaperSize;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_selectedPaperSize != value)
                    {
                        propertyChanged = true;
                        _selectedPaperSize = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("SelectedPaperSize"); }
            }
        }

        /// <summary>
        /// True if PdfScan should close after saving
        /// </summary>
        public bool CloseOnSave
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _closeOnSave;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_closeOnSave != value)
                    {
                        propertyChanged = true;
                        _closeOnSave = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("CloseOnSave"); }
            }
        }

        /// <summary>
        /// True if settings have been upgraded from the 1.00 (Properties.Settings) format
        /// </summary>
        public bool SettingsUpgraded
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _settingsUpgraded;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_settingsUpgraded != value)
                    {
                        propertyChanged = true;
                        _settingsUpgraded = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("SettingsUpgraded"); }
            }
        }

        /// <summary>
        /// True to use the ADF
        /// </summary>
        public bool UseAdf
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _useAdf;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_useAdf != value)
                    {
                        propertyChanged = true;
                        _useAdf = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("UseAdf"); }
            }
        }

        /// <summary>
        /// Gets the Settings instance
        /// </summary>
        public static UserSettings Settings
        {
            get { return UserSettings._settings; }
        }

        protected override void LoadFromXmlElement(string name, string value)
        {
            base.LoadFromXmlElement(name, value);

            switch (name)
            {
                case ElementCloseOnSave:
                    try
                    {
                        this.CloseOnSave = Convert.ToBoolean(value);
                    }
                    catch
                    {
                        this.CloseOnSave = true;
                    }
                    break;

                case ElementInitialRunCount:
                    try
                    {
                        this.InitialRunCount = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        this.InitialRunCount = 0;
                    }
                    break;

                case ElementSelectedPaperSize:
                    this.SelectedPaperSize = value;
                    break;

                case ElementSettingsUpgraded:
                    try
                    {
                        this.SettingsUpgraded = Convert.ToBoolean(value);
                    }
                    catch
                    {
                        this.SettingsUpgraded = false;
                    }
                    break;

                case ElementUseAdf:
                    try
                    {
                        this.UseAdf = Convert.ToBoolean(value);
                    }
                    catch
                    {
                        this.UseAdf = true;
                    }
                    break;
            }
        }

        protected override void SaveToXmlWriter(XmlWriter writer)
        {
            base.SaveToXmlWriter(writer);

            writer.WriteStartElement(ElementSettingsUpgraded);
            writer.WriteValue(_settingsUpgraded);
            writer.WriteEndElement();

            writer.WriteStartElement(ElementCloseOnSave);
            writer.WriteValue(_closeOnSave);
            writer.WriteEndElement();

            writer.WriteStartElement(ElementInitialRunCount);
            writer.WriteValue(_initialRunCount);
            writer.WriteEndElement();

            writer.WriteStartElement(ElementUseAdf);
            writer.WriteValue(_useAdf);
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(_selectedPaperSize))
            {
                writer.WriteStartElement(ElementSelectedPaperSize);
                writer.WriteString(_selectedPaperSize);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Reset Settings
        /// </summary>
        public override void ResetSettings()
        {
            _initialRunCount = 0;
            _selectedPaperSize = "Letter";
            _closeOnSave = true;
            _settingsUpgraded = false;
            _useAdf = true;

            base.ResetSettings();
        }

        private UserSettings()
            : base("PdfScan")
        {
            ResetSettings();
        }
    }
}
