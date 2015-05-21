using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;

namespace Catfood.Utils
{
    /// <summary>
    /// Provides access to the current user's default browser
    /// </summary>
    public static class DefaultBrowser
    {
        private static string _defaultBrowserPath;

        /// <summary>
        /// Opens a URL with the user's default browser
        /// </summary>
        /// <param name="url">URL to open</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void OpenUrl(string url)
        {
            if ((string.IsNullOrEmpty(url)) || (!Uri.IsWellFormedUriString(url, UriKind.Absolute)))
            {
                return;
            }

            try
            {
                // find the browser if necessary
                if (_defaultBrowserPath == null)
                {
                    SetBrowserPath();
                }

                if (_defaultBrowserPath == null)
                {
                    // can't find the default browser, try to launch without
                    Process.Start(url);
                }
                else
                {
                    // launch using the default browser
                    Process.Start(_defaultBrowserPath, url);
                }
            }
            catch 
            {
                
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void SetBrowserPath()
        {
            RegistryKey userDefault = null;
            RegistryKey systemDefault = null;
            RegistryKey browserPath = null;

            try
            {
                string defaultKey = null;

                // if the user has set a default browser it's in HKCU
                userDefault = Registry.CurrentUser.OpenSubKey(@"Software\Clients\StartMenuInternet");
                if (userDefault != null)
                {
                    defaultKey = userDefault.GetValue(string.Empty) as string;
                }
                else
                {
                    // if not then we need to look in HKLM
                    systemDefault = Registry.LocalMachine.OpenSubKey(@"Software\Clients\StartMenuInternet");
                    if (systemDefault != null)
                    {
                        defaultKey = systemDefault.GetValue(string.Empty) as string;
                    }
                }

                //  use the value form HKCU or HKLM to get the path to the browser
                if (defaultKey != null)
                {
                    string browserPathKey = string.Format(CultureInfo.InvariantCulture,
                        @"Software\Clients\StartMenuInternet\{0}\shell\open\command",
                        defaultKey);

                    browserPath = Registry.LocalMachine.OpenSubKey(browserPathKey);
                    if (browserPath != null)
                    {
                        _defaultBrowserPath = browserPath.GetValue(string.Empty) as string;
                        if (_defaultBrowserPath != null)
                        {
                            // remove any quotes from the path
                            _defaultBrowserPath = _defaultBrowserPath.Trim(new char[] { '"' });
                        }
                    }
                }
            }
            catch
            {
                
            }
            finally
            {
                if (userDefault != null)
                {
                    userDefault.Close();
                    userDefault = null;
                }

                if (systemDefault != null)
                {
                    systemDefault.Close();
                    systemDefault = null;
                }

                if (browserPath != null)
                {
                    browserPath.Close();
                    browserPath = null;
                }
            }
        }
    }
}
