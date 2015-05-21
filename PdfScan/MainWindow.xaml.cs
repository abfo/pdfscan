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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WIA;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Globalization;
using System.Reflection;
using Catfood.Utils;
using Catfood.Utils.Xaml;
using System.Threading;
using System.Collections.ObjectModel;


// same problem: http://social.msdn.microsoft.com/Forums/en/windowssdk/thread/2d7dd50d-b876-4d5a-8586-dfe3eb2e42bb

namespace PdfScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Application icon for use in CatfoodMessageBox
        /// </summary>
        public static ImageSource MessageBoxIcon { get; private set; }

        private const int FreeRunCount = 10;

        private List<string> _tempFilesToDelete;
        private string _deviceId;
        private PdfDocument _doc;
        private bool _docSaved;
        private double _width = 8.5;
        private double _height = 11.0;
        private bool _adf = true;
        private string _versionString;
        public PaperSize SelectedSize { get; set; }
        public List<PaperSize> Sizes { get; private set; }
        public ObservableCollection<string> PageImages { get; private set; }
        private string _lastItem;

        private const string formatJpeg = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";

        public MainWindow()
        {
            InitializeComponent();

            _tempFilesToDelete = new List<string>();
            this.PageImages = new ObservableCollection<string>();

            

            if (UserSettings.Settings.InitialRunCount < FreeRunCount)
            {
                UserSettings.Settings.InitialRunCount++;
            }

            checkBoxCloseOnSave.IsChecked = UserSettings.Settings.CloseOnSave;
            checkBoxADF.IsChecked = UserSettings.Settings.UseAdf;

            UpdateState();
        }

        private void UpdateState()
        {
            int pageCount = 0;

            if (_doc != null)
            {
                pageCount = _doc.PageCount;
            }

            buttonSave.IsEnabled = pageCount > 0;
            buttonClear.IsEnabled = pageCount > 0;
            this.Title = string.Format(CultureInfo.CurrentCulture, 
                "{2}: {0} Page{1}", 
                pageCount, 
                pageCount == 1 ? "" : "s", 
                _versionString);
        }

        private void ResetState()
        {
            _doc = new PdfDocument();
            _docSaved = true;
            textBoxTitle.Text = string.Empty;
            textBoxSubject.Text = string.Empty;
            textBoxAuthor.Text = string.Empty;
            textBoxKeywords.Text = string.Empty;
            this.PageImages.Clear();

            UpdateState();
        }

        private void buttonScanPages_Click(object sender, RoutedEventArgs e)
        {
            // select device if this failed at startup
            if (string.IsNullOrEmpty(_deviceId))
            {
                if (!SelectDevice())
                {
                    return;
                }
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                _width = Convert.ToDouble(textBoxWidth.Text, CultureInfo.CurrentCulture);
                _height = Convert.ToDouble(textBoxHeight.Text, CultureInfo.CurrentCulture);
                _adf = (checkBoxADF.IsChecked == true);

                XImage ximage = null;

                while ((ximage = ScanOne()) != null)
                {
                    PdfPage page = _doc.AddPage();
                    page.Width = XUnit.FromInch(_width);
                    page.Height = XUnit.FromInch(_height);

                    using (XGraphics g = XGraphics.FromPdfPage(page))
                    {
                        g.DrawImage(ximage, 0, 0);
                        ximage.Dispose();
                    }

                    // flag that the document needs saving
                    _docSaved = false;

                    // only scan one page if not using the ADF
                    if (!_adf)
                    {
                        break;
                    }

                    UpdateState();
                }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;

                UserSettings.Settings.LogException(LogSeverity.Warning, "Failed to scan page", ex);

                CatfoodMessageBox.Show(MainWindow.MessageBoxIcon,
                    this,
                    string.Format(CultureInfo.InvariantCulture, "{0} ({1})", WiaErrorOrMessage(ex), _lastItem),
                    "Failed to scan - Catfood PdfScan",
                    CatfoodMessageBoxType.Ok,
                    CatfoodMessageBoxIcon.Error,
                    ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                UpdateState();
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.AddExtension = true;
                sfd.CheckFileExists = false;
                sfd.CheckPathExists = true;
                sfd.DefaultExt = "pdf";
                sfd.Filter = "PDF Documents (*.pdf)|*.pdf";
                sfd.OverwritePrompt = true;
                sfd.Title = "Save PDF - Catfood PdfScan";

                if (textBoxTitle.Text.Length > 0)
                {
                    char[] badFileChars = System.IO.Path.GetInvalidFileNameChars();

                    bool charIsBad = false;
                    StringBuilder sbFileName = new StringBuilder(textBoxTitle.Text.Length);
                    foreach (char c in textBoxTitle.Text)
                    {
                        charIsBad = false;

                        foreach (char badChar in badFileChars)
                        {
                            if (c == badChar)
                            {
                                charIsBad = true;
                                break;
                            }
                        }

                        if (!charIsBad)
                        {
                            sbFileName.Append(c);
                        }
                    }

                    sfd.FileName = sbFileName.ToString();
                }

                if (sfd.ShowDialog(this) == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    _doc.Info.Title = textBoxTitle.Text.Trim();
                    _doc.Info.Author = textBoxAuthor.Text.Trim();
                    _doc.Info.Subject = textBoxSubject.Text.Trim();
                    _doc.Info.Keywords = textBoxKeywords.Text.Trim();
                    _doc.Info.Creator = "PdfScan by Catfood Software: http://catfood.net/products/pdfscan/";

                    _doc.Save(sfd.FileName);
                    _docSaved = true;

                    if (UserSettings.Settings.CloseOnSave)
                    {
                        this.Close();
                    }
                    else
                    {
                        ResetState();
                    }
                }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;

                UserSettings.Settings.LogException(LogSeverity.Warning, "Failed to save PDF", ex);

                CatfoodMessageBox.Show(MainWindow.MessageBoxIcon,
                    this,
                    WiaErrorOrMessage(ex),
                    "Failed to save - PdfScan",
                    CatfoodMessageBoxType.Ok,
                    CatfoodMessageBoxIcon.Error,
                    ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool SelectDevice()
        {
            try
            {
                CommonDialog commonDialog = new CommonDialog();
                Device device = commonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, false, true);
                _deviceId = device.DeviceID;
            }
            catch (Exception ex)
            {
                UserSettings.Settings.LogException(LogSeverity.Warning, "Failed to select scanner", ex);

                CatfoodMessageBox.Show(MainWindow.MessageBoxIcon,
                    this,
                    WiaErrorOrMessage(ex),
                    "Failed to select scanner - Catfood PdfScan",
                    CatfoodMessageBoxType.Ok,
                    CatfoodMessageBoxIcon.Error,
                    ex);
            }

            return !string.IsNullOrEmpty(_deviceId);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            _versionString = string.Format(CultureInfo.CurrentCulture,
                "Catfood PdfScan {0}.{1:00}.{2:0000}",
                fvi.ProductMajorPart,
                fvi.ProductMinorPart,
                fvi.ProductBuildPart);

            PaperSize selectedSize;
            this.Sizes = PaperSize.GetSizes(out selectedSize);

            // see if one of the sizes matches the previously selected one
            foreach (PaperSize size in this.Sizes)
            {
                if (size.Description == UserSettings.Settings.SelectedPaperSize)
                {
                    selectedSize = size;
                    break;
                }
            }

            this.SelectedSize = selectedSize;
            this.DataContext = this;

            ResetState();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            SelectDevice();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_docSaved)
            {
                if (CatfoodMessageBox.Show(MainWindow.MessageBoxIcon,
                    this,
                    "PDF not saved - are you sure you want to exit?",
                    "Exit Catfood PdfScan?",
                    CatfoodMessageBoxType.YesNo,
                    CatfoodMessageBoxIcon.Question) != CatfoodMessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (string file in _tempFilesToDelete)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
        }

        private XImage ScanOne()
        {
            XImage ximage = null;

            try
            {
                // find our device (scanner previously selected with commonDialog.ShowSelectDevice)
                DeviceManager manager = new DeviceManager();
                DeviceInfo deviceInfo = null;
                foreach (DeviceInfo info in manager.DeviceInfos)
                {
                    if (info.DeviceID == _deviceId)
                    {
                        deviceInfo = info;
                    }
                }

                if (deviceInfo != null)
                {                    
                    Device device = deviceInfo.Connect();
                    CommonDialog commonDialog = new CommonDialog();

                    Item item = device.Items[1];
                    int dpi = 150;

                    // configure item                    
                    SetItemIntProperty(ref item, 6147, dpi); // 150 dpi
                    SetItemIntProperty(ref item, 6148, dpi); // 150 dpi
                    SetItemIntProperty(ref item, 6151, (int)(dpi * _width)); // scan width
                    SetItemIntProperty(ref item, 6152, (int)(dpi * _height)); // scan height

                    try
                    {
                        SetItemIntProperty(ref item, 6146, 2); // greyscale
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to set greyscale");
                    }

                    try
                    {
                        SetItemIntProperty(ref item, 4104, 8); // bit depth
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to set bit depth");
                    }

                    int deviceHandling = _adf ? 1 : 2; // 1 for ADF, 2 for flatbed

                    // configure device
                    SetDeviceIntProperty(ref device, 3088, deviceHandling); 
                    int handlingStatus = GetDeviceIntProperty(ref device, 3087);

                    if (handlingStatus == deviceHandling)
                    {
                        ImageFile image = commonDialog.ShowTransfer(item, formatJpeg, true);

                        // save image to a temp file and then load into an XImage
                        string tempPath = System.IO.Path.GetTempFileName();
                        File.Delete(tempPath);
                        tempPath = System.IO.Path.ChangeExtension(tempPath, "jpg");
                        image.SaveFile(tempPath);
                        ximage = XImage.FromFile(tempPath);

                        this.PageImages.Add(tempPath);
                        _tempFilesToDelete.Add(tempPath);
                    }
                }
            }
            catch (COMException ex)
            {
                ximage = null;

                // paper empty
                if ((uint)ex.ErrorCode != 0x80210003)
                {
                    throw;
                }
            }

            return ximage;
        }

        private string WiaErrorOrMessage(Exception ex)
        {
            string error = null;

            COMException comException = ex as COMException;
            if (comException != null)
            {
                switch ((uint)comException.ErrorCode)
                {
                    case 0x80210001: { error = "WIA: General error"; break; }
                    case 0x80210002: { error = "WIA: Paper jam"; break; }
                    case 0x80210003: { error = "WIA: Paper empty"; break; }
                    case 0x80210004: { error = "WIA: Paper problem"; break; }
                    case 0x80210005: { error = "WIA: Offline"; break; }
                    case 0x80210006: { error = "WIA: Busy"; break; }
                    case 0x80210007: { error = "WIA: Warming up"; break; }
                    case 0x80210008: { error = "WIA: User intervention required"; break; }
                    case 0x80210009: { error = "WIA: Item deleted"; break; }
                    case 0x8021000A: { error = "WIA: Failed to communicate with device"; break; }
                    case 0x8021000B: { error = "WIA: Invalid command"; break; }
                    case 0x8021000C: { error = "WIA: Incorrect hardware setting"; break; }
                    case 0x8021000D: { error = "WIA: Device locked"; break; }
                    case 0x8021000E: { error = "WIA: Exception in driver"; break; }
                    case 0x8021000F: { error = "WIA: Invalid Driver response"; break; }
                }
            }

            if (error == null)
            {
                error = ex.Message;
            }

            return error;
        }

        private void SetDeviceIntProperty(ref Device device, int propertyID, int propertyValue)
        {
            _lastItem = string.Format(CultureInfo.InvariantCulture, "Device {0}={1}", propertyID, propertyValue);

            foreach (Property p in device.Properties)
            {
                if (p.PropertyID == propertyID)
                {
                    object value = propertyValue;
                    p.set_Value(ref value);
                    break;
                }
            }
        }

        private int GetDeviceIntProperty(ref Device device, int propertyID)
        {
            int ret = -1;

            foreach (Property p in device.Properties)
            {
                if (p.PropertyID == propertyID)
                {
                    ret = (int)p.get_Value();
                    break;
                }
            }

            return ret;
        }

        private void SetItemIntProperty(ref Item item, int propertyID, int propertyValue)
        {
            _lastItem = string.Format(CultureInfo.InvariantCulture, "Item {0}={1}", propertyID, propertyValue);

            foreach (Property p in item.Properties)
            {
                if (p.PropertyID == propertyID)
                {
                    object value = propertyValue;
                    p.set_Value(ref value);
                    break;
                }
            }
        }

        private int GetItemIntProperty(ref Item item, int propertyID)
        {
            int ret = -1;

            foreach (Property p in item.Properties)
            {
                if (p.PropertyID == propertyID)
                {
                    ret = (int)p.get_Value();
                    break;
                }
            }

            return ret;
        }

        private void comboBoxPaperSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PaperSize selectedSize = comboBoxPaperSize.SelectedItem as PaperSize;
            if (selectedSize != null)
            {
                textBoxWidth.Text = selectedSize.Width.ToString(CultureInfo.CurrentCulture);
                textBoxHeight.Text = selectedSize.Height.ToString(CultureInfo.CurrentCulture);

                UserSettings.Settings.SelectedPaperSize = selectedSize.Description;
            }
        }

        private void checkBoxCloseOnSave_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Settings.CloseOnSave = true;
        }

        private void checkBoxCloseOnSave_Unchecked(object sender, RoutedEventArgs e)
        {
            UserSettings.Settings.CloseOnSave = false;
        }

        private void checkBoxADF_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Settings.UseAdf = true;
            _adf = true;
        }

        private void checkBoxADF_Unchecked(object sender, RoutedEventArgs e)
        {
            UserSettings.Settings.UseAdf = false;
            _adf = false;
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            if (!_docSaved)
            {
                if (CatfoodMessageBox.Show(MainWindow.MessageBoxIcon,
                    this,
                    "PDF not saved - are you sure you want to clear all pages?",
                    "Clear All Pages? - Catfood PdfScan",
                    CatfoodMessageBoxType.YesNo,
                    CatfoodMessageBoxIcon.Question) == CatfoodMessageBoxResult.Yes)
                {
                    ResetState();
                }
            }
        }



        // Device Props

        //        Item Name, 4098, True, Root
        //Full Item Name, 4099, True, 0000\Root
        //Item Flags, 4101, True, 76
        //Unique Device ID, 2, False, {6BDD1FC6-810F-11D0-BEC7-08002BE2092F}\0000
        //Manufacturer, 3, True, Canon
        //Description, 4, True, Canon MF4100 Series
        //Type, 5, True, 65537
        //Port, 6, True, \\.\Usbscan0
        //Name, 7, True, WIA Canon MF4100 Series
        //Server, 8, False, local
        //Remote Device ID, 9, False, 
        //UI Class ID, 10, True, {00000000-0000-0000-0000-000000000000}
        //Hardware Configuration, 11, True, 0
        //BaudRate, 12, True, 
        //STI Generic Capabilities, 13, True, 17
        //WIA Version, 14, True, 2.0
        //Driver Version, 15, True, 2.0.0.0
        //PnP ID String, 16, True, \\?\usb#vid_04a9&pid_26a3&mi_00#6&1cd25df1&0&0000#{6bdd1fc6-810f-11d0-bec7-08002be2092f}
        //STI Driver Version, 17, True, 2
        //Firmware Version, 1026, True, 1.00
        //Access Rights, 4102, True, 3
        //Horizontal Optical Resolution, 3090, True, 600
        //Vertical Optical Resolution, 3091, True, 1200
        //Horizontal Bed Size, 3074, True, 8500
        //Vertical Bed Size, 3075, True, 11670
        //Max Scan Time, 3095, True, 210000
        //Horizontal Sheet Feed Size, 3076, True, 8500
        //Vertical Sheet Feed Size, 3077, True, 14000
        //Document Handling Capabilities, 3086, True, 3
        //Document Handling Status, 3087, True, 2
        //Document Handling Select, 3088, False, 2
        //Pages, 3096, False, 1
        //Sheet Feeder Registration, 3078, True, 1
        //Horizontal Bed Registration, 3079, True, 0
        //Vertical Bed Registration, 3080, True, 0
        //Image Class, 116747, False, 2
        //Pixel Type, 116746, False, 2
        //DeviceKey, 116743, True, MF4100

        // ITEM Props

        //        Item Name, 4098, True, Top
        //Full Item Name, 4099, True, 0000\Root\Top
        //Item Flags, 4101, True, 67
        //Color Profile Name, 4120, False, sRGB Color Space Profile.icm
        //Horizontal Resolution, 6147, False, 300
        //Vertical Resolution, 6148, False, 300
        //Horizontal Start Position, 6149, False, 0
        //Vertical Start Position, 6150, False, 0
        //Horizontal Extent, 6151, False, 850
        //Vertical Extent, 6152, False, 1167
        //Brightness, 6154, False, 0
        //Contrast, 6155, False, 0
        //Threshold, 6159, False, 128
        //Data Type, 4103, False, 0
        //Bits Per Pixel, 4104, False, 1
        //Channels Per Pixel, 4109, True, 1
        //Bits Per Channel, 4110, True, 1
        //Format, 4106, False, {B96B3CAA-0728-11D3-9D7B-0000F81EF32E}
        //Media Type, 4108, False, 128
        //Mirror, 6158, False, 0
        //Photometric Interpretation, 6153, False, 1
        //Current Intent, 6146, False, 1
        //Pixels Per Line, 4112, True, 850
        //Bytes Per Line, 4113, True, 108
        //Number of Lines, 4114, True, 1167
        //Item Size, 4116, True, 126084
        //Buffer Size, 4118, True, 65535
        //Preferred Format, 4105, True, {B96B3CAA-0728-11D3-9D7B-0000F81EF32E}
        //Access Rights, 4102, True, 3
        //Planar, 4111, True, 0
        //Compression, 4107, True, 0
        //Bit Depth Reduction, 117009, False, 0
        //Halftone, 117002, False, 0
        //Actual X Resolution, 117010, False, 300
        //Actual Y Resolution, 117011, False, 300
        //116743, 116743, True, MF4100
    }
}
