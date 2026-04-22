using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class RelayCommandTests
    {
        [Fact]
        public void Constructor_WhenExecuteIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<int>(null));
        }

        [Fact]
        public void Execute_WhenCalled_CallsAction()
        {
            bool isCalled = false;
            var command = new RelayCommand<int>(_ => isCalled = true);

            command.Execute(1);

            Assert.True(isCalled);
        }

        [Fact]
        public void Execute_WhenCalled_PassesParameterCorrectly()
        {
            int receivedParameter = 0;
            var command = new RelayCommand<int>(value => receivedParameter = value);

            command.Execute(28);

            Assert.Equal(28, receivedParameter);
        }

        [Fact]
        public void CanExecute_WhenNoPredicateProvided_ReturnsTrue()
        {
            var command = new RelayCommand<int>(_ => { });

            Assert.True(command.CanExecute(1));
        }

        [Fact]
        public void CanExecute_WhenPredicateReturnsTrue_ReturnsTrue()
        {
            var command = new RelayCommand<int>(_ => { }, _ => true);

            Assert.True(command.CanExecute(1));
        }

        [Fact]
        public void CanExecute_WhenPredicateReturnsFalse_ReturnsFalse()
        {
            var command = new RelayCommand<int>(_ => { }, _ => false);

            Assert.False(command.CanExecute(1));
        }

        [Fact]
        public void RaiseCanExecuteChanged_WhenCalled_FiresEvent()
        {
            var command = new RelayCommand<int>(_ => { });
            bool eventFired = false;
            command.CanExecuteChanged += (s, e) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }

        [Fact]
        public void RaiseCanExecuteChanged_WhenNoSubscribers_DoesNotThrow()
        {
            var command = new RelayCommand<int>(_ => { });
            var exception = Record.Exception(() => command.RaiseCanExecuteChanged());

            Assert.Null(exception);
        }
    }

    public class RelayCommandNoParamTests
    {
        [Fact]
        public void Constructor_WhenExecuteIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommandNoParam(null));
        }

        [Fact]
        public void Execute_WhenCalled_CallsAction()
        {
            bool isCalled = false;
            var command = new RelayCommandNoParam(() => isCalled = true);

            command.Execute(null);

            Assert.True(isCalled);
        }

        [Fact]
        public void CanExecute_WhenNoPredicateProvided_ReturnsTrue()
        {
            var command = new RelayCommandNoParam(() => { });

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WhenPredicateReturnsTrue_ReturnsTrue()
        {
            var command = new RelayCommandNoParam(() => { }, () => true);

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WhenPredicateReturnsFalse_ReturnsFalse()
        {
            var command = new RelayCommandNoParam(() => { }, () => false);

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void RaiseCanExecuteChanged_WhenCalled_FiresEvent()
        {
            var command = new RelayCommandNoParam(() => { });
            bool eventFired = false;
            command.CanExecuteChanged += (s, e) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }

        [Fact]
        public void RaiseCanExecuteChanged_WhenNoSubscribers_DoesNotThrow()
        {
            var command = new RelayCommandNoParam(() => { });
            var exception = Record.Exception(() => command.RaiseCanExecuteChanged());

            Assert.Null(exception);
        }

        [Fact]
        public void Execute_WhenCalled_IgnoresParameter()
        {
            bool isCalled = false;
            var command = new RelayCommandNoParam(() => isCalled = true);

            command.Execute("ignored parameter");

            Assert.True(isCalled);
        }
    }
}
