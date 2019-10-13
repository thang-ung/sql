
--drop table audit.AUDITSTAGEDATA
if object_id(N'audit.AUDITSTAGEDATA') is not null
	drop table audit.AUDITSTAGEDATA;
go

create table audit.AUDITSTAGEDATA(
	  parent int primary key
	, stamp datetime
	, ins xml, dels xml
)
on [STAGING];

go

