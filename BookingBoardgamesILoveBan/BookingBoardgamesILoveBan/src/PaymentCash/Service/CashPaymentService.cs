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
        private ICashPaymentMapper mapper;

		public CashPaymentService(IPaymentRepository repository, ICashPaymentMapper mapper, IReceiptService receiptService) : base(repository, receiptService)
		{
			this.mapper = mapper;
		}

		public int AddCashPayment(CashPaymentDto paymentDto)
		{
            PaymentCommon.Model.Payment payment = this.mapper.ToEntity(paymentDto);
			payment.State = PaymentConstrants.StateCompleted;

			int paymentId = this.paymentRepository.AddPayment(payment);

			return paymentId;
		}

		public CashPaymentDto GetCashPayment(int paymentId)
		{
			return this.mapper.ToDto(this.paymentRepository.GetById(paymentId));
		}

		public void ConfirmDelivery(int paymentId)
		{
            PaymentCommon.Model.Payment payment = this.paymentRepository.GetById(paymentId);
			payment.DateConfirmedBuyer = DateTime.Now;

			if (this.IsAllConfirmed(paymentId))
			{
				this.receiptService.GenerateReceiptRelativePath(payment.RequestId);
			}

			this.paymentRepository.UpdatePayment(payment);
		}

		public void ConfirmPayment(int paymentId)
		{
            PaymentCommon.Model.Payment payment = this.paymentRepository.GetById(paymentId);
			payment.DateConfirmedSeller = DateTime.Now;

			if (this.IsAllConfirmed(paymentId))
			{
				this.receiptService.GenerateReceiptRelativePath(payment.RequestId);
			}

			this.paymentRepository.UpdatePayment(payment);
		}

		public bool IsAllConfirmed(int paymentId)
		{
            PaymentCommon.Model.Payment payment = this.paymentRepository.GetById(paymentId);

			if (payment.DateConfirmedSeller != null && payment.DateConfirmedBuyer != null)
			{
				payment.State = PaymentConstrants.StateConfirmed;

				return true;
			}

			return false;
		}

		public bool IsDeliveryConfirmed(int paymentId)
		{
            PaymentCommon.Model.Payment payment = this.paymentRepository.GetById(paymentId);

			if (payment.DateConfirmedBuyer != null)
			{
				return true;
			}

			return false;
		}

		public bool IsPaymentConfirmed(int paymentId)
		{
            PaymentCommon.Model.Payment payment = this.paymentRepository.GetById(paymentId);

			if (payment.DateConfirmedSeller != null)
			{
				return true;
			}

			return false;
		}
	}
}
