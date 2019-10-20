if object_id(N'audit.IsSuppressAuditLog') is not null
	drop function audit.IsSuppressAuditLog;
go
create function audit.IsSuppressAuditLog()
returns bit
as
begin
	return case
		when dbo.GetConnCtxClientUserID() in('sys','bulk','bcp') then 1
		else 0
		end;
end--IsSuppressAuditLog
go