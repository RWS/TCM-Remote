BeforeAll {
    $cmdletName = "Get-TcmGroup"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmGroup" -Tags "Read" {

    Context "Get-TcmGroup Parameters" {
        It "Invalid TcmSession throws" {
            { Get-TcmGroup -TcmSession "INVALIDTCMSESSION" } | Should -Throw
        }
    }

    Context "Get-TcmGroup returns group list" {
        BeforeAll {
            $groups = Get-TcmGroup
        }
        It "Returns a non-empty list" {
            $groups | Should -Not -BeNullOrEmpty
        }
        It "Each group GetType()" {
            $groups[0].GetType().Name | Should -BeExactly "Group"
        }
        It "Each group has an Id" {
            $groups | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Each group has a Title" {
            $groups | ForEach-Object { $_.Title | Should -Not -BeNullOrEmpty }
        }
        It "Contains at least one predefined system group" {
            $predefined = $groups | Where-Object { $_.IsPredefined -eq $true }
            $predefined | Should -Not -BeNullOrEmpty
        }
    }

    Context "Get-TcmGroup scoped to publication" {
        It "Returns groups for publication without error" {
            { Get-TcmGroup -InPublicationId $tcmPublicationId_Master } | Should -Not -Throw
        }
        It "Result count is <= unscoped count" {
            $all    = (Get-TcmGroup).Count
            $scoped = (Get-TcmGroup -InPublicationId $tcmPublicationId_Master).Count
            $scoped | Should -BeLessOrEqual $all
        }
    }

    Context "Get-TcmGroup with Search filter" {
        It "Returns fewer results than unfiltered" {
            $all    = Get-TcmGroup
            $search = Get-TcmGroup -Search "author"
            $search.Count | Should -BeLessOrEqual $all.Count
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
