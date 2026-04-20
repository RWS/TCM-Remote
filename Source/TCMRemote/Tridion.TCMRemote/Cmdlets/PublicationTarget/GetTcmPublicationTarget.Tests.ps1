BeforeAll {
    $cmdletName = "Get-TcmPublicationTarget"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmPublicationTarget" -Tags "Read" {

    Context "Get-TcmPublicationTarget returns target list" {
        BeforeAll {
            $targets = Get-TcmPublicationTarget
        }
        It "Returns a non-empty list" {
            $targets | Should -Not -BeNullOrEmpty
        }
        It "Each target GetType()" {
            $targets[0].GetType().Name | Should -BeExactly "TargetType"
        }
        It "Each target has an Id" {
            $targets | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }
        It "Each target has a Title" {
            $targets | ForEach-Object { $_.Title | Should -Not -BeNullOrEmpty }
        }
        It "Stores first target Id for downstream use" {
            $script:tcmTargetTypeId = $targets[0].Id
            $script:tcmTargetTypeId | Should -Not -BeNullOrEmpty
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
