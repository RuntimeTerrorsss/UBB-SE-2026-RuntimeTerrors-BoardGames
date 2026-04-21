using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BookingBoardgamesILoveBan.Src.Interface.View
{
    public sealed partial class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            if (value is bool boolValue && boolValue)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            return value is Visibility.Visible;
        }
    }
}