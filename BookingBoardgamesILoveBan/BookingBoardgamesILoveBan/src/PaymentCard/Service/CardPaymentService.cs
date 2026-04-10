using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentCard.Constants;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Service;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using BookingBoardgamesILoveBan.Src.PaymentCard.DTO;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.Service
{
    public class CardPaymentService : PaymentService
    {
        private readonly UserService userService;
        private readonly RequestService requestService;

        public CardPaymentService(
            PaymentRepository repo,
            UserService userService,
            ReceiptService receiptService,
            RequestService requestService) : base(repo, receiptService)
        {
            this.userService = userService;
            this.requestService = requestService;
        }

        public CardPaymentDTO AddCardPayment(int requestId, int clientId, int ownerId, decimal amount)
        {
            if (!CheckBalanceSufficiency(requestId, clientId))
            {
                throw new Exception("Insufficient Funds");
            }

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

            payment.Tid = this.paymentRepository.AddPayment(payment);
            string filePath = receiptService.GenerateReceiptRelativePath(payment.RequestId);
            payment.FilePath = filePath;
            paymentRepository.UpdatePayment(payment);

            return this.ToDto(payment);
        }

        public bool CheckBalanceSufficiency(int requestID, int clientId)
        {
            return requestService.GetRequestPrice(requestID) <= userService.GetUserBalance(clientId);
        }

        public CardPaymentDTO GetCardPayment(int paymentID)
        {
            return this.ToDto(paymentRepository.GetById(paymentID));
        }

        public decimal GetCurrentBalance(int clientID)
        {
            return userService.GetUserBalance(clientID);
        }

        public void ProcessPayment(int requestID, int clientId, int ownerId)
        {
            decimal priceRequest = requestService.GetRequestPrice(requestID);
            decimal clientBalance = userService.GetUserBalance(clientId);
            decimal ownerBalance = userService.GetUserBalance(ownerId);
            decimal newClientBalance = clientBalance - priceRequest;
            if (newClientBalance < 0)
            {
                throw new Exception("Insufficient Funds");
            }
            userService.UpdateBalance(clientId, newClientBalance);
            userService.UpdateBalance(ownerId, ownerBalance + priceRequest);
        }

        public CardPaymentDTO ToDto(PaymentCommon.Model.Payment cardPayment)
        {
            return new CardPaymentDTO(
                    tid: cardPayment.Tid,
                    requestId: cardPayment.RequestId,
                    clientId: cardPayment.ClientId,
                    ownerId: cardPayment.OwnerId,
                    amount: cardPayment.Amount,
                    dateOfTransaction: cardPayment.DateOfTransaction ?? DateTime.Now,
                    paymentMethod: cardPayment.PaymentMethod);
        }

        public RequestDto GetRequestDto(int requestId)
        {
            Request request = this.requestService.GetById(requestId);
            string gameName = this.requestService.GetGameName(request.Id);
            string ownerName = this.userService.GetById(request.OwnerId).Username;
            string clientName = this.userService.GetById(request.ClientId).Username;
            decimal gamePrice = this.requestService.GetRequestPrice(request.Id);
            return new RequestDto(request.Id, gameName, request.ClientId, request.OwnerId, ownerName, clientName,  request.StartDate, request.EndDate, gamePrice);
        }
    }
}
