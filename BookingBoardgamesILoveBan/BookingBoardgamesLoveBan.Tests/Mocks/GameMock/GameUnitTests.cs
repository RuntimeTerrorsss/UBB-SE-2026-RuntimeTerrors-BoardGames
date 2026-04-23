using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.GameMock
{
    public class GameUnitTests
    {
        [Fact]
        public void Game_ValidParameters_SetsPropertiesCorrectly()
        {
            var expected = new { Id = 1, Name = "Catan", PricePerDay = 10.5m };
            
            var game = new Game(expected.Id, expected.Name, expected.PricePerDay);
            var actualGame = new { game.Id, game.Name, game.PricePerDay };

            Assert.Equal(expected, actualGame);
        }

        [Fact]
        public void Game_PropertyUpdates_SetsPropertiesCorrectly()
        {
            var game = new Game(1, "OldName", 5.0m);
            game.Id = 2;
            game.Name = "NewName";
            game.PricePerDay = 15.0m;

            var expected = new { Id = 2, Name = "NewName", PricePerDay = 15.0m };
            var actualGame = new { game.Id, game.Name, game.PricePerDay };

            Assert.Equal(expected, actualGame);
        }
    }
}
