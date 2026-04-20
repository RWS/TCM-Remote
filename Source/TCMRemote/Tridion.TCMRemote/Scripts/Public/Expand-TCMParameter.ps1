#
# Hat tip to
# * https://github.com/thinkbeforecoding/PSCompletion
# * http://www.powershellmagazine.com/2012/11/29/using-custom-argument-completers-in-powershell-3-0/
# * http://www.powertheshell.com/dynamicargumentcompletion/
# * https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/register-argumentcompleter?view=powershell-5.1
#
# Relies on
#   Get-TcmAuxSessionState.ps1
#   New-TcmAuxCompletionResult.ps1
#   Register-TcmAuxParameterCompleter.ps1
#


<#
    Create a global options to pass to [System.Management.Automation.CommandCompletion]::CompleteInput
    containing registered argument completers
#>
if (-not $global:options) {
    $global:options = @{CustomArgumentCompleters = @{};NativeArgumentCompleters = @{}}
}

<#
    Change the orignal TabExpansion2 function used for PS 3.0 completion
    to merge passed $options with $global:options
    The change will happen only once event if executed several times because
    on second pass, the function text doesn't match anymore
#>
$function:tabexpansion2 = $function:tabexpansion2 -replace 'End\r\n{','End { if ($null -ne $options) { $options += $global:options} else {$options = $global:options}'

#
# Thanks to the above boost, finally the added value for this module
#
