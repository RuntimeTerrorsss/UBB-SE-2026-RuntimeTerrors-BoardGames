using BookingBoardgamesILoveBan.Src.PaymentCard.Service;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Service;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Repository;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BookingBoardgamesILoveBan
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        ///
        public static Src.Mocks.UserMock.UserService UserService { get; private set; } = new UserService();

        public static GameService GameService { get; private set; } = new GameService();
        public static RequestService RequestService { get; private set; } = new RequestService(App.GameService);

        public static PaymentRepository PaymentRepository { get; private set; } = new PaymentRepository();
        public static ReceiptService ReceiptService { get; private set; } = new ReceiptService(App.UserService, App.RequestService, App.GameService);
        public static CardPaymentService CardPaymentService { get; private set; } = new CardPaymentService(App.PaymentRepository,
            App.UserService, App.ReceiptService, App.RequestService);

        public static MapService MapService { get; private set; } = new MapService();

        public static IRepositoryPayment HistoryRepository = new RepositoryPayment();

        public static ServicePayment ServicePayment { get; private set; } = new ServicePayment(App.HistoryRepository,
            App.ReceiptService);

        public static CashPaymentService CashPaymentService { get; private set; } = new CashPaymentService(App.PaymentRepository,
            new CashPaymentMapper(), App.ReceiptService);

        public static ConversationRepository ConversationRepository { get; private set; } = new ConversationRepository();

        public static int DASHBOARD_USER = 3;
        public static int NO_CHATS_USER = 8;

        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Make sure database is properly set up
            DatabaseBootstrap.Initialize();

            window = new MainWindow();
            window.Activate();
        }
    }
}
