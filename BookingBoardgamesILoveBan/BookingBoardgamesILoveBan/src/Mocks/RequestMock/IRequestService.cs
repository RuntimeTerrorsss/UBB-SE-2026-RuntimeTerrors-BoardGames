namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public interface IRequestService
    {
        public Request GetRequestById(int requestId);
        public decimal GetRequestPrice(int requestId);
        public string GetGameName(int requestId);
    }
}