BeforeAll {
    $cmdletName = "Get-TcmItemsInContainer"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmItemsInContainer" -Tags "Read" {

    Context "Get-TcmItemsInContainer Parameters" {
        It "Invalid ContainerId throws" {
            { Get-TcmItemsInContainer -ContainerId "tcm:0-99999-1" -ErrorAction Stop } | Should -Throw
        }
    }

    Context "Get-TcmItemsInContainer from publication root" {
        BeforeAll {
            $items = Get-TcmItemsInContainer -ContainerId $tcmPublicationId_Master
        }
        It "Returns items" {
            $items | Should -Not -BeNullOrEmpty
        }
        It "Each item has an Id" {
            $items | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Each item has a Title" {
            $items | ForEach-Object { $_.Title | Should -Not -BeNullOrEmpty }
        }
    }

    Context "Get-TcmItemsInContainer from structure group" {
        BeforeAll {
            $items = Get-TcmItemsInContainer -ContainerId $tcmItemId_HomeStructureGroup
        }
        It "Returns items from structure group" {
            $items | Should -Not -BeNullOrEmpty
        }
    }

    Context "Get-TcmItemsInContainer with Recursive flag" {
        It "Runs without error" {
            { Get-TcmItemsInContainer -ContainerId $tcmPublicationId_Master -Recursive } | Should -Not -Throw
        }
        It "Returns more items than non-recursive" {
            $flat      = (Get-TcmItemsInContainer -ContainerId $tcmPublicationId_Master).Count
            $recursive = (Get-TcmItemsInContainer -ContainerId $tcmPublicationId_Master -Recursive).Count
            $recursive | Should -BeGreaterOrEqual $flat
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
