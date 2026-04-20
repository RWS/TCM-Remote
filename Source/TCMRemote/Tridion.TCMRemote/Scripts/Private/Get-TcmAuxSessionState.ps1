function Get-TcmAuxSessionState
{
<#
.SYNOPSIS
	Returns SessionState module variables
.DESCRIPTION
	Returns SessionState module variables
.EXAMPLE
	Get-TcmAuxSessionState -Name "TCMRemoteSessionStateTcmSession"
#>
[CmdletBinding()]
param(
	$Name
)
Process
{
	Write-Output ($PsCmdlet.SessionState.PSVariable.Get($Name)).Value
}
}