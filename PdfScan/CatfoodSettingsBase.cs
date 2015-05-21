using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Diagnostics;

namespace Catfood.Utils
{
    /// <summary>
    /// Severity level for logged messages
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Information
        /// </summary>
        Information,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Error
        /// </summary>
        Error
    }

    /// <summary>
    /// Base settings class for a catfood product, includes logging
    /// </summary>
    public class CatfoodSettingsBase : INotifyPropertyChanged, IDataErrorInfo
    {
        private const string AppDataParentFolder = "Catfood Software";
        private const string AppDataSettingsFilename = "settings.xml";
        private const string ElementSettings = "Settings";
        private const string ElementXml = "xml";
        private const int SaveRetryLimit = 4;
        private const int SaveRetryWaitMS = 250;

        private object _settingsLock;
        private object _loggingLock;
        private string _appName;
        private string _settingsFolder;
        private string _settingsFile;
        private string _logFile;
        private bool _updateProxySettings;
        private string _versionText;
        private int _versionMajor;
        private int _versionMinor;

        /// <summary>
        /// Event fired when settings are saved
        /// </summary>
        public event EventHandler SettingsSaved;

        #region Base Settings

        private bool _isLoggingEnabled;
        private const string ElementIsLoggingEnabled = "BaseIsLoggingEnabled";
        private string _registrationEmail;
        private const string ElementRegistrationEmail = "BaseRegistrationEmail";
        private int _registrationKey;
        private const string ElementRegistrationKey = "BaseRegistrationKey";
        private bool _useProxyServer;
        private const string ElementUseProxyServer = "BaseUseProxyServer";
        private bool _useProxyServerCredentials;
        private const string ElementUseProxyServerCredentials = "BaseUseProxyServerCredentials";
        private string _proxyAddress;
        private const string ElementProxyAddress = "BaseProxyAddress";
        private int _proxyPort;
        private const string ElementProxyPort = "BaseProxyPort";
        private string _proxyUser;
        private const string ElementProxyUser = "BaseProxyUser";
        private string _proxyPass;
        private const string ElementProxyPass = "BaseProxyPass";
        private string _proxyDomain;
        private const string ElementProxyDomain = "BaseProxyDomain";
        private Guid _instanceId;
        private const string ElementInstanceId = "BaseInstanceId";
        private DateTime _installDate;
        private const string ElementInstallDate = "BaseInstallDate";
        private DateTime _nextUpdateCheck;
        private const string ElementNextUpdateCheck = "BaseNextUpdateCheck";

        /// <summary>
        /// Gets the Instance Id for this settings file
        /// </summary>
        public Guid InstanceId
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _instanceId;
                }
            }
            private set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_instanceId != value)
                    {
                        propertyChanged = true;
                        _instanceId = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("InstanceId"); }
            }
        }

        /// <summary>
        /// Gets the date/time that this setting file was created
        /// </summary>
        public DateTime InstallDate
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _installDate;
                }
            }
            private set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_installDate != value)
                    {
                        propertyChanged = true;
                        _installDate = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("InstallDate"); }
            }
        }

        /// <summary>
        /// Gets or sets the date and time of the next update check
        /// </summary>
        public DateTime NextUpdateCheck
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _nextUpdateCheck;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_nextUpdateCheck != value)
                    {
                        propertyChanged = true;
                        _nextUpdateCheck = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("NextUpdateCheck"); }
            }
        }

        /// <summary>
        /// True if a proxy server should be used
        /// </summary>
        public bool UseProxyServer
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _useProxyServer;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_useProxyServer != value)
                    {
                        propertyChanged = true;
                        _useProxyServer = value;
                        _updateProxySettings = true;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("UseProxyServer"); }
            }
        }

        /// <summary>
        /// True if the proxy server requires authentication
        /// </summary>
        public bool UseProxyServerCredentials
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _useProxyServerCredentials;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_useProxyServerCredentials != value)
                    {
                        propertyChanged = true;
                        _useProxyServerCredentials = value;
                        _updateProxySettings = true;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("UseProxyServerCredentials"); }
            }
        }

        /// <summary>
        /// Gets or sets the address or hostname of the proxy server
        /// </summary>
        public string ProxyAddress
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _proxyAddress;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_proxyAddress != value)
                    {
                        propertyChanged = true;
                        _proxyAddress = value;
                        _updateProxySettings = true;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("ProxyAddress"); }
            }
        }

        /// <summary>
        /// Gets or sets the port of the proxy server
        /// </summary>
        public int ProxyPort
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _proxyPort;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_proxyPort != value)
                    {
                        propertyChanged = true;
                        _proxyPort = value;
                        _updateProxySettings = true;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("ProxyPort"); }
            }
        }

        /// <summary>
        /// Gets or sets the username to use for proxy credentials
        /// </summary>
        public string ProxyUser
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _proxyUser;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_proxyUser != value)
                    {
                        propertyChanged = true;
                        _proxyUser = value;
                        _updateProxySettings = true;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("ProxyUser"); }
            }
        }

        /// <summary>
        /// Gets or sets the password to use for proxy credentials
        /// </summary>
        public string ProxyPass
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _proxyPass;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_proxyPass != value)
                    {
                        propertyChanged = true;
                        _proxyPass = value;
                        _updateProxySettings = true;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("ProxyPass"); }
            }
        }

        /// <summary>
        /// Gets or sets the domain to use for proxy credentials
        /// </summary>
        public string ProxyDomain
        {
            get
            {
                lock (this.SettingsLock)
                {
                    return _proxyDomain;
                }
            }
            set
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_proxyDomain != value)
                    {
                        propertyChanged = true;
                        _proxyDomain = value;
                        _updateProxySettings = true;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("ProxyDomain"); }
            }
        }

        /// <summary>
        /// True if logging is enabled for the application
        /// </summary>
        public bool IsLoggingEnabled
        {
            get 
            {
                lock (this.SettingsLock)
                {
                    return _isLoggingEnabled;
                }
            }
            set 
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_isLoggingEnabled != value)
                    {
                        propertyChanged = true;
                        _isLoggingEnabled = value;

                        if (_isLoggingEnabled)
                        {
                            LogMessage(LogSeverity.Information,
                                string.Format(CultureInfo.InvariantCulture,
                                "Logging enabled, diagnostic information:\r\n\r\n{0}",
                                CatfoodSettingsBase.DiagInfo));
                        }
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("IsLoggingEnabled"); }
            }
        }

        /// <summary>
        /// Gets the email used to register this product
        /// </summary>
        public string RegistrationEmail
        {
            get 
            {
                lock (this.SettingsLock)
                {
                    return _registrationEmail;
                }
            }
            protected set 
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_registrationEmail != value)
                    {
                        propertyChanged = true;
                        _registrationEmail = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("RegistrationEmail"); }
            }
        }

        /// <summary>
        /// Gets the key used to register this product
        /// </summary>
        public int RegistrationKey
        {
            get 
            {
                lock (this.SettingsLock)
                {
                    return _registrationKey;
                }
            }
            protected set 
            {
                bool propertyChanged = false;

                lock (this.SettingsLock)
                {
                    if (_registrationKey != value)
                    {
                        propertyChanged = true;
                        _registrationKey = value;
                    }
                }

                if (propertyChanged) { NotifyPropertyChanged("RegistrationKey"); }
            }
        }


        #endregion

        /// <summary>
        /// Gets the version string for the product (i.e. 2.20.0211)
        /// </summary>
        public string VersionText
        {
            get { return _versionText; }
        }

        /// <summary>
        /// Gets the major version number for the product
        /// </summary>
        public int VersionMajor
        {
            get { return _versionMajor; }
        }

        /// <summary>
        /// Gets the minor version number for the product
        /// </summary>
        public int VersionMinor
        {
            get { return _versionMinor; }
        }

        /// <summary>
        /// Lock this object when accessing settings
        /// </summary>
        protected object SettingsLock
        {
            get { return _settingsLock; }
        }

        /// <summary>
        /// Gets the folder used to store settings for this application. Note
        /// that this folder might not exist if settings have not been saved.
        /// </summary>
        public string SettingsFolder
        {
            get { return _settingsFolder; }
        }

        /// <summary>
        /// Gets the catfood product name associated with this settings instance
        /// </summary>
        public string AppName
        {
            get { return _appName; }
        }

        

        

        /// <summary>
        /// Save settings
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Save()
        {
            Exception lastException = null;
            int retryAttempt = 0;
            int sleepFor = SaveRetryWaitMS;

            while (retryAttempt <= SaveRetryLimit)
            {
                lastException = null;

                try
                {
                    lock (this.SettingsLock)
                    {
                        // create the settings folder if necessary
                        if (!Directory.Exists(_settingsFolder))
                        {
                            Directory.CreateDirectory(_settingsFolder);
                        }

                        XmlWriterSettings writerSettings = new XmlWriterSettings();
                        writerSettings.CheckCharacters = true;
                        writerSettings.CloseOutput = true;
                        writerSettings.ConformanceLevel = ConformanceLevel.Document;
                        writerSettings.Indent = true;
                        
                        using (XmlWriter writer = XmlWriter.Create(_settingsFile, writerSettings))
                        {
                            writer.WriteStartDocument();
                            writer.WriteStartElement(ElementSettings);

                            // save base settings
                            writer.WriteStartElement(ElementIsLoggingEnabled);
                            writer.WriteValue(_isLoggingEnabled);
                            writer.WriteEndElement();

                            if (!string.IsNullOrEmpty(_registrationEmail))
                            {
                                writer.WriteStartElement(ElementRegistrationEmail);
                                writer.WriteString(CatfoodCrypto.EncryptString(_registrationEmail, _appName));
                                writer.WriteEndElement();

                                writer.WriteStartElement(ElementRegistrationKey);
                                writer.WriteValue(_registrationKey);
                                writer.WriteEndElement();
                            }

                            writer.WriteStartElement(ElementUseProxyServer);
                            writer.WriteValue(_useProxyServer);
                            writer.WriteEndElement();

                            writer.WriteStartElement(ElementUseProxyServerCredentials);
                            writer.WriteValue(_useProxyServerCredentials);
                            writer.WriteEndElement();

                            if (!string.IsNullOrEmpty(_proxyAddress))
                            {
                                writer.WriteStartElement(ElementProxyAddress);
                                writer.WriteString(_proxyAddress);
                                writer.WriteEndElement();
                            }

                            if (!string.IsNullOrEmpty(_proxyDomain))
                            {
                                writer.WriteStartElement(ElementProxyDomain);
                                writer.WriteString(CatfoodCrypto.EncryptString(_proxyDomain, _appName));
                                writer.WriteEndElement();
                            }

                            if (!string.IsNullOrEmpty(_proxyPass))
                            {
                                writer.WriteStartElement(ElementProxyPass);
                                writer.WriteString(CatfoodCrypto.EncryptString(_proxyPass, _appName));
                                writer.WriteEndElement();
                            }

                            writer.WriteStartElement(ElementProxyPort);
                            writer.WriteValue(_proxyPort);
                            writer.WriteEndElement();

                            if (!string.IsNullOrEmpty(_proxyUser))
                            {
                                writer.WriteStartElement(ElementProxyUser);
                                writer.WriteString(CatfoodCrypto.EncryptString(_proxyUser, _appName));
                                writer.WriteEndElement();
                            }

                            writer.WriteStartElement(ElementInstanceId);
                            writer.WriteString(_instanceId.ToString());
                            writer.WriteEndElement();

                            writer.WriteStartElement(ElementInstallDate);
                            writer.WriteValue(_installDate.Ticks);
                            writer.WriteEndElement();

                            writer.WriteStartElement(ElementNextUpdateCheck);
                            writer.WriteValue(_nextUpdateCheck.Ticks);
                            writer.WriteEndElement();

                            // save derived class settings
                            SaveToXmlWriter(writer);

                            writer.WriteEndElement();
                            writer.WriteEndDocument();
                        }
                    }

                    // if we get this far save succeeded
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                retryAttempt++;

                LogException(LogSeverity.Warning,
                    string.Format(CultureInfo.CurrentCulture,
                    "Failed to save settings, retrying (attempt {0} of {1})",
                    retryAttempt,
                    SaveRetryLimit),
                    lastException);

                // sleep for a bit in case the settings file is locked, and
                // bump up the sleep interval for the next save attempt
                Thread.Sleep(sleepFor);
                sleepFor += SaveRetryWaitMS;
            }

            // throw the last exception if we reached the retry limit and still didn't save
            if ((retryAttempt == SaveRetryLimit) && (lastException != null))
            {
                LogMessage(LogSeverity.Error, "Failed to save settings");
                throw lastException;
            }
            else
            {
                UpdateProxyServer();

                // fire the settings saved event
                if (SettingsSaved != null)
                {
                    SettingsSaved(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Called to save derrived class settings. Note that the base class elements
        /// all start Base... to avoid any conflict
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void SaveToXmlWriter(XmlWriter writer)
        {
            if (writer == null) { throw new ArgumentNullException("writer"); }
        }

        /// <summary>
        /// Load settings
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Load()
        {
            if (File.Exists(_settingsFile))
            {
                lock (this.SettingsLock)
                {
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.CheckCharacters = true;
                    readerSettings.CloseInput = true;
                    readerSettings.ConformanceLevel = ConformanceLevel.Document;
                    readerSettings.IgnoreComments = true;
                    readerSettings.IgnoreWhitespace = true;

                    using (XmlReader reader = XmlReader.Create(_settingsFile, readerSettings))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.Name)
                                {
                                    case ElementXml:
                                    case ElementSettings:
                                        // do nothing
                                        break;

                                    case ElementIsLoggingEnabled:
                                        try
                                        {
                                            this.IsLoggingEnabled = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            this.IsLoggingEnabled = false;
                                        }
                                        break;

                                    case ElementRegistrationEmail:
                                        try
                                        {
                                            this.RegistrationEmail = CatfoodCrypto.DecryptString(reader.ReadString(), _appName);
                                        }
                                        catch
                                        {
                                            this.RegistrationEmail = null;
                                        }
                                        break;

                                    case ElementRegistrationKey:
                                        try
                                        {
                                            this.RegistrationKey = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            this.RegistrationKey = 0;
                                        }
                                        break;

                                    case ElementUseProxyServer:
                                        try
                                        {
                                            this.UseProxyServer = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            this.UseProxyServer = false;
                                        }
                                        break;

                                    case ElementUseProxyServerCredentials:
                                        try
                                        {
                                            this.UseProxyServerCredentials = Convert.ToBoolean(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            this.UseProxyServerCredentials = false;
                                        }
                                        break;

                                    case ElementProxyAddress:
                                        try
                                        {
                                            this.ProxyAddress = reader.ReadString();
                                        }
                                        catch
                                        {
                                            this.ProxyAddress = null;
                                        }
                                        break;

                                    case ElementProxyDomain:
                                        try
                                        {
                                            this.ProxyDomain = CatfoodCrypto.DecryptString(reader.ReadString(), _appName);
                                        }
                                        catch
                                        {
                                            this.ProxyDomain = null;
                                        }
                                        break;

                                    case ElementProxyPass:
                                        try
                                        {
                                            this.ProxyPass = CatfoodCrypto.DecryptString(reader.ReadString(), _appName);
                                        }
                                        catch
                                        {
                                            this.ProxyPass = null;
                                        }
                                        break;

                                    case ElementProxyPort:
                                        try
                                        {
                                            this.ProxyPort = Convert.ToInt32(reader.ReadString(), CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            this.ProxyPort = 8080;
                                        }
                                        break;

                                    case ElementProxyUser:
                                        try
                                        {
                                            this.ProxyUser = CatfoodCrypto.DecryptString(reader.ReadString(), _appName);
                                        }
                                        catch
                                        {
                                            this.ProxyUser = null;
                                        }
                                        break;

                                    case ElementInstanceId:
                                        try
                                        {
                                            this.InstanceId = new Guid(reader.ReadString());
                                        }
                                        catch
                                        {
                                            this.InstanceId = Guid.Empty;
                                        }
                                        break;

                                    case ElementInstallDate:
                                        try
                                        {
                                            this.InstallDate = new DateTime(Convert.ToInt64(reader.ReadString(), CultureInfo.InvariantCulture));
                                        }
                                        catch
                                        {
                                            this.InstallDate = DateTime.Now;
                                        }
                                        break;

                                    case ElementNextUpdateCheck:
                                        try
                                        {
                                            this.NextUpdateCheck = new DateTime(Convert.ToInt64(reader.ReadString(), CultureInfo.InvariantCulture));
                                        }
                                        catch
                                        {
                                            this.NextUpdateCheck = DateTime.Now;
                                        }
                                        break;

                                    default:
                                        // give derrived class a chance to handle elements not related to us
                                        try
                                        {
                                            LoadFromXmlElement(reader.Name, reader.ReadString());
                                            LoadFromXmlReader(reader.Name, reader);
                                        }
                                        catch { }
                                        break;
                                }
                            }
                        }
                    }
                }

                UpdateProxyServer();
            }

            // generate an instance Id if needed
            if (this.InstanceId == Guid.Empty)
            {
                this.InstanceId = Guid.NewGuid();
            }
        }

        /// <summary>
        /// Called for each element not handled by the base settings class
        /// </summary>
        /// <param name="name">XML element name</param>
        /// <param name="value">XML element value</param>
        protected virtual void LoadFromXmlElement(string name, string value)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (value == null) { throw new ArgumentNullException("value"); }
        }

        /// <summary>
        /// Called for each element not handled by the base settings class
        /// </summary>
        /// <param name="name">XML element name at time called</param>
        /// <param name="reader">XmlReader</param>
        protected virtual void LoadFromXmlReader(string name, XmlReader reader)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (reader == null) { throw new ArgumentNullException("reader"); }
        }

        /// <summary>
        /// Override in derrived classes to validate properties
        /// </summary>
        /// <param name="propertyName">Property name to validate</param>
        /// <returns>Error text or null if no error</returns>
        protected virtual string ValidateProperty(string propertyName)
        {
            return null;
        }

        /// <summary>
        /// Reset settings to their default values
        /// </summary>
        public virtual void ResetSettings()
        {
            ResetSettingsCore();
        }

        private void ResetSettingsCore()
        {
            _isLoggingEnabled = false;
            _registrationEmail = null;
            _registrationKey = 0;
            _useProxyServer = false;
            _useProxyServerCredentials = false;
            _proxyAddress = null;
            _proxyDomain = null;
            _proxyPass = null;
            _proxyPort = 8080;
            _proxyUser = null;
            _instanceId = Guid.Empty;
            _installDate = DateTime.Now;
            _nextUpdateCheck = DateTime.Now;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void UpdateProxyServer()
        {
            if (_updateProxySettings)
            {
                lock (this.SettingsLock)
                {
                    if (_updateProxySettings)
                    {
                        try
                        {
                            if (_useProxyServer)
                            {
                                WebProxy proxy = new WebProxy(_proxyAddress, _proxyPort);

                                if (_useProxyServerCredentials)
                                {
                                    NetworkCredential credentials;

                                    if (string.IsNullOrEmpty(_proxyDomain))
                                    {
                                        credentials = new NetworkCredential(_proxyUser, _proxyPass);
                                    }
                                    else
                                    {
                                        credentials = new NetworkCredential(_proxyUser, _proxyPass, _proxyDomain);
                                    }

                                    proxy.Credentials = credentials;
                                }

                                System.Net.WebRequest.DefaultWebProxy = proxy;
                            }
                            else
                            {
                                // use default (IE) proxy settings
                                System.Net.WebRequest.DefaultWebProxy = System.Net.WebRequest.GetSystemWebProxy();
                            }

                            _updateProxySettings = false;
                        }
                        catch (Exception ex)
                        {
                            LogException(LogSeverity.Error, "Failed to update proxy settings", ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="severity">Severity level</param>
        /// <param name="message">Message</param>
        public void LogMessage(LogSeverity severity, string message)
        {
            lock (_loggingLock)
            {
                string severityText;
                switch (severity)
                {
                    case LogSeverity.Information:
                    default:
                        severityText = "---";
                        break;

                    case LogSeverity.Warning:
                        severityText = "!--";
                        break;

                    case LogSeverity.Error:
                        severityText = "!!-";
                        break;
                }

                string fullMessage = string.Format(CultureInfo.InvariantCulture,
                    "{0} {1} [{2}] {3}\r\n",
                    severityText,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    Thread.CurrentThread.ManagedThreadId,
                    message);

                Debug.WriteLine(fullMessage);

                if (_isLoggingEnabled)
                {
                    File.AppendAllText(_logFile, fullMessage);
                }
            }
        }

        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="severity">Severity level</param>
        /// <param name="message">Message</param>
        /// <param name="ex">Exception</param>
        public void LogException(LogSeverity severity, string message, Exception ex)
        {
            LogMessage(severity,
                string.Format(CultureInfo.CurrentCulture,
                "{0}: {1}",
                message,
                DumpException(ex)));
        }

        /// <summary>
        /// Dumps as much information as possible from an exception
        /// </summary>
        /// <param name="ex">Exception to dump</param>
        /// <returns>Exception dump</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string DumpException(Exception ex)
        {
            string exceptionDump = string.Empty;

            if (ex != null)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    Type exType = ex.GetType();

                    sb.AppendLine(exType.FullName);

                    PropertyInfo[] properties = exType.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        // try to dump each property
                        try
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture,
                                "{0}: {1}",
                                property.Name,
                                property.GetValue(ex, null));
                            sb.AppendLine();
                        }
                        catch { }
                    }

                    if ((ex.Data != null) && (ex.Data.Count > 0))
                    {
                        foreach (Object key in ex.Data.Keys)
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture,
                                "Data: Key={0}, Value={1}",
                                key,
                                ex.Data[key]);
                            sb.AppendLine();
                        }
                    }

                    exceptionDump += sb.ToString();

                    // Recurse through any inner exceptions.
                    if (ex.InnerException != null)
                    {
                        exceptionDump += "\r\n\r\n";
                        exceptionDump += DumpException(ex.InnerException);
                    }
                }
                catch { }
            }

            return exceptionDump;
        }

        /// <summary>
        /// Returns a string containing useful diagnostic information
        /// </summary>
        /// <returns>Diagnostic information</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string DiagInfo
        {
            get
            {
                StringBuilder sb = new StringBuilder(1024);

                try
                {
                    WindowsPrincipal windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    bool admin = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

                    Assembly thisAssembly = Assembly.GetExecutingAssembly();
                    Assembly entryAssembly = Assembly.GetEntryAssembly();

                    if (entryAssembly != null)
                    {
                        sb.AppendLine(entryAssembly.GetName().Name + " " + entryAssembly.GetName().Version);
                    }

                    if (thisAssembly != null)
                    {
                        sb.AppendLine(thisAssembly.GetName().Name + " " + thisAssembly.GetName().Version);
                    }

                    sb.AppendLine("Command Line: " + Environment.CommandLine);
                    sb.AppendLine("Current Directory: " + Environment.CurrentDirectory);
                    sb.AppendLine("Framework: " + Environment.Version.ToString());
                    sb.AppendLine("OS: " + Environment.OSVersion.VersionString);
                    sb.AppendLine("Processors: " + Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine("Current Culture: " + System.Globalization.CultureInfo.CurrentCulture.ToString());
                    sb.AppendLine("Current UI Culture (for current thread): " + Thread.CurrentThread.CurrentUICulture.ToString());
                    sb.AppendLine("Administrator: " + admin.ToString());
                    sb.AppendLine("Machine Name: " + Environment.MachineName);
                    sb.AppendLine("64-bit OS: " + Environment.Is64BitOperatingSystem.ToString());
                    sb.AppendLine("64-bit Process: " + Environment.Is64BitProcess.ToString());
                    sb.AppendLine("Working Set: " + Environment.WorkingSet.ToString("n0", CultureInfo.InvariantCulture));
                }
                catch { }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Fires the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the changed property</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (propertyName == null) { throw new ArgumentNullException("propertyName"); }
            if (propertyName.Length == 0) { throw new ArgumentException("propertyName may not be empty"); }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Base settings class for a catfood product, includes logging
        /// </summary>
        /// <param name="appName">Application name</param>
        protected CatfoodSettingsBase(string appName)
        {
            if (appName == null) { throw new ArgumentNullException("appName"); }
            if (appName.Length == 0) { throw new ArgumentException("appName may not be empty"); }

            _settingsLock = new object();
            _loggingLock = new object();
            _appName = appName;
            _updateProxySettings = true;

            Version v = Assembly.GetEntryAssembly().GetName().Version;
            _versionText = string.Format(CultureInfo.InvariantCulture,
                "{0}.{1:00}.{2:0000}",
                v.Major,
                v.Minor,
                v.Build);

            _versionMajor = v.Major;
            _versionMinor = v.Minor;

            _logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                string.Format(CultureInfo.InvariantCulture, "{0} Diagnostic Log.txt", _appName));

            _settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppDataParentFolder,
                _appName);

            _settingsFile = Path.Combine(_settingsFolder, AppDataSettingsFilename);

            ResetSettingsCore();
        }

        #region INotifyPropertyChanged Members

        /// <summary />
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IDataErrorInfo Members

        /// <summary />
        public string Error
        {
            get { return null; }
        }

        /// <summary />
        public string this[string columnName]
        {
            get 
            {
                string error = null;

                switch (columnName)
                {
                    case "ProxyAddress":
                        if (this.UseProxyServer)
                        {
                            if (string.IsNullOrEmpty(this.ProxyAddress))
                            {
                                error = "Proxy Address Required";
                            }
                        }
                        break;

                    case "ProxyPort":
                        if (this.UseProxyServer)
                        {
                            if ((this.ProxyPort < 0) || (this.ProxyPort > 65536))
                            {
                                error = "Proxy Port Invalid";
                            }
                        }
                        break;

                    case "ProxyUser":
                        if ((this.UseProxyServer) && (this.UseProxyServerCredentials))
                        {
                            if (string.IsNullOrEmpty(this.ProxyUser))
                            {
                                error = "Username Required";
                            }
                        }
                        break;

                    case "ProxyPass":
                        if ((this.UseProxyServer) && (this.UseProxyServerCredentials))
                        {
                            if (string.IsNullOrEmpty(this.ProxyPass))
                            {
                                error = "Password Required";
                            }
                        }
                        break;

                    default:
                        error = ValidateProperty(columnName);
                        break;
                }

                return error;
            }
        }

        #endregion
    }
}
