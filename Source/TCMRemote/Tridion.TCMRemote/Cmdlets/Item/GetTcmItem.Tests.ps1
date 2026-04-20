BeforeAll {
    $cmdletName = "Get-TcmItem"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmItem" -Tags "Read" {

    Context "Get-TcmItem Parameters" {
        It "Parameter ItemId invalid throws" {
            { Get-TcmItem -ItemId "tcm:0-99999-4" -ErrorAction Stop } | Should -Throw
        }
        It "Invalid TcmSession throws" {
            { Get-TcmItem -ItemId $tcmItemId_HomeStructureGroup -TcmSession "INVALID" } | Should -Throw
        }
    }

    Context "Get-TcmItem returns RepositoryLocalObject" {
        BeforeAll {
            $item = Get-TcmItem -ItemId $tcmItemId_HomeStructureGroup
        }
        It "Returns an object" {
            $item | Should -Not -BeNullOrEmpty
        }
        It "Id matches requested Id" {
            $item.Id | Should -Be $tcmItemId_HomeStructureGroup
        }
        It "Title is not empty" {
            $item.Title | Should -Not -BeNullOrEmpty
        }
        It "IsEditable is set" {
            $item.IsEditable | Should -Not -BeNullOrEmpty
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
