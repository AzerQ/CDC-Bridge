$ServiceName = "CdcBridgeService"
$AppDir = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$ServiceExecutable = "$AppDir\CdcBridge.Host.exe"
$ServiceUser = "DOMAIN\user"
$ServiceDescription = "Service for capture data changes in relational databases (CDC)"


Export-ModuleMember -Variable ServiceName, AppDir, ServiceExecutable, ServiceUser, ServiceDescription 