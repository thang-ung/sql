
if object_id('audit.logException') is not null
	drop table audit.logException;
go
create table audit.logException(
	 audit_log_transaction_id int primary key
	 ,reason nvarchar(256)
);
