using System;
using Xunit;
using BookingBoardgamesILoveBan.Src.PaymentCard.Commands;

namespace BookingBoardgamesILoveBan.Tests.PaymentCard.Commands
{
    public class RelayCommandTests
    {
        [Fact]
        public void Execute_InvokesAction()
        {
            bool actionWasInvoked = false;
            var command = new RelayCommand(() => actionWasInvoked = true);

            command.Execute(null);

            Assert.True(actionWasInvoked);
        }

        [Fact]
        public void CanExecute_NoConditionProvided_ReturnsTrue()
        {
            var command = new RelayCommand(() => { });

            bool canExecute = command.CanExecute(null);

            Assert.True(canExecute);
        }

        [Fact]
        public void CanExecute_ConditionProvided_ReturnsConditionResult()
        {
            bool condition = false;
            var command = new RelayCommand(() => { }, () => condition);

            bool canExecuteFalse = command.CanExecute(null);

            condition = true;
            bool canExecuteTrue = command.CanExecute(null);

            Assert.False(canExecuteFalse);
            Assert.True(canExecuteTrue);
        }

        [Fact]
        public void NotifyCanExecuteChanged_FiresEvent()
        {
            var command = new RelayCommand(() => { });
            bool eventFired = false;
            command.CanExecuteChanged += (sender, args) => eventFired = true;

            command.NotifyCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}