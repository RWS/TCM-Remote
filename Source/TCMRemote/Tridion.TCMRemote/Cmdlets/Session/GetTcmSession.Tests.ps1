BeforeAll {
    $cmdletName = "Get-TcmSession"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmSession" -Tags "Read" {

    Context "Get-TcmSession returns active session" {
        BeforeAll {
            $result = Get-TcmSession
        }
        It "GetType()" {
            $result.GetType().Name | Should -BeExactly "TcmSession"
        }
        It "BaseUrl matches server" {
            $result.BaseUrl | Should -Be $tcmBaseUrl
        }
        It "Name is not empty" {
            $result.Name | Should -Not -BeNullOrEmpty
        }
        It "AuthenticationType is not empty" {
            $result.AuthenticationType | Should -Not -BeNullOrEmpty
        }
        It "CreatedAt is set" {
            $result.CreatedAt | Should -Not -BeNullOrEmpty
        }
    }

    Context "Get-TcmSession returns consistent data across calls" {
        It "Two consecutive calls return same BaseUrl" {
            $first  = Get-TcmSession
            $second = Get-TcmSession
            $first.BaseUrl | Should -Be $second.BaseUrl
        }
        It "Returned session matches global tcmSession" {
            $result = Get-TcmSession
            $result.BaseUrl | Should -Be $global:tcmSession.BaseUrl
            $result.Name    | Should -Be $global:tcmSession.Name
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
