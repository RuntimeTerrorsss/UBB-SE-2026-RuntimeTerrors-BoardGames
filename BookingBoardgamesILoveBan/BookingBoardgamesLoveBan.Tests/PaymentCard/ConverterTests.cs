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
            var converter = new BoolToVisibilityConverter();
            var result = converter.Convert(true, typeof(Visibility), null, null);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void BoolToVisibility_False_ReturnsCollapsed()
        {
            var converter = new BoolToVisibilityConverter();
            var result = converter.Convert(false, typeof(Visibility), null, null);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void InverseBoolToVisibility_True_ReturnsCollapsed()
        {
            var converter = new InverseBoolToVisibilityConverter();
            var result = converter.Convert(true, typeof(Visibility), null, null);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void InverseBoolToVisibility_False_ReturnsVisible()
        {
            var converter = new InverseBoolToVisibilityConverter();
            var result = converter.Convert(false, typeof(Visibility), null, null);
            Assert.Equal(Visibility.Visible, result);
        }
    }
}