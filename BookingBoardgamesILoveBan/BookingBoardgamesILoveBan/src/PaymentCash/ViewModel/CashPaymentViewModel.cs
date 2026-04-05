using BookingBoardgamesILoveBan.src.Chat.Service;
using BookingBoardgamesILoveBan.src.Mocks.GameMock;
using BookingBoardgamesILoveBan.src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.src.Mocks.UserMock;
using BookingBoardgamesILoveBan.src.PaymentCash.Model;
using BookingBoardgamesILoveBan.src.PaymentCash.Service;
using System.ComponentModel;


namespace BookingBoardgamesILoveBan.src.PaymentCash.ViewModel
{
	public class CashPaymentViewModel
	{
		private ICashPaymentService _service;
		private readonly UserService _userService;
		private readonly RequestService _requestService;
		private readonly GameService _gameService;
		private readonly ConversationService _conversationService;

		public string OwnerName { get; set; }
		public string GameName { get; set; }
		public string DeliveryAddress { get; set; }
		public string RequestDates { get; set; }
		public string Amount { get; set; }

		private int _rentalRequestMessageId;

		public CashPaymentViewModel(
			ICashPaymentService service, 
			UserService userService, 
			RequestService requestService, 
			GameService gameService, 
			int requestId,
			string deliveryAddress,
			int messageId,
			ConversationService conversationService
		) {
			this._service = service;
			this._userService = userService;
			this._requestService = requestService;
			this._gameService = gameService;
			this._conversationService = conversationService;
			this._rentalRequestMessageId = messageId;

			Request request = this._requestService.GetById(requestId);
			Game game = this._gameService.GetById(request.GameId);
			User client = this._userService.GetById(request.ClientId);
			User owner = this._userService.GetById(request.OwnerId);

			this.OwnerName = owner.Username;
			this.GameName = game.Name;
			this.DeliveryAddress = deliveryAddress;
			this.RequestDates = request.StartDate.ToShortDateString() + " to " + request.EndDate.ToShortDateString();

			decimal amount = this._requestService.GetRequestPrice(requestId);
			this.Amount = amount.ToString();

			int paymentId = this._service.AddCashPayment(new CashPaymentDto(-1, requestId, client.Id, owner.Id, amount));
			this._conversationService.OnCashPaymentSelected(_rentalRequestMessageId, paymentId);
		}
	}
}
