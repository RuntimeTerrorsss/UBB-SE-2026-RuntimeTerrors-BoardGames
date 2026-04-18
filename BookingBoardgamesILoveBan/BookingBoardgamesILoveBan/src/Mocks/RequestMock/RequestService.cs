using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository requestRepository;
        private readonly IGameRepository gameRepository;

        public RequestService(IRequestRepository requestRepository, IGameRepository gameRepository)
        {
            this.requestRepository = requestRepository;
            this.gameRepository = gameRepository;
        }

        public Request GetRequestById(int requestId)
        {
            return requestRepository.GetById(requestId);
        }

        public decimal GetRequestPrice(int requestId)
        {
            var request = requestRepository.GetById(requestId);
            int days = (request.EndDate - request.StartDate).Days;
            if (days == 0)
            {
                days = 1;
            }
            var price = gameRepository.GetPriceGameById(request.GameId);

            return price * days;
        }

        public string GetGameName(int requestId)
        {
            var request = requestRepository.GetById(requestId);
            return gameRepository.GetById(request.GameId).Name;
        }
    }
}