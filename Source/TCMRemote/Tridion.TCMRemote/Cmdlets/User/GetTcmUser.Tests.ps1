BeforeAll {
    $cmdletName = "Get-TcmUser"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmUser" -Tags "Read" {

    Context "Get-TcmUser Parameters" {
        It "Invalid TcmSession throws" {
            { Get-TcmUser -TcmSession "INVALIDTCMSESSION" } | Should -Throw
        }
    }

    Context "Get-TcmUser returns user list" {
        BeforeAll {
            $users = Get-TcmUser
        }
        It "Returns a non-empty list" {
            $users | Should -Not -BeNullOrEmpty
        }
        It "Each user GetType()" {
            $users[0].GetType().Name | Should -BeExactly "User"
        }
        It "Each user has an Id" {
            $users | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Each user has a Title" {
            $users | ForEach-Object { $_.Title | Should -Not -BeNullOrEmpty }
        }
        It "Each user has an IsEnabled flag" {
            $users | ForEach-Object { $_.IsEnabled | Should -Not -BeNullOrEmpty }
        }
        It "Contains the current user" {
            $currentUserId = (Get-TcmCurrentUser).User.Id
            $match = $users | Where-Object { $_.Id -eq $currentUserId }
            $match | Should -Not -BeNullOrEmpty
        }
    }

    Context "Get-TcmUser with IncludeDisabled" {
        It "Runs without error" {
            { Get-TcmUser -IncludeDisabled } | Should -Not -Throw
        }
        It "Result count is >= enabled-only count" {
            $all      = (Get-TcmUser -IncludeDisabled).Count
            $enabled  = (Get-TcmUser).Count
            $all | Should -BeGreaterOrEqual $enabled
        }
    }

    Context "Get-TcmUser with Search filter" {
        It "Returns fewer results than unfiltered" {
            $all    = Get-TcmUser
            $search = Get-TcmUser -Search "admin"
            $search.Count | Should -BeLessOrEqual $all.Count
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
