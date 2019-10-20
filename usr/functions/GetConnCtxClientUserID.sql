if object_id(N'dbo.GetConnCtxClientUserID') is not null
	drop function dbo.GetConnCtxClientUserID;
go
create function dbo.GetConnCtxClientUserID()
returns sysname
as
begin
	declare @usr sysname =(select top(1)userName from dbo.vwSessionsRecent where sessionId =@@spid);
	return case when @usr is null then N'unidentified' else @usr end;
end--GetConnCtxClientUserID
go