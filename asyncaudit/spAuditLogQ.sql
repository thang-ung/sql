SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

if object_id(N'audit.spAuditLogQ') is null
	exec('create procedure audit.spAuditLogQ as return');
go

alter procedure audit.spAuditLogQ(
	  @AuditLogTransactionID int	--handles the collision --same id-- between two audit log tables
	, @key sysname =N'__id_expr'
)
as	--targets AUDIT_LOG_DATA or ORDERS_AUDIT_LOG_DATA
begin
	declare @stamp datetime
		, @ins xml
		, @dels xml
		, @delta int;
	declare @rec table(ins xml, dels xml, stamp datetime);

	--avoid collision via the time gap between select ... delete.
	delete top(1) audit.AUDITSTAGEDATA
	output deleted.ins, deleted.dels, deleted.stamp into @rec
	where parent =@AuditLogTransactionID;
	if @@rowcount =0	return -1;

	select @ins=ins, @dels =dels, @stamp=stamp from @rec;
	declare @destination sysname =(select top(1)loggerIn
				from audit.LOG_TXN where audit_log_transaction=@AuditLogTransactionID);

	create table #T(ky sysname, colname sysname
		, oldvalue varbinary(3500) null	--binary storage for faster comparison
		, newvalue varbinary(3500) null
		,primary key(ky,colname));

	;with cteRo as(
		select ky =isnull(ins.value('./@*[local-name(.)=sql:variable("@key")][1]', 'sysname')
					,dels.value('./@*[local-name(.)=sql:variable("@key")][1]', 'sysname'))
			, curr =dels.query('.')
			, post =ins.query('.')
		from @ins.nodes('/rows/r') j(ins)
		  full join @dels.nodes('/rows/r') m(dels) on ins.value('./@*[local-name(.)=sql:variable("@key")][1]', 'sysname')
			 = dels.value('./@*[local-name(.)=sql:variable("@key")][1]', 'sysname')
	)
	insert into #T
	select
		ky, UPIV.*
	from cteRo
	  cross apply(	--unpivots data from XML
		select colname =isnull(nullif(ins.value('local-name(.)','sysname'),N''), dels.value('local-name(.)','sysname'))
		, oldval =cast(dels.value('.','varchar(3500)') as varbinary(3500))
		, newval =cast(ins.value('.','varchar(3500)') as varbinary(3500))
		from curr.nodes('r/@*') m(dels)
		 full join post.nodes('r/@*') j(ins) on dels.value('local-name(.)','sysname') =ins.value('local-name(.)','sysname')
	 )UPIV
	where @key != colname
	;

	if @@ROWCOUNT =0
		EXEC xp_logevent 0xFFFF, 'SPHANDAUDITQ', informational;
	--gets best perf w/ coming from a temp table
	else if @destination ='LOG_DATA_ALT'
		insert into audit.LOG_DATA_ALT(
			AUDIT_LOG_TRANSACTION_ID,
			PRIMARY_KEYVAL,
			COL_NAME,
			OLD_VALUE,
			NEW_VALUE
		)
		select @AuditLogTransactionID,*	--varbinary implicitly cast to varchar(...)
		from #T
		where iif(oldvalue is null, 0,1)+iif(newvalue is null,0,1) =1--mutual exclusive null
			or oldvalue !=newvalue
	else
	begin
		insert into audit.LOG_DATA(
			AUDIT_LOG_TRANSACTION_ID,
			PRIMARY_KEYVAL,
			COL_NAME,
			OLD_VALUE,
			NEW_VALUE
		)
		select @AuditLogTransactionID,*	--varbinary implicitly cast to nvarchar(...)
		from #T
		where iif(oldvalue is null, 0,1)+iif(newvalue is null,0,1) =1--mutual exclusive null
			or oldvalue !=newvalue;
	end--if

	set @delta =@@rowcount;
	if @delta =0
		delete from AUDIT.LOG_Txn where AUDIT_LOG_TRANSACTION = @AuditLogTransactionID;

	return @delta;
end--spAuditLogQ
go