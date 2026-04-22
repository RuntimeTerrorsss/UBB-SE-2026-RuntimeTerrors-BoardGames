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

            if (request == null)
            {
                return 0m;
            }

            int totalDays = (request.EndDate - request.StartDate).Days;
            int billedDays = Math.Max(1, totalDays);

            var pricePerDay = gameRepository.GetPriceGameById(request.GameId);

            return pricePerDay * billedDays;
        }

        public string GetGameName(int requestId)
        {
            var request = requestRepository.GetById(requestId);
            if (request == null)
            {
                return "Unknown Request";
            }

            var game = gameRepository.GetById(request.GameId);
            if (game == null)
            {
                return "Unknown Game";
            }

            return game.Name;
        }
    }
}