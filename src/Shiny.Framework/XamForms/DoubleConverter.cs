﻿using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Shiny.XamForms
{
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value == null)
                return null;

            var str = value as string;
            if (String.IsNullOrWhiteSpace(str))
                return null;

            if (Double.TryParse(str, out var dbl))
                return dbl;

            return null;
        }
    }
}
