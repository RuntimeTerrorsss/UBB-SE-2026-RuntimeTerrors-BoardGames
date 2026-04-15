using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Model;

namespace BookingBoardgamesILoveBan.Src.Mocks.UserMock
{
    public interface IUserService
    {
        public User GetById(int id);

        public void SaveAddress(int id, Address address);
        public decimal GetUserBalance(int userId);
        public void UpdateBalance(int userId, decimal newBalance);
    }
}
