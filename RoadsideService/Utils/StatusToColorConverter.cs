using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RoadsideService.Utils
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "Свободно":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); // Green
                    case "Занято":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")); // Red
                    case "На уборке":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")); // Orange
                    case "На брони":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEB3B")); // Yellow
                    default:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E")); // Gray
                }
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E")); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
