if object_id(N'audit.userSession') is not null
	drop table audit.userSession;
go

create table audit.userSession(
	 userName sysname
	,sessionId smallint default(@@spid)
	,now datetime default(getdate())

	,primary key(now desc, sessionId)
)

go

