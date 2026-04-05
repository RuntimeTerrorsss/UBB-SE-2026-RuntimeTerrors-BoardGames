using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BookingBoardgamesILoveBan.src.Interface.View
{
    // partial is required — WinUI 3 mandates it for any class implementing a WinRT interface
    public sealed partial class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, global::System.Type targetType, object parameter, string language)
            => value is true ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, global::System.Type targetType, object parameter, string language)
            => value is Visibility.Visible;
    }
}