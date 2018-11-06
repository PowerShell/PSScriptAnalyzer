Describe "Issue 1095: An exception is thrown when CurrentCulture is Turkish (tr-TR)" {
    It "Should not throw an exception when CurrentCulture is tr-TR" {
        # https://github.com/PowerShell/PSScriptAnalyzer/issues/1095

		$initialCulture = Get-Culture
        $initialUICulture = Get-UICulture

        {
            $trTRculture = [System.Globalization.CultureInfo]::GetCultureInfo('tr-TR')
            [System.Threading.Thread]::CurrentThread.CurrentUICulture = $trTRculture
            [System.Threading.Thread]::CurrentThread.CurrentCulture = $trTRculture
            Invoke-Formatter "`$test" -ErrorAction Stop

        } | Should -Throw -Not

        [System.Threading.Thread]::CurrentThread.CurrentCulture = $initialCulture
        [System.Threading.Thread]::CurrentThread.CurrentUICulture = $initialUICulture
       
    }
}