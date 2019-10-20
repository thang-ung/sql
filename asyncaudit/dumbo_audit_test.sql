
exec dbo.spBindsUser N'perp';

--truncate table audit.AuditLogColumns;
truncate table dbo.dumbo;
truncate table audit.auditstagedata;
truncate table audit.log_txn;
truncate table audit.log_data;

insert into dbo.dumbo(valint,valchar)
values(6,'a'), (21,'dem'),(810,'umberlank');

update top(2)dbo.dumbo
set valint =valint *3;
delete top(1) from dbo.dumbo where id =(select max(id)from dbo.dumbo);

select*from audit.AUDITSTAGEDATA;
select*from audit.log_Txn;
waitfor delay '00:00.1';
select*from audit.log_data;
select*from dbo.dumbo;
