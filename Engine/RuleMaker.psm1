Function New-RuleObject
{
    param(
        [string] $Name,
        [string] $Severity,
        [string] $CommonName,
        [string] $Description,
        [string] $Error)

    New-Object -TypeName 'PSObject' -Property @{
        Name = $Name
        Severity = $Severity
        CommonName = $CommonName
        Description = $Description
        Error = $Error
        SourceFileName = $Name + ".cs"
    }
}

Function Get-SolutionRoot
{
    $PSModule = $ExecutionContext.SessionState.Module
    $path = $PSModule.ModuleBase
    $root = Split-Path -Path $path -Parent
    $solutionFilename = 'psscriptanalyzer.sln'
    if (-not (Test-Path (Join-Path $root $solutionFilename)))
    {
        $null
    }
    $root
}

Function Get-RuleProjectRoot
{
    $slnRoot = Get-SolutionRoot
    if ($slnRoot -eq $null)
    {
        $null
    }
    Join-Path $slnRoot "Rules"
}

Function Get-RuleProjectFile
{
    $prjRoot = Get-RuleProjectRoot
    if ($prjRoot -eq $null)
    {
        $null
    }
    Join-Path $prjRoot "ScriptAnalyzerBuiltinRules.csproj"
}

Function Get-RuleSourcePath($Rule)
{
    $ruleRoot = Get-RuleProjectRoot
    Join-Path $ruleRoot $Rule.SourceFileName
}

Function Get-RuleTemplate
{
    $ruleTemplate = @'
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{{
    /// <summary>
    /// A class to walk an AST to check for [violation]
    /// </summary>
    #if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class {0} : IScriptRule
    {{
        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {{
            if (ast == null)
            {{
                throw new ArgumentNullException("ast");
            }}

            // your code goes here
            yield return new DiagnosticRecord()
        }}

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public string GetCommonName()
        {{
            return string.Format(CultureInfo.CurrentCulture, Strings.{0}CommonName);
        }}

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public string GetDescription()
        {{
            return string.Format(CultureInfo.CurrentCulture, Strings.{0}Description);
        }}

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public string GetName()
        {{
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.{0}Name);
        }}

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public RuleSeverity GetSeverity()
        {{
            return RuleSeverity.{1};
        }}

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {{
            return DiagnosticSeverity.{1};
        }}

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {{
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }}

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public SourceType GetSourceType()
        {{
            return SourceType.Builtin;
        }}
    }}
}}
'@
    $ruleTemplate
}

Function Get-RuleSource($Rule)
{
    $source = (Get-RuleTemplate) -f $Rule.Name,$Rule.Severity
    $source
}

Function New-RuleSource($Rule)
{
    $ruleSourceFilepath = Get-RuleSourcePath $Rule
    $ruleSource = Get-RuleSource $Rule
    New-Item -Path $ruleSourceFilepath -ItemType File
    Set-Content -Path $ruleSourceFilepath -Value $ruleSource -Encoding UTF8
}

Function Remove-RuleSource($Rule)
{
    $ruleSourceFilePath = Get-RuleSourcePath $Rule
    if (Test-Path $ruleSourceFilePath)
    {
        Remove-Item $ruleSourceFilePath
    }
}

Function Get-RuleDocumentationPath($Rule)
{
    $root = Get-SolutionRoot
    $ruleDocDir = Join-Path $root 'RuleDocumentation'
    $ruleDocPath = Join-Path $ruleDocDir ($Rule.Name + ".md")
    $ruleDocPath
}

Function New-RuleDocumentation($Rule)
{
    $ruleDocTemplate = @"
# {0}
**Severity Level: {1}**

## Description

## How to Fix

## Example
### Wrong：
``````PowerShell

``````

### Correct:
``````PowerShell

``````
"@
    $ruleDoc = $ruleDocTemplate -f $Rule.Name,$Rule.Severity
    $ruleDocPath = Get-RuleDocumentationPath $Rule
    Set-Content -Path $ruleDocPath -Value $ruleDoc -Encoding UTF8
}

