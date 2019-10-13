SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

if object_id('audit.sphandlerAuditQ') is null
begin
	exec('create procedure audit.sphandlerAuditQ as return');
end
go
alter procedure audit.sphandlerAuditQ
as
begin
	declare @message_type sysname;
	declare @banter uniqueidentifier, @id int;


	while(1 = 1)
	begin -- receive the next available message from the queue
		waitfor(
			receive top(1) @message_type = message_type_name,
				@id = cast(message_body as int),
				@banter = conversation_handle
			from audit.sbAuditQueue )
			, timeout 100;
		if( @@rowcount =0 or @id is null ) --response is null
			break;
		else
		begin
			--process xml message here...
			exec audit.spAuditLogQ @id;
		end
		end conversation @banter
	end

end--sphandlerAuditQ
go
