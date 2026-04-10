using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Service
{
	public abstract class PaymentService : IPaymentService
	{
		protected IPaymentRepository paymentRepository;
		protected IReceiptService receiptService;

		protected PaymentService(IPaymentRepository repository, IReceiptService receiptService)
		{
			this.receiptService = receiptService;
			this.paymentRepository = repository;
		}

		/// <summary>
		/// Set the receipt file path of a payment (when everything is confirmed).
		/// </summary>
		/// <param name="paymentId">of payment to set file path to</param>
		public void GenerateReceipt(int paymentId)
		{
            Model.Payment payment = this.paymentRepository.GetById(paymentId);

			payment.FilePath = this.receiptService.GenerateReceiptRelativePath(payment.RequestId);

			this.paymentRepository.UpdatePayment(payment);
		}

		/// <summary>
		/// Get the full path to the saved receipt pdf.
		/// </summary>
		/// <param name="paymentId">of payment to get pdf path</param>
		/// <returns>full path to pdf</returns>
		public string GetReceipt(int paymentId)
		{
            Model.Payment payment = this.paymentRepository.GetById(paymentId);

			if (payment.FilePath == null || payment.FilePath == string.Empty)
			{
				this.GenerateReceipt(paymentId);
                payment = this.paymentRepository.GetById(paymentId);
            }

			return this.receiptService.GetReceiptDocument(payment);
		}
	}
}
