CREATE TABLE [AspNetRoles] (
	[Id] varchar(36) NOT NULL PRIMARY KEY, 
	[Name] nvarchar(30), 
	[NormalizedName] nvarchar(30), 
	[ConcurrencyStamp] varchar(36)
);

CREATE TABLE [AspNetRoleClaims] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[RoleId] varchar(36) NOT NULL, 
	[ClaimType] varchar(50), 
	[ClaimValue] nvarchar(200), 
	FOREIGN KEY ([RoleId])
		REFERENCES [AspNetRoles] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE [AspNetUsers] (
	[Id] varchar(36) NOT NULL PRIMARY KEY, 
	[UserName] nvarchar(30) NOT NULL, 
	[NormalizedUserName] nvarchar(30), 
	[Email] varchar(90), 
	[NormalizedEmail] varchar(90), 
	[EmailConfirmed] boolean NOT NULL, 
	[PasswordHash] varchar(90), 
	[SecurityStamp] varchar(36), 
	[ConcurrencyStamp] varchar(36), 
	[PhoneNumber] varchar(36), 
	[PhoneNumberConfirmed] boolean NOT NULL, 
	[TwoFactorEnabled] boolean NOT NULL, 
	[LockoutEnd] datetime, 
	[LockoutEnabled] boolean NOT NULL, 
	[AccessFailedCount] int NOT NULL, 
	[FullName] nvarchar(30), 
	[Avatar] varchar(255)
);

CREATE TABLE [AspNetUserClaims] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[UserId] varchar(36) NOT NULL, 
	[ClaimType] varchar(50), 
	[ClaimValue] nvarchar(200), 
	FOREIGN KEY ([UserId])
		REFERENCES [AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
	[LoginProvider] varchar(36) NOT NULL, 
	[ProviderKey] varchar(128) NOT NULL, 
	[ProviderDisplayName] nvarchar(128), 
	[UserId] varchar(36) NOT NULL,
	CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]), 
	FOREIGN KEY ([UserId])
		REFERENCES [AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
	[UserId] varchar(36) NOT NULL, 
	[RoleId] varchar(36) NOT NULL,
	CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]), 
	FOREIGN KEY ([RoleId])
		REFERENCES [AspNetRoles] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE, 
	FOREIGN KEY ([UserId])
		REFERENCES [AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
	[UserId] varchar(36) NOT NULL, 
	[LoginProvider] varchar(36) NOT NULL, 
	[Name] varchar(50) NOT NULL, 
	[Value] nvarchar(200),
	CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]), 
	FOREIGN KEY ([UserId])
		REFERENCES [AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE [Rooms] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Name] nvarchar(50) NOT NULL, 
	[AdminId] varchar(36) NOT NULL, 
	FOREIGN KEY ([AdminId])
		REFERENCES [AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
);

CREATE TABLE [Messages] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Content] nvarchar(500) NOT NULL, 
	[Timestamp] datetime NOT NULL, 
	[FromUserId] varchar(36), 
	[ToRoomId] int NOT NULL, 
	FOREIGN KEY ([FromUserId])
		REFERENCES [AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE NO ACTION, 
	FOREIGN KEY ([ToRoomId])
		REFERENCES [Rooms] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
);
