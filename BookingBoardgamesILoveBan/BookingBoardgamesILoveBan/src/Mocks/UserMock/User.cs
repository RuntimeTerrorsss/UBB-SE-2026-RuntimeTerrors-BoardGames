namespace BookingBoardgamesILoveBan.Src.Mocks.UserMock
{
    public class User
    {
        public User(int id, string name, string country, string city, string street, string streetNumber)
        {
            Id = id;
            Username = name;
            Country = country;
            Street = street;
            StreetNumber = streetNumber;
            City = city;
        }

        public User(int id, string username, string displayName, string country, string city, string street, string streetNumber, string avatarUrl, decimal balance)
        {
            Id = id;
            Username = username;
            DisplayName = displayName;
            City = city;
            Country = country;
            Street = street;
            StreetNumber = streetNumber;
            AvatarUrl = avatarUrl;
            Balance = balance;
        }

        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;

        public string StreetNumber { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0;
    }
}