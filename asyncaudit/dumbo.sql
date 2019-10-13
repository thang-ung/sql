SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

if object_id(N'dbo.dumbo') is not null
	drop table dbo.dumbo;
go

create table dbo.dumbo(
	id int identity primary key
	,valint int
	,valchar sysname
	, unique(valint, valchar)
);
go


if object_id(N'dbo.audit_dumbo') is not null
	drop trigger dbo.audit_dumbo;
go

create trigger dbo.audit_dumbo on dbo.dumbo
after delete, update, insert
not for replication
as
begin
	Set nocount on;

	declare @AffectedRows int =(select count(1)from deleted)
		,@inserts int =(select count(1)from inserted);
	if(@AffectedRows+@inserts = 0) return;

	--test abstraction to business rule
	if(audit.IsSuppressAuditLog() = 1) return; --exits from trigger,  no further logic

	--prep data: customize the key expression (__id_expr) to applicable value !!! REQUIRED CUSTOMIZATION !!!
	declare @ins xml =(select __id_expr=id,*from inserted r for xml auto, root('rows'), type)
		, @dels xml =(select __id_expr=id,*from deleted r for xml auto, root('rows'), type)
		, @updateMask varbinary(64) =columns_updated();
	/* Insert audit data for table [CashDisbursements]  */
	exec @inserts =audit.spAuditTxnData @inserts, @AffectedRows, @ins, @dels, @@procid, @updateMask;

end--trigger
go

truncate table audit.AuditLogger;
truncate table audit.AuditLogColumns;
go

insert into audit.AuditLogger(tableName)values('dumbo');
insert into audit.AuditLogColumns(tableName,columnName)
	values('dumbo','valint'),('dumbo','valchar');
