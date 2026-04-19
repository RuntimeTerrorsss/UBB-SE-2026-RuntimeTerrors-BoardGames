using System;
using BookingBoardgamesILoveBan.Src.PaymentCard.Constants;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Service;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using BookingBoardgamesILoveBan.Src.PaymentCard.DataTransferObjects;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.Service
{
    public class CardPaymentService : PaymentService
    {
        private readonly IUserRepository userRepository;
        private readonly IRequestService requestService;

        public CardPaymentService(
            PaymentRepository paymentRepository,
            UserRepository userService,
            ReceiptService receiptService,
            IRequestService requestService) : base(paymentRepository, receiptService)
        {
            this.userRepository = userService;
            this.requestService = requestService;
        }

        public virtual CardPaymentDataTransferObject AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount)
        {
            if (!CheckBalanceSufficiency(requestIdentifier, clientIdentifier))
            {
                throw new Exception("Insufficient Funds");
            }

            ProcessPayment(requestIdentifier, clientIdentifier, ownerIdentifier);

            PaymentCommon.Model.Payment payment = new PaymentCommon.Model.Payment
            {
                RequestId = requestIdentifier,
                ClientId = clientIdentifier,
                OwnerId = ownerIdentifier,
                Amount = amount,
                PaymentMethod = CardPaymentConstants.CardPaymentMethodName,
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null,
                State = CardPaymentConstants.SuccessfulPaymentState,
                FilePath = null
            };

            payment.Tid = this.paymentRepository.AddPayment(payment);
            string receiptFilePath = receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.FilePath = receiptFilePath;
            paymentRepository.UpdatePayment(payment);

            return this.ConvertToDataTransferObject(payment);
        }

        public bool CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier)
        {
            return requestService.GetRequestPrice(requestIdentifier) <= userRepository.GetUserBalance(clientIdentifier);
        }

        public CardPaymentDataTransferObject GetCardPayment(int paymentIdentifier)
        {
            return this.ConvertToDataTransferObject(paymentRepository.GetById(paymentIdentifier));
        }

        public decimal GetCurrentBalance(int clientIdentifier)
        {
            return userRepository.GetUserBalance(clientIdentifier);
        }

        public void ProcessPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier)
        {
            decimal requestPrice = requestService.GetRequestPrice(requestIdentifier);
            decimal clientBalance = userRepository.GetUserBalance(clientIdentifier);
            decimal ownerBalance = userRepository.GetUserBalance(ownerIdentifier);
            decimal newClientBalance = clientBalance - requestPrice;

            if (newClientBalance < 0)
            {
                throw new Exception("Insufficient Funds");
            }

            userRepository.UpdateBalance(clientIdentifier, newClientBalance);
            userRepository.UpdateBalance(ownerIdentifier, ownerBalance + requestPrice);
        }

        public CardPaymentDataTransferObject ConvertToDataTransferObject(PaymentCommon.Model.Payment cardPayment)
        {
            return new CardPaymentDataTransferObject(
                    transactionIdentifier: cardPayment.Tid,
                    requestIdentifier: cardPayment.RequestId,
                    clientIdentifier: cardPayment.ClientId,
                    ownerIdentifier: cardPayment.OwnerId,
                    amount: cardPayment.Amount,
                    dateOfTransaction: cardPayment.DateOfTransaction ?? DateTime.Now,
                    paymentMethod: cardPayment.PaymentMethod);
        }

        public virtual RequestDto GetRequestDataTransferObject(int requestIdentifier)
        {
            Request request = this.requestService.GetRequestById(requestIdentifier);
            string gameName = this.requestService.GetGameName(request.Id);
            string ownerName = this.userRepository.GetById(request.OwnerId).Username;
            string clientName = this.userRepository.GetById(request.ClientId).Username;
            decimal gamePrice = this.requestService.GetRequestPrice(request.Id);

            return new RequestDto(request.Id, gameName, request.ClientId, request.OwnerId, ownerName, clientName, request.StartDate, request.EndDate, gamePrice);
        }
    }
}