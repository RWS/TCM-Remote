$ErrorActionPreference = 'Stop'

if (-not (Get-PSRepository -Name LocalPSRepo -ErrorAction SilentlyContinue)) {
    Register-PSRepository `
        -Name LocalPSRepo `
        -SourceLocation "$PSScriptRoot" `
        -InstallationPolicy Trusted
}

Get-InstalledModule TCMRemote -AllVersions -ErrorAction SilentlyContinue |
    Uninstall-Module -Force

Install-Module TCMRemote -Repository LocalPSRepo -Scope CurrentUser -Force