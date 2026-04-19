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
        // ================================ RelayCommand ======================================
        [Fact]
        public void RelayCommand_NullExecute_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<int>(null));
        }

        [Fact]
        public void RelayCommand_Execute_CallsAction()
        {
            bool called = false;
            var command = new RelayCommand<int>(_ => called = true);

            command.Execute(1);

            Assert.True(called);
        }

        [Fact]
        public void RelayCommand_Execute_PassesParameterCorrectly()
        {
            int received = 0;
            var command = new RelayCommand<int>(value => received = value);

            command.Execute(42);

            Assert.Equal(42, received);
        }

        [Fact]
        public void RelayCommand_CanExecute_NoPredicate_ReturnsTrue()
        {
            var command = new RelayCommand<int>(_ => { });

            Assert.True(command.CanExecute(1));
        }

        [Fact]
        public void RelayCommand_CanExecute_PredicateReturnsTrue_ReturnsTrue()
        {
            var command = new RelayCommand<int>(_ => { }, _ => true);

            Assert.True(command.CanExecute(1));
        }

        [Fact]
        public void RelayCommand_CanExecute_PredicateReturnsFalse_ReturnsFalse()
        {
            var command = new RelayCommand<int>(_ => { }, _ => false);

            Assert.False(command.CanExecute(1));
        }

        [Fact]
        public void RelayCommand_RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new RelayCommand<int>(_ => { });
            bool eventFired = false;
            command.CanExecuteChanged += (s, e) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }

        [Fact]
        public void RelayCommand_RaiseCanExecuteChanged_NoSubscribers_DoesNotThrow()
        {
            var command = new RelayCommand<int>(_ => { });
            var exception = Record.Exception(() => command.RaiseCanExecuteChanged());

            Assert.Null(exception);
        }

        // ================================ RelayCommandNoParam ======================================
        [Fact]
        public void RelayCommandNoParam_NullExecute_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommandNoParam(null));
        }

        [Fact]
        public void RelayCommandNoParam_Execute_CallsAction()
        {
            bool called = false;
            var command = new RelayCommandNoParam(() => called = true);

            command.Execute(null);

            Assert.True(called);
        }

        [Fact]
        public void RelayCommandNoParam_CanExecute_NoFunc_ReturnsTrue()
        {
            var command = new RelayCommandNoParam(() => { });

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void RelayCommandNoParam_CanExecute_FuncReturnsTrue_ReturnsTrue()
        {
            var command = new RelayCommandNoParam(() => { }, () => true);

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void RelayCommandNoParam_CanExecute_FuncReturnsFalse_ReturnsFalse()
        {
            var command = new RelayCommandNoParam(() => { }, () => false);

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void RelayCommandNoParam_RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new RelayCommandNoParam(() => { });
            bool eventFired = false;
            command.CanExecuteChanged += (s, e) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }

        [Fact]
        public void RelayCommandNoParam_RaiseCanExecuteChanged_NoSubscribers_DoesNotThrow()
        {
            var command = new RelayCommandNoParam(() => { });
            var exception = Record.Exception(() => command.RaiseCanExecuteChanged());

            Assert.Null(exception);
        }

        [Fact]
        public void RelayCommandNoParam_Execute_IgnoresParameter()
        {
            bool called = false;
            var command = new RelayCommandNoParam(() => called = true);

            command.Execute("ignored parameter");

            Assert.True(called);
        }
    }
}
