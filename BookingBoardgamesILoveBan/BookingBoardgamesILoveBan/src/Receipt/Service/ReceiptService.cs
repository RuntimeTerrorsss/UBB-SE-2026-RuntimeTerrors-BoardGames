using System;
using System.IO;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace BookingBoardgamesILoveBan.Src.Receipt.Service
{
	public class ReceiptService : IReceiptService
	{
		private static string baseFolderPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			"BookingBoardgames");

		private readonly IUserRepository userService;
		private readonly IRequestService requestService;
		private readonly IGameRepository gameRepository;

		public ReceiptService(IUserRepository userService, IRequestService requestService, IGameRepository gameRepository)
		{
			this.userService = userService;
			this.requestService = requestService;
			this.gameRepository = gameRepository;
		}

		/// <summary>
		/// Get a new relative path for a receipt.
		/// IMPORTANT: It does NOT create the receipt pdf.
		/// Used for assigning a unique receipt file name to transaction so pdf file can be found or created when needed.
		/// </summary>
		/// <param name="requestId">id of request for generating a unique file name</param>
		/// <returns>unique relative path allocated for the receipt</returns>
		public virtual string GenerateReceiptRelativePath(int requestId)
		{
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
		public string GetReceiptDocument(PaymentCommon.Model.Payment payment)
		{
			if (payment.FilePath == null || payment.FilePath == string.Empty)
			{
				throw new InvalidOperationException("Receipt path is missing.");
			}

			string fullReceiptPath = this.GetFullPath(payment.FilePath);

			if (!File.Exists(fullReceiptPath))
			{
				return this.CreateReceipt(payment);
			}

			return fullReceiptPath;
		}

        private string PrepareDocumentPath(PaymentCommon.Model.Payment payment)
        {
            if (string.IsNullOrWhiteSpace(payment.FilePath))
            {
                throw new InvalidOperationException("Receipt path is missing.");
            }

            string documentPath = GetFullPath(payment.FilePath);

            string? directory = Path.GetDirectoryName(documentPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return documentPath;
        }

        private PdfDocument CreateDocument()
        {
            var document = new PdfDocument
            {
                PageLayout = PdfPageLayout.SinglePage
            };

            document.Info.Title = "Receipt";

            return document;
        }

        private double DrawLine(XGraphics gfx, PdfPage page, XFont font, string line, double positionX, double positionY)
        {
            gfx.DrawString(line, font, XBrushes.Black, new XRect(positionX, positionY, page.Width - 80, page.Height), XStringFormats.TopLeft);
            var size = gfx.MeasureString(line, font);

            return positionY + size.Height;
        }

        private double DrawSection(XGraphics gfx, PdfPage page, XFont font, string section, double positionX, double positionY)
        {
            foreach (string line in section.Split("\n"))
            {
                positionY = DrawLine(gfx, page, font, line, positionX, positionY);
            }
            return positionY;
        }

        private double DrawSections(XGraphics gfx, PdfPage page, XFont font, PaymentCommon.Model.Payment payment, double positionX, double positionY)
        {
            const int sectionSpacing = 10;

            foreach (string section in GetReceiptContent(payment))
            {
                positionY = DrawSection(gfx, page, font, section, positionX, positionY);
                positionY += sectionSpacing;
            }

            return positionY;
        }

        private void DrawReceiptContent(PdfDocument document, PdfPage page, PaymentCommon.Model.Payment payment)
        {
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);

            double positionX = 40;
            double positionY = 40;

            positionY = DrawSections(gfx, page, font, payment, positionX, positionY);
        }

        /// <summary>
        /// Creates a new pdf locally for a receipt at relative path.
        /// Destination: D:\Downloads\BookingBoardgames\receipts
        /// </summary>
        /// <param name="payment">transaction for generating the content of pdf</param>
        /// <returns>full path to created pdf</returns>
        /// <exception cref="InvalidOperationException">receipt path of transaction is missing</exception>
        private string CreateReceipt(PaymentCommon.Model.Payment payment)
        {
            string documentPath = PrepareDocumentPath(payment);

            using var document = CreateDocument();
            var page = document.AddPage();

            DrawReceiptContent(document, page, payment);
            document.Save(documentPath);

            return documentPath;
        }

        /// <summary>
        /// Get full path from a relative path in base folder.
        /// Base folder: D:\Downloads\BookingBoardgames\
        /// </summary>
        /// <param name="relativePath">string</param>
        /// <returns>full path</returns>
        private string GetFullPath(string relativePath)
		{
			return Path.Combine(baseFolderPath, relativePath.TrimStart('\\', '/'));
		}

        private string BuildHeader(PaymentCommon.Model.Payment payment)
        {
            string issuedDate = GetIssuedDateFromFilename(payment.FilePath.Split("\\")[1]);

            return $"Receipt - Boardgame Rental\n" +
                   $"Rental ID: {payment.RequestId}\n" +
                   $"Date Issued: {issuedDate}";
        }

        private string BuildRequestInfo(PaymentCommon.Model.Payment payment, Request request)
        {
            var game = gameService.GetById(request.GameId);
            var client = userService.GetById(payment.ClientId);
            var owner = userService.GetById(payment.OwnerId);

			string requestInfo = $"Rental Information\n" +
				$"- Rental ID: {payment.RequestId}\n" +
				$"- Boardgame: {this.gameService.GetById(request.GameId).Name}\n" +
				$"- Rental Period: {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}\n" +
				$"- Client: {this.userService.GetById(payment.ClientId).Username}\n" +
				$"- Owner: {this.userService.GetById(payment.OwnerId).Username}";

        private string BuildPaymentDetails(PaymentCommon.Model.Payment payment)
        {
            return $"Payment Details\n" +
                   $"- Payment Method: {payment.PaymentMethod}\n" +
                   $"- Amount Paid: {payment.Amount} RON";
        }

        private string BuildConfirmation(PaymentCommon.Model.Payment payment)
        {
            string confirmation = "Confirmation\n";

            if (string.Equals(payment.PaymentMethod, "cash", StringComparison.OrdinalIgnoreCase))
            {
                confirmation += $"- Owner Confirmed Payment Received: {payment.DateConfirmedSeller}\n" +
                                $"- Client Confirmed Game Received: {payment.DateConfirmedBuyer}";
            }
            else
            {
                confirmation += $"- Payment Confirmed On: {payment.DateOfTransaction}";
            }

            return confirmation;
        }

        private string BuildSummary()
        {
            return "Summary\n" +
                   "- the client has paid for the boardgame and the owner has acknowleded the transaction\n" +
                   "- the owner has delivered the boardgame and the client has acknowledged the delivery";
        }

        /// <summary>
        /// Get pdf content for generating the receipt pdf.
        /// </summary>
        /// <param name="payment">transaction with relevant transaction data</param>
        /// <returns>pdf content text</returns>
        private string[] GetReceiptContent(PaymentCommon.Model.Payment payment)
        {
            var request = requestService.GetById(payment.RequestId);

            return new[]
            {
                BuildHeader(payment),
                BuildRequestInfo(payment, request),
                BuildPaymentDetails(payment),
                BuildConfirmation(payment),
                BuildSummary()
            };
        }

        /// <summary>
        /// Get formated date for "Date Issued" field in pdf content from the receipt file name.
        /// If file name has different pattern, date of today is returned.
        /// </summary>
        /// <param name="fileName">from where to extract the date</param>
        /// <returns>reformated date (dd/MM/yyyy)</returns>
        private string GetIssuedDateFromFilename(string fileName)
		{
			try
			{
				DateTime exactDate = DateTime.ParseExact(fileName.Split("_")[2], "yyMMdd", null);
				return exactDate.ToString("dd/MM/yyyy");
			}
			catch (Exception)
			{
				return DateTime.Now.ToString("dd/MM/yyyy");
			}
		}
	}
}
