SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

if object_id(N'audit.spAuditTxnData') is null
	exec('create procedure audit.spAuditTxnData as return');
go

alter procedure audit.spAuditTxnData(
	  @inserts int, @affectedRows int
	, @ins xml
	, @dels xml
	, @procid int =@@procid
	, @mask varbinary(64)	--64 --> 512 columns
	, @key sysname =N'__id_expr'
)
as
begin
	declare @verb tinyint =case
					when @dels is null then 1
					when @ins is null then 4
					else 2
					end;
	declare @AuditLogTransactionID int
      , @ClientUserID sysname= dbo.GetConnCtxClientUserID()	--get from abstracted function
      , @AuditLogException tinyint =0;	--extended behaviour

	insert into audit.LOG_Txn( tableName,audit_verb,HOST_NAME,APP_NAME, MODIFIED_BY,AFFECTED_ROWS)
	select top(1)
		 object_name(parent_id)
		,case when @inserts=0 then 'D' when @affectedRows=0 then 'I' else 'U' end
		,host_name()
		,app_name()
		,@ClientUserID
		,case when @inserts =0 then @affectedRows else @inserts end
	from sys.triggers where @procid = object_id;

	set @AuditLogTransactionID = scope_identity();
	update T
	set T.loggerIn =(select top(1)logName from audit.AuditLogger(nolock) where active=1 and tableName =T.tableName)
	from audit.log_txn  T
	where T.audit_log_transaction =@AuditLogTransactionID;


	if(@AuditLogException & 1 !=0)
	begin
		insert into audit.LogException(AUDIT_LOG_TRANSACTION_ID) values(@AuditLogTransactionID);
	end

	declare @fields table(tbl sysname, cname sysname, ordinal smallint unique, active bit, primary key(tbl,cname));

	insert into @fields
	select T.tbl, T.name, T.ordinal, T.active
	from audit.fntb_updated_columns(case when @ins is null then null else @mask end
			, @procid, default, @verb)T;

	if(select count(1)from @fields where active=1) =0	return 0;	--exit proc

	--remove n/a columns (xml attributes)
	declare @void nvarchar(max) =cast((select '|'+cname from @fields where active=0 for xml path(''),type) as nvarchar(max)) +'|';

	if @ins is not null
		set @ins.modify('delete (/rows/r/@*[contains(sql:variable("@void"),concat("|",local-name(.),"|"))])');
	if @dels is not null
		set @dels.modify('delete (/rows/r/@*[contains(sql:variable("@void"),concat("|",local-name(.),"|"))])');

	--the asynchronous solution:
	declare @InitDlgHandle uniqueidentifier =newid();

	insert into audit.AUDITSTAGEDATA values(@AuditLogTransactionID, getdate(), @ins, @dels);
	begin dialog @InitDlgHandle
		from service svcQueueAuditTx to service 'svcQueueAuditRx' on contract sbAuditLog
		with encryption =off;

	send on conversation @InitDlgHandle message type svcmsgAuditID(@AuditLogTransactionID);
	end conversation @InitDlgHandle;
	return 1;

end--spAuditTxnData
go
