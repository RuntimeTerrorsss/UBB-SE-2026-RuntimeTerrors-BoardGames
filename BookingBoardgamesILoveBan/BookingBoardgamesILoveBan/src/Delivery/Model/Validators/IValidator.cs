using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Delivery.Model.Validators
{
    public interface IValidator<T, E>
    {
        T Validate(E element);
    }
}
