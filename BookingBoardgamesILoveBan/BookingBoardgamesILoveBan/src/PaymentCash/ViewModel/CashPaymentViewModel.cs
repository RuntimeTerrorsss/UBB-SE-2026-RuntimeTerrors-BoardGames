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
        private const int NewPaymentPlaceholderId = -1;
        private const string DateRangeSeparator = " to ";

		private readonly ICashPaymentService cashPaymentService;
		private readonly IUserRepository userRepository;
		private readonly IRequestService rentalRequestService;
		private readonly IGameRepository gameRepository;
		private readonly ConversationService conversationService;

		public string OwnerName { get; set; }
		public string GameName { get; set; }
		public string DeliveryAddress { get; set; }
		public string RequestDates { get; set; }
		public string PaidAmount { get; set; }

		private readonly int rentalRequestMessageIdentifier;

		public CashPaymentViewModel(
            ICashPaymentService cashPaymentService,
			IUserRepository userRepository,
			IRequestService rentalRequestService,
			IGameRepository gameRepository,
			int rentalRequestId,
			string deliveryAddress,
			int rentalRequestMessageIdentifier,
			ConversationService conversationService)
		{
			this.cashPaymentService = cashPaymentService;
			this.userRepository = userRepository;
			this.rentalRequestService = rentalRequestService;
			this.gameRepository = gameRepository;
			this.conversationService = conversationService;
			this.rentalRequestMessageIdentifier = rentalRequestMessageIdentifier;

			Request rentalRequest = this.rentalRequestService.GetRequestById(rentalRequestId);
			Game game = this.gameRepository.GetById(rentalRequest.GameId);
			User clientUser = this.userRepository.GetById(rentalRequest.ClientId);
			User ownerUser = this.userRepository.GetById(rentalRequest.OwnerId);

			this.OwnerName = ownerUser.Username;
			this.GameName = game.Name;
			this.DeliveryAddress = deliveryAddress;
			this.RequestDates = rentalRequest.StartDate.ToShortDateString() + DateRangeSeparator + rentalRequest.EndDate.ToShortDateString();

			decimal rentalPrice = this.rentalRequestService.GetRequestPrice(rentalRequestId);
			this.PaidAmount = rentalPrice.ToString();

			int createdPaymentIdentifier = this.cashPaymentService.AddCashPayment(
                new CashPaymentDataTransferObject(NewPaymentPlaceholderId, rentalRequestId, clientUser.Id, ownerUser.Id, rentalPrice));
			this.conversationService.OnCashPaymentSelected(this.rentalRequestMessageIdentifier, createdPaymentIdentifier);
		}
	}
}
