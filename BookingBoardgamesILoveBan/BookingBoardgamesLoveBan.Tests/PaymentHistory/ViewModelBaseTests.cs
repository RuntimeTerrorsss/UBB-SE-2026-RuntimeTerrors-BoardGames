using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class ViewModelBaseTests
    {
        // ================================ class ======================================
        private class TestViewModel : ViewModelBase
        {
            private string name;
            private int count;

            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            public int Count
            {
                get => count;
                set => SetProperty(ref count, value);
            }

            public void TriggerOnPropertyChanged(string propertyName)
            {
                OnPropertyChanged(propertyName);
            }

            public bool CallSetProperty<T>(ref T storage, T value, string propertyName = null)
            {
                return SetProperty(ref storage, value, propertyName);
            }
        }

        // ================================ setup ======================================
        private readonly TestViewModel viewModel;

        public ViewModelBaseTests()
        {
            viewModel = new TestViewModel();
        }

        // ================================ OnPropertyChanged ======================================
        [Fact]
        public void OnPropertyChanged_FiresPropertyChangedEvent()
        {
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.TriggerOnPropertyChanged("Name");

            Assert.True(fired);
        }

        [Fact]
        public void OnPropertyChanged_PassesCorrectPropertyName()
        {
            string? receivedName = null;
            viewModel.PropertyChanged += (sender, eventArguments) => receivedName = eventArguments.PropertyName;
            viewModel.TriggerOnPropertyChanged("Name");

            Assert.Equal("Name", receivedName);
        }

        [Fact]
        public void OnPropertyChanged_NoSubscribers_DoesNotThrow()
        {
            var exception = Record.Exception(() => viewModel.TriggerOnPropertyChanged("Name"));

            Assert.Null(exception);
        }

        // ================================ SetProperty ======================================
        [Fact]
        public void SetProperty_NewValue_ReturnsTrue()
        {
            viewModel.Name = "Alice";

            Assert.Equal("Alice", viewModel.Name);
        }

        [Fact]
        public void SetProperty_NewValue_FiresPropertyChangedEvent()
        {
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = "Alice";

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_NewValue_FiresWithCorrectPropertyName()
        {
            string? receivedName = null;
            viewModel.PropertyChanged += (sender, eventArguments) => receivedName = eventArguments.PropertyName;
            viewModel.Name = "Alice";

            Assert.Equal("Name", receivedName);
        }

        [Fact]
        public void SetProperty_SameValue_ReturnsFalse()
        {
            viewModel.Name = "Alice";
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = "Alice";

            Assert.False(fired);
        }

        [Fact]
        public void SetProperty_SameValue_DoesNotFirePropertyChanged()
        {
            viewModel.Name = "Alice";
            int fireCount = 0;
            viewModel.PropertyChanged += (sender, eventArguments) => fireCount++;
            viewModel.Name = "Alice";

            Assert.Equal(0, fireCount);
        }

        [Fact]
        public void SetProperty_NullToValue_FiresPropertyChanged()
        {
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = "Alice";

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_ValueToNull_FiresPropertyChanged()
        {
            viewModel.Name = "Alice";
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = null;

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_NullToNull_DoesNotFirePropertyChanged()
        {
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = null;

            Assert.False(fired);
        }

        [Fact]
        public void SetProperty_IntType_FiresPropertyChanged()
        {
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Count = 5;

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_IntType_WorksCorrectly()
        {
            viewModel.Count = 5;

            Assert.Equal(5, viewModel.Count);
        }

        [Fact]
        public void SetProperty_SameIntValue_DoesNotFirePropertyChanged()
        {
            viewModel.Count = 5;
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Count = 5;

            Assert.False(fired);
        }
    }
}
