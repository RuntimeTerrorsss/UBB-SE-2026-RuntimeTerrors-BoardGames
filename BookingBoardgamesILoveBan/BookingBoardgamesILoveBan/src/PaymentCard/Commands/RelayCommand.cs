using System;
using System.Windows.Input;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action executeAction;
        private readonly Func<bool> canExecuteFunction;

        public RelayCommand(Action executeAction, Func<bool> canExecuteFunction = null)
        {
            this.executeAction = executeAction;
            this.canExecuteFunction = canExecuteFunction;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => canExecuteFunction?.Invoke() ?? true;
        public void Execute(object parameter) => executeAction();

        public void NotifyCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}