using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Shiny.XamForms
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => !(bool)value;

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => !(bool)value;
    }
}
