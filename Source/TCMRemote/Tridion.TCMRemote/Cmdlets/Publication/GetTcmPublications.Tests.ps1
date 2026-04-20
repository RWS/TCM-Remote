BeforeAll {
    $cmdletName = "Get-TcmPublications"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmPublications" -Tags "Read" {

    Context "Get-TcmPublications returns publication list" {
        BeforeAll {
            $publications = Get-TcmPublications
        }
        It "Returns a non-empty list" {
            $publications | Should -Not -BeNullOrEmpty
        }
        It "Returns more than one publication" {
            $publications.Count | Should -BeGreaterThan 1
        }
        It "Each publication GetType()" {
            $publications[0].GetType().Name | Should -BeExactly "Publication"
        }
        It "Each publication has a non-empty Id" {
            $publications | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Each publication has a non-empty Title" {
            $publications | ForEach-Object { $_.Title | Should -Not -BeNullOrEmpty }
        }
        It "Contains the known 100 Master publication" {
            $master = $publications | Where-Object { $_.Id -eq $tcmPublicationId_Master }
            $master | Should -Not -BeNullOrEmpty
            $master.Title | Should -BeLike "*Master*"
        }
        It "Contains a publication with a Locale set" {
            $withLocale = $publications | Where-Object { $_.Locale }
            $withLocale | Should -Not -BeNullOrEmpty
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
