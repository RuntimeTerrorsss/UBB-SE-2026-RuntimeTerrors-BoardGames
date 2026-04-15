using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => (value is bool booleanValue && booleanValue) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}