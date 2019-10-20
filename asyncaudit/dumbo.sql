SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

if object_id(N'audit.dumbo') is not null
	drop table audit.dumbo;
go

create table audit.dumbo(
	id int identity primary key
	,valint int
	,valchar sysname
	, unique(valint, valchar)
);
go


if object_id(N'audit.audit_dumbo') is not null
	drop trigger audit.audit_dumbo;
go

create trigger audit.audit_dumbo on audit.dumbo
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
	declare @dels xml=(select __n=row_number()over(order by @AffectedRows),*from(
				select top(0)__id_expr=cast(null as sysname),*from deleted
				union all select id,*from deleted
			) r for xml auto, root('rows'), type
		)
		,@ins xml =(select __n=row_number()over(order by @AffectedRows),*from(
				select top(0)__id_expr=cast(null as sysname),*from deleted
				union all select id,*from inserted
			) r for xml auto, root('rows'), type
		)
		, @updateMask varbinary(64) =columns_updated();
	/* Insert audit data for table [CashDisbursements]  */
	exec @inserts =audit.spAuditTxnData @inserts, @AffectedRows, @ins, @dels, @@procid, @updateMask;

end--trigger
go

truncate table audit.AuditLogger;
truncate table audit.AuditLogColumns;
go

insert into audit.AuditLogger(tableName)values('dumbo');
insert into audit.AuditLogColumns(tableName,columnName, events)
	values('dumbo','valint',default),('dumbo','valchar',default),('dumbo','id',5);

select*from audit.AuditLogColumns
