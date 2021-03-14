USE [MyApiDB]
GO

/****** Object:  Table [dbo].[Nlog]    Script Date: 2020-07-21 1:15:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Nlog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LogDate] [datetime] NOT NULL,
	[LogShortDate] [date] NOT NULL,
	[Url] [nvarchar](max) NOT NULL,
	[Action] [nvarchar](500) NOT NULL,
	[Level] [nvarchar](50) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[Logger] [nvarchar](250) NOT NULL,
	[Callsite] [nvarchar](max) NOT NULL,
	[Exception] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.NlogDBLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


