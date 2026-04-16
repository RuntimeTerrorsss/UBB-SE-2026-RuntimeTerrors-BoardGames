/* LOGS */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '_Migrations')
CREATE TABLE _Migrations (
    mid INTEGER PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL,
    RanAt DATETIME DEFAULT GETDATE()
)
/* MOCKS */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Game')
CREATE TABLE Game(
	gid INTEGER PRIMARY KEY IDENTITY(1,1),
	Name VARCHAR(50) NOT NULL,
	PricePerDay DECIMAL NOT NULL
)

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'User')
CREATE TABLE [User] (
	[uid] INTEGER PRIMARY KEY IDENTITY(0,1), /* USER 0 WILL BE THE SYSTEM */
	AvatarUrl VARCHAR(512) DEFAULT NULL,
	DisplayName VARCHAR(50),
	UserName VARCHAR(50),
	Balance DECIMAL DEFAULT 0,
	Country NVARCHAR(50),
	City NVARCHAR(50),
	Street NVARCHAR(50),
	StreetNumber NVARCHAR(50),
)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Request') 
CREATE TABLE Request ( 
	rid INTEGER PRIMARY KEY IDENTITY(1,1),
	GameId INTEGER FOREIGN KEY REFERENCES Game(gid),
	ClientId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	OwnerId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	StartDate DATE,
	EndDate DATE
)
/* Payment */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payment')
CREATE TABLE [Payment] (
	tid INTEGER PRIMARY KEY IDENTITY(1,1),
	RequestId INTEGER FOREIGN KEY REFERENCES Request(rid),
	ClientId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	OwnerId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	Amount DECIMAL CHECK(Amount >= 0),
	[DateOfTransaction] DATETIME,	/*Greatest between the 2 below for cash*/
	DateConfirmedBuyer DATETIME,	/*I suppose null if card*/
	DateConfirmedSeller DATETIME,	/*Null if card*/
	PaymentMethod VARCHAR(5) CHECK(PaymentMethod = 'CASH' OR PaymentMethod = 'CARD'),
	[State] INTEGER,
	FilePath VARCHAR(500)
)

/* MESSAGE STUFF */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Conversation') 
CREATE TABLE [Conversation] (
	cid INTEGER PRIMARY KEY IDENTITY(1,1)
)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConversationUser') 
CREATE TABLE [ConversationUser] (
	cid INTEGER FOREIGN KEY REFERENCES Conversation(cid),
	[uid] INTEGER FOREIGN KEY REFERENCES [User](uid),
	LastRead DATETIME
	PRIMARY KEY (cid, [uid])
)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Message')
CREATE TABLE [Message] (
	mid INTEGER PRIMARY KEY IDENTITY(1,1),
	ConversationId INTEGER FOREIGN KEY REFERENCES [Conversation](cid),
	SenderId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	ReceiverId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	SentAt DATETIME,
	MessageType VARCHAR(20) /* May be nice to add a check restriction */
)

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RentalRequestMessage')
CREATE TABLE RentalRequestMessage (
	mid INTEGER PRIMARY KEY FOREIGN KEY REFERENCES [Message](mid),
	RequestId INTEGER FOREIGN KEY REFERENCES Request(rid),
	Content VARCHAR(512),
	IsResolved BIT DEFAULT 0,
	IsAccepted BIT DEFAULT 0
)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImageMessage')
CREATE TABLE ImageMessage (
	mid INTEGER PRIMARY KEY FOREIGN KEY REFERENCES [Message](mid),
	Content VARCHAR(250) /*location*/
)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TextMessage')
CREATE TABLE TextMessage (
	mid INTEGER PRIMARY KEY FOREIGN KEY REFERENCES [Message](mid),
	Content VARCHAR(512)
)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CashAgreementMessage')
CREATE TABLE CashAgreementMessage (
	mid INTEGER PRIMARY KEY FOREIGN KEY REFERENCES [Message](mid),
	SellerId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	BuyerId INTEGER FOREIGN KEY REFERENCES [User]([uid]),
	PaymentId INTEGER FOREIGN KEY REFERENCES [Payment](tid),
	Content VARCHAR(512),
	AcceptedBySeller BIT DEFAULT 0,
	AcceptedByBuyer BIT DEFAULT 0
)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemMessage')
CREATE TABLE SystemMessage (
	mid INTEGER PRIMARY KEY FOREIGN KEY REFERENCES [Message](mid),
	Content VARCHAR(512)
)

IF NOT EXISTS (SELECT 1 FROM [User] WHERE [uid] = 0)
INSERT INTO [User] (AvatarUrl, DisplayName, UserName, Balance, Country, City, Street , StreetNumber) VALUES
('https://i.pravatar.cc/150?u=alice', 'Alice',   'alice99',  150.00, 'Romania', 'Cluj', 'Aleea Godeanu', '23-25')

INSERT INTO _Migrations (Name) VALUES ('InitialSchema')

ALTER TABLE Game
ALTER COLUMN PricePerDay DECIMAL(18, 2);

ALTER TABLE [User]
ALTER COLUMN Balance DECIMAL(18, 2);