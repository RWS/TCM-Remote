BeforeAll {
    $cmdletName = "Get-TcmItemHistory"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmItemHistory" -Tags "Read" {

    Context "Get-TcmItemHistory returns version history" {
        BeforeAll {
            # StructureGroup is not a VersionedItem; find a page or component from the container
            # Collect all items first to avoid Select-Object -First 1 stopping the pipeline mid-stream (PS 5.1 issue)
            $allItems = @(Get-TcmItemsInContainer -ContainerId $tcmItemId_HomeStructureGroup -Recursive)
            $versionedItem = $allItems | Where-Object { $_.Id -match "tcm:\d+-\d+-(64|16|32|4096)$" } | Select-Object -First 1
            $script:versionedItemId = $versionedItem.Id
            $history = if ($script:versionedItemId) {
                Get-TcmItemHistory -ItemId $script:versionedItemId
            } else { $null }
            $script:history = $history
        }
        It "Found a VersionedItem to test with" {
            $script:versionedItemId | Should -Not -BeNullOrEmpty
        }
        It "Returns history entries" {
            $script:history | Should -Not -BeNullOrEmpty
        }
        It "Each entry has an Id" {
            $script:history | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Each entry has a Title" {
            $script:history | ForEach-Object { $_.Title | Should -Not -BeNullOrEmpty }
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
