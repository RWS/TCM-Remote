#
# Script module for module 'TCMRemote'
# Routes to the correct framework binary (net48, net8.0) based on the PowerShell host.
#

# Required for Expand-TCMParameter.ps1 because $global:options might not exist yet
Set-StrictMode -Off

# Set up some helper variables to make it easier to work with the module
$PSModule     = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase

# ---------------------------------------------------------------------------
# Binary module selection
# ---------------------------------------------------------------------------
#   PSEdition 'Desktop'    => Windows PowerShell 5.1  => net48
#   PSVersion 7.x          => PowerShell 7.x (.NET 8+) => net8.0
# ---------------------------------------------------------------------------
$binaryModuleRoot = $PSModuleRoot

if (($PSVersionTable.Keys -contains 'PSEdition') -and ($PSVersionTable.PSEdition -eq 'Desktop'))
{
    # Windows PowerShell 5.1 / .NET Framework 4.8
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'net48'
}
else
{
    # PowerShell 7.x / .NET 8+ — use net8.0 (runs on .NET 8, 9, and 10)
    $net8Path = Join-Path -Path $PSModuleRoot -ChildPath 'net8.0'
    $binaryModuleRoot = if (Test-Path $net8Path) { $net8Path }
                        else { Join-Path -Path $PSModuleRoot -ChildPath 'net48' }
}

Write-Debug ("[TCMRemote] Loading binary from [$binaryModuleRoot]  " +
             "PSEdition[$($PSVersionTable.PSEdition)]  PSVersion[$($PSVersionTable.PSVersion)]")

$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'Tridion.TCMRemote.dll'
$binaryModule     = $null
$binaryModule     = Import-Module -Name $binaryModulePath -PassThru

# ---------------------------------------------------------------------------
# Load helper scripts
# ---------------------------------------------------------------------------
$privateCmdlet = @(Get-ChildItem -Path "$PSScriptRoot\Scripts\Private\*.ps1" `
                    -ErrorAction SilentlyContinue -Exclude '*.Tests.ps1')
$publicCmdlet  = @(Get-ChildItem -Path "$PSScriptRoot\Scripts\Public\*.ps1"  `
                    -ErrorAction SilentlyContinue -Exclude '*.Tests.ps1')

foreach ($import in @($privateCmdlet + $publicCmdlet))
{
    try
    {
        Write-Debug ("[TCMRemote] Importing [$($import.FullName)]")
        . $import.FullName
    }
    catch
    {
        Write-Error -Message "Failed to import function $($import.FullName): $_"
    }
}

Set-StrictMode -Version Latest

# ---------------------------------------------------------------------------
# Cleanup on module removal
# ---------------------------------------------------------------------------
$PSModule.OnRemove = {
    if ($null -ne $binaryModule) {
        Remove-Module -ModuleInfo $binaryModule -ErrorAction SilentlyContinue
    }
}
