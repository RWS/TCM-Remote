BeforeAll {
    $cmdletName = "Test-TcmSession"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Test-TcmSession" -Tags "Read" {

    Context "Test-TcmSession with active session" {
        It "Returns true for a valid session" {
            $result = Test-TcmSession
            $result | Should -BeTrue
        }
        It "Returns bool type" {
            $result = Test-TcmSession
            $result | Should -BeOfType [bool]
        }
    }

    Context "Test-TcmSession with explicit TcmSession parameter" {
        It "Returns true when session passed explicitly" {
            $result = Test-TcmSession -TcmSession $tcmSession
            $result | Should -BeTrue
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
