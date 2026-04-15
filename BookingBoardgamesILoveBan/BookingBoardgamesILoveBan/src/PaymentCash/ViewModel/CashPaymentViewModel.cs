using System.ComponentModel;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Service;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.ViewModel
{
	public class CashPaymentViewModel
	{
		private ICashPaymentService service;
		private readonly IUserService userService;
		private readonly IRequestService requestService;
		private readonly IGameService gameService;
		private readonly ConversationService conversationService;

		public string OwnerName { get; set; }
		public string GameName { get; set; }
		public string DeliveryAddress { get; set; }
		public string RequestDates { get; set; }
		public string Amount { get; set; }

		private int rentalRequestMessageId;

		public CashPaymentViewModel(
			ICashPaymentService service,
			IUserService userService,
			IRequestService requestService,
			IGameService gameService,
			int requestId,
			string deliveryAddress,
			int messageId,
			ConversationService conversationService)
		{
			this.service = service;
			this.userService = userService;
			this.requestService = requestService;
			this.gameService = gameService;
			this.conversationService = conversationService;
			this.rentalRequestMessageId = messageId;

			Request request = this.requestService.GetById(requestId);
			Game game = this.gameService.GetById(request.GameId);
			User client = this.userService.GetById(request.ClientId);
			User owner = this.userService.GetById(request.OwnerId);

			this.OwnerName = owner.Username;
			this.GameName = game.Name;
			this.DeliveryAddress = deliveryAddress;
			this.RequestDates = request.StartDate.ToShortDateString() + " to " + request.EndDate.ToShortDateString();

			decimal amount = this.requestService.GetRequestPrice(requestId);
			this.Amount = amount.ToString();

			int paymentId = this.service.AddCashPayment(new CashPaymentDto(-1, requestId, client.Id, owner.Id, amount));
			this.conversationService.OnCashPaymentSelected(rentalRequestMessageId, paymentId);
		}
	}
}
