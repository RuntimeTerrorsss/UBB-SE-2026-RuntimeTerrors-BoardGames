using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Service
{
	public abstract class PaymentService : IPaymentService
	{
		protected readonly IPaymentRepository paymentRepository;
		protected readonly IReceiptService receiptService;

		protected PaymentService(IPaymentRepository paymentRepository, IReceiptService receiptService)
		{
			this.receiptService = receiptService;
			this.paymentRepository = paymentRepository;
		}

		/// <summary>
		/// Set the receipt file path of a payment (when everything is confirmed).
		/// </summary>
		/// <param name="paymentId">of payment to set file path to</param>
		public void GenerateReceipt(int paymentId)
		{
            Payment paymentToUpdate = this.paymentRepository.GetById(paymentId);

			paymentToUpdate.FilePath = this.receiptService.GenerateReceiptRelativePath(paymentToUpdate.RequestId);

			this.paymentRepository.UpdatePayment(paymentToUpdate);
		}

		/// <summary>
		/// Get the full path to the saved receipt pdf.
		/// </summary>
		/// <param name="paymentId">of payment to get pdf path</param>
		/// <returns>full path to pdf</returns>
		public string GetReceipt(int paymentId)
		{
            Payment paymentToRead = this.paymentRepository.GetById(paymentId);

			if (string.IsNullOrEmpty(paymentToRead.FilePath))
			{
				this.GenerateReceipt(paymentId);
                paymentToRead = this.paymentRepository.GetById(paymentId);
            }

			return this.receiptService.GetReceiptDocument(paymentToRead);
		}
	}
}
