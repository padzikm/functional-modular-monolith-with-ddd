
USE [MyMeetings];

GO
PRINT N'Creating SqlSchema [administration]...';


GO
CREATE SCHEMA [administration]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating SqlSchema [app]...';


GO
PRINT N'Creating SqlSchema [meetings]...';


GO
CREATE SCHEMA [meetings]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating SqlSchema [payments]...';


GO
CREATE SCHEMA [payments]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating SqlSchema [users]...';


GO
CREATE SCHEMA [users]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating SqlTable [meetings].[Meetings]...';


GO
CREATE TABLE [meetings].[Meetings] (
                                       [Id]    UNIQUEIDENTIFIER NOT NULL,
                                       [Title] NVARCHAR (200)   NOT NULL,
                                       CONSTRAINT [PK_meetings_Meetings_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
PRINT N'Creating SqlView [meetings].[v_MeetingDetails]...';


GO
CREATE VIEW [meetings].[v_MeetingDetails]
AS
SELECT
    [Meeting].[Id],
--     [Meeting].[MeetingGroupId],
    [Meeting].[Title]
--     [Meeting].[TermStartDate],
--     [Meeting].[TermEndDate],
--     [Meeting].[Description],
--     [Meeting].[LocationName],
--     [Meeting].[LocationAddress],
--     [Meeting].[LocationPostalCode],
--     [Meeting].[LocationCity],
--     [Meeting].[AttendeesLimit],
--     [Meeting].[GuestsLimit],
--     [Meeting].[RSVPTermStartDate],
--     [Meeting].[RSVPTermEndDate],
--     [Meeting].[EventFeeValue],
--     [Meeting].[EventFeeCurrency]
FROM [meetings].[Meetings] AS [Meeting]
GO
PRINT N'Creating SqlView [meetings].[v_Meetings]...';


GO

CREATE VIEW [meetings].[v_Meetings]
AS
SELECT
    Meeting.[Id],
    Meeting.[Title]
--     Meeting.[Description],
--     Meeting.LocationAddress,
--     Meeting.LocationCity,
--     Meeting.LocationPostalCode,
--     Meeting.TermStartDate,
--     Meeting.TermEndDate
FROM meetings.Meetings AS [Meeting]
GO
PRINT N'Update complete.';


GO
