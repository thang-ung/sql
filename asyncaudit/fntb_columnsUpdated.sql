if object_id(N'audit.fntb_updated_columns') is not null
	exec('drop function audit.fntb_updated_columns');
go

create function audit.fntb_updated_columns(
	 @affectedMask varbinary(64)
	,@srcproc int =null
	,@tbl sysname =null
	,@bevt tinyint =1	--Audit Insert/Update/Delete bitwise value
)
returns @fields table(tbl sysname
		, name sysname
		, ordinal smallint unique
		, active tinyint	--bitwise ={ 2= updated, 1=in dbo.auditlogcolumns, 0 =omissible}
		, primary key(tbl,name)
		)
as
begin
	--resolve table name
	select top(1)
		@tbl =isnull(@tbl, object_name(parent_object_id))
	from sys.objects
	where @srcproc is null or object_id =@srcproc;
	if @affectedMask is null
	begin
		declare @frep tinyint=(select ceiling(max(column_id)/4.0) From sys.columns where object_id=object_id('dumbo'));
		set @affectedMask= convert(varbinary(64), replicate('F', @frep +(@frep % 2)), 2);
	end

	;with ctehex as(
		select k=1
		union all
		select k+1 from ctehex where k <=64
	)
	,cteMask as(
		select
			[source] =@tbl
			, cols =@affectedMask
	)
	,cteHit as(
		select top(1024)k, byt =cast(substring(cols, k, 1) as tinyint)
			, [source]
		from cteMask
		  cross apply ctehex
		where substring(cols, k, 1) > 0x0
		order by cols
	)
	insert into @fields
	select tbl=object_name(col.object_id)
		, col.name, col.column_id
		, iif(auds.events & @bevt = 0, 0, iif(hit.k is null, 0, 2) | 1)
	from cteHit hit
	  join sys.columns col on ceiling(col.column_id/8.0) =hit.k
		and (hit.byt & cast(power(2, (col.column_id-1) % 8) as tinyint)) != 0
	  join audit.AuditLogColumns(nolock) auds on auds.ColumnName =col.name
		and auds.TableName =@tbl
	where col.object_id =object_id(@tbl);

	if @@rowcount =0
		insert into @fields values(@tbl, '--', 0, 0)
	return;
end--fntb_updated_columns
go

