BeforeAll {
    $cmdletName = "Get-TcmPublication"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmPublication" -Tags "Read" {

    Context "Get-TcmPublication Parameters" {
        It "Parameter ItemId invalid throws" {
            { Get-TcmPublication -ItemId "tcm:0-99999-1" -ErrorAction Stop } | Should -Throw
        }
    }

    Context "Get-TcmPublication returns Publication object" {
        BeforeAll {
            $publication = Get-TcmPublication -ItemId $tcmPublicationId_ExampleSite
        }
        It "GetType()" {
            $publication.GetType().Name | Should -BeExactly "Publication"
        }
        It "Id matches requested Id" {
            $publication.Id | Should -Be $tcmPublicationId_ExampleSite
        }
        It "Title is not empty" {
            $publication.Title | Should -Not -BeNullOrEmpty
        }
        It "Locale is set" {
            $publication.Locale | Should -Not -BeNullOrEmpty
        }
        It "PublicationUrl is set" {
            $publication.PublicationUrl | Should -Not -BeNullOrEmpty
        }
    }

    Context "Get-TcmPublication for Master publication" {
        BeforeAll {
            $master = Get-TcmPublication -ItemId $tcmPublicationId_Master
        }
        It "Title contains Master" {
            $master.Title | Should -BeLike "*Master*"
        }
        It "Key is not empty" {
            $master.Key | Should -Not -BeNullOrEmpty
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
