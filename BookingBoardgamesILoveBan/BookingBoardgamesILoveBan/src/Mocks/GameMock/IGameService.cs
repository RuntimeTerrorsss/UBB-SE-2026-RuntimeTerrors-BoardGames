using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Mocks.GameMock
{
    public interface IGameService
    {
        public Game GetById(int id);

        public decimal GetPriceGameById(int gameId);
    }
}
