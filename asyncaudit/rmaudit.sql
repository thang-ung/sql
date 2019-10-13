
drop service svcQueueAuditTx
drop service svcQueueAuditRx
drop contract sbAuditLog
drop message type svcmsgAuditID
drop queue audit.sbAuditQueue
go

drop procedure audit.spAuditTxnData
drop procedure audit.sphandlerAuditQ
drop procedure audit.spAuditLogQ;

drop function audit.fntb_updated_columns;
drop function audit.IsSuppressAuditLog;
drop function dbo.GetConnCtxClientUserID;
go

drop table audit.AUDITSTAGEDATA
drop table audit.LOG_TXN
drop table audit.LOG_DATA
drop table audit.logException
drop table audit.AuditLogColumns
drop table audit.AuditLogger
go


if exists (select 1 from sys.schemas where name ='audit')
	exec('drop schema audit');
go

if exists(select*from sysfilegroups where groupname='STAGING')
	alter database [$(db)] remove FILE STAGING;
	alter database [$(db)]  remove FILEGROUP [STAGING];
go


