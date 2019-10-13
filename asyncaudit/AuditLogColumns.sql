
if object_id('audit.AuditLogger') is not null
	drop table audit.AuditLogger;
go
create table audit.AuditLogger(
	 tableName sysname not null
	,logName sysname default('LOG_DATA')
	,active bit not null default(1)
	, logger_id int identity

	,primary key(tableName, active)
);
go

if object_id('audit.AuditLogColumns') is not null
	drop table audit.AuditLogColumns;
go
create table audit.AuditLogColumns(
	 tableName sysname not null
	,columnName sysname not null
	,active bit not null default(1)

	,primary key(tableName, columnName) 
);
go
