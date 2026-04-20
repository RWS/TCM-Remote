BeforeAll {
    $cmdletName = "Get-TcmPublishTransaction"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmPublishTransaction" -Tags "Read" {

    Context "Get-TcmPublishTransaction unfiltered" {
        BeforeAll {
            $transactions = Get-TcmPublishTransaction
        }
        It "Returns without error" {
            $transactions | Should -Not -BeNullOrEmpty
        }
        It "Each transaction GetType()" {
            $transactions[0].GetType().Name | Should -BeExactly "PublishTransaction"
        }
        It "Each transaction has an Id" {
            $transactions | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Each transaction has a State" {
            $transactions | ForEach-Object { $_.State | Should -Not -BeNullOrEmpty }
        }
        It "Each transaction has a Priority" {
            $transactions | ForEach-Object { $_.Priority | Should -Not -BeNullOrEmpty }
        }
    }

    Context "Get-TcmPublishTransaction filtered by State=Success" {
        BeforeAll {
            $successful = Get-TcmPublishTransaction -State Success
        }
        It "Returns only successful transactions" {
            $successful | ForEach-Object { $_.State | Should -Be "Success" }
        }
    }

    Context "Get-TcmPublishTransaction filtered by State=Failed" {
        It "Runs without error even when no failures exist" {
            { Get-TcmPublishTransaction -State Failed } | Should -Not -Throw
        }
    }

    Context "Get-TcmPublishTransaction filtered by PublicationId" {
        It "Runs without error for a known publication" {
            { Get-TcmPublishTransaction -PublicationId $tcmPublicationId_ExampleSite } | Should -Not -Throw
        }
        It "Result count <= unfiltered count" {
            $all    = (Get-TcmPublishTransaction).Count
            $scoped = (Get-TcmPublishTransaction -PublicationId $tcmPublicationId_ExampleSite).Count
            $scoped | Should -BeLessOrEqual $all
        }
    }

    Context "Get-TcmPublishTransaction filtered by date range" {
        It "Returns transactions within last 7 days" {
            $start = (Get-Date).AddDays(-7)
            { Get-TcmPublishTransaction -StartDate $start } | Should -Not -Throw
        }
        It "Returns transactions within last 30 days" {
            $start = (Get-Date).AddDays(-30)
            $end   = Get-Date
            { Get-TcmPublishTransaction -StartDate $start -EndDate $end } | Should -Not -Throw
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
