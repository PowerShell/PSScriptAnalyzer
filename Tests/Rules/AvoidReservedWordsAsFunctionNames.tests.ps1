# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Keep in sync with the rule's reserved words list in
# Rules/AvoidReservedWordsAsFunctionNames.cs
$reservedWords = @(
    'begin','break','catch','class','configuration',
    'continue','data','define','do',
    'dynamicparam','else','elseif','end',
    'enum','exit','filter','finally',
    'for','foreach','from','function',
    'if','parallel','param','process',
    'return','sequence','switch',
    'throw','trap','try','type',
    'until','using','var','while','workflow'
)

$randomCasedReservedWords = @(
    'bEgIN','bReAk','cAtCh','CLasS','cONfiGuRaTioN',
    'cONtiNuE','dAtA','dEFInE','Do',
    'DyNaMiCpArAm','eLsE','eLsEiF','EnD',
    'EnUm','eXiT','fIlTeR','fINaLLy',
    'FoR','fOrEaCh','fROm','fUnCtIoN',
    'iF','pArAlLeL','PaRaM','pRoCeSs',
    'ReTuRn','sEqUeNcE','SwItCh',
    'tHrOw','TrAp','tRy','TyPe',
    'uNtIl','UsInG','VaR','wHiLe','wOrKfLoW'
)

$functionScopes = @(
	"global", "local", "script", "private"
)

# Generate all combinations of reserved words and function scopes
$scopedReservedWordCases = foreach ($scope in $functionScopes) {
    foreach ($word in $reservedWords) {
        @{
            Scope = $scope
            Name  = $word
        }
    }
}

# Build variants of reserved words where the reserverd word:
# appearing at the start and end of a function
# name.
$substringReservedWords = $reservedWords | ForEach-Object {
    "$($_)A", "A$($_)", $_.Substring(0, $_.Length - 1)
}

$safeFunctionNames = @(
    'Get-Something','Do-Work','Classify-Data','Begin-Process'
)

BeforeAll {
    $ruleName = 'PSAvoidReservedWordsAsFunctionNames'
}

Describe 'AvoidReservedWordsAsFunctionNames' {
	Context 'When function names are reserved words' {
		It 'flags reserved word "<_>" as a violation' -TestCases $reservedWords {

			$scriptDefinition = "function $_ { 'test' }"
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)

			$violations.Count | Should -Be 1
			$violations[0].Severity | Should -Be 'Warning'
			$violations[0].RuleName | Should -Be $ruleName
			# Message text should include the function name as used
			$violations[0].Message | Should -Be "The reserved word '$_' was used as a function name. This should be avoided."
			# Extent should ideally capture only the function name
			$violations[0].Extent.Text | Should -Be $_
		}

		It 'flags the correct extent for a function named Function' {

			$scriptDefinition = "Function Function { 'test' }"
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)

			$violations.Count | Should -Be 1
			$violations[0].Severity | Should -Be 'Warning'
			$violations[0].RuleName | Should -Be $ruleName
			# Message text should include the function name as used
			$violations[0].Message | Should -Be "The reserved word 'Function' was used as a function name. This should be avoided."
			# Extent should ideally capture only the function name
			$violations[0].Extent.Text | Should -Be 'Function'

			# Make sure the extent is the correct `Function` (not the one at the
			# very start)
			$violations[0].Extent.StartOffset | Should -not -Be 0
		}

		# Functions can have scopes. So function global:function {} should still
		# alert.
		It 'flags reserved word "<Name>" with scope "<Scope>" as a violation' -TestCases $scopedReservedWordCases {
			param($Scope, $Name)

			$scriptDefinition = "function $($Scope):$($Name) { 'test' }"
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)

			$violations.Count | Should -Be 1
			$violations[0].Severity | Should -Be 'Warning'
			$violations[0].RuleName | Should -Be $ruleName
			$violations[0].Message | Should -Be "The reserved word '$Name' was used as a function name. This should be avoided."
			$violations[0].Extent.Text | Should -Be "$($Scope):$($Name)"
		}


		It 'detects case-insensitively for "<_>"' -TestCases $randomCasedReservedWords {
			$scriptDefinition = "function $_ { }"
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
			$violations.Count | Should -Be 1
			$violations[0].Message | Should -Be "The reserved word '$_' was used as a function name. This should be avoided."
		}

		It 'reports one finding per offending function' {
			$scriptDefinition = 'function class { };function For { };function Safe-Name { };function TRy { }'
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)

			$violations.Count | Should -Be 3
			$violations | ForEach-Object { $_.Severity | Should -Be 'Warning' }
			($violations | Select-Object -ExpandProperty Extent | Select-Object -ExpandProperty Text) |
				Sort-Object |
				Should -Be @('class','For','TRy')
		}
	}

	Context 'When there are no violations' {
		It 'does not flag safe function name "<_>"' -TestCases $safeFunctionNames {
			$scriptDefinition = "function $_ { }"
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
			$violations.Count | Should -Be 0
		}

		It 'does not flag when script has no functions' {
			$scriptDefinition = '"hello";$x = 1..3 | ForEach-Object { $_ }'
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
			$violations.Count | Should -Be 0
		}

		It 'does not flag substring-like name "<_>"' -TestCases $substringReservedWords {
			$scriptDefinition = "function $_ { }"
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
			$violations.Count | Should -Be 0
		}
	}
}
