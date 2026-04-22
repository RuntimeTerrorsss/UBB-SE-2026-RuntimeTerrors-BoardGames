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
        private class TestViewModel : ViewModelBase // it is an abstract class
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
        private TestViewModel viewModel;

        private TestViewModel InitializeViewModelBase()
        {
            viewModel = new TestViewModel();
            return viewModel;
        }

        // ================================ OnPropertyChanged ======================================
        [Fact]
        public void OnPropertyChanged_WhenCalled_FiresPropertyChangedEvent()
        {
            var viewModel = InitializeViewModelBase();

            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.TriggerOnPropertyChanged("Name");

            Assert.True(fired);
        }

        [Fact]
        public void OnPropertyChanged_WhenCalled_PassesCorrectPropertyName()
        {
            var viewModel = InitializeViewModelBase();

            string? receivedName = null;
            viewModel.PropertyChanged += (sender, eventArguments) => receivedName = eventArguments.PropertyName;
            viewModel.TriggerOnPropertyChanged("Name");

            Assert.Equal("Name", receivedName);
        }

        [Fact]
        public void OnPropertyChanged_NoSubscribers_DoesNotThrow()
        {
            var viewModel = InitializeViewModelBase();

            var exception = Record.Exception(() => viewModel.TriggerOnPropertyChanged("Name"));

            Assert.Null(exception);
        }

        // ================================ SetProperty ======================================
        [Fact]
        public void SetProperty_NewValue_ReturnsTrue()
        {
            var viewModel = InitializeViewModelBase();

            viewModel.Name = "Alice";

            Assert.Equal("Alice", viewModel.Name);
        }

        [Fact]
        public void SetProperty_NewValue_FiresPropertyChangedEvent()
        {
            var viewModel = InitializeViewModelBase();

            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = "Alice";

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_NewValue_FiresWithCorrectPropertyName()
        {
            var viewModel = InitializeViewModelBase();

            string? receivedName = null;
            viewModel.PropertyChanged += (sender, eventArguments) => receivedName = eventArguments.PropertyName;
            viewModel.Name = "Alice";

            Assert.Equal("Name", receivedName);
        }

        [Fact]
        public void SetProperty_SameValue_ReturnsFalse()
        {
            var viewModel = InitializeViewModelBase();

            viewModel.Name = "Alice";
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = "Alice";

            Assert.False(fired);
        }

        [Fact]
        public void SetProperty_SameValue_DoesNotFirePropertyChanged()
        {
            var viewModel = InitializeViewModelBase();

            viewModel.Name = "Alice";
            int fireCount = 0;
            viewModel.PropertyChanged += (sender, eventArguments) => fireCount++;
            viewModel.Name = "Alice";

            Assert.Equal(0, fireCount);
        }

        [Fact]
        public void SetProperty_NullToValue_FiresPropertyChanged()
        {
            var viewModel = InitializeViewModelBase();

            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = "Alice";

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_ValueToNull_FiresPropertyChanged()
        {
            var viewModel = InitializeViewModelBase();

            viewModel.Name = "Alice";
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = null;

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_NullToNull_DoesNotFirePropertyChanged()
        {
            var viewModel = InitializeViewModelBase();

            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Name = null;

            Assert.False(fired);
        }

        [Fact]
        public void SetProperty_IntType_FiresPropertyChanged()
        {
            var viewModel = InitializeViewModelBase();

            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Count = 5;

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_IntType_WorksCorrectly()
        {
            var viewModel = InitializeViewModelBase();

            viewModel.Count = 5;

            Assert.Equal(5, viewModel.Count);
        }

        [Fact]
        public void SetProperty_SameIntValue_DoesNotFirePropertyChanged()
        {
            var viewModel = InitializeViewModelBase();

            viewModel.Count = 5;
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) => fired = true;
            viewModel.Count = 5;

            Assert.False(fired);
        }
    }
}
