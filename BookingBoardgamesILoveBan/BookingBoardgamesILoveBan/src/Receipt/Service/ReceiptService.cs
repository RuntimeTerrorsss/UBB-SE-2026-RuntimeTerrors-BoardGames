using BookingBoardgamesILoveBan.src.Mocks.GameMock;
using BookingBoardgamesILoveBan.src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.src.Mocks.UserMock;
using BookingBoardgamesILoveBan.src.PaymentCommon.Model;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.IO;


namespace BookingBoardgamesILoveBan.src.Receipt.Service
{
	public class ReceiptService : IReceiptService
	{
		private static string s_baseFolderPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
			"BookingBoardgames"
			);

		private readonly UserService userService;
		private readonly RequestService requestService;
		private readonly GameService gameService;

		public ReceiptService(UserService userService, RequestService requestService, GameService gameService) {
			this.userService = userService;
			this.requestService = requestService;
			this.gameService = gameService;
		}

		/// <summary>
		/// Get a new relative path for a receipt.
		/// IMPORTANT: It does NOT create the receipt pdf.
		/// Used for assigning a unique receipt file name to transaction so pdf file can be found or created when needed.
		/// </summary>
		/// <param name="requestId">id of request for generating a unique file name</param>
		/// <returns>unique relative path allocated for the receipt</returns>
		public string GenerateReceiptRelativePath(int requestId) {
			string fileName = $"receipt_{requestId}_{DateTime.Now:yyMMdd_HHmmss}.pdf";

			return $"receipts\\{fileName}";
		}

		/// <summary>
		/// Get the full path to the receipt pdf.
		/// Source: D:\Downloads\BookingBoardgames\receipts
		/// 
		/// If pdf for receipt does not exist at source, it is created and full path to it returned.
		/// Otherwise, full path to existing pdf is returned.
		/// </summary>
		/// <param name="payment">transaction for getting relative path to receipt</param>
		/// <returns>full path to existing or newly created pdf</returns>
		/// <exception cref="InvalidOperationException">receipt path of transaction is missing</exception>
		public string GetReceiptDocument(PaymentCommon.Model.Payment payment) {		
			if (payment.FilePath == null || payment.FilePath == "") {
				throw new InvalidOperationException("Receipt path is missing.");
			}

			string fullReceiptPath = this.GetFullPath(payment.FilePath);

			if (!File.Exists(fullReceiptPath)) {
				return this.CreateReceipt(payment);
			}

			return fullReceiptPath;
		}

		/// <summary>
		/// Creates a new pdf locally for a receipt at relative path.
		/// Destination: D:\Downloads\BookingBoardgames\receipts
		/// </summary>
		/// <param name="payment">transaction for generating the content of pdf</param>
		/// <returns>full path to created pdf</returns>
		/// <exception cref="InvalidOperationException">receipt path of transaction is missing</exception>
		private string CreateReceipt(PaymentCommon.Model.Payment payment) {
			if (payment.FilePath == null || payment.FilePath == "") {
				throw new InvalidOperationException("Receipt path is missing.");
			}

			string documentPath = this.GetFullPath(payment.FilePath);
			Directory.CreateDirectory(Path.GetDirectoryName(documentPath));

			PdfDocument document = new PdfDocument();
			document.PageLayout = PdfPageLayout.SinglePage;
			document.Info.Title = "Receipt";

			var page = document.AddPage();
			var gfx = XGraphics.FromPdfPage(page);
			var font = new XFont("Arial", 12, XFontStyle.Regular);

			double positionX = 40;
			double positionY = 40;
			int sectionSpacing = 10;

			foreach (string section in this.GetReceiptContent(payment)) {
				foreach (string line in section.Split("\n")) {
					gfx.DrawString(line, font, XBrushes.Black,
						new XRect(positionX, positionY, page.Width - 80, page.Height), XStringFormats.TopLeft);

					var sectionSize = gfx.MeasureString(line, font);
					positionY += sectionSize.Height;
				}

				positionY += sectionSpacing;
			}

			document.Save(documentPath);

			return documentPath;
		}

		/// <summary>
		/// Get full path from a relative path in base folder.
		/// Base folder: D:\Downloads\BookingBoardgames\
		/// </summary>
		/// <param name="relativePath">string</param>
		/// <returns>full path</returns>
		private string GetFullPath(string relativePath) {
			return Path.Combine(s_baseFolderPath, relativePath.TrimStart('\\', '/'));
		}

		/// <summary>
		/// Get pdf content for generating the receipt pdf.
		/// </summary>
		/// <param name="payment">transaction with relevant transaction data</param>
		/// <returns>pdf content text</returns>
		private string[] GetReceiptContent(PaymentCommon.Model.Payment payment) {
			string header = $"Receipt - Boardgame Rental\n" +
				$"Rental ID: {payment.RequestId}\n" +
				$"Date Issued: {this.GetIssuedDateFromFilename(payment.FilePath.Split("\\")[1])}";

			Request request = this.requestService.GetById(payment.RequestId);

			string requestInfo = $"Rental Information\n" +
				$"- Rental ID: {payment.RequestId}\n" +
				$"- Boardgame: {this.gameService.GetById(request.GameId).Name}\n" +
				$"- Rental Period: {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}\n" +
				$"- Client: {this.userService.GetById(payment.ClientId).Username}\n" +
				$"- Owner: {this.userService.GetById(payment.OwnerId).Username}";

			string paymentDetails = $"Payment Details\n" +
				$"- Payment Method: {payment.PaymentMethod}\n" +
				$"- Amount Paid: {payment.Amount} RON";

			string confirmation = "Confirmation\n";

			if (payment.PaymentMethod.ToLower() == "cash") {
				confirmation += $"- Owner Confirmed Payment Received: {payment.DateConfirmedSeller}\n" +
					$"- Client Confirmed Game Received: {payment.DateConfirmedBuyer}";
			} else {
				confirmation += $"- Payment Confirmed On: {payment.DateOfTransaction}";
			}

			string summary = "Summary\n" +
				"- the client has paid for the boardgame and the owner has acknowleded the transaction\n" +
				"- the owner has delivered the boardgame and the client has acknowledged the delivery";

			return [header, requestInfo, paymentDetails, confirmation, summary];
		}

		/// <summary>
		/// Get formated date for "Date Issued" field in pdf content from the receipt file name.
		/// If file name has different pattern, date of today is returned.
		/// </summary>
		/// <param name="fileName">from where to extract the date</param>
		/// <returns>reformated date (dd/MM/yyyy)</returns>
		private string GetIssuedDateFromFilename(string fileName) {
			try {
				DateTime exactDate = DateTime.ParseExact(fileName.Split("_")[2], "yyMMdd", null);
				return exactDate.ToString("dd/MM/yyyy");
			} catch (Exception) {
				return DateTime.Now.ToString("dd/MM/yyyy");
			}
		}
	}
}
