using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Mocks.GameMock
{
	public class Game
	{
		public int Gid { get; set; }
		public string Name { get; set; }

        public decimal PricePerDay { get; set; }
        public Game(int id, string name, decimal pricePerDay)
        {
            this.Gid = id;
            this.Name = name;
            this.PricePerDay = pricePerDay;
        }
    }
}
