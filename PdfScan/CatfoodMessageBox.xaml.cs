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
using System.Globalization;
using System.Windows.Interop;

namespace Catfood.Utils.Xaml
{
    /// <summary>
    /// Possible result values from a CatfoodMessageBox
    /// </summary>
    public enum CatfoodMessageBoxResult
    {
        /// <summary>
        /// Ok
        /// </summary>
        Ok,

        /// <summary>
        /// Cancel
        /// </summary>
        Cancel,

        /// <summary>
        /// Yes
        /// </summary>
        Yes,

        /// <summary>
        /// No
        /// </summary>
        No,
    }

    /// <summary>
    /// The type of Catfood Message Box to show
    /// </summary>
    public enum CatfoodMessageBoxType
    {
        /// <summary>
        /// Ok
        /// </summary>
        Ok,

        /// <summary>
        /// OkCancel
        /// </summary>
        OkCancel,

        /// <summary>
        /// YesNo
        /// </summary>
        YesNo,
    }

    /// <summary>
    /// The icon to show with the Catfood Message Box
    /// </summary>
    public enum CatfoodMessageBoxIcon
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
        /// Question
        /// </summary>
        Question,

        /// <summary>
        /// Error
        /// </summary>
        Error,
    }

    /// <summary>
    /// Catfood Message Box
    /// </summary>
    public partial class CatfoodMessageBox : Window
    {
        
        /// <summary>
        /// Gets or sets the icon to show with the Catfood Message Box
        /// </summary>
        public CatfoodMessageBoxIcon MessageBoxIcon { get; set; }

        /// <summary>
        /// Gets or sets the type of the Catfood Message Box
        /// </summary>
        public CatfoodMessageBoxType MessageBoxType { get; set; }

        /// <summary>
        /// Gets the result of the Catfood Message Box
        /// </summary>
        public CatfoodMessageBoxResult MessageBoxResult { get; private set; }

        /// <summary>
        /// Gets or sets the icon for the window
        /// </summary>
        public ImageSource WindowIcon { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string WindowMessage { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// Gets or sets the exception associated with the message box
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Catfood Message Box
        /// </summary>
        public CatfoodMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows a CatfoodMessageBox with an Ok button and an Information icon
        /// </summary>
        /// <param name="icon">Icon for the window</param>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Message</param>
        /// <param name="title">Title</param>
        /// <returns>MessageBoxResult</returns>
        public static CatfoodMessageBoxResult Show(ImageSource icon,
            Window owner,
            string message,
            string title)
        {
            return Show(icon,
                owner,
                message,
                title,
                CatfoodMessageBoxType.Ok,
                CatfoodMessageBoxIcon.Information,
                null);
        }

        /// <summary>
        /// Shows a CatfoodMessageBox
        /// </summary>
        /// <param name="icon">Icon for the window</param>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Message</param>
        /// <param name="title">Title</param>
        /// <param name="messageBoxType">Type of message box</param>
        /// <param name="messageBoxIcon">Icon to show</param>
        /// <returns>MessageBoxResult</returns>
        public static CatfoodMessageBoxResult Show(ImageSource icon,
            Window owner,
            string message,
            string title,
            CatfoodMessageBoxType messageBoxType,
            CatfoodMessageBoxIcon messageBoxIcon)
        {
            return Show(icon,
                owner,
                message,
                title,
                messageBoxType,
                messageBoxIcon,
                null);
        }

        /// <summary>
        /// Shows a CatfoodMessageBox
        /// </summary>
        /// <param name="icon">Icon for the window</param>
        /// <param name="owner">Parent window</param>
        /// <param name="message">Message</param>
        /// <param name="title">Title</param>
        /// <param name="messageBoxType">Type of message box</param>
        /// <param name="messageBoxIcon">Icon to show</param>
        /// <param name="exception">Associated exception</param>
        /// <returns>MessageBoxResult</returns>
        public static CatfoodMessageBoxResult Show(ImageSource icon,
            Window owner,
            string message,
            string title,
            CatfoodMessageBoxType messageBoxType,
            CatfoodMessageBoxIcon messageBoxIcon,
            Exception exception)
        {
            //if (icon == null) { throw new ArgumentNullException("icon"); }
            if (message == null) { throw new ArgumentNullException("message"); }
            if (title == null) { throw new ArgumentNullException("title"); }

            CatfoodMessageBox box = new CatfoodMessageBox();

            box.MessageBoxIcon = messageBoxIcon;
            box.MessageBoxType = messageBoxType;
            box.WindowIcon = icon;
            box.WindowMessage = message;
            box.WindowTitle = title;
            box.Exception = exception;

            if ((owner == null) || (!owner.IsLoaded))
            {
                box.ShowInTaskbar = true;
                box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                box.Owner = owner;
            }

            box.ShowDialog();

            return box.MessageBoxResult;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MessageBoxResult = CatfoodMessageBoxResult.Ok;

            this.Icon = this.WindowIcon;
            this.Title = this.WindowTitle;
            textBlockMessage.Text = this.WindowMessage;

            switch (this.MessageBoxType)
            {
                case CatfoodMessageBoxType.Ok:
                    button2.Visibility = Visibility.Collapsed;
                    button1.Content = "OK";
                    button1.IsDefault = true;
                    break;

                case CatfoodMessageBoxType.OkCancel:
                    button2.Content = "OK";
                    button2.IsDefault = true;
                    button1.Content = "Cancel";
                    button1.IsCancel = true;
                    break;

                case CatfoodMessageBoxType.YesNo:
                    button2.Content = "Yes";
                    button2.IsDefault = true;
                    button1.Content = "No";
                    button2.IsCancel = true;
                    break;
            }

            switch (this.MessageBoxIcon)
            {
                case CatfoodMessageBoxIcon.Information:
                    System.Media.SystemSounds.Asterisk.Play();
                    imageBox.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Information.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    break;

                case CatfoodMessageBoxIcon.Question:
                    System.Media.SystemSounds.Question.Play();
                    imageBox.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Question.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    break;

                case CatfoodMessageBoxIcon.Warning:
                    System.Media.SystemSounds.Exclamation.Play();
                    imageBox.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Warning.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    break;

                case CatfoodMessageBoxIcon.Error:
                    System.Media.SystemSounds.Hand.Play();
                    imageBox.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    break;
            }

            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            switch (this.MessageBoxType)
            {
                case CatfoodMessageBoxType.Ok:
                    // shouldn't happen
                    break;

                case CatfoodMessageBoxType.OkCancel:
                    this.MessageBoxResult = CatfoodMessageBoxResult.Ok;
                    break;

                case CatfoodMessageBoxType.YesNo:
                    this.MessageBoxResult = CatfoodMessageBoxResult.Yes;
                    break;
            }

            Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch (this.MessageBoxType)
            {
                case CatfoodMessageBoxType.Ok:
                    this.MessageBoxResult = CatfoodMessageBoxResult.Ok;
                    break;

                case CatfoodMessageBoxType.OkCancel:
                    this.MessageBoxResult = CatfoodMessageBoxResult.Cancel;
                    break;

                case CatfoodMessageBoxType.YesNo:
                    this.MessageBoxResult = CatfoodMessageBoxResult.No;
                    break;
            }

            Close();
        }
    }
}
