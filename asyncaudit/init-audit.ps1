##################################################################
#call $> ./init-audit.ps1 -cat biny -server DEVB -ops "sys tables"
#
# initialise database for asynchronous user data audit
# ** -ops test
##   to run unit test
#
##################################################################
param(
	[string]$ops ="sys tables progs queue",
	[string]$server =".\SQLEXPRESS",
	[string]$cat,
	[string]$creds ="-E"
)

function install{
	param([string]$ops, $jdir)

	foreach($op in $ops.split(' ')){
	write-Host "#operation [${op}]:"
		foreach($isql in $jdir.$op){
			#write-Host "sqlcmd ${creds} -S ""${server}"" -d ""${cat}"" -v db =""${cat}"" -i ""${isql}"""
			sqlcmd ${creds} -S "${server}" -d "${cat}" -v db ="${cat}" -i "${isql}" | write-Host
		}
	}
	return $prop	#return last server name
}#end install


$now =get-Date -format s
write-Host "started ${now}"
$json = @'
	{"sys":["filegroup-staging.sql"]
	,"tables":["AUDITSTAGEDATA.sql"
		,"AUDIT_LOG_DATA.tb.sql"
		,"logException.sql"
		,"AuditLogColumns.sql"]
	,"progs":["GetConnCtxClientUserID.sql"
		,"IsSuppressAuditLog.sql"
		,"fntb_columnsUpdated.sql"
		,"spAuditLogQ.sql"
		,"sphandlerAuditQ.sql"
		,"spAuditTxnData.sql"]
	,"queue":["sbAuditQueue.sql"]

	,"test":["dumbo.sql"
		,"dumbo_audit_test.sql"]
	,"clear-test":["rmdumbo.sql"]

	,"clear":["rmaudit.sql"]
	}
'@  | ConvertFrom-Json

$prop =install -ops $ops -jdir $json

