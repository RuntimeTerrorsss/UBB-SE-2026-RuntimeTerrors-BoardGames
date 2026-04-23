using System;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Service;
using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Service
{
	public class CashPaymentService : PaymentService, ICashPaymentService
	{
        private const string CashPaymentMethod = "CASH";
        private readonly ICashPaymentMapper cashPaymentMapper;

		public CashPaymentService(
            IPaymentRepository paymentRepository,
            ICashPaymentMapper cashPaymentMapper,
            IReceiptService receiptService) : base(paymentRepository, receiptService)
		{
			this.cashPaymentMapper = cashPaymentMapper;
		}

		public int AddCashPayment(CashPaymentDataTransferObject cashPaymentDataTransferObject)
		{
            Payment paymentEntity = this.cashPaymentMapper.TurnDataTransferObjectIntoEntity(cashPaymentDataTransferObject);
            paymentEntity.PaymentMethod = CashPaymentMethod;
			paymentEntity.PaymentState = PaymentConstrants.StateCompleted;

			int paymentIdentifier = this.paymentRepository.AddPayment(paymentEntity);

			return paymentIdentifier;
		}

		public CashPaymentDataTransferObject GetCashPayment(int paymentIdentifier)
		{
			return this.cashPaymentMapper.TurnEntityIntoDataTransferObject(this.paymentRepository.GetPaymentByIdentifier(paymentIdentifier));
		}

		public void ConfirmDelivery(int paymentIdentifier)
		{
            Payment paymentToConfirm = this.paymentRepository.GetPaymentByIdentifier(paymentIdentifier);
			paymentToConfirm.DateConfirmedBuyer = DateTime.Now;

			if (this.IsAllConfirmed(paymentIdentifier))
			{
				this.receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
			}

			this.paymentRepository.UpdatePayment(paymentToConfirm);
		}

		public void ConfirmPayment(int paymentIdentifier)
		{
            Payment paymentToConfirm = this.paymentRepository.GetPaymentByIdentifier(paymentIdentifier);
			paymentToConfirm.DateConfirmedSeller = DateTime.Now;

			if (this.IsAllConfirmed(paymentIdentifier))
			{
				this.receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
			}

			this.paymentRepository.UpdatePayment(paymentToConfirm);
		}

		public bool IsAllConfirmed(int paymentIdentifier)
		{
            Payment paymentEntity = this.paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

			if (paymentEntity.DateConfirmedSeller != null && paymentEntity.DateConfirmedBuyer != null)
			{
				paymentEntity.PaymentState = PaymentConstrants.StateConfirmed;

				return true;
			}

			return false;
		}

		public bool IsDeliveryConfirmed(int paymentIdentifier)
		{
            Payment paymentEntity = this.paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

			if (paymentEntity.DateConfirmedBuyer != null)
			{
				return true;
			}

			return false;
		}

		public bool IsPaymentConfirmed(int paymentIdentifier)
		{
            Payment paymentEntity = this.paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

			if (paymentEntity.DateConfirmedSeller != null)
			{
				return true;
			}

			return false;
		}
	}
}
