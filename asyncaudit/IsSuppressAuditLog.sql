if object_id(N'audit.IsSuppressAuditLog') is not null
	drop function audit.IsSuppressAuditLog;
go
create function audit.IsSuppressAuditLog()
returns bit
as
begin
	return 0;
end--IsSuppressAuditLog
go