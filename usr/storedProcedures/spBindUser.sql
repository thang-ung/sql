if object_id(N'dbo.spBindsUser') is not null
	drop procedure dbo.spBindsUser;
go
create procedure dbo.spBindsUser(@userName sysname)
as
begin
	insert into audit.userSession(userName)values(@userName);
end
go

--exec dbo.spBindsUser N'perp';
--select top(1)*from dbo.vwSessionsRecent;
--select*from dbo.vwSessionsRecent;
