using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Mocks.GameMock
{
    public class Game
    {
        public Game(int id, string name, decimal pricePerDay)
        {
            Gid = id;
            Name = name;
            PricePerDay = pricePerDay;
        }

        public int Gid { get; set; }

        public string Name { get; set; }

        public decimal PricePerDay { get; set; }
    }
}