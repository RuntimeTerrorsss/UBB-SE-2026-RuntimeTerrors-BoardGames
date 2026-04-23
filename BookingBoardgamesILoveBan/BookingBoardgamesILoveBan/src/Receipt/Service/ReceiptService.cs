using System;
using System.IO;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.Receipt.Constants;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace BookingBoardgamesILoveBan.Src.Receipt.Service
{
	public class ReceiptService : IReceiptService
	{
		private static string baseFolderPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			ReceiptServiceConstants.BaseFolderName);

		private readonly IUserRepository userRepository;
		private readonly IRequestService requestService;
		private readonly IGameRepository gameRepository;

		public ReceiptService(IUserRepository userRepository, IRequestService requestService, IGameRepository gameRepository)
		{
			this.userRepository = userRepository;
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
		/// <param name="selectedPayment">transaction for getting relative path to receipt</param>
		/// <returns>full path to existing or newly created pdf</returns>
		/// <exception cref="InvalidOperationException">receipt path of transaction is missing</exception>
		public string GetReceiptDocument(PaymentCommon.Model.Payment selectedPayment)
		{
			if (selectedPayment.ReceiptFilePath == null || selectedPayment.ReceiptFilePath == string.Empty)
			{
				throw new InvalidOperationException("Receipt path is missing.");
			}

			string fullReceiptPath = this.GetFullPath(selectedPayment.ReceiptFilePath);

			if (!File.Exists(fullReceiptPath))
			{
				return this.CreateReceipt(selectedPayment);
			}

			return fullReceiptPath;
		}

        private string PrepareDocumentPath(PaymentCommon.Model.Payment selectedPayment)
        {
            if (string.IsNullOrWhiteSpace(selectedPayment.ReceiptFilePath))
            {
                throw new InvalidOperationException("Receipt path is missing.");
            }

            string documentPath = GetFullPath(selectedPayment.ReceiptFilePath);

            string? directoryName = Path.GetDirectoryName(documentPath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            return documentPath;
        }

        private PdfDocument CreateDocument()
        {
            var createdDocument = new PdfDocument
            {
                PageLayout = PdfPageLayout.SinglePage
            };

            createdDocument.Info.Title = ReceiptServiceConstants.DocumentTitle;

            return createdDocument;
        }

        private double DrawLine(
            XGraphics graphicsContext,
            PdfPage pdfPage,
            XFont font,
            string textLine,
            double positionX,
            double positionY)
        {
            graphicsContext.DrawString(
                textLine,
                font,
                XBrushes.Black,
                new XRect(positionX, positionY, pdfPage.Width - ReceiptServiceConstants.ContentWidthPadding, pdfPage.Height),
                XStringFormats.TopLeft);

            var textSize = graphicsContext.MeasureString(textLine, font);

            return positionY + textSize.Height;
        }

        private double DrawTextSection(
            XGraphics graphicsContext,
            PdfPage pdfPage,
            XFont font,
            string textSection,
            double currentXPosition,
            double currentYPosition)
        {
            foreach (string textLine in textSection.Split("\n"))
            {
                currentYPosition = DrawLine(
                    graphicsContext,
                    pdfPage,
                    font,
                    textLine,
                    currentXPosition,
                    currentYPosition);
            }

            return currentYPosition;
        }

        private double DrawAllSections(
            XGraphics graphicsContext,
            PdfPage pdfPage,
            XFont font,
            PaymentCommon.Model.Payment payment,
            double currentXPosition,
            double currentYPosition)
        {
            foreach (string textSection in GetReceiptContent(payment))
            {
                currentYPosition = DrawTextSection(
                    graphicsContext,
                    pdfPage,
                    font,
                    textSection,
                    currentXPosition,
                    currentYPosition);

                currentYPosition += ReceiptServiceConstants.SectionSpacing;
            }

            return currentYPosition;
        }

        private void DrawReceiptContent(
            PdfDocument pdfDocument,
            PdfPage pdfPage,
            PaymentCommon.Model.Payment payment)
        {
            var graphicsContext = XGraphics.FromPdfPage(pdfPage);

            var font = new XFont(
                ReceiptServiceConstants.DefaultFontFamily,
                ReceiptServiceConstants.DefaultFontSize,
                XFontStyle.Regular);

            double currentXPosition = ReceiptServiceConstants.HorizontalMargin;
            double currentYPosition = ReceiptServiceConstants.VerticalStart;

            currentYPosition = DrawAllSections(
                graphicsContext,
                pdfPage,
                font,
                payment,
                currentXPosition,
                currentYPosition);
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
            string issuedDate = GetIssuedDateFromFilename(payment.ReceiptFilePath.Split("\\")[ReceiptServiceConstants.FileNameIndexInPath]);

            return $"Receipt - Boardgame Rental\n" +
                   $"Rental ID: {payment.RequestId}\n" +
                   $"Date Issued: {issuedDate}";
        }

        private string BuildRequestInfo(PaymentCommon.Model.Payment payment, Request request)
        {
            var requestedGame = gameRepository.GetById(request.GameId);
            var client = userRepository.GetById(payment.ClientId);
            var owner = userRepository.GetById(payment.OwnerId);

            string requestInfo = $"Rental Information\n" +
                $"- Rental ID: {payment.RequestId}\n" +
                $"- Boardgame: {requestedGame.Name}\n" +
                $"- Rental Period: {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}\n" +
                $"- Client: {client.Username}\n" +
                $"- Owner: {owner.Username}";
            return requestInfo;
        }

        private string BuildPaymentDetails(PaymentCommon.Model.Payment payment)
        {
            return $"Payment Details\n" +
                   $"- Payment Method: {payment.PaymentMethod}\n" +
                   $"- Amount Paid: {payment.PaidAmount} RON";
        }

        private string BuildConfirmation(PaymentCommon.Model.Payment payment)
        {
            string confirmationText = "Confirmation\n";

            if (string.Equals(payment.PaymentMethod, "cash", StringComparison.OrdinalIgnoreCase))
            {
                confirmationText += $"- Owner Confirmed Payment Received: {payment.DateConfirmedSeller}\n" +
                                $"- Client Confirmed Game Received: {payment.DateConfirmedBuyer}";
            }
            else
            {
                confirmationText += $"- Payment Confirmed On: {payment.DateOfTransaction}";
            }

            return confirmationText;
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
            var request = requestService.GetRequestById(payment.RequestId);

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
				DateTime exactDate = DateTime.ParseExact(fileName.Split(ReceiptServiceConstants.FileNameSeparator)[ReceiptServiceConstants.DatePartIndex], ReceiptServiceConstants.FileDateFormat, null);
				return exactDate.ToString(ReceiptServiceConstants.DisplayDateFormat);
			}
			catch (Exception)
			{
				return DateTime.Now.ToString(ReceiptServiceConstants.DisplayDateFormat);
			}
		}
	}
}
