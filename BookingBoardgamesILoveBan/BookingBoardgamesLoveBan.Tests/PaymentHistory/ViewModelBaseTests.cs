using BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class ViewModelBaseTests
    {
        // ================================ class ======================================
        private class TestViewModel : ViewModelBase
        {
            private string _name;
            private int _count;

            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            public int Count
            {
                get => _count;
                set => SetProperty(ref _count, value);
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

        // ================================ OnPropertyChanged ======================================

        [Fact]
        public void OnPropertyChanged_FiresPropertyChangedEvent()
        {
            var vm = new TestViewModel();
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.TriggerOnPropertyChanged("Name");

            Assert.True(fired);
        }

        [Fact]
        public void OnPropertyChanged_PassesCorrectPropertyName()
        {
            var vm = new TestViewModel();
            string receivedName = null;
            vm.PropertyChanged += (s, e) => receivedName = e.PropertyName;

            vm.TriggerOnPropertyChanged("Name");

            Assert.Equal("Name", receivedName);
        }

        [Fact]
        public void OnPropertyChanged_NoSubscribers_DoesNotThrow()
        {
            var vm = new TestViewModel();

            var exception = Record.Exception(() => vm.TriggerOnPropertyChanged("Name"));

            Assert.Null(exception);
        }

        // ================================ SetProperty ======================================

        [Fact]
        public void SetProperty_NewValue_ReturnsTrue()
        {
            var vm = new TestViewModel();

            vm.Name = "Alice";

            Assert.Equal("Alice", vm.Name);
        }

        [Fact]
        public void SetProperty_NewValue_FiresPropertyChangedEvent()
        {
            var vm = new TestViewModel();
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.Name = "Alice";

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_NewValue_FiresWithCorrectPropertyName()
        {
            var vm = new TestViewModel();
            string receivedName = null;
            vm.PropertyChanged += (s, e) => receivedName = e.PropertyName;

            vm.Name = "Alice";

            Assert.Equal("Name", receivedName);
        }

        [Fact]
        public void SetProperty_SameValue_ReturnsFalse()
        {
            var vm = new TestViewModel();
            vm.Name = "Alice";
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.Name = "Alice"; // same value again

            Assert.False(fired);
        }

        [Fact]
        public void SetProperty_SameValue_DoesNotFirePropertyChanged()
        {
            var vm = new TestViewModel();
            vm.Name = "Alice";
            int fireCount = 0;
            vm.PropertyChanged += (s, e) => fireCount++;

            vm.Name = "Alice";

            Assert.Equal(0, fireCount);
        }

        [Fact]
        public void SetProperty_NullToValue_FiresPropertyChanged()
        {
            var vm = new TestViewModel();
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.Name = "Alice";

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_ValueToNull_FiresPropertyChanged()
        {
            var vm = new TestViewModel();
            vm.Name = "Alice";
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.Name = null;

            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_NullToNull_DoesNotFirePropertyChanged()
        {
            var vm = new TestViewModel();
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.Name = null; // already null by default

            Assert.False(fired);
        }

        [Fact]
        public void SetProperty_IntType_WorksCorrectly()
        {
            var vm = new TestViewModel();
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.Count = 5;

            Assert.Equal(5, vm.Count);
            Assert.True(fired);
        }

        [Fact]
        public void SetProperty_SameIntValue_DoesNotFirePropertyChanged()
        {
            var vm = new TestViewModel();
            vm.Count = 5;
            bool fired = false;
            vm.PropertyChanged += (s, e) => fired = true;

            vm.Count = 5;

            Assert.False(fired);
        }
    }
}
