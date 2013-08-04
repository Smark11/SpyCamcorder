using SpyCamcorder.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SpyCamcorder.Converters
{
    public class LockScreenConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string returnvalue = string.Empty;

            if (value != null)
            {
                bool conVal = (bool)value;

                if (conVal)
                {
                    returnvalue = AppResources.LockDisabled;
                }
                else
                {
                    returnvalue = AppResources.LockEnabled;
                }
            }

            return returnvalue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
