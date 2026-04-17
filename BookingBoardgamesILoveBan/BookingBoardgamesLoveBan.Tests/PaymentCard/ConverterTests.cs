using System;
using Microsoft.UI.Xaml;
using Xunit;
using BookingBoardgamesILoveBan.Src.PaymentCard.Converters;

namespace BookingBoardgamesILoveBan.Tests.PaymentCard.Converters
{
    public class ConverterTests
    {
        [Fact]
        public void BoolToVisibility_True_ReturnsVisible()
        {
            BoolToVisibilityConverter boolToVisibilityConverter = new BoolToVisibilityConverter();
            bool booleanValue = true;
            Type targetType = typeof(Visibility);
            object nullParameter = null;
            string nullLanguage = null;

            object conversionResult = boolToVisibilityConverter.Convert(booleanValue, targetType, nullParameter, nullLanguage);

            Assert.Equal(Visibility.Visible, conversionResult);
        }

        [Fact]
        public void BoolToVisibility_False_ReturnsCollapsed()
        {
            BoolToVisibilityConverter boolToVisibilityConverter = new BoolToVisibilityConverter();
            bool booleanValue = false;
            Type targetType = typeof(Visibility);
            object nullParameter = null;
            string nullLanguage = null;

            object conversionResult = boolToVisibilityConverter.Convert(booleanValue, targetType, nullParameter, nullLanguage);

            Assert.Equal(Visibility.Collapsed, conversionResult);
        }

        [Fact]
        public void InverseBoolToVisibility_True_ReturnsCollapsed()
        {
            InverseBoolToVisibilityConverter inverseBoolToVisibilityConverter = new InverseBoolToVisibilityConverter();
            bool booleanValue = true;
            Type targetType = typeof(Visibility);
            object nullParameter = null;
            string nullLanguage = null;

            object conversionResult = inverseBoolToVisibilityConverter.Convert(booleanValue, targetType, nullParameter, nullLanguage);

            Assert.Equal(Visibility.Collapsed, conversionResult);
        }

        [Fact]
        public void InverseBoolToVisibility_False_ReturnsVisible()
        {
            InverseBoolToVisibilityConverter inverseBoolToVisibilityConverter = new InverseBoolToVisibilityConverter();
            bool booleanValue = false;
            Type targetType = typeof(Visibility);
            object nullParameter = null;
            string nullLanguage = null;

            object conversionResult = inverseBoolToVisibilityConverter.Convert(booleanValue, targetType, nullParameter, nullLanguage);

            Assert.Equal(Visibility.Visible, conversionResult);
        }

        [Fact]
        public void BoolToVisibility_ConvertBack_Visible_ReturnsTrue()
        {
            BoolToVisibilityConverter boolToVisibilityConverter = new BoolToVisibilityConverter();
            Visibility visibilityValue = Visibility.Visible;
            Type targetType = typeof(bool);
            object nullParameter = null;
            string nullLanguage = null;

            object conversionResult = boolToVisibilityConverter.ConvertBack(visibilityValue, targetType, nullParameter, nullLanguage);

            Assert.True((bool)conversionResult);
        }

        [Fact]
        public void BoolToVisibility_ConvertBack_Collapsed_ReturnsFalse()
        {
            BoolToVisibilityConverter boolToVisibilityConverter = new BoolToVisibilityConverter();
            Visibility visibilityValue = Visibility.Collapsed;
            Type targetType = typeof(bool);
            object nullParameter = null;
            string nullLanguage = null;

            object conversionResult = boolToVisibilityConverter.ConvertBack(visibilityValue, targetType, nullParameter, nullLanguage);

            Assert.False((bool)conversionResult);
        }

        [Fact]
        public void BoolToVisibility_ConvertBack_Null_ReturnsFalse()
        {
            BoolToVisibilityConverter boolToVisibilityConverter = new BoolToVisibilityConverter();
            object nullVisibilityValue = null;
            Type targetType = typeof(bool);
            object nullParameter = null;
            string nullLanguage = null;

            object conversionResult = boolToVisibilityConverter.ConvertBack(nullVisibilityValue, targetType, nullParameter, nullLanguage);

            Assert.False((bool)conversionResult);
        }

        [Fact]
        public void InverseBoolToVisibility_ConvertBack_ThrowsNotImplementedException()
        {
            InverseBoolToVisibilityConverter inverseBoolToVisibilityConverter = new InverseBoolToVisibilityConverter();
            object nullVisibilityValue = null;
            Type targetType = typeof(bool);
            object nullParameter = null;
            string nullLanguage = null;

            Assert.Throws<NotImplementedException>(() =>
                inverseBoolToVisibilityConverter.ConvertBack(nullVisibilityValue, targetType, nullParameter, nullLanguage));
        }
    }
}