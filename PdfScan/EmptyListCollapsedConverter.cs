using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Catfood.Utils.Xaml.ValueConverters
{
    /// <summary>
    /// Converts an IList to Visibility.Collapsed if empty, Visibility.Visible otherwise
    /// </summary>
    public class EmptyListCollapsedConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = Visibility.Collapsed;

            IList ilist = value as IList;
            if (ilist != null)
            {
                if (ilist.Count > 0)
                {
                    visibility = Visibility.Visible;
                }
            }

            return visibility;
        }

        /// <summary />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
