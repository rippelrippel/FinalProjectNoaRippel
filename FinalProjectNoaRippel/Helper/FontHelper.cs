using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FinalProjectNoaRippel.Helper
{
    public static class FontHelper
    {
        public const string CLOSED_EYE_ICON = "eye_open.png";
        public const string OPEN_EYE_ICON = "eye_close.png";
    }
    public class BoolToStrikethroughConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool b && b ? TextDecorations.Strikethrough : TextDecorations.None;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class Base64ToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string source && !string.IsNullOrEmpty(source))
            {
                System.Diagnostics.Debug.WriteLine($"Converting image, length: {source.Length}, first chars: {source.Substring(0, Math.Min(20, source.Length))}");

                if (!source.Contains("/") && source.Length < 100)
                    return ImageSource.FromFile(source);

                try
                {
                    var bytes = System.Convert.FromBase64String(source);
                    System.Diagnostics.Debug.WriteLine($"Bytes length: {bytes.Length}");
                    return ImageSource.FromStream(() => new MemoryStream(bytes));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                    return ImageSource.FromFile(source);
                }
            }
            System.Diagnostics.Debug.WriteLine("Value is null or empty");
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
