
PRINT N'Altering SqlTable [meetings].[Meetings]...';


GO
ALTER TABLE [meetings].[Meetings]
    ADD [EventFeeValue]    DECIMAL (5) NULL,
        [EventFeeCurrency] VARCHAR (3) NULL;


GO
PRINT N'Altering SqlView [meetings].[v_MeetingDetails]...';


GO
ALTER VIEW [meetings].[v_MeetingDetails]
AS
SELECT
    [Meeting].[Id],
--     [Meeting].[MeetingGroupId],
    [Meeting].[Title],
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
    [Meeting].[EventFeeValue],
    [Meeting].[EventFeeCurrency]
FROM [meetings].[Meetings] AS [Meeting]
GO
PRINT N'Refreshing SqlView [meetings].[v_Meetings]...';


GO
EXECUTE sp_refreshsqlmodule N'[meetings].[v_Meetings]';


GO
PRINT N'Update complete.';


GO
