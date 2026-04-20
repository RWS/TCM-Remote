BeforeAll {
    $cmdletName = "New-TcmSession"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "New-TcmSession" -Tags "Read" {

    Context "New-TcmSession Parameter validation" {
        It "Parameter BaseUrl empty throws" {
            { New-TcmSession -BaseUrl "" } | Should -Throw
        }
        It "Parameter BaseUrl unreachable throws" {
            # Uses WriteError (non-terminating) so -ErrorAction Stop is needed
            { New-TcmSession -BaseUrl "http://does-not-exist.invalid" -ErrorAction Stop } | Should -Throw
        }
        It "Invalid ClientId creates session (auth is lazy) and Test-TcmSession returns false" {
            # New-TcmSession defers authentication — credential validation happens on first API call, not at session creation
            $badSession = New-TcmSession -BaseUrl $tcmBaseUrl -ClientId "INVALIDCLIENTID" -ClientSecret "INVALIDCLIENTSECRET"
            $badSession | Should -Not -BeNullOrEmpty
            # Test-TcmSession catches the auth failure and returns $false (WriteWarning, not throw)
            $result = Test-TcmSession -TcmSession $badSession
            $result | Should -Be $false
        }
    }

    Context "New-TcmSession returns TcmSession object" {
        BeforeAll {
            $localTcmSession = New-TcmSession -BaseUrl $tcmBaseUrl
        }
        It "GetType()" {
            $localTcmSession.GetType().Name | Should -BeExactly "TcmSession"
        }
        It "TcmSession.BaseUrl" {
            $localTcmSession.BaseUrl | Should -Be $tcmBaseUrl
        }
        It "TcmSession.Name is not empty" {
            $localTcmSession.Name | Should -Not -BeNullOrEmpty
        }
        It "TcmSession.AuthenticationType is not empty" {
            $localTcmSession.AuthenticationType | Should -Not -BeNullOrEmpty
        }
        It "TcmSession.CreatedAt is set" {
            $localTcmSession.CreatedAt | Should -Not -BeNullOrEmpty
        }
        It "Session is stored in SessionState" {
            $stored = $ExecutionContext.SessionState.PSVariable.GetValue("TCMRemoteSessionStateTcmSession")
            $stored | Should -Not -BeNullOrEmpty
            $stored.BaseUrl | Should -Be $localTcmSession.BaseUrl
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
