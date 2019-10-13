
if object_id('audit.LOG_TXN') is not null
	drop table audit.LOG_TXN;
go


if object_id('audit.LOG_TXN') is not null
	drop table audit.LOG_TXN;
go
create table audit.LOG_TXN(
	 audit_log_transaction int identity unique
	,tableName sysname
	,audit_verb char(1) not null default('I') --(U)pdate , (I)nsert, (D)elete
	,host_name sysname not null default(host_name())
	,app_name sysname null
	,modified_by sysname not null default('sys')
	,modified_on datetime default(getdate())
	,affected_rows int not null default(0)
	,loggerIn sysname not null default('LOG_DATA')

	,primary key(tableName, modified_by, modified_on) 
);
go

if object_id('audit.LOG_DATA') is not null
	drop table audit.LOG_DATA;
go
create table audit.LOG_DATA(
	 AUDIT_LOG_TRANSACTION_ID int not null
	,PRIMARY_KEYVAL sysname not null --expand if bigger than 128 (unicode) length
	,[COL_NAME] sysname not null
	,OLD_VALUE varchar(max) null
	,NEW_VALUE varchar(max) null

	,primary key(AUDIT_LOG_TRANSACTION_ID, PRIMARY_KEYVAL, [COL_NAME]) 
);
go
