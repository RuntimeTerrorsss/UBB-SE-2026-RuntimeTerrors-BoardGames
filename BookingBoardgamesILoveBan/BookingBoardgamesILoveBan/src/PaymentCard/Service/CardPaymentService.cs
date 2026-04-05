using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.src.PaymentCard.Constants;
using BookingBoardgamesILoveBan.src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.src.Mocks.UserMock;
using BookingBoardgamesILoveBan.src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.src.PaymentCommon.Service;
using BookingBoardgamesILoveBan.src.Receipt.Service;
using BookingBoardgamesILoveBan.src.PaymentCard.DTO;

namespace BookingBoardgamesILoveBan.src.PaymentCard.Service
{
    public class CardPaymentService : PaymentService
    {
        
        private readonly UserService _userService;
        private readonly RequestService _requestService;

        public CardPaymentService(
            PaymentRepository repo,
            UserService userService,
            ReceiptService receiptService,
            RequestService requestService) : base(repo, receiptService)
        {
            this._userService = userService;
            this._requestService = requestService;
            
        }

        public CardPaymentDTO AddCardPayment(int requestId, int clientId, int ownerId, decimal amount)
        {
            if (!checkBalanceSufficiency(requestId, clientId))
                throw new Exception("Insufficient Funds");

            ProcessPayment(requestId, clientId, ownerId);

            PaymentCommon.Model.Payment payment = new PaymentCommon.Model.Payment
            {
                RequestId = requestId,
                ClientId = clientId,
                OwnerId = ownerId,
                Amount = amount,
                PaymentMethod = "CARD",
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null,
                State = 1,
                FilePath = null
            };

            payment.tid = this._paymentRepository.AddPayment(payment);
            string filePath = _receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.FilePath = filePath;
            _paymentRepository.UpdatePayment(payment);

            return this.toDto(payment);
        }

        public bool checkBalanceSufficiency(int requestID, int clientId)
        {
            return _requestService.GetRequestPrice(requestID) <= _userService.GetUserBalance(clientId);
        }


        public CardPaymentDTO GetCardPayment(int paymentID)
        {
            return this.toDto(_paymentRepository.GetById(paymentID));
        }

        public decimal GetCurrentBalance(int clientID)
        {
            return _userService.GetUserBalance(clientID);
        }

        public void ProcessPayment(int requestID, int clientId, int ownerId)
        {
            decimal priceRequest = _requestService.GetRequestPrice(requestID);
            decimal clientBalance = _userService.GetUserBalance(clientId);
            decimal ownerBalance = _userService.GetUserBalance(ownerId);
            decimal newClientBalance = clientBalance - priceRequest;
            if (newClientBalance < 0)
            {
                throw new Exception("Insufficient Funds");
            }
            _userService.UpdateBalance(clientId, newClientBalance);
            _userService.UpdateBalance(ownerId, ownerBalance + priceRequest);
            
        }

        public CardPaymentDTO toDto(PaymentCommon.Model.Payment cardPayment)
        {
            return new CardPaymentDTO(
                    tid: cardPayment.tid,
                    requestId: cardPayment.RequestId,
                    clientId: cardPayment.ClientId,
                    ownerId: cardPayment.OwnerId,
                    amount: cardPayment.Amount,
                    dateOfTransaction: cardPayment.DateOfTransaction ?? DateTime.Now,
                    paymentMethod: cardPayment.PaymentMethod);
        }

        public RequestDto GetRequestDto(int requestId){
            Request request = this._requestService.GetById(requestId);
            String gameName = this._requestService.GetGameName(request.Id);
            String ownerName = this._userService.GetById(request.OwnerId).Username;
            String clientName = this._userService.GetById(request.ClientId).Username;
            decimal gamePrice = this._requestService.GetRequestPrice(request.Id);
            return new RequestDto(request.Id, gameName, request.ClientId, request.OwnerId, ownerName, clientName,  request.StartDate, request.EndDate, gamePrice);
        }
        
    }
}
