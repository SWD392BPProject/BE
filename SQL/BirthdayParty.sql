USE [master]
GO
/****** Object:  Database [SWD392-BirthdayParty]    Script Date: 2/3/2024 12:07:50 PM ******/
CREATE DATABASE [SWD392-BirthdayParty]

GO
ALTER DATABASE [SWD392-BirthdayParty] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [SWD392-BirthdayParty].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [SWD392-BirthdayParty] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET ARITHABORT OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET  DISABLE_BROKER 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET  MULTI_USER 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SWD392-BirthdayParty] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [SWD392-BirthdayParty] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [SWD392-BirthdayParty] SET QUERY_STORE = OFF
GO
USE [SWD392-BirthdayParty]
GO
/****** Object:  Table [dbo].[Bookings]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bookings](
	[BookingID] [INT] IDENTITY(1,1) NOT NULL,
	[UserID] [INT] NULL,
	[PartyID] [INT] NULL,
	[PartyName] [NVARCHAR](255) NULL,
	[RoomID] [INT] NULL,
	[RoomName] [NVARCHAR](255) NULL,
	[RoomPrice] [INT] NULL,
	[SlotID] [INT] NULL,
	[SlotTimeStart] [TIME](7) NULL,
	[SlotTimeEnd] [TIME](7) NULL,
	[MenuID] [INT] NULL,
	[MenuName] [NVARCHAR](255) NULL,
	[MenuPrice] [INT] NULL,
	[DiningTable] [INT] NULL,
	[PaymentAmount] [INT] NULL,
	[Description] [NVARCHAR](MAX) NULL,
	[BookingDate] [date] NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](10) NOT NULL,
 CONSTRAINT [PK_Bookings] PRIMARY KEY CLUSTERED 
(
	[BookingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Chats]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Chats](
	[ChatID] [INT] NOT NULL,
	[HostUserID] [INT] NULL,
	[CustomerUserID] [INT] NULL,
	[PartyID] [INT] NULL,
	[Message] [text] NULL,
	[Timestamp] [timestamp] NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Chats] PRIMARY KEY CLUSTERED 
(
	[ChatID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Feedbacks]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedbacks](
	[FeedbackID] [nchar](10) NOT NULL,
	[UserID] [INT] NULL,
	[BookingID] [INT] NULL,
	[Rating] [float] NULL,
	[Comment] [text] NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Feedbacks] PRIMARY KEY CLUSTERED 
(
	[FeedbackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Menus]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Menus](
	[MenuID] [INT] IDENTITY(1,1) NOT NULL,
	[HostUserID] [INT] NULL,
	[MenuName] [NVARCHAR](255) NOT NULL,
	[Description] [NVARCHAR](MAX) NULL,
	[Image] [NVARCHAR](255) NULL,
	[Price] [INT] NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Menus] PRIMARY KEY CLUSTERED 
(
	[MenuID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE TABLE [dbo].[MenuParty](
	[MenuPartyID] [INT] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[MenuID] [INT] NOT NULL,
	[PartyID] [INT] NOT NULL
)
GO
/****** Object:  Table [dbo].[PackageBookings]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PackageBookings](
	[PackageBookingID] [nchar](10) NOT NULL,
	[HostUserID] [INT] NULL,
	[PackageID] [INT] NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_PackageBookings] PRIMARY KEY CLUSTERED 
(
	[PackageBookingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Packages]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Packages](
	[PackageID] [INT] IDENTITY(1,1) NOT NULL,
	[AdminUserID] [INT] NULL,
	[PackageName] [nvarchar](255) NULL,
	[ActiveDays] [INT] NULL,
	[Description] [NVARCHAR](MAX) NULL,
	[Image] [NVARCHAR](255) NULL,
	[Price] [INT] NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Packages] PRIMARY KEY CLUSTERED 
(
	[PackageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Parties]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Parties](
	[PartyID] [INT] IDENTITY(1,1) NOT NULL,
	[HostUserID] [INT] NULL,
	[MonthViewed] [INT] NULL,
	[PartyName] [nvarchar](255) NULL,
	[Image] [NVARCHAR](255) NULL,
	[Address] [NVARCHAR](255) NULL,
	[Type] [NVARCHAR](255) NULL,
	[MenuList] [NVARCHAR](255) NULL,
	[Description] [NVARCHAR](MAX) NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Parties] PRIMARY KEY CLUSTERED 
(
	[PartyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rooms]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rooms](
	[RoomID] [INT] IDENTITY(1,1) NOT NULL,
	[RoomName] [nvarchar](50) NULL,
	[HostUserID] [INT] NULL,
	[Price] [INT] NULL,
	[MinPeople] [INT] NULL,
	[MaxPeople] [INT] NULL,
	[Image] [NVARCHAR](255) NULL,
	[Type] [NVARCHAR](255) NULL,
	[Description] [NVARCHAR](MAX) NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Rooms] PRIMARY KEY CLUSTERED 
(
	[RoomID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Slots]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Slots](
	[SlotID] [INT] IDENTITY(1,1) NOT NULL,
	[RoomID] [INT] NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
 CONSTRAINT [PK_Slots] PRIMARY KEY CLUSTERED 
(
	[SlotID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [INT] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[Password] [NVARCHAR](MAX) NULL,
	[PhoneNumber] [varchar](13) NULL,
	[Role] [varchar](10) NOT NULL,
	[Image] [varchar](255) NULL,
	[CreateDate] [date],
	[LastUpdateDate] [date],
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vouchers]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vouchers](
	[VoucherID] [INT] IDENTITY(1,1) NOT NULL,
	[VoucherCode] [NVARCHAR](255) NULL,
	[PackageID] [INT] NULL,
	[DiscountAmount] [INT] NULL,
	[DiscountPercent] [INT] NULL,
	[ExpiryDate] [date] NULL,
	[DiscountMax] [INT] NULL,
 CONSTRAINT [PK_Voucher] PRIMARY KEY CLUSTERED 
(
	[VoucherID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VoucherUsages]    Script Date: 2/3/2024 12:07:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VoucherUsages](
	[VoucherUsageID] [nchar](10) NOT NULL,
	[VoucherID] [INT] NULL,
	[HostUserID] [INT] NULL,
	[CreateDate] [date] NULL,
	[LastUpdateDate] [date] NULL,
	[Status] [varchar](7) NOT NULL,
 CONSTRAINT [PK_VoucherUsages] PRIMARY KEY CLUSTERED 
(
	[VoucherUsageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK_Bookings_Menus] FOREIGN KEY([MenuID])
REFERENCES [dbo].[Menus] ([MenuID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Menus]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK_Bookings_Parties] FOREIGN KEY([PartyID])
REFERENCES [dbo].[Parties] ([PartyID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Parties]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK_Bookings_Slots] FOREIGN KEY([SlotID])
REFERENCES [dbo].[Slots] ([SlotID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Slots]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK_Bookings_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Users]
GO
ALTER TABLE [dbo].[Chats]  WITH CHECK ADD  CONSTRAINT [FK_Chats_Parties] FOREIGN KEY([PartyID])
REFERENCES [dbo].[Parties] ([PartyID])
GO
ALTER TABLE [dbo].[Chats] CHECK CONSTRAINT [FK_Chats_Parties]
GO
ALTER TABLE [dbo].[Chats]  WITH CHECK ADD  CONSTRAINT [FK_Chats_Users] FOREIGN KEY([HostUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Chats] CHECK CONSTRAINT [FK_Chats_Users]
GO
ALTER TABLE [dbo].[Chats]  WITH CHECK ADD  CONSTRAINT [FK_Chats_Users1] FOREIGN KEY([CustomerUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Chats] CHECK CONSTRAINT [FK_Chats_Users1]
GO
ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD  CONSTRAINT [FK_Feedbacks_Bookings] FOREIGN KEY([BookingID])
REFERENCES [dbo].[Bookings] ([BookingID])
GO
ALTER TABLE [dbo].[Feedbacks] CHECK CONSTRAINT [FK_Feedbacks_Bookings]
GO
ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD  CONSTRAINT [FK_Feedbacks_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Feedbacks] CHECK CONSTRAINT [FK_Feedbacks_Users]
GO
ALTER TABLE [dbo].[PackageBookings]  WITH CHECK ADD  CONSTRAINT [FK_PackageBookings_Packages] FOREIGN KEY([PackageID])
REFERENCES [dbo].[Packages] ([PackageID])
GO
ALTER TABLE [dbo].[PackageBookings] CHECK CONSTRAINT [FK_PackageBookings_Packages]
GO
ALTER TABLE [dbo].[PackageBookings]  WITH CHECK ADD  CONSTRAINT [FK_PackageBookings_Users] FOREIGN KEY([HostUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[PackageBookings] CHECK CONSTRAINT [FK_PackageBookings_Users]
GO
ALTER TABLE [dbo].[Packages]  WITH CHECK ADD  CONSTRAINT [FK_Packages_Users] FOREIGN KEY([AdminUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Packages] CHECK CONSTRAINT [FK_Packages_Users]
GO
ALTER TABLE [dbo].[Parties]  WITH CHECK ADD  CONSTRAINT [FK_Parties_Users] FOREIGN KEY([HostUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Parties] CHECK CONSTRAINT [FK_Parties_Users]
GO
ALTER TABLE [dbo].[Rooms]  WITH CHECK ADD  CONSTRAINT [FK_Rooms_Users] FOREIGN KEY([HostUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Slots]  WITH CHECK ADD  CONSTRAINT [FK_Slots_Rooms] FOREIGN KEY([RoomID])
REFERENCES [dbo].[Rooms] ([RoomID])
GO
ALTER TABLE [dbo].[Slots] CHECK CONSTRAINT [FK_Slots_Rooms]
GO
ALTER TABLE [dbo].[VoucherUsages]  WITH CHECK ADD  CONSTRAINT [FK_VoucherUsages_Users] FOREIGN KEY([HostUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[VoucherUsages] CHECK CONSTRAINT [FK_VoucherUsages_Users]
GO
ALTER TABLE [dbo].[VoucherUsages]  WITH CHECK ADD  CONSTRAINT [FK_VoucherUsages_Vouchers] FOREIGN KEY([VoucherID])
REFERENCES [dbo].[Vouchers] ([VoucherID])
GO
ALTER TABLE [dbo].[VoucherUsages] CHECK CONSTRAINT [FK_VoucherUsages_Vouchers]
GO
ALTER TABLE [dbo].[MenuParty]  WITH CHECK ADD  CONSTRAINT [FK_MenuParty_Menu] FOREIGN KEY([MenuID])
REFERENCES [dbo].[Menus] ([MenuID])
GO
ALTER TABLE [dbo].[MenuParty] CHECK CONSTRAINT [FK_MenuParty_Menu]
GO
ALTER TABLE [dbo].[MenuParty]  WITH CHECK ADD  CONSTRAINT [FK_MenuParty_Party] FOREIGN KEY([PartyID])
REFERENCES [dbo].Parties ([PartyID])
GO
ALTER TABLE [dbo].[MenuParty] CHECK CONSTRAINT [FK_MenuParty_Party]
GO

SET IDENTITY_INSERT [dbo].[Users] ON 
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (1, N'Trương Quỳnh Anh', N'account01@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900474', N'Admin', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (2, N'Trần Ngọc Huy', N'account02@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900475', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (3, N'Trần Gia Bảo', N'account03@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900476', N'Host', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (4, N'Nguyễn Văn Tài', N'account04@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900477', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (5, N'Nguyễn Thị Hoài', N'account05@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900478', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (6, N'Nguyễn Văn Khánh', N'account06@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900479', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (7, N'Trần Văn Tuấn', N'account07@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900480', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (8, N'Nguyễn Văn Toàn', N'account08@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900481', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (9, N'Nguyễn Văn Minh', N'account09@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900482', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
INSERT [dbo].[Users] ([UserID], [FullName], [Email], [Password], [PhoneNumber], [Role], [Image], [CreateDate], [LastUpdateDate], [Status]) VALUES (10, N'Nguyễn Văn Lý', N'account10@gmail.com', N'$2a$11$La/puOON4nwg8OUlgQQ0V.ok9Qz6nqMOk5MP3RlBHsytvM2Ls/14S', N'0968900483', N'User', NULL, CAST(N'2024-02-22' AS Date), CAST(N'2024-02-22' AS Date), N'Active')
SET IDENTITY_INSERT [dbo].[Users] OFF

GO
SET IDENTITY_INSERT [dbo].[Parties] ON 
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (5, 3,N'1,2', 18, N'Tiệc sinh nhật 18 tuổi LUXURY', N'999c9e64-5917-4613-bf86-565b3f5e6360.jpg', N'Hồ Chí Minh', N'SINHNHAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (6, 3,N'1,2', 758, N'Tiệc sinh nhật chuẩn JAPAN', N'7f1b8cef-9900-44ee-bd66-904e1af28e70.jpg', N'Hồ Chí Minh', N'SINHNHAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (7, 3,N'1,2', 1218, N'Tiệc liên hoan bạn bè SUPER LUXUTY [100% nhận quà]', N'c299ad8a-21d0-4a9d-9b51-9d10f9c4e94e.jpg', N'Hồ Chí Minh', N'SINHNHAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (8, 3,N'1,2', 1528, N'Tiệc sinh nhật BEAUTY-GIRL HX-LUXURY', N'391741cb-6236-402f-baf3-18d1fc426919.jpg', N'Hồ Chí Minh', N'SINHNHAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (9, 3,N'1,2', 128, N'Liên hoan bạn bè giữa năm LUXURY', N'a149337f-7b5a-4e4a-a2cd-7769d11fd279.jpg', N'Hồ Chí Minh', N'LIENHOAN', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (10, 3,N'1,2', 1328, N'Gói sinh nhật VIP cho gia đình', N'ebfc7a67-47c8-45a8-8119-e61d7c7edaae.jpg', N'Hồ Chí Minh', N'SINHNHAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (11, 3,N'1,2', 474, N'Tiệc người trời cho phái nữ LUXURY', N'44c60921-ea99-440a-82f1-f21409b8539d.jpg', N'Hồ Chí Minh', N'NGOAITROI', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (12, 3,N'1,2', 52, N'Tiệc sinh nhật PRE-USED H1', N'e5e0c22f-215d-4722-a469-6475ca63ae12.jpg', N'Hồ Chí Minh', N'SINHNHAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (13, 3,N'1,2', 245, N'Tiệc họp mặt bạn bè DELUX-H22', N'26974e5f-a1d6-4798-8409-4f4859034125.jpg', N'Hồ Chí Minh', N'HOPMAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (14, 3,N'1,2', 988, N'Tiệc liên hoan trẻ em LUXURY-HU8', N'ca2c4471-f7ad-4265-9ad7-003a9a69cedb.jpg', N'Hồ Chí Minh', N'LIENHOAN', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (15, 3,N'1,2', 125, N'Tiệc liên hoan vui chơi LUXURY-X1', N'f451b709-d48a-487f-a8eb-b6d79c2f811c.jpg', N'Hồ Chí Minh', N'KHUVUICHOI', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (16, 3,N'1,2', 865, N'Tiệc họp mặt LUXURY-X2 Utra', N'c5d485e8-dc1a-40a1-8158-05df078a8408.jpg', N'Hồ Chí Minh', N'HOPMAT', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (17, 3,N'1,2', 245, N'Tiệc họp bạn bè Extra [Nhận quà]', N'd2b0c361-19f0-4b84-9db5-dbc58f73a9c9.jpg', N'Hồ Chí Minh', N'LIENHOAN', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (18, 3,N'1,2', 1223, N'Tiệc họp bạn bè hấp dẫn [99% Nhận quà]', N'82c652ff-741b-4a91-9b45-3fd13057651e.jpg', N'Hồ Chí Minh', N'LIENHOAN', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Parties] ([PartyID], [HostUserID],[MenuList], [MonthViewed], [PartyName], [Image], [Address], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (19, 3,N'1,2', 178, N'Tiệc liên hoan bạn bè [Extra VIP]', N'df5443e3-8a98-421e-ae7a-3c26dd68e970.jpg', N'Hồ Chí Minh', N'LIENHOAN', N'Một sinh nhật thật ý nghĩa và đặc biệt để đánh dấu cột mốc quan trọng của các thiên thần nhỏ luôn là điều bố mẹ băn khoăn? Với sự đa dạng trong các gói tiệc sinh nhật, tiNi hứa hẹn sẽ mang đến cho các thiên thần nhỏ một bữa tiệc đầy bất ngờ và tràn ngập những khoảnh khắc đáng nhớ.', CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
SET IDENTITY_INSERT [dbo].[Parties] OFF
GO


SET IDENTITY_INSERT [dbo].[Rooms] ON 
INSERT [dbo].[Rooms] ([RoomID], [RoomName], [HostUserID], [Price], [MinPeople], [MaxPeople], [Image], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (1, N'Phòng Sofa 1A', 3, 1000000, 1, 10, N'1e0e7606-2177-4dad-9699-983313614ba1.jpg', N'SINHNHAT', N'Phòng hội trường 50-100 chỗ. Đối với không gian tổ chức nhỏ chỉ từ 50-100 chỗ, đây sẽ không gian phù hợp với các buổi tổ chức nhỏ như họp mặt, CLB, đào tạo,… với sơ đồ thường thấy nhất là lớp học hoặc nhóm nhỏ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Rooms] ([RoomID], [RoomName], [HostUserID], [Price], [MinPeople], [MaxPeople], [Image], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (2, N'Phòng Sofa 2A', 3, 1200000, 7, 20, N'e207e208-a0c1-4ebf-a7aa-91d1c3ed6958.jpg', N'SINHNHAT', N'Phòng hội trường 50-100 chỗ. Đối với không gian tổ chức nhỏ chỉ từ 50-100 chỗ, đây sẽ không gian phù hợp với các buổi tổ chức nhỏ như họp mặt, CLB, đào tạo,… với sơ đồ thường thấy nhất là lớp học hoặc nhóm nhỏ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Rooms] ([RoomID], [RoomName], [HostUserID], [Price], [MinPeople], [MaxPeople], [Image], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (3, N'Phòng Sofa 3A', 3, 1300000, 20, 30, N'9a3fac12-df04-49ad-9382-25f9d243ea23.jpg', N'SINHNHAT', N'Phòng hội trường 50-100 chỗ. Đối với không gian tổ chức nhỏ chỉ từ 50-100 chỗ, đây sẽ không gian phù hợp với các buổi tổ chức nhỏ như họp mặt, CLB, đào tạo,… với sơ đồ thường thấy nhất là lớp học hoặc nhóm nhỏ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Rooms] ([RoomID], [RoomName], [HostUserID], [Price], [MinPeople], [MaxPeople], [Image], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (4, N'Phòng Sofa 4A', 3, 1100000, 1, 10, N'de6c017b-4f73-4d4a-8966-996d212cb070.jpg', N'SINHNHAT', N'Phòng hội trường 50-100 chỗ. Đối với không gian tổ chức nhỏ chỉ từ 50-100 chỗ, đây sẽ không gian phù hợp với các buổi tổ chức nhỏ như họp mặt, CLB, đào tạo,… với sơ đồ thường thấy nhất là lớp học hoặc nhóm nhỏ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Rooms] ([RoomID], [RoomName], [HostUserID], [Price], [MinPeople], [MaxPeople], [Image], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (5, N'Phòng Sofa 5A', 3, 2100000, 40, 50, N'97217835-182f-48df-a550-a5adf026a101.jpg', N'SINHNHAT,KHUVUICHOI,LIENHOAN', N'Phòng hội trường 50-100 chỗ. Đối với không gian tổ chức nhỏ chỉ từ 50-100 chỗ, đây sẽ không gian phù hợp với các buổi tổ chức nhỏ như họp mặt, CLB, đào tạo,… với sơ đồ thường thấy nhất là lớp học hoặc nhóm nhỏ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
INSERT [dbo].[Rooms] ([RoomID], [RoomName], [HostUserID], [Price], [MinPeople], [MaxPeople], [Image], [Type], [Description], [CreateDate], [LastUpdateDate], [Status]) VALUES (6, N'Phòng Sofa 6A', 3, 1900000, 30, 40, N'97217835-182f-48df-a550-a5adf026a102.jpg', N'SINHNHAT,KHUVUICHOI,LIENHOAN', N'Phòng hội trường 50-100 chỗ. Đối với không gian tổ chức nhỏ chỉ từ 50-100 chỗ, đây sẽ không gian phù hợp với các buổi tổ chức nhỏ như họp mặt, CLB, đào tạo,… với sơ đồ thường thấy nhất là lớp học hoặc nhóm nhỏ.', CAST(N'2024-02-24' AS Date), CAST(N'2024-02-24' AS Date), N'Active')
SET IDENTITY_INSERT [dbo].[Rooms] OFF
GO

SET IDENTITY_INSERT [dbo].[Slots] ON 
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (4, 1, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time))
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (5, 2, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time))
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (6, 3, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time))
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (7, 4, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time))
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (8, 5, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time))
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (1, 6, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time))
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (2, 6, CAST(N'13:00:00' AS Time), CAST(N'16:00:00' AS Time))
INSERT [dbo].[Slots] ([SlotID], [RoomID], [StartTime], [EndTime]) VALUES (3, 6, CAST(N'17:00:00' AS Time), CAST(N'20:00:00' AS Time))
SET IDENTITY_INSERT [dbo].[Slots] OFF
GO


SET IDENTITY_INSERT [dbo].[Packages] ON 
INSERT [dbo].[Packages] ([PackageID], [AdminUserID], [PackageName], [ActiveDays], [Description], [Image], [Price], [CreateDate], [LastUpdateDate], [Status]) VALUES (3, 1, N'Gói VIP', 90, N'Gói dịch vụ tổng hợp sử dụng dịch vụ cao cấp của Kid Booking với hàng ngàng tính năng vượt trội. Miễn phí truy cập và đăng tải dịch vụ Booking của cá nhân hoặc tổ chức. Mang đến trải nghiệm dịch vụ tuyệt vời cho các Host Party trên toàn quốc.', N'2241b3aa-7d13-4506-88bd-900a5e9a3424.jpg', 2700000, CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Packages] ([PackageID], [AdminUserID], [PackageName], [ActiveDays], [Description], [Image], [Price], [CreateDate], [LastUpdateDate], [Status]) VALUES (4, 1, N'Gói MEMBER', 30, N'Gói dịch vụ tổng hợp sử dụng dịch vụ cao cấp của Kid Booking với hàng ngàng tính năng vượt trội. Miễn phí truy cập và đăng tải dịch vụ Booking của cá nhân hoặc tổ chức. Mang đến trải nghiệm dịch vụ tuyệt vời cho các Host Party trên toàn quốc.', N'40573b1c-2621-4900-9ed1-1df17cdfbf81.jpg', 1000000, CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
INSERT [dbo].[Packages] ([PackageID], [AdminUserID], [PackageName], [ActiveDays], [Description], [Image], [Price], [CreateDate], [LastUpdateDate], [Status]) VALUES (5, 1, N'Gói PREMIUM', 365, N'Gói dịch vụ tổng hợp sử dụng dịch vụ cao cấp của Kid Booking với hàng ngàng tính năng vượt trội. Miễn phí truy cập và đăng tải dịch vụ Booking của cá nhân hoặc tổ chức. Mang đến trải nghiệm dịch vụ tuyệt vời cho các Host Party trên toàn quốc.', N'09c793c4-c5ef-47e4-bc59-b4cef241188d.jpg', 5400000, CAST(N'2024-02-25' AS Date), CAST(N'2024-02-25' AS Date), N'Active')
SET IDENTITY_INSERT [dbo].[Packages] OFF
GO

SET IDENTITY_INSERT [dbo].[Menus] ON 
INSERT [dbo].[Menus] ([MenuID], [HostUserID], [MenuName], [Description], [Image], [Price], [CreateDate], [LastUpdateDate], [Status]) VALUES (1, 3, N'Combo gà siêu quậy', N'Món 1: Gà chiên nước mắm 
Món 2: Sụn gà chiên giòn
Món 3: Gà rán phomai', N'a2fea70c-d6c8-40d7-b963-f1facfa07cf8.png', 500000, CAST(N'2024-02-26' AS Date), CAST(N'2024-02-26' AS Date), N'Active')
INSERT [dbo].[Menus] ([MenuID], [HostUserID], [MenuName], [Description], [Image], [Price], [CreateDate], [LastUpdateDate], [Status]) VALUES (2, 3, N'Combo cá 4 món', N'Món 1: Cá chiên nước mắm 
Món 2: Cá kho tiêu
Món 3: Bánh canh cá', N'07f96bc1-d4c4-4ac5-874a-ee854da255dd.jpg', 600000, CAST(N'2024-02-26' AS Date), CAST(N'2024-02-26' AS Date), N'Active')
INSERT [dbo].[Menus] ([MenuID], [HostUserID], [MenuName], [Description], [Image], [Price], [CreateDate], [LastUpdateDate], [Status]) VALUES (3, 3, N'Combo thịt nướng BBQ', N'Món 1: Thịt nướng sa tế
Món 2: Thịt nướng phomai
Món 3: Thịt luộc', N'82854107-2d02-4a25-8c11-edd808d5e37d.jpg', 800000, CAST(N'2024-02-26' AS Date), CAST(N'2024-02-26' AS Date), N'Active')
SET IDENTITY_INSERT [dbo].[Menus] OFF
GO

SET IDENTITY_INSERT [dbo].[MenuParty] ON 
INSERT [dbo].[MenuParty] ([MenuPartyID], [MenuID], [PartyID]) VALUES (1, 1, 5);
INSERT [dbo].[MenuParty] ([MenuPartyID], [MenuID], [PartyID]) VALUES (2, 2, 5);
INSERT [dbo].[MenuParty] ([MenuPartyID], [MenuID], [PartyID]) VALUES (3, 3, 5);
SET IDENTITY_INSERT [dbo].[MenuParty] OFF
GO

SET IDENTITY_INSERT [dbo].[Bookings] ON 

INSERT [dbo].[Bookings] ([BookingID], [UserID], [PartyID], [PartyName], [RoomID], [RoomName], [RoomPrice], [SlotID], [SlotTimeStart], [SlotTimeEnd], [MenuID], [MenuName], [MenuPrice], [DiningTable], [PaymentAmount], [Description], [BookingDate], [CreateDate], [LastUpdateDate], [Status]) VALUES (1, 2, 5, N'Tiệc sinh nhật 18 tuổi LUXURY', 6, N'Phòng Sofa 6A', 1900000, 1, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time), 1, N'Combo gà siêu quậy', 500000, 5, 4400000, NULL, CAST(N'2024-02-28' AS Date), CAST(N'2024-02-27' AS Date), CAST(N'2024-02-27' AS Date), N'Create')
INSERT [dbo].[Bookings] ([BookingID], [UserID], [PartyID], [PartyName], [RoomID], [RoomName], [RoomPrice], [SlotID], [SlotTimeStart], [SlotTimeEnd], [MenuID], [MenuName], [MenuPrice], [DiningTable], [PaymentAmount], [Description], [BookingDate], [CreateDate], [LastUpdateDate], [Status]) VALUES (2, 2, 5, N'Tiệc sinh nhật 18 tuổi LUXURY', 5, N'Phòng Sofa 5A', 2100000, 8, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time), 1, N'Combo gà siêu quậy', 500000, 2, 3100000, NULL, CAST(N'2024-03-01' AS Date), CAST(N'2024-02-27' AS Date), CAST(N'2024-02-27' AS Date), N'Create')
INSERT [dbo].[Bookings] ([BookingID], [UserID], [PartyID], [PartyName], [RoomID], [RoomName], [RoomPrice], [SlotID], [SlotTimeStart], [SlotTimeEnd], [MenuID], [MenuName], [MenuPrice], [DiningTable], [PaymentAmount], [Description], [BookingDate], [CreateDate], [LastUpdateDate], [Status]) VALUES (3, 2, 5, N'Tiệc sinh nhật 18 tuổi LUXURY', 6, N'Phòng Sofa 6A', 1900000, 1, CAST(N'08:00:00' AS Time), CAST(N'11:00:00' AS Time), 1, N'Combo gà siêu quậy', 500000, 3, 3400000, NULL, CAST(N'2024-03-02' AS Date), CAST(N'2024-02-27' AS Date), CAST(N'2024-02-27' AS Date), N'Create')
SET IDENTITY_INSERT [dbo].[Bookings] OFF
GO


USE [master]
GO
ALTER DATABASE [SWD392-BirthdayParty] SET  READ_WRITE 
GO

