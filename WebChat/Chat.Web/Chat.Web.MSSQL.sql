CREATE TABLE [dbo].[AspNetRoles] (
	[Id] varchar(36) NOT NULL PRIMARY KEY CLUSTERED, 
	[Name] nvarchar(30), 
	[NormalizedName] nvarchar(30), 
	[ConcurrencyStamp] varchar(36)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AspNetRoleClaims] (
	[Id] int NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED, 
	[RoleId] varchar(36) NOT NULL, 
	[ClaimType] varchar(50), 
	[ClaimValue] nvarchar(200), 
	FOREIGN KEY ([RoleId])
		REFERENCES [dbo].[AspNetRoles] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AspNetUsers] (
	[Id] varchar(36) NOT NULL PRIMARY KEY CLUSTERED, 
	[UserName] nvarchar(30) NOT NULL, 
	[NormalizedUserName] nvarchar(30), 
	[Email] varchar(90), 
	[NormalizedEmail] varchar(90), 
	[EmailConfirmed] bit NOT NULL, 
	[PasswordHash] varchar(90), 
	[SecurityStamp] varchar(36), 
	[ConcurrencyStamp] varchar(36), 
	[PhoneNumber] varchar(36), 
	[PhoneNumberConfirmed] bit NOT NULL, 
	[TwoFactorEnabled] bit NOT NULL, 
	[LockoutEnd] datetimeoffset(7), 
	[LockoutEnabled] bit NOT NULL, 
	[AccessFailedCount] int NOT NULL, 
	[FullName] nvarchar(30), 
	[Avatar] varchar(255)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AspNetUserClaims] (
	[Id] int NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED, 
	[UserId] varchar(36) NOT NULL, 
	[ClaimType] varchar(50), 
	[ClaimValue] nvarchar(200), 
	FOREIGN KEY ([UserId])
		REFERENCES [dbo].[AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AspNetUserLogins] (
	[LoginProvider] varchar(36) NOT NULL, 
	[ProviderKey] varchar(128) NOT NULL, 
	[ProviderDisplayName] nvarchar(128), 
	[UserId] varchar(36) NOT NULL,
	CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider], [ProviderKey]), 
	FOREIGN KEY ([UserId])
		REFERENCES [dbo].[AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AspNetUserRoles] (
	[UserId] varchar(36) NOT NULL, 
	[RoleId] varchar(36) NOT NULL,
	CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId], [RoleId]), 
	FOREIGN KEY ([RoleId])
		REFERENCES [dbo].[AspNetRoles] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE, 
	FOREIGN KEY ([UserId])
		REFERENCES [dbo].[AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[AspNetUserTokens] (
	[UserId] varchar(36) NOT NULL, 
	[LoginProvider] varchar(36) NOT NULL, 
	[Name] varchar(50) NOT NULL, 
	[Value] nvarchar(200),
	CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId], [LoginProvider], [Name]), 
	FOREIGN KEY ([UserId])
		REFERENCES [dbo].[AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Rooms] (
	[Id] int NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED, 
	[Name] nvarchar(50) NOT NULL, 
	[AdminId] varchar(36) NOT NULL, 
	FOREIGN KEY ([AdminId])
		REFERENCES [dbo].[AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Messages] (
	[Id] int NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED, 
	[Content] nvarchar(500) NOT NULL, 
	[Timestamp] datetime2(7) NOT NULL, 
	[FromUserId] varchar(36), 
	[ToRoomId] int NOT NULL, 
	FOREIGN KEY ([FromUserId])
		REFERENCES [dbo].[AspNetUsers] ([Id])
		ON UPDATE NO ACTION ON DELETE NO ACTION, 
	FOREIGN KEY ([ToRoomId])
		REFERENCES [dbo].[Rooms] ([Id])
		ON UPDATE NO ACTION ON DELETE CASCADE
) ON [PRIMARY]
GO
