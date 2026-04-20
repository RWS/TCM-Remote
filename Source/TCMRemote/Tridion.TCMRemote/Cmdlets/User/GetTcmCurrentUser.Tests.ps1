BeforeAll {
    $cmdletName = "Get-TcmCurrentUser"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmCurrentUser" -Tags "Read" {

    Context "Get-TcmCurrentUser returns UserProfile object" {
        BeforeAll {
            $userProfile = Get-TcmCurrentUser
        }
        It "GetType()" {
            $userProfile.GetType().Name | Should -BeExactly "UserProfile"
        }
        It "DisplayName is not empty" {
            $userProfile.DisplayName | Should -Not -BeNullOrEmpty
        }
        It "User is not null" {
            $userProfile.User | Should -Not -BeNullOrEmpty
        }
        It "User.Id is not empty" {
            $userProfile.User.Id | Should -Not -BeNullOrEmpty
        }
        It "User.Id has TCM URI format" {
            $userProfile.User.Id | Should -Match "^tcm:\d+-\d+-\d+$"
        }
    }

    Context "Get-TcmCurrentUser with explicit TcmSession" {
        It "Returns UserProfile when session passed explicitly" {
            $result = Get-TcmCurrentUser -TcmSession $tcmSession
            $result.DisplayName | Should -Not -BeNullOrEmpty
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
