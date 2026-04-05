using BookingBoardgamesILoveBan.src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.src.PaymentCash.Model;
using BookingBoardgamesILoveBan.src.Receipt.Service;


namespace BookingBoardgamesILoveBan.src.PaymentCommon.Service
{
	public abstract class PaymentService : IPaymentService
	{
		protected IPaymentRepository _paymentRepository;
		protected IReceiptService _receiptService;

		protected PaymentService(IPaymentRepository repository, IReceiptService receiptService) {
			this._receiptService = receiptService;
			this._paymentRepository = repository;
		}

		/// <summary>
		/// Set the receipt file path of a payment (when everything is confirmed).
		/// </summary>
		/// <param name="paymentId">of payment to set file path to</param>
		public void GenerateReceipt(int paymentId) {
            Model.Payment payment = this._paymentRepository.GetById(paymentId);

			payment.FilePath = this._receiptService.GenerateReceiptRelativePath(payment.RequestId);

			this._paymentRepository.UpdatePayment(payment);
		}

		/// <summary>
		/// Get the full path to the saved receipt pdf.
		/// </summary>
		/// <param name="paymentId">of payment to get pdf path</param>
		/// <returns>full path to pdf</returns>
		public string GetReceipt(int paymentId) {
            Model.Payment payment = this._paymentRepository.GetById(paymentId);

			if (payment.FilePath == null || payment.FilePath == "") {
				this.GenerateReceipt(paymentId);
                payment = this._paymentRepository.GetById(paymentId);
            }


			return this._receiptService.GetReceiptDocument(payment);
		}
	}
}