Function Remove-RuleDocumentation($Rule)
{
    $ruleDocPath = Get-RuleDocumentationPath $Rule
    Remove-Item $ruleDocPath
}

Function Get-RuleStringsPath
{
    $ruleRoot = Get-RuleProjectRoot
    $stringsFilepath = Join-Path $ruleRoot 'Strings.resx'
    $stringsFilepath
}

Function Get-RuleStrings
{
    $stringsFilepath = Get-RuleStringsPath
    [xml]$stringsXml = New-Object xml
    $stringsXml.Load($stringsFilepath)
}

Function Set-RuleStrings
{
    param([xml]$stringsXml)
    $stringsFilepath = Get-RuleStringsPath
    $stringsXml.Save($stringsFilepath)
}

Function Add-RuleStrings($Rule)
{
    $stringsXml = Get-RuleStrings $Rule

    Function Add-Node($nodeName, $nodeValue)
    {
        $dataNode = $stringsXml.CreateElement("data")
        $nameAttr = $stringsXml.CreateAttribute("name")
        $nameAttr.Value = $nodeName
        $xmlspaceAttr = $stringsXml.CreateAttribute("xml:space")
        $xmlspaceAttr.Value = "preserve"
        $valueElem = $stringsXml.CreateElement("value")
        $valueElem.InnerText = $nodeValue
        $dataNode.Attributes.Append($nameAttr)
        $dataNode.Attributes.Append($xmlspaceAttr)
        $dataNode.AppendChild($valueElem)
        $stringsXml.AppendChild($dataNode)
    }

    Add-Node ($Rule.Name + 'Name') $Rule.Name
    Add-Node ($Rule.Name + 'CommonName') $Rule.CommonName
    Add-Node ($Rule.Name + 'Description') $Rule.Description
    Add-Node ($Rule.Name + 'Error') $Rule.Error
    Set-RuleStrings $stringsXml
}

Function Remove-RuleStrings($Rule)
{
    $stringsXml = Get-RuleStrings $Rule
    $nodesToRemove = $stringsXml.root.GetElementsByTagName("data") | ? {$_.name -match $Rule.Name}
    $nodesToRemove | Foreach-Object { $stringsXml.root.RemoveChild($_) }
    Set-RuleStrings $stringsXml
}

Function Get-RuleProjectXml
{
    $ruleProject = Get-RuleProjectFile
    $projectXml = New-Object -TypeName 'xml'
    $projectXml.Load($ruleProject)
    $projectXml
}

Function Set-RuleProjectXml($projectXml)
{
    $ruleProjectFilepath = Get-RuleProjectFile
    $projectXml.Save($ruleProjectFilepath)
}

Function Get-CompileTargetGroup($projectXml)
{
    $projectXml.Project.ItemGroup | ? {$_.Compile -ne $null}
}

Function Add-RuleToProject($Rule)
{
    $projectXml = Get-RuleProjectXml
    $compileItemgroup = Get-CompileTargetGroup $projectXml
    $compileElement = $compileItemgroup.Compile.Item(0).Clone()
    $compileElement.Include = $Rule.SourceFileName
    $compileItemgroup.AppendChild($compileElement)
    Set-RuleProjectXml $projectXml
}

Function Remove-RuleFromProject($Rule)
{
    $projectXml = Get-RuleProjectXml
    $compileItemgroup = Get-CompileTargetGroup $projectXml
    $itemToRemove = $compileItemgroup.Compile | ? {$_.Include -eq $Rule.SourceFileName}
    $compileItemgroup.RemoveChild($itemToRemove)
    Set-RuleProjectXml $projectXml
}

Function New-Rule
{
    param(
        [string] $Name,
        [string] $Severity,
        [string] $CommonName,
        [string] $Description,
        [string] $Error)

    $rule = New-RuleObject $Name $Severity $CommonName $Description $Error
}