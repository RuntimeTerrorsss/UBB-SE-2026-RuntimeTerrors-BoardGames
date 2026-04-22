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
    private readonly RequestService service;

    public RequestServiceUnitTests()
    {
        mockRequestRepository = new Mock<IRequestRepository>();
        mockGameRepository = new Mock<IGameRepository>();
        service = new RequestService(mockRequestRepository.Object, mockGameRepository.Object);
    }

    [Fact]
    public void GetRequestById_RequestExists_ReturnsCorrectRequest()
    {
        var expected = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(expected);

        var result = service.GetRequestById(TestRequestId);

        Assert.NotNull(result);
        Assert.Equal(
            new { expected.Id, expected.GameId, expected.StartDate, expected.EndDate },
            new { result.Id, result.GameId, result.StartDate, result.EndDate });
    }

    [Fact]
    public void GetRequestPrice_RequestExists_ReturnsCorrectPrice()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var result = service.GetRequestPrice(TestRequestId);

        Assert.Equal(TestExpectedPrice, result);
    }

    [Fact]
    public void GetGameName_RequestAndGameExist_ReturnsCorrectName()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        var game = new Game(TestGameId, TestGameName, TestPricePerDay);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetById(TestGameId)).Returns(game);

        var result = service.GetGameName(TestRequestId);

        Assert.Equal(TestGameName, result);
    }

    [Fact]
    public void GetRequestPrice_RequestDoesNotExist_ReturnsZero()
    {
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns((Request)null);

        var result = service.GetRequestPrice(TestRequestId);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetRequestPrice_ZeroDays_CalculatesAsOneDay()
    {
        var sameDay = new DateTime(2023, 10, 1);
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, sameDay, sameDay);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetPriceGameById(TestGameId)).Returns(TestPricePerDay);

        var result = service.GetRequestPrice(TestRequestId);

        Assert.Equal(TestPricePerDay * 1, result);
    }

    [Fact]
    public void GetGameName_RequestDoesNotExist_ReturnsUnknownRequest()
    {
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns((Request)null);

        var result = service.GetGameName(TestRequestId);

        Assert.Equal("Unknown Request", result);
    }

    [Fact]
    public void GetGameName_GameDoesNotExist_ReturnsUnknownGame()
    {
        var request = new Request(TestRequestId, TestGameId, TestClientId, TestOwnerId, TestStartDate, TestEndDate);
        mockRequestRepository.Setup(r => r.GetById(TestRequestId)).Returns(request);
        mockGameRepository.Setup(g => g.GetById(TestGameId)).Returns((Game)null);

        var result = service.GetGameName(TestRequestId);

        Assert.Equal("Unknown Game", result);
    }
}