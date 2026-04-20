#
# TCMRemote.PesterSetup.ps1
# Tests header dot-sourced import PS1 file for all *.Tests.ps1 files.
#
# Ensure Pester 5 is loaded — Windows ships Pester 3.4 in the system path which
# conflicts with the -Output parameter and other Pester 5 syntax.
Import-Module Pester -MinimumVersion 5.0 -Force -ErrorAction Stop

# Quick check if you are using Pester 5, or default installed Pester 3.4.0
$pesterVersion = [version](Get-Command Invoke-Pester).Version
if ($pesterVersion -lt [version]("5.3.0")) { Write-Warning ("TCMRemote.PesterSetup.ps1 Invoke-Pester version["+$pesterVersion+"] while 5.3+ is expected!") }

$DebugPreference    = "SilentlyContinue"  # Continue or SilentlyContinue
$VerbosePreference  = "SilentlyContinue"  # Continue or SilentlyContinue
$WarningPreference  = "Continue"          # Continue or SilentlyContinue or Stop
$ProgressPreference = "SilentlyContinue"  # Continue or SilentlyContinue

# ---------------------------------------------------------------------------
# Module path — Debug build by default, Release when running in GitHub Actions
# ---------------------------------------------------------------------------
$moduleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ($env:GITHUB_ACTIONS -eq "true") {
    $moduleFolder = Join-Path $moduleRoot "bin\Release\TCMRemote"
} else {
    $moduleFolder = Join-Path $moduleRoot "bin\Debug\TCMRemote"
}
Write-Host ("Running TCMRemote.PesterSetup.ps1 Import-Module folder[" + $moduleFolder + "] ...")
Import-Module $moduleFolder -DisableNameChecking -Force

Write-Host ("Running TCMRemote.PesterSetup.ps1 Global Test Data and Variables initialization on " + (Get-Date -UFormat "%Y-%m-%dT%H-%M-%S%Z"))
$timestamp = Get-Date -Format "yyyyMMddHHmmss"

# ---------------------------------------------------------------------------
# Server connection — read from environment variables, fall back to defaults
# ---------------------------------------------------------------------------
$tcmBaseUrl = $env:TCM_BASE_URL
if ([string]::IsNullOrEmpty($tcmBaseUrl)) { $tcmBaseUrl = 'http://sites.tridiondemo.com' }

$tcmClientId = $env:TCM_CLIENT_ID
if ([string]::IsNullOrEmpty($tcmClientId)) { $tcmClientId = '' }

$tcmClientSecret = $env:TCM_CLIENT_SECRET
if ([string]::IsNullOrEmpty($tcmClientSecret)) { $tcmClientSecret = '' }

# Optional: supply Access Management URL to skip auto-discovery
$tcmAccessManagementUrl = $env:TCM_ACCESS_MANAGEMENT_URL
if ([string]::IsNullOrEmpty($tcmAccessManagementUrl)) { $tcmAccessManagementUrl = '' }

# ---------------------------------------------------------------------------
# Known items in the DXA reference website on the demo server.
# Override in TCMRemote.PesterSetup.Debug.ps1 for your own environment.
# ---------------------------------------------------------------------------
$tcmPublicationId_Empty       = "tcm:0-1-1"   # 000 Empty
$tcmPublicationId_Master      = "tcm:0-2-1"   # 100 Master
$tcmPublicationId_ExampleSite = "tcm:0-5-1"   # 400 Example Site (en-GB)
$tcmItemId_HomeStructureGroup = "tcm:2-5-4"   # Home structure group in 100 Master

# ---------------------------------------------------------------------------
# Load optional local overrides (not committed to source control)
# ---------------------------------------------------------------------------
Write-Host "Running TCMRemote.PesterSetup.ps1 Global Test Data and Variables for debug initialization"
$debugSetupFilePath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "TCMRemote.PesterSetup.Debug.ps1"
if (Test-Path -Path $debugSetupFilePath -PathType Leaf) {
    . ($debugSetupFilePath)
}

# ---------------------------------------------------------------------------
# Create ONE shared global session.
# Uses Client Credentials when TCM_CLIENT_ID / TCM_CLIENT_SECRET are set
# (suitable for CI/CD), otherwise falls back to interactive browser login.
#
# Note: Only variables and generic initialization above.
#       $global:tcmSession creation is the only TCMRemote execution here.
# ---------------------------------------------------------------------------
$sessionParams = @{ BaseUrl = $tcmBaseUrl }
if (-not [string]::IsNullOrEmpty($tcmAccessManagementUrl)) {
    $sessionParams.AccessManagementUrl = $tcmAccessManagementUrl
}

if (-not [string]::IsNullOrEmpty($tcmClientId) -and -not [string]::IsNullOrEmpty($tcmClientSecret)) {
    Write-Host "Running TCMRemote.PesterSetup.ps1 creating global session over ClientCredentials..."
    $sessionParams.ClientId     = $tcmClientId
    $sessionParams.ClientSecret = $tcmClientSecret
} else {
    Write-Host "Running TCMRemote.PesterSetup.ps1 creating global session over Browser (interactive)..."
}

$global:tcmSession = New-TcmSession @sessionParams -WarningAction SilentlyContinue
$tcmSession = $global:tcmSession
