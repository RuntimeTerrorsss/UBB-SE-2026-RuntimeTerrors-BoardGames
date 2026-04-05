IF NOT EXISTS (SELECT * FROM _Migrations WHERE Name = 'SeedMockData')
BEGIN
	-- GAMES
	INSERT INTO Game (Name, PricePerDay) VALUES ('Catan', 1.99)
	INSERT INTO Game (Name, PricePerDay) VALUES ('Activity', 0.50)
	INSERT INTO Game (Name, PricePerDay) VALUES ('Chess', 0.86)
	INSERT INTO Game (Name, PricePerDay) VALUES ('Monopoly', 1.50)
	INSERT INTO Game (Name, PricePerDay) VALUES ('Scrabble', 0.75)
	INSERT INTO Game (Name, PricePerDay) VALUES ('Risk', 1.20)

	-- USERS (uid 0 = system, already inserted; uid 1..8 below)
	INSERT INTO [User] (AvatarUrl, DisplayName, UserName, Balance, Country, City, Street, StreetNumber) VALUES
	('alice.jpg',                          'Alice',   'alice99',   150.00, 'Romania', 'Cluj',      'Aleea Godeanu',      '23-25'),  -- uid 1
	('hamster.jpg',                        'Bob',     'bobby_b',    75.50, 'Romania', 'Oradea',    'Dorobantilor',       '27'),     -- uid 2
	('carol.jpg',                          'Carol',   'carol_xo',  200.00, 'Romania', 'Bucuresti', 'Nicolae Titulescu',  '26'),     -- uid 3
	('https://i.pravatar.cc/150?u=dan',    'Dan',     'dan_the_m',  50.00, 'Romania', 'Timisoara', 'Bulevardul Eroilor', '4'),      -- uid 4
	('https://i.pravatar.cc/150?u=eva',    'Eva',     'eva_plays', 320.00, 'Romania', 'Iasi',      'Strada Pacurari',    '88'),     -- uid 5
	('https://i.pravatar.cc/150?u=florin', 'Florin',  'flo99',      10.00, 'Romania', 'Brasov',    'Strada Lunga',       '12'),     -- uid 6
	('https://i.pravatar.cc/150?u=gabi',   'Gabi',    'gabi_g',    500.00, 'Romania', 'Cluj',      'Calea Turzii',       '3'),      -- uid 7
	('https://i.pravatar.cc/150?u=horia',  'Horia',   'horia_h',     0.00, 'Romania', 'Cluj',      'Strada Horea',       '7')       -- uid 8  (no conversations)

	-- REQUESTS
	-- rid 1: Bob    rents Catan    from Alice  (convo 1: Alice <-> Bob)
	-- rid 2: Bob    rents Activity from Carol  (convo 2: Bob   <-> Carol)
	-- rid 3: Dan    rents Chess    from Carol  (convo 3: Carol <-> Dan)   <- was Carol<->Alice, now Carol<->Dan
	-- rid 4: Dan    rents Monopoly from Alice  (convo 4: Dan   <-> Alice)
	-- rid 5: Eva    rents Scrabble from Bob    (convo 5: Eva   <-> Bob)
	-- rid 6: Florin rents Risk     from Carol  (convo 6: Florin <-> Carol)
	INSERT INTO Request (GameId, ClientId, OwnerId, StartDate, EndDate) VALUES
	(1, 2, 1, '2026-03-01', '2026-03-07'),   -- rid 1
	(2, 2, 3, '2026-03-10', '2026-03-15'),   -- rid 2
	(3, 4, 3, '2026-03-20', '2026-03-25'),   -- rid 3  changed: Dan rents from Carol
	(4, 4, 1, '2026-03-05', '2026-03-08'),   -- rid 4
	(5, 5, 2, '2026-03-12', '2026-03-14'),   -- rid 5
	(6, 6, 3, '2026-03-18', '2026-03-22')    -- rid 6

	-- TRANSACTIONS
	INSERT INTO [Payment] (RequestId, ClientId, OwnerId, Amount, DateOfTransaction, DateConfirmedBuyer, DateConfirmedSeller, PaymentMethod, [State], FilePath) VALUES
	(1, 2, 1, 11.94, '2026-03-01 10:00:00', '2026-03-01 10:00:00', NULL, 'CARD', 1, NULL),  -- tid 1
	(2, 2, 3,  2.50, '2026-03-11 14:30:00', NULL,                  NULL, 'CASH', 1, NULL),  -- tid 2
	(3, 4, 3,  5.16, '2026-03-20 09:00:00', '2026-03-20 09:00:00', NULL, 'CARD', 0, NULL),  -- tid 3  changed: Dan <-> Carol
	(4, 4, 1,  4.50, '2026-03-05 08:00:00', NULL,                  NULL, 'CASH', 1, NULL),  -- tid 4
	(5, 5, 2,  1.50, '2026-03-12 11:00:00', '2026-03-12 11:00:00', NULL, 'CARD', 1, NULL),  -- tid 5
	(6, 6, 3,  4.80, '2026-03-18 16:00:00', NULL,                  NULL, 'CASH', 0, NULL)   -- tid 6

	-- CONVERSATIONS
	-- cid 1: Alice  <-> Bob
	-- cid 2: Bob    <-> Carol
	-- cid 3: Carol  <-> Dan    <- changed
	-- cid 4: Dan    <-> Alice
	-- cid 5: Eva    <-> Bob
	-- cid 6: Florin <-> Carol
	INSERT INTO Conversation DEFAULT VALUES  -- cid 1
	INSERT INTO Conversation DEFAULT VALUES  -- cid 2
	INSERT INTO Conversation DEFAULT VALUES  -- cid 3
	INSERT INTO Conversation DEFAULT VALUES  -- cid 4
	INSERT INTO Conversation DEFAULT VALUES  -- cid 5
	INSERT INTO Conversation DEFAULT VALUES  -- cid 6

	-- CONVERSATION USERS
	INSERT INTO ConversationUser (cid, [uid], LastRead) VALUES
	(1, 1, '2026-03-05 12:00:00'),
	(1, 2, '2026-03-05 11:45:00'),
	(2, 2, '2026-03-12 09:00:00'),
	(2, 3, '2026-03-12 08:50:00'),
	(3, 3, '2026-03-22 10:00:00'),
	(3, 4, '2026-03-22 09:55:00'),  -- changed: Dan instead of Alice
	(4, 4, '2026-03-06 08:30:00'),
	(4, 1, '2026-03-06 08:20:00'),
	(5, 5, '2026-03-13 15:00:00'),
	(5, 2, '2026-03-13 14:45:00'),
	(6, 6, '2026-03-19 17:00:00'),
	(6, 3, '2026-03-19 16:50:00')
	-- uid 7 (Gabi) and uid 8 (Horia) have no conversations

	-- MESSAGES
	-- Conversation 1: Alice <-> Bob
	INSERT INTO [Message] (ConversationId, SenderId, ReceiverId, SentAt, MessageType) VALUES
	(1, 0, 0, '2026-03-01 08:55:00', 'SYSTEM'),          -- mid 1
	(1, 2, 1, '2026-03-01 09:00:00', 'RENTAL_REQUEST'),  -- mid 2
	(1, 1, 2, '2026-03-01 09:05:00', 'TEXT'),            -- mid 3
	(1, 2, 1, '2026-03-01 09:08:00', 'IMAGE'),           -- mid 4
	(1, 2, 1, '2026-03-01 09:10:00', 'TEXT'),            -- mid 5

	-- Conversation 2: Bob <-> Carol
	(2, 0, 0, '2026-03-10 09:55:00', 'SYSTEM'),          -- mid 6
	(2, 2, 3, '2026-03-10 10:00:00', 'RENTAL_REQUEST'),  -- mid 7
	(2, 3, 2, '2026-03-10 10:10:00', 'TEXT'),            -- mid 8
	(2, 2, 3, '2026-03-10 10:15:00', 'TEXT'),            -- mid 9

	-- Conversation 3: Carol <-> Dan
	(3, 0, 0, '2026-03-20 08:55:00', 'SYSTEM'),          -- mid 10
	(3, 4, 3, '2026-03-20 09:00:00', 'RENTAL_REQUEST'),  -- mid 11  changed: Dan asks Carol
	(3, 3, 4, '2026-03-20 09:05:00', 'TEXT'),            -- mid 12  changed: Carol replies to Dan
	(3, 4, 3, '2026-03-20 09:08:00', 'IMAGE'),           -- mid 13  changed: Dan sends image
	(3, 3, 4, '2026-03-20 09:12:00', 'TEXT'),            -- mid 14  changed: Carol replies to Dan

	-- Conversation 4: Dan <-> Alice
	(4, 0, 0, '2026-03-05 07:55:00', 'SYSTEM'),          -- mid 15
	(4, 4, 1, '2026-03-05 08:00:00', 'RENTAL_REQUEST'),  -- mid 16
	(4, 1, 4, '2026-03-05 08:10:00', 'TEXT'),            -- mid 17
	(4, 4, 1, '2026-03-05 08:20:00', 'TEXT'),            -- mid 18

	-- Conversation 5: Eva <-> Bob
	(5, 0, 0, '2026-03-12 10:55:00', 'SYSTEM'),          -- mid 19
	(5, 5, 2, '2026-03-12 11:00:00', 'RENTAL_REQUEST'),  -- mid 20
	(5, 2, 5, '2026-03-12 11:05:00', 'TEXT'),            -- mid 21
	(5, 5, 2, '2026-03-12 11:10:00', 'IMAGE'),           -- mid 22

	-- Conversation 6: Florin <-> Carol
	(6, 0, 0, '2026-03-18 15:55:00', 'SYSTEM'),          -- mid 23
	(6, 6, 3, '2026-03-18 16:00:00', 'RENTAL_REQUEST'),  -- mid 24
	(6, 3, 6, '2026-03-18 16:10:00', 'TEXT'),            -- mid 25
	(6, 6, 3, '2026-03-18 16:20:00', 'TEXT')             -- mid 26

	-- SYSTEM MESSAGES
	INSERT INTO SystemMessage (mid, Content) VALUES
	(1,  'New conversation'),
	(6,  'New conversation'),
	(10, 'New conversation'),
	(15, 'New conversation'),
	(19, 'New conversation'),
	(23, 'New conversation')

	-- RENTAL REQUEST MESSAGES
	INSERT INTO RentalRequestMessage (mid, RequestId, Content, IsResolved, IsAccepted) VALUES
	(2,  1, 'Hey, is Catan available March 1–7?',           0, 0),
	(7,  2, 'Can I borrow Activity March 10–15?',           0, 0),
	(11, 3, 'Hi, is Chess free from the 20th?',             0, 1),
	(16, 4, 'Would love to rent Monopoly for the weekend.', 0, 0),
	(20, 5, 'Is Scrabble available March 12–14?',           0, 0),
	(24, 6, 'Can I get Risk from the 18th to 22nd?',        0, 0)

	-- TEXT MESSAGES
	INSERT INTO TextMessage (mid, Content) VALUES
	(3,  'Yes, it''s free — it''s all yours!'),
	(5,  'Perfect, thanks a lot!'),
	(8,  'Sure, I can bring it over Monday.'),
	(9,  'Great, see you then!'),
	(12, 'Of course, come pick it up anytime.'),
	(14, 'Will be there Tuesday morning!'),
	(17, 'Sure, it''s available. Want to meet Saturday?'),
	(18, 'Saturday works perfectly for me.'),
	(21, 'Yep, I''ll have it ready by Tuesday.'),
	(25, 'Sounds good, just message me before you come.'),
	(26, 'Will do, cheers!')

	-- IMAGE MESSAGES
	INSERT INTO ImageMessage (mid, Content) VALUES
	(4,  'hamster.jpg'),   -- Bob sends pic in convo 1
	(13, 'hamster.jpg'),   -- Dan sends pic in convo 3
	(22, 'hamster.jpg')    -- Eva sends pic in convo 5

	INSERT INTO _Migrations (Name) VALUES ('SeedMockData')
END