using System;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using Moq;
using Xunit;

public class RequestServiceUnitTests
{
    private const int TestRequestId = 1;
    private const int TestGameId = 10;
    private const int TestClientId = 2;
    private const int TestOwnerId = 3;
    private const decimal TestPricePerDay = 25.50m;
    private const string TestGameName = "Chess";
    private static readonly DateTime TestStartDate = new DateTime(2023, 10, 01);
    private static readonly DateTime TestEndDate = new DateTime(2023, 10, 06);
    private const int TestDays = 5;
    private const decimal TestExpectedPrice = TestPricePerDay * TestDays;

    private readonly Mock<IRequestRepository> mockRequestRepository;
    private readonly Mock<IGameRepository> mockGameRepository;
    private readonly RequestService requestService;

    public RequestServiceUnitTests()
    {
        mockRequestRepository = new Mock<IRequestRepository>();
        mockGameRepository = new Mock<IGameRepository>();
        requestService = new RequestService(mockRequestRepository.Object, mockGameRepository.Object);
    }

    [Fact]
    public void GetRequestById_RequestExists_ReturnsCorrectRequest()
    {
        var expectedRequest = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(expectedRequest);

        var resultedRequest = requestService.GetRequestById(TestRequestId);

        Assert.NotNull(resultedRequest);
        Assert.Equal(
            new { expectedRequest.Id, expectedRequest.GameId, expectedRequest.StartDate, expectedRequest.EndDate },
            new { resultedRequest.Id, resultedRequest.GameId, resultedRequest.StartDate, resultedRequest.EndDate });
    }

    [Fact]
    public void GetRequestPrice_RequestExists_ReturnsCorrectPrice()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var resultedRequest = requestService.GetRequestPrice(TestRequestId);

        Assert.Equal(TestExpectedPrice, resultedRequest);
    }

    [Fact]
    public void GetGameName_RequestAndGameExist_ReturnsCorrectName()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        var game = new Game(TestGameId, TestGameName, TestPricePerDay);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetById(TestGameId)).Returns(game);

        var resultedRequest = requestService.GetGameName(TestRequestId);

        Assert.Equal(TestGameName, resultedRequest);
    }

    [Fact]
    public void GetRequestPrice_RequestDoesNotExist_ReturnsZero()
    {
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns((Request)null);

        var resultedRequest = requestService.GetRequestPrice(TestRequestId);

        Assert.Equal(0m, resultedRequest);
    }

    [Fact]
    public void GetRequestPrice_ZeroDays_CalculatesAsOneDay()
    {
        var sameDay = new DateTime(2023, 10, 1);
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, sameDay, sameDay);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var resultedRequest = requestService.GetRequestPrice(TestRequestId);

        Assert.Equal(TestPricePerDay * 1, resultedRequest);
    }

    [Fact]
    public void GetGameName_RequestDoesNotExist_ReturnsUnknownRequest()
    {
        mockRequestRepository.Setup(mockRepository => mockRepository.GetById(TestRequestId)).Returns((Request)null);

        var resultedRequest = requestService.GetGameName(TestRequestId);

        Assert.Equal("Unknown Request", resultedRequest);
    }

    [Fact]
    public void GetGameName_GameDoesNotExist_ReturnsUnknownGame()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetById(TestGameId)).Returns((Game)null);

        var resultedRequest = requestService.GetGameName(TestRequestId);

        Assert.Equal("Unknown Game", resultedRequest);
    }
}