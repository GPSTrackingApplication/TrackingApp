using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TrackingApp
{
    class ValuePorgressBarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if 30 sec is your maximum time
            //return (double)value / 30;

            //if 60 sec if your maximum time
            return (double)value / 60;

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
