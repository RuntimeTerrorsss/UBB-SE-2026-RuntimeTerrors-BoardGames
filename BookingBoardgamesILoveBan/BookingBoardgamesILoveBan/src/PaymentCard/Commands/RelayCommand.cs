using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.Commands
{
    /// <summary>
    /// RelayCommand is aICommand implementation that can expose a method or delegate to the view.
    /// These types act as a way to bind commands between the viewmodel and UI elements.
    /// It exists in winUi packages, but for some reason, I could not get a hold of those, so I had to improvise
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => execute();

        public void NotifyCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
