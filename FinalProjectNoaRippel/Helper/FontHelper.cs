using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
// קובץ עזר המכיל קבועים וקונבקטורים שמשמשים לכלהאפלקציה
namespace FinalProjectNoaRippel.Helper
{
    public static class FontHelper
    {
        public const string CLOSED_EYE_ICON = "eye_open.png";
        public const string OPEN_EYE_ICON = "eye_close.png";
    }
    public class BoolToStrikethroughConverter : IValueConverter
        //מתקשר ל  RecipePageViewModel
        //שלוחצים על המילה זה משנההולך לכאן בשביל לשנות מF לT
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool b && b ? TextDecorations.Strikethrough : TextDecorations.None;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
    //קונברטור שממיר מחרוזת לתמונה+משמש להצגת תמונות קטגוריות ומתכונים שנשמרו
    public class Base64ToImageConverter : IValueConverter
    {
        //  ההמרה עצמה מבייס 64 ללתמונה
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string source && !string.IsNullOrEmpty(source))
            {
                System.Diagnostics.Debug.WriteLine($"Converting image, length: {source.Length}, first chars: {source.Substring(0, Math.Min(20, source.Length))}");

                //מוודא שיש צורך בהמרה ולא מדובר בקובץ תמונהרגיל
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

        //לא באמת עושה משוה אבל אם לא נכתוב את זה האינטרפייס יכתוב שגיאה
        //המרה בחזרה אבל אין שימוש ולא נדרשת אם בטעות אני אקרא לזה זה יחזיר טעיות
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
