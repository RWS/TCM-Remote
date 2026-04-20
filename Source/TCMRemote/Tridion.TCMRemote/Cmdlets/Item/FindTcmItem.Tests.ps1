BeforeAll {
    $cmdletName = "Find-TcmItem"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Find-TcmItem" -Tags "Read" {

    Context "Find-TcmItem with full-text query" {
        It "Returns results for a broad search term" {
            $results = Find-TcmItem -Query "Home"
            $results | Should -Not -BeNullOrEmpty
        }
        It "Each result has an Id" {
            $results = Find-TcmItem -Query "Home"
            $results | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Returns empty for an unmatchable term" {
            $results = Find-TcmItem -Query "xyzzy_no_match_tcmremote_$timestamp"
            $results.Count | Should -Be 0
        }
    }

    Context "Find-TcmItem with ResultLimit" {
        It "Respects ResultLimit" {
            $limited = Find-TcmItem -Query "the" -ResultLimit 2
            $limited.Count | Should -BeLessOrEqual 2
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
