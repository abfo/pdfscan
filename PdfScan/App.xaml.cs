using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Catfood.Utils.Xaml;
using Catfood.Utils;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace PdfScan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UserSettings _settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            _settings = UserSettings.Settings;
            _settings.Load();
            _settings.LogMessage(LogSeverity.Information, "OnStartup");

            if (!_settings.SettingsUpgraded)
            {
                // migrate over settings
                PdfScan.Properties.Settings.Default.Reload();
                _settings.InitialRunCount = PdfScan.Properties.Settings.Default.InitialRunCount;
                

                _settings.SettingsUpgraded = true;

                try
                {
                    _settings.Save();
                }
                catch (Exception ex)
                {
                    _settings.LogException(LogSeverity.Warning, "Failed to save settings after migrating to new format", ex);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _settings.Save();
            }
            catch (Exception ex)
            {
                _settings.LogException(LogSeverity.Error, "OnExit: Failed to save settings", ex);
            }

            _settings.LogMessage(LogSeverity.Information,
                string.Format(CultureInfo.InvariantCulture,
                "OnExit, Exit Code = {0}",
                e.ApplicationExitCode));

            this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            base.OnExit(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowExceptionAndDie(e.ExceptionObject as Exception);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowExceptionAndDie(e.Exception);
            e.Handled = true;
        }

        private void ShowExceptionAndDie(Exception ex)
        {
            try
            {
                _settings.LogException(LogSeverity.Error,
                    "Unhandled Exception",
                    ex);

                CatfoodMessageBox.Show(new BitmapImage(new Uri("pack://application:,,,/PdfScan.ico")),
                    MainWindow,
                   "Something has gone horribly wrong and Catfood PdfScan will now close. Sorry about that. Please report the error so that it gets fixed.",
                    "Fatal Error - Catfood PdfScan",
                    CatfoodMessageBoxType.Ok,
                    CatfoodMessageBoxIcon.Error,
                    ex);
            }
            catch
            {

            }
            finally
            {
                Shutdown(1);
            }
        }
    }
}
