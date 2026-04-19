namespace BookingBoardgamesILoveBan.Src.Mocks.GameMock
{
    public interface IGameRepository
    {
        public Game GetById(int id);

        public decimal GetPriceGameById(int gameId);
    }
}
