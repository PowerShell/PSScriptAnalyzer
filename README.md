<html>
<head>
	<title></title>
</head>
<body>
<p><strong>Introduction</strong></p>

<p>ScriptAnalyzer is a static code checker for Windows PowerShell modules and scripts. ScriptAnalyzer checks the quality of Windows PowerShell code by running a set of rules. The rules are based on PowerShell best practices identified by PowerShell Team and the community. It generates DiagnosticResults (errors and warnings) to inform users about potential code defects and suggests possible solutions for improvements.</p>

<p>ScriptAnalyzer is shipped with a collection of built-in rules that checks various aspects of PowerShell code such as presence of uninitialized variables, usage of PSCredential Type, usage of Invoke-Expression etc. Additional functionalities such as exclude/include specific rules are also supported.</p>

<p>&nbsp;</p>

<p><strong>ScriptAnalyzer cmdlets:</strong></p>

<p>Get-ScriptAnalyzerRule &nbsp;[-CustomizedRulePath &lt;string[]&gt;]<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;[-Name &lt;string[]&gt;]<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;[&lt;CommonParameters&gt;]</p>

<p><br />
Invoke-ScriptAnalyzer [-Path] &lt;string&gt; [-CustomizedRulePath &lt;string[]&gt;]&nbsp;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; [-ExcludeRule &lt;string[]&gt;]&nbsp;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; [-IncludeRule &lt;string[]&gt;]&nbsp;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; [-Severity &lt;string[]&gt;]&nbsp;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; [-Recurse]&nbsp;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; [&lt;CommonParameters&gt;]</p>

<p>&nbsp;</p>

<p>&nbsp;</p>

<p><strong>Requirements</strong></p>

<p>WS2012R2 / Windows 8.1 / Windows OS containing PowerShell v5.0 which can be obtained using Windows Management Framework 5.0 Preview February 2015.</p>

<p>&nbsp;</p>

<p><strong>Installation</strong></p>

<p>1) &nbsp; &nbsp; &nbsp;Build the Code using Visual Studio</p>

<p>2) &nbsp; &nbsp; &nbsp;Copy following files to $env:ProgramFiles\WindowsPowerShell\Modules\ScriptAnalyzer</p>

<p>3) &nbsp; &nbsp; &nbsp;In PowerShell Console :<br />
Import-Module $env:ProgramFiles\WindowsPowerShell\Modules\ScriptAnalyzer\PSScriptAnalyzer.psd1</p>

<p>To confirm installation:</p>

<p>&middot; &nbsp; &nbsp; &nbsp; &nbsp; Run Get-ScriptAnalyzerRule in the PowerShell console to obtain the built-in rules</p>

<p>&nbsp;</p>

<p><br />
<strong>Building the Code</strong></p>

<p>Use Visual Studio or any C# compiler to build the code</p>

<p>&nbsp;</p>

<p><br />
<strong>Running Tests</strong></p>

<p>Pester based ScriptAnalyzer Tests are located in &ldquo;&lt;branch&gt;/ScriptAnalyzer/Tests&rdquo; folder</p>

<ul>
	<li>Ensure Pester is installed on the machine</li>
	<li>Go the Tests folder in your local repository</li>
	<li>Run Engine Tests:
	<ul>
		<li>.\InvokeScriptAnalyzer.tests.ps1</li>
	</ul>
	</li>
	<li>Run Tests for Built-in rules:
	<ul>
		<li>.\*.ps1 (Example - .\ AvoidConvertToSecureStringWithPlainText.ps1)<br />
		&nbsp;</li>
	</ul>
	</li>
</ul>

<p><br />
<strong>Contributing to ScriptAnalyzer</strong></p>

<p>You are welcome to contribute to this project. There are many ways to contribute:</p>

<ul>
	<li>Submit a bug report via Issues. For a guide to submitting good bug reports, please read Painless Bug Tracking.</li>
	<li>Verify fixes for bugs.</li>
	<li>Submit your fixes for a bug. Before submitting, please make sure you have:
	<ul>
		<li>Performed code reviews of your own</li>
		<li>Updated the test cases if needed</li>
		<li>Run the test cases to ensure no feature breaks or test breaks</li>
		<li>Added the test cases for new code</li>
	</ul>
	</li>
	<li>Submit a feature request.</li>
	<li>Help answer questions in the discussions list.</li>
	<li>Submit test cases.</li>
	<li>Tell others about the project.</li>
	<li>Tell the developers how much you appreciate the product!</li>
</ul>

<p><br />
You might also read these two blog posts about contributing code: Open Source Contribution Etiquette by Miguel de Icaza, and Don&rsquo;t &ldquo;Push&rdquo; Your Pull Requests by Ilya Grigorik.</p>

<p>Before submitting a feature or substantial code contribution, please discuss it with the Windows PowerShell team via Issues, and ensure it follows the product roadmap. Note that all code submissions will be rigorously reviewed by the Windows PowerShell Team. Only those that meet a high bar for both quality and roadmap fit will be merged into the source.</p>

<p>&nbsp;</p>
</body>
</html>
