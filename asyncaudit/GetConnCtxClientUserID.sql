if object_id(N'dbo.GetConnCtxClientUserID') is not null
	drop function dbo.GetConnCtxClientUserID;
go
create function dbo.GetConnCtxClientUserID()
returns sysname
as
begin
	return N'sys';
end--GetConnCtxClientUserID
go