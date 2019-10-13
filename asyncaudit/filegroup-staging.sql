--example: sqlcmd -E -S ".\sqlexpress" -d master -v db ="LASA" -i filegroup-staging.sql
USE [master]
GO
alter database [$(db)]
set enable_broker
with rollback immediate;
go

use [$(db)];
go

if not exists (select 1 from sys.schemas where name ='audit')
	exec('create schema audit');
go

if not exists(select*from sysfilegroups where groupname='STAGING')
	ALTER DATABASE [$(db)]  ADD FILEGROUP [STAGING];
go

declare @filepath nvarchar(512)=(SELECT top(1)left(filename, len(filename)-patindex('%[\/]%',reverse(filename))+1)
				FROM dbo.sysfiles) +N'$(db)_STAGING.ndf';

if not exists(select*from sysfiles where filename =@filepath)
begin
	declare @sql nvarchar(max) ='
	alter database [$(db)]
	add file(
		  NAME = N''STAGING''
		, FILENAME =N'''+@filepath+'''
		, SIZE = 4096KB 
		, MAXSIZE = UNLIMITED
		, FILEGROWTH = 1024KB )
	to filegroup [STAGING];';
	exec(@sql);
end--if

go
