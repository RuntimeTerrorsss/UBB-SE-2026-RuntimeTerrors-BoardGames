using System;
using BookingBoardgamesILoveBan.src.PaymentCommon.Constants;
using BookingBoardgamesILoveBan.src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.src.PaymentCommon.Service;
using BookingBoardgamesILoveBan.src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.src.PaymentCash.Model;
using BookingBoardgamesILoveBan.src.Receipt.Service;

namespace BookingBoardgamesILoveBan.src.PaymentCash.Service
{
	public class CashPaymentService: PaymentService, ICashPaymentService
	{
        private ICashPaymentMapper _mapper;

		public CashPaymentService(IPaymentRepository repository, ICashPaymentMapper mapper, IReceiptService receiptService) : base(repository, receiptService) {
			this._mapper = mapper;
		}

		public int AddCashPayment(CashPaymentDto paymentDto) {
            PaymentCommon.Model.Payment payment = this._mapper.toEntity(paymentDto);
			payment.State = PaymentConstrants.StateCompleted;

			int paymentId = this._paymentRepository.AddPayment(payment);

			return paymentId;
		}

		public CashPaymentDto GetCashPayment(int paymentId) {
			return this._mapper.toDto(this._paymentRepository.GetById(paymentId));
		}

		public void ConfirmDelivery(int paymentId) {
            PaymentCommon.Model.Payment payment = this._paymentRepository.GetById(paymentId);
			payment.DateConfirmedBuyer = DateTime.Now;

			if (this.IsAllConfirmed(paymentId)) {
				this._receiptService.GenerateReceiptRelativePath(payment.RequestId);
			}

			this._paymentRepository.UpdatePayment(payment);
		}

		public void ConfirmPayment(int paymentId) {
            PaymentCommon.Model.Payment payment = this._paymentRepository.GetById(paymentId);
			payment.DateConfirmedSeller = DateTime.Now;

			if (this.IsAllConfirmed(paymentId)) {
				this._receiptService.GenerateReceiptRelativePath(payment.RequestId);
			}

			this._paymentRepository.UpdatePayment(payment);
		}

		public bool IsAllConfirmed(int paymentId) {
            PaymentCommon.Model.Payment payment = this._paymentRepository.GetById(paymentId);

			if (payment.DateConfirmedSeller != null && payment.DateConfirmedBuyer != null) {
				payment.State = PaymentConstrants.StateConfirmed;

				return true;
			}

			return false;
		}

		public bool IsDeliveryConfirmed(int paymentId) {
            PaymentCommon.Model.Payment payment = this._paymentRepository.GetById(paymentId);

			if (payment.DateConfirmedBuyer != null) {
				return true;
			}

			return false;
		}

		public bool IsPaymentConfirmed(int paymentId) {
            PaymentCommon.Model.Payment payment = this._paymentRepository.GetById(paymentId);

			if (payment.DateConfirmedSeller != null) {
				return true;
			}

			return false;
		}
	}
}
