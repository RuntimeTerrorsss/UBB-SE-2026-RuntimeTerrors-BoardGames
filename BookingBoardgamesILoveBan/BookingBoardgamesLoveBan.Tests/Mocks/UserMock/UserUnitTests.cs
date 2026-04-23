using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.UserMock
{
    public class UserUnitTests
    {
        [Fact]
        public void User_ShortConstructor_SetsPropertiesCorrectly()
        {
            var user = new User(1, "testuser", "Romania", "Cluj", "Street", "5");

            var expectedUser = new { Id = 1, Username = "testuser", Country = "Romania", City = "Cluj", Street = "Street", StreetNumber = "5", DisplayName = "testuser", AvatarUrl = "", Balance = 0m };
            var actualUser = new { user.Id, user.Username, user.Country, user.City, user.Street, user.StreetNumber, user.DisplayName, user.AvatarUrl, user.Balance };

            Assert.Equal(expectedUser, actualUser);
        }

        [Fact]
        public void User_FullConstructor_SetsPropertiesCorrectly()
        {
            var user = new User(2, "fulluser", "Display", "Moldova", "Chisinau", "Main", "10", "http://avatar", 50.5m);

            var expectedUser = new { Id = 2, Username = "fulluser", Country = "Moldova", City = "Chisinau", Street = "Main", StreetNumber = "10", DisplayName = "Display", AvatarUrl = "http://avatar", Balance = 50.5m };
            var actualUser = new { user.Id, user.Username, user.Country, user.City, user.Street, user.StreetNumber, user.DisplayName, user.AvatarUrl, user.Balance };

            Assert.Equal(expectedUser, actualUser);
        }

        [Fact]
        public void User_PropertyUpdates_SetsPropertiesCorrectly()
        {
            var user = new User(1, "old", "ro", "city", "st", "1");
            user.Id = 3;
            user.Username = "new";
            user.DisplayName = "newDisplay";
            user.Country = "newCountry";
            user.City = "newCity";
            user.Street = "newStreet";
            user.StreetNumber = "newNumber";
            user.AvatarUrl = "newUrl";
            user.Balance = 99m;

            var expectedUser = new { Id = 3, Username = "new", Country = "newCountry", City = "newCity", Street = "newStreet", StreetNumber = "newNumber", DisplayName = "newDisplay", AvatarUrl = "newUrl", Balance = 99m };
            var actualUser = new { user.Id, user.Username, user.Country, user.City, user.Street, user.StreetNumber, user.DisplayName, user.AvatarUrl, user.Balance };

            Assert.Equal(expectedUser, actualUser);
        }
    }
}
