namespace BookingBoardgamesILoveBan.src.Mocks.UserMock
{
	public class User
	{
		public int Id { get; set; }

		public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;

        public string StreetNumber { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0;

        public User(int Id, string Name, string country, string city, string street, string streetNumber)
        {
            this.Id = Id;
            this.Username = Name;
            this.Country = country;
            this.Street = street;
            this.StreetNumber = streetNumber;
            this.City = city;

        }
        
        public User(int id, string username, string displayName, string country, string city, string street, string streetNumber, string avatarUrl, decimal balance) {
            this.Id = id;
            this.Username = username;
            this.DisplayName = displayName;
            this.City = city;
            this.Country = country;
            this.Street = street;
            this.StreetNumber = streetNumber;
            this.AvatarUrl = avatarUrl;
            this.Balance = balance;
        }

	}
}
