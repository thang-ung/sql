
--queue/broker service
create message type svcmsgAuditID validation=none;
create contract sbAuditLog( svcmsgAuditID sent by initiator );

create queue audit.sbAuditQueue
	with status=on,
	activation(
		 procedure_name=audit.sphandlerAuditQ
		,max_queue_readers =4
		,execute as SELF
	);

--create services
create service svcQueueAuditTx on queue audit.sbAuditQueue--(sbAuditLog);	--from
create service svcQueueAuditRx on queue audit.sbAuditQueue(sbAuditLog);	--to
go
