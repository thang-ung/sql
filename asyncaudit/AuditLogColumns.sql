
if object_id('audit.AuditLogger') is not null
	drop table audit.AuditLogger;
go
create table audit.AuditLogger(
	 tableName sysname not null
	,logName sysname default('LOG_DATA')
	,active bit not null default(1)
	, logger_id int identity
	, keyExpression sysname null

	,primary key(tableName, active, logName)
);
go

if object_id('audit.AuditLogColumns') is not null
	drop table audit.AuditLogColumns;
go
create table audit.AuditLogColumns(
	 tableName sysname not null
	,columnName sysname not null
	,active bit not null default(1)

	,events tinyint not null default(0x06)	--1: insert, 2: update; 4: delete; 16: key
	,primary key(tableName, columnName)
);
go
