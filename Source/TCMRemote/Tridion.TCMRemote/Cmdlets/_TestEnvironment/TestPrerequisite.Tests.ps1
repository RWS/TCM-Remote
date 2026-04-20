#
# TestPrerequisite.Tests.ps1
# Run this BEFORE the full test suite to validate the test environment.
# Checks Pester version, module availability, server reachability, and session creation.
#
BeforeAll {
    $cmdletName = "TestPrerequisite"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "TestPrerequisite" -Tags "Read" {

    Context "Pester version" {
        It "Pester 5.3.0 or later is installed" {
            $pesterVersion = [version](Get-Command Invoke-Pester).Version
            $pesterVersion | Should -BeGreaterOrEqual ([version]"5.3.0")
        }
    }

    Context "PowerShell version" {
        It "PowerShell 5.1 or later is running" {
            $PSVersionTable.PSVersion | Should -BeGreaterOrEqual ([version]"5.1")
        }
        It "PSEdition is Desktop or Core" {
            $PSVersionTable.PSEdition | Should -BeIn @("Desktop", "Core")
        }
    }

    Context "TCMRemote module" {
        It "TCMRemote module is imported" {
            $module = Get-Module -Name TCMRemote
            $module | Should -Not -BeNullOrEmpty
        }
        It "TCMRemote exports at least 10 cmdlets" {
            $cmdlets = (Get-Module -Name TCMRemote).ExportedCommands.Count
            $cmdlets | Should -BeGreaterThan 10
        }
        It "New-TcmSession cmdlet exists" {
            Get-Command New-TcmSession -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
        It "Get-TcmSession cmdlet exists" {
            Get-Command Get-TcmSession -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
        It "Get-TcmPublications cmdlet exists" {
            Get-Command Get-TcmPublications -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }

    Context "Server reachability" {
        It "Server base URL is reachable" {
            $response = Invoke-WebRequest -Uri "$tcmBaseUrl/api/v1.0/system/capabilities" -UseBasicParsing -ErrorAction Stop
            $response.StatusCode | Should -Be 200
        }
    }

    Context "Session creation" {
        It "Global tcmSession is not null" {
            $global:tcmSession | Should -Not -BeNullOrEmpty
        }
        It "Global tcmSession is a TcmSession" {
            $global:tcmSession.GetType().Name | Should -BeExactly "TcmSession"
        }
        It "Global tcmSession BaseUrl matches configured server" {
            $global:tcmSession.BaseUrl | Should -Be $tcmBaseUrl
        }
        It "Test-TcmSession returns true" {
            Test-TcmSession -TcmSession $global:tcmSession | Should -BeTrue
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
