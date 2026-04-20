# TCMRemote

**Business automation PowerShell module for Tridion Sites Content Manager (TCM)**

TCMRemote is a binary PowerShell module built on the Tridion Sites REST API.  
It provides a thin, automation-friendly layer over the Content Manager REST endpoints — inspired by the architecture of [ISHRemote](https://github.com/RWS/ISHRemote) but purpose-built for **Tridion Sites**.

> **Current version:** 1.0.0  
> **Published to:** [PowerShell Gallery](https://www.powershellgallery.com/packages/TCMRemote/)

---

## Why TCMRemote?

| | Legacy WCF Module | TCMRemote |
|---|---|---|
| **Protocol** | WCF / SOAP | REST API (v3.0) |
| **Authentication** | Windows / ADFS / SAML 2.0 / OIDC via Access Management |
| **PowerShell 5.1** | ✅ | ✅ (net48) |
| **PowerShell 7.x** | ⚠️ limited | ✅ (net8.0) |
| **Linux / macOS** | ❌ | ✅ |
| **Unattended CI/CD** | ⚠️ hard | ✅ Client Credentials flow |

---

## Installation

```powershell
# PowerShell 7.x (.NET 8+ / CoreCLR)
Install-PSResource -Name TCMRemote -Repository PSGallery -Scope CurrentUser

# Windows PowerShell 5.1 (.NET Framework 4.8)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Install-Module TCMRemote -Repository PSGallery -Scope CurrentUser -Force
```

Verify installation:

```powershell
Import-Module TCMRemote
Get-Command -Module TCMRemote
```

---

## Quick Start

### Interactive login (browser)

```powershell
Import-Module TCMRemote

$tcmSession = New-TcmSession -BaseUrl "https://tridion.example.com"
# → Opens your default browser for single sign-on

Get-TcmCurrentUser
Get-TcmPublications
```

### Unattended / CI-CD (Client Credentials)

```powershell
Import-Module TCMRemote

$tcmSession = New-TcmSession `
    -BaseUrl      "https://tridion.example.com" `
    -ClientId     "c826e7e1-c35c-43fe-9756-e1b61f44bb40" `
    -ClientSecret "ziKiGbx6N0G3m69/vWMZUTs2paVO1Mzqt6Y6TX7mnpPJyFVODsI1Vw=="

Get-TcmPublications | Select-Object Title, Id | Format-Table
```

---

## Cmdlet Reference

### Session Management

| Cmdlet | Description |
|--------|-------------|
| `New-TcmSession` | Creates an authenticated session (browser or client-credentials). |
| `Get-TcmSession` | Returns the active session from the current PowerShell session state. |
| `Remove-TcmSession` | Closes and removes the active session. |
| `Test-TcmSession` | Pings the server to verify the session is still valid. Returns `$true`/`$false`. |

### Users

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmCurrentUser` | Returns the profile of the currently authenticated user. |
| `Get-TcmUser` | Returns users. Supports `-Search`, `-IncludePredefined`, `-IncludeDisabled`. |
| `New-TcmUser` | Creates a new user account. |
| `Set-TcmUser` | Updates a user's description or enabled state. |
| `Disable-TcmUser` | Disables a user account. Supports `-WhatIf` / `-Confirm`. |
| `Enable-TcmUser` | Re-enables a disabled user account. Supports `-WhatIf` / `-Confirm`. |

### Groups

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmGroup` | Returns groups. Supports `-Search`, `-InPublicationId`, `-IncludeEveryone`. |
| `New-TcmGroup` | Creates a new group. Supports `-WhatIf` / `-Confirm`. |

### Publications

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmPublications` | Returns all publications. |
| `Get-TcmPublication` | Returns a single publication by TCM URI. |
| `New-TcmPublication` | Creates a new publication. Supports `-WhatIf` / `-Confirm`. |

### Publication Targets

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmPublicationTarget` | Returns all publication targets (target types). |

### Content Items

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmItem` | Returns an item by TCM URI (component, page, schema, …). |
| `New-TcmItem` | Creates a new item from an IdentifiableObject. Supports `-WhatIf` / `-Confirm`. |
| `Set-TcmItem` | Updates an existing item. Supports `-WhatIf` / `-Confirm`. |
| `Remove-TcmItem` | Deletes an item. Supports `-WhatIf` / `-Confirm`. |
| `Test-TcmItem` | Returns `$true` if the item exists, `$false` otherwise. |
| `Move-TcmItem` | Moves an item to a different container. Supports `-WhatIf` / `-Confirm`. |
| `Copy-TcmItem` | Copies an item to a container. Supports `-WhatIf` / `-Confirm`. |
| `Find-TcmItem` | Full-text search across all repository items. Supports `-ResultLimit`. |

### Versioning

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmItemHistory` | Returns the version history of a versioned item. |
| `Invoke-TcmCheckOut` | Checks out a versioned item. Supports `-PermanentLock`. |
| `Invoke-TcmCheckIn` | Checks in a checked-out item. Supports `-RemovePermanentLock`. |
| `Undo-TcmCheckOut` | Releases the checkout lock without saving a new version. |

### Relationships

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmItemUses` | Returns the items that the specified item uses (outgoing links). |
| `Get-TcmItemUsedBy` | Returns the items that use the specified item (incoming links). |
| `Get-TcmItemsInContainer` | Returns the items in a Folder or StructureGroup. Supports `-Recursive`. |

### Publishing

| Cmdlet | Description |
|--------|-------------|
| `Publish-TcmItem` | Publishes an item to a publish target. |
| `Unpublish-TcmItem` | Removes a published item from a publish target. |
| `Get-TcmPublishTransaction` | Returns publish transactions. Supports multiple filters. |
| `Remove-TcmPublishTransaction` | Removes a publish transaction. Supports pipeline input. |

### Workflow

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmActivityInstance` | Returns workflow activity instances. Supports `-ForAllUsers`, `-ActivityState`. |

### Application Settings

| Cmdlet | Description |
|--------|-------------|
| `Get-TcmApplicationSettings` | Returns the settings for a named application. |
| `Set-TcmApplicationSettings` | Saves settings for a named application. |
| `Remove-TcmApplicationSettings` | Removes all settings for a named application. |

---

## Authentication

TCMRemote authenticates via **Tridion Access Management** (an OpenID Connect / OAuth2 server bundled with Tridion Sites 10+).

### Browser flow (default)

1. TCMRemote calls `/api/v1.0/system/capabilities` to discover the Access Management URL.  
2. An OIDC Authorization Code + PKCE request is built and the system browser is opened.  
3. A lightweight HTTP listener on `http://127.0.0.1:<random-port>/` captures the callback.  
4. The access token (and refresh token) are cached for the lifetime of the PowerShell session.

**Requirement:** the OIDC client `Tridion_Sites_Desktop_Client` must have `http://127.0.0.1` (or a wildcard) as an allowed redirect URI in Access Management.

```powershell
New-TcmSession -BaseUrl "https://tridion.example.com"
# Optionally override the OIDC client ID:
New-TcmSession -BaseUrl "https://tridion.example.com" -BrowserClientId "MyCustomClient"
```

### Client Credentials flow

Suitable for CI/CD pipelines and scheduled tasks. No browser interaction.

```powershell
New-TcmSession `
    -BaseUrl      "https://tridion.example.com" `
    -ClientId     "<client-id>" `
    -ClientSecret "<client-secret>"
```

The `ClientId` / `ClientSecret` pair must be provisioned in Tridion Access Management with the `client_credentials` grant type enabled.

---

## WhatIf / Confirm support

Write operations support PowerShell's standard `-WhatIf` and `-Confirm` parameters:

```powershell
Remove-TcmItem  -ItemId "tcm:1-2-16" -WhatIf
Publish-TcmItem -ItemId "tcm:1-10-64" -PublishTargetId "tcm:0-1-65537" -WhatIf
```

---

## Verbose & Debug output

```powershell
New-TcmSession   -BaseUrl "https://tridion.example.com" -Verbose
Get-TcmPublications -Verbose
```

All cmdlets emit timing information at the `-Debug` level.

---

## Module structure

The module ships two binary sub-folders selected at import time by `TCMRemote.psm1`:

| Folder | Runtime | PowerShell |
|--------|---------|-----------|
| `net48/` | .NET Framework 4.8 | Windows PowerShell 5.1 |
| `net8.0/` | .NET 8+ | PowerShell 7.x (7.0 – 7.6+) |

---

## Source layout

```
Source/TCMRemote/
├── Directory.Build.props                    # Centralised versioning & module metadata
├── TCMRemote.sln
├── Tridion.TCMRemote/                       # Main binary module (C#)
│   ├── Cmdlets/
│   │   ├── TridionCmdlet.cs                 # Abstract base class for all cmdlets
│   │   ├── _TestEnvironment/
│   │   │   └── TestPrerequisite.Tests.ps1   # Verify environment before running full suite
│   │   ├── Session/            New-, Get-, Remove-, Test-TcmSession
│   │   │   ├── *.cs                         # Cmdlet implementations
│   │   │   └── *.Tests.ps1                  # Co-located Pester tests
│   │   ├── User/               Get-TcmCurrentUser, Get-TcmUser, New-, Set-, Disable-, Enable-TcmUser
│   │   ├── Group/              Get-TcmGroup, New-TcmGroup
│   │   ├── Publication/        Get-TcmPublication(s), New-TcmPublication
│   │   ├── PublicationTarget/  Get-TcmPublicationTarget
│   │   ├── Item/               Get-, New-, Set-, Remove-, Test-, Move-, Copy-, Find-TcmItem
│   │   │                       Get-TcmItemHistory, Invoke-TcmCheckIn/Out, Undo-TcmCheckOut
│   │   │                       Get-TcmItemUses, Get-TcmItemUsedBy, Get-TcmItemsInContainer
│   │   ├── Publishing/         Publish-, Unpublish-TcmItem, Get-, Remove-TcmPublishTransaction
│   │   ├── Workflow/           Get-TcmActivityInstance
│   │   └── ApplicationSettings/ Get-, Set-, Remove-TcmApplicationSettings
│   ├── Connection/                          # OAuth2 / OIDC connection layer
│   │   ├── TcmAccessManagementClient.cs     # Token acquisition (mirrors AM Desktop Client)
│   │   ├── TcmLocalHttpEndpoint.cs          # Embedded HTTP server for OIDC redirect
│   │   ├── ITcmUserAuthListener.cs          # UI feedback interface
│   │   └── Exceptions/   TcmClientException, TcmTokenException, TcmTokenErrorCode
│   ├── Authentication/
│   │   ├── TcmCapabilitiesHelper.cs         # Discovers Access Management URL
│   │   ├── BrowserAuthService.cs            # OIDC Auth Code + PKCE
│   │   └── ClientCredentialsAuthService.cs  # OAuth2 Client Credentials
│   ├── Clients/
│   │   ├── ITridionCoreServiceClient.cs     # REST client interface
│   │   └── TridionCoreServiceClient.cs      # HttpClient-based REST implementation
│   ├── Objects/Public/
│   │   └── TcmSession.cs                    # Session object returned by New-TcmSession
│   ├── Interfaces/
│   │   ├── IAuthenticationService.cs        # Core auth contract
│   │   └── ITcmAuthenticationService.cs     # Extended auth contract
│   ├── HelperClasses/
│   │   ├── AppDomainModuleAssemblyInitializer.cs  # NET48 assembly redirect fix
│   │   └── CertificateValidationHelper.cs         # Dev/test TLS bypass
│   ├── Scripts/
│   │   ├── Public/   Expand-TCMParameter.ps1, Write-TcmRemoteLog.ps1
│   │   └── Private/  Get-TcmAuxSessionState.ps1, Register-TcmAuxParameterCompleter.ps1
│   ├── TCMRemote.psm1                # Module loader (routes to net48 or net8.0 binary)
│   ├── TCMRemote.Format.ps1xml       # Output format views for all public types
│   ├── TCMRemote.PesterSetup.ps1     # Shared test setup — dot-sourced by every test file
│   └── TCMRemote.PesterSetup.Debug.ps1  # Local overrides (not committed — gitignored)
└── Tridion.TCMRemote.OpenApiTCM30/  # Auto-generated OpenAPI v3.0 typed client
```

---

## Building from source

**Prerequisites:**

- .NET SDK 8 or later (for `net8.0` target)
- .NET Framework 4.8 Developer Pack (for `net48` target — Windows only)
- PowerShell 7.x (`pwsh`) in `PATH`
- Windows PowerShell 5.1 (`powershell.exe`) — used only for module manifest generation

```powershell
# Restore + build (Debug)
dotnet restore Source/TCMRemote/TCMRemote.sln
dotnet build   Source/TCMRemote/TCMRemote.sln --configuration Debug

# Build + package (Release)
dotnet build   Source/TCMRemote/TCMRemote.sln --configuration Release

# Load the Debug build locally
Import-Module "Source/TCMRemote/Tridion.TCMRemote/bin/Debug/TCMRemote/TCMRemote.psd1" -Force
```

### Local PSGallery test

```powershell
$localRepo = "C:\temp\LocalPSRepo"
New-Item $localRepo -ItemType Directory -Force
Register-PSRepository -Name LocalPSRepo -SourceLocation $localRepo -InstallationPolicy Trusted

Publish-Module -Path "...\bin\Release\TCMRemote" -Repository LocalPSRepo -NuGetApiKey "Any"
Install-Module   TCMRemote -Repository LocalPSRepo -Scope CurrentUser -Force

# Cleanup
Unregister-PSRepository -Name LocalPSRepo
```

---

## Testing

Tests are written in [Pester 5](https://pester.dev/) and live **co-located** with the cmdlets they cover — each cmdlet folder contains a `<Cmdlet>.Tests.ps1` alongside the `.cs` source file.

### Prerequisites

- Pester 5.3.0 or later:

  ```powershell
  Install-Module Pester -Force -SkipPublisherCheck -Scope CurrentUser
  ```

- A running Tridion Sites server (the tests are acceptance tests that hit a real server).
- A Debug or Release build of the module (`dotnet build`).

### Running tests

**Validate the test environment first** (Pester version, module load, server reachability, session):

```powershell
cd Source/TCMRemote/Tridion.TCMRemote
Invoke-Pester -Path "Cmdlets/_TestEnvironment/TestPrerequisite.Tests.ps1" -Output Detailed
```

**Run the full suite:**

```powershell
Invoke-Pester -Path "Cmdlets" -Output Detailed
```

**Run a single test file:**

```powershell
Invoke-Pester -Path "Cmdlets/Session/NewTcmSession.Tests.ps1" -Output Detailed
```

### Authentication in tests

Each test file dot-sources `TCMRemote.PesterSetup.ps1` from the module root in its `BeforeAll` block. The setup script:

1. Imports the Debug build (or Release when `$env:GITHUB_ACTIONS -eq "true"`).
2. Reads connection settings from environment variables, falling back to demo-server defaults.
3. Creates a single `$global:tcmSession` shared across all tests in that file.

| Environment variable | Purpose | Default |
|---|---|---|
| `TCM_BASE_URL` | Root URL of the Content Manager server | `http://sites.tridiondemo.com` |
| `TCM_CLIENT_ID` | OAuth2 Client ID (Client Credentials flow) | _(empty — browser login used)_ |
| `TCM_CLIENT_SECRET` | OAuth2 Client Secret | _(empty — browser login used)_ |
| `TCM_ACCESS_MANAGEMENT_URL` | Override Access Management URL (optional) | _(auto-discovered)_ |

When `TCM_CLIENT_ID` and `TCM_CLIENT_SECRET` are set the setup uses the **Client Credentials** flow (no browser, suitable for CI). Otherwise it opens an interactive browser login once per test file.

**Local server overrides** — create `TCMRemote.PesterSetup.Debug.ps1` in the module root (gitignored) to override any setup variable for your own environment:

```powershell
# TCMRemote.PesterSetup.Debug.ps1  (not committed)
$tcmBaseUrl              = "https://my-dev-server.example.com"
$tcmPublicationId_Master = "tcm:0-3-1"   # different pub IDs on your server
$tcmItemId_HomeStructureGroup = "tcm:3-7-4"
```

### Test structure

Every test file follows this standard pattern:

```powershell
BeforeAll {
    $cmdletName = "Verb-TcmNoun"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Verb-TcmNoun" -Tags "Read" {
    Context "Verb-TcmNoun Parameter validation" {
        It "Parameter <X> invalid throws" {
            { Verb-TcmNoun -Param "INVALID" -ErrorAction Stop } | Should -Throw
        }
    }
    Context "Verb-TcmNoun returns <Type>" {
        BeforeAll { $result = Verb-TcmNoun -Param $validParam }
        It "Returns an object"         { $result | Should -Not -BeNullOrEmpty }
        It "Id matches requested Id"   { $result.Id | Should -Be $validParam }
    }
}

AfterAll { Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup") }
```

Key conventions:

- **Non-terminating errors** — cmdlets use `WriteError` rather than `ThrowTerminatingError`. Add `-ErrorAction Stop` inside a `{ }` block when testing error cases with `Should -Throw`.
- **Pipeline collection** — when filtering a stream from a cmdlet, always collect to an array first (`$all = @(Get-TcmItemsInContainer ...)`) before applying `Where-Object` / `Select-Object -First 1`. This avoids PS 5.1 pipeline-stop errors.
- **Lazy authentication** — `New-TcmSession` defers token acquisition to the first API call. A session created with invalid credentials returns a valid `TcmSession` object; the error only surfaces when you call `Test-TcmSession` or another cmdlet against the server.
- **Tags** — use `"Read"` for read-only tests, `"Write"` for mutating tests. This allows running a safe read-only subset in production-like environments.

---

## Publishing to PSGallery

The GitHub Actions workflow publishes automatically based on a tag in the commit message:

| Commit message tag | Action |
|---|---|
| `[PublishToPSGalleryAsPreview]` | Publishes with `-Prerelease` suffix |
| `[PublishToPSGalleryAsRelease]` | Publishes as a stable release |

Manual publish:

```powershell
Publish-Module -Path "...\bin\Release\TCMRemote" -NuGetApiKey $env:NUGET_API_KEY -Repository PSGallery
```

---

## Contributing

Pull requests are welcome. Please follow these guidelines:

- Follow [Microsoft's Strongly Encouraged Development Guidelines](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines).
- Add XML doc comments (`///`) on all public members — these drive `Get-Help` and the MAML help file.
- Add `-WhatIf` / `-Confirm` support (`SupportsShouldProcess = true`) to all write cmdlets.
- Write Pester 5 tests (`.Tests.ps1`) **co-located** with every new cmdlet, following the standard test structure described above.
- Keep `-TcmSession` optional on every cmdlet — fall back to the session state variable set by `New-TcmSession`.
- All parameters: singular form (`-ItemId`, not `-ItemIds`).
- Use `WriteError` (non-terminating) for recoverable errors; use `ThrowTerminatingError` only for unrecoverable ones. Document the choice in the corresponding test with `-ErrorAction Stop` or a `WriteWarning` + return value pattern.
- When filtering large result sets in tests, collect the stream to an array before filtering — avoid piping directly into `Select-Object -First 1` from a C# cmdlet (PS 5.1 pipeline-stop issue).

---

## License

Apache 2.0 — see [LICENSE.TXT](LICENSE.TXT).

Copyright © 2024–2026 RWS Holdings plc including its subsidiaries and affiliated companies.

---

## Related projects

| Project | Description |
|---------|-------------|
| [TCMRemote](https://github.com/RWS/TCM-Remote) | PowerShell automation for Tridion Docs Content Manager |
| [tridion-powershell-modules](https://github.com/Tridion/tridion-powershell-modules) | Legacy WCF-based Tridion Sites PowerShell module |
| Tridion Access Management | OIDC / OAuth2 identity provider bundled with Tridion Sites 10+ |
