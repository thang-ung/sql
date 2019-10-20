
if object_id(N'dbo.vwSessionsRecent') is not null
	drop view dbo.vwSessionsRecent;
go
create view dbo.vwSessionsRecent
as
	select*from audit.userSession(nolock)
	where now >= dateadd(dd, datediff(dd, 0, getdate())-3, 0);
go
