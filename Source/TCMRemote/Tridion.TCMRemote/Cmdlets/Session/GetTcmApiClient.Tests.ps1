BeforeAll {
    $cmdletName = "Get-TcmApiClient"
    Write-Host ("`r`nLoading TCMRemote.PesterSetup.ps1 on PSVersion[" + $PSVersionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
    . (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\TCMRemote.PesterSetup.ps1")
    Write-Host ("Running " + $cmdletName + " Test Data and Variables initialization")
}

Describe "Get-TcmApiClient" -Tags "Read" {

    Context "Get-TcmApiClient returns the raw REST API client" {
        BeforeAll {
            $result = Get-TcmApiClient
        }
        It "Returns a non-null client" {
            $result | Should -Not -BeNullOrEmpty
        }
        It "Returns OpenApiTCM30Client type" {
            $result.GetType().Name | Should -BeExactly "OpenApiTCM30Client"
        }
        It "Client has REST methods (GetItemAsync exists)" {
            $method = $result | Get-Member -MemberType Method -Name "GetItemAsync"
            $method | Should -Not -BeNullOrEmpty
        }
        It "Client has GetOwnUserProfileAsync method" {
            $method = $result | Get-Member -MemberType Method -Name "GetOwnUserProfileAsync"
            $method | Should -Not -BeNullOrEmpty
        }
        It "Client has GetPublicationsAsync method" {
            $method = $result | Get-Member -MemberType Method -Name "GetPublicationsAsync"
            $method | Should -Not -BeNullOrEmpty
        }
    }

    Context "Get-TcmApiClient accepts -TcmSession parameter" {
        It "Accepts explicit session and returns client" {
            $result = Get-TcmApiClient -TcmSession $tcmSession
            $result | Should -Not -BeNullOrEmpty
            $result.GetType().Name | Should -BeExactly "OpenApiTCM30Client"
        }
    }

    Context "Get-TcmApiClient raw REST calls" {
        BeforeAll {
            $api = Get-TcmApiClient
        }
        It "GetOwnUserProfileAsync returns a user profile" {
            $profile = $api.GetOwnUserProfileAsync().GetAwaiter().GetResult()
            $profile | Should -Not -BeNullOrEmpty
        }
        It "GetOwnUserProfileAsync returns a profile with a Title" {
            $profile = $api.GetOwnUserProfileAsync().GetAwaiter().GetResult()
            $profile.Title | Should -Not -BeNullOrEmpty
        }
        It "GetPublicationsAsync(null) returns publications" {
            $pubs = $api.GetPublicationsAsync($null).GetAwaiter().GetResult()
            $pubs | Should -Not -BeNullOrEmpty
        }
        It "GetPublicationsAsync(null) returns at least one publication" {
            $pubs = $api.GetPublicationsAsync($null).GetAwaiter().GetResult()
            @($pubs).Count | Should -BeGreaterThan 0
        }
        It "GetItemAsync with TCM URI escape returns an item (tcm:0-2-1 -> tcm_0-2-1)" {
            # TCM URI escaping: replace ':' with '_'
            $escapedId = $tcmPublicationId_Master -replace ':', '_'
            $item = $api.GetItemAsync($escapedId, $null).GetAwaiter().GetResult()
            $item | Should -Not -BeNullOrEmpty
        }
        It "GetItemAsync returns item with matching Id" {
            $escapedId = $tcmPublicationId_Master -replace ':', '_'
            $item = $api.GetItemAsync($escapedId, $null).GetAwaiter().GetResult()
            $item.Id | Should -Be $tcmPublicationId_Master
        }
    }

    Context "Get-TcmApiClient error handling" {
        It "Returns non-terminating error (not exception) with invalid credentials" {
            $badSession = New-TcmSession -BaseUrl $tcmBaseUrl -ClientId "INVALIDID" -ClientSecret "INVALIDSECRET"
            $errors = @()
            # Must NOT throw — error is written via WriteError, not ThrowTerminatingError
            { Get-TcmApiClient -TcmSession $badSession -ErrorAction SilentlyContinue -ErrorVariable errors } | Should -Not -Throw
        }
        It "Writes an error record with invalid credentials" {
            $badSession = New-TcmSession -BaseUrl $tcmBaseUrl -ClientId "INVALIDID" -ClientSecret "INVALIDSECRET"
            $errors = @()
            Get-TcmApiClient -TcmSession $badSession -ErrorAction SilentlyContinue -ErrorVariable errors
            $errors.Count | Should -BeGreaterThan 0
        }
        It "Throws with -ErrorAction Stop and invalid credentials" {
            $badSession = New-TcmSession -BaseUrl $tcmBaseUrl -ClientId "INVALIDID" -ClientSecret "INVALIDSECRET"
            { Get-TcmApiClient -TcmSession $badSession -ErrorAction Stop } | Should -Throw
        }
    }
}

AfterAll {
    Write-Host ("Running " + $cmdletName + " Test Data and Variables cleanup")
}
