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
            RelayCommand executeRelayCommand = new RelayCommand(() => actionWasInvoked = true);
            object nullCommandParameter = null;

            executeRelayCommand.Execute(nullCommandParameter);

            Assert.True(actionWasInvoked);
        }

        [Fact]
        public void CanExecute_NoConditionProvided_ReturnsTrue()
        {
            RelayCommand canExecuteRelayCommand = new RelayCommand(() => { });
            object nullCommandParameter = null;

            bool canExecuteResult = canExecuteRelayCommand.CanExecute(nullCommandParameter);

            Assert.True(canExecuteResult);
        }

        [Fact]
        public void CanExecute_ConditionProvidedAndFalse_ReturnsFalse()
        {
            bool executeCondition = false;
            RelayCommand conditionalRelayCommand = new RelayCommand(() => { }, () => executeCondition);
            object nullCommandParameter = null;

            bool canExecuteResult = conditionalRelayCommand.CanExecute(nullCommandParameter);

            Assert.False(canExecuteResult);
        }

        [Fact]
        public void CanExecute_ConditionProvidedAndTrue_ReturnsTrue()
        {
            bool executeCondition = true;
            RelayCommand conditionalRelayCommand = new RelayCommand(() => { }, () => executeCondition);
            object nullCommandParameter = null;

            bool canExecuteResult = conditionalRelayCommand.CanExecute(nullCommandParameter);

            Assert.True(canExecuteResult);
        }

        [Fact]
        public void NotifyCanExecuteChanged_FiresEvent()
        {
            RelayCommand eventRelayCommand = new RelayCommand(() => { });
            bool commandEventFired = false;
            eventRelayCommand.CanExecuteChanged += (eventSender, eventArguments) => commandEventFired = true;

            eventRelayCommand.NotifyCanExecuteChanged();

            Assert.True(commandEventFired);
        }
    }
}