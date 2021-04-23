#Requires -Version 3.0

# Import Localized Data
# Explicit culture needed for culture that do not match when using PowerShell Core: https://github.com/PowerShell/PowerShell/issues/8219
if ([System.Threading.Thread]::CurrentThread.CurrentUICulture.Name -ne 'en-US')
{
    Import-LocalizedData -BindingVariable Messages -UICulture 'en-US'
}
else
{
    Import-LocalizedData -BindingVariable Messages
}

<#
.SYNOPSIS
    Uses #Requires -RunAsAdministrator instead of your own methods.
.DESCRIPTION
    The #Requires statement prevents a script from running unless the Windows PowerShell version, modules, snap-ins, and module and snap-in version prerequisites are met.
    From Windows PowerShell 4.0, the #Requires statement let script developers require that sessions be run with elevated user rights (run as Administrator).
    Script developers does not need to write their own methods any more.
    To fix a violation of this rule, please consider to use #Requires -RunAsAdministrator instead of your own methods.
.EXAMPLE
    Measure-RequiresRunAsAdministrator -ScriptBlockAst $ScriptBlockAst
.INPUTS
    [System.Management.Automation.Language.ScriptBlockAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    None
#>
function Measure-RequiresRunAsAdministrator
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    Process
    {
        $results = @()

        try
        {
            #region Define predicates to find ASTs.

            # Finds specific method, IsInRole.
            [ScriptBlock]$predicate1 = {
                param ([System.Management.Automation.Language.Ast]$Ast)

                [bool]$returnValue = $false

                if ($Ast -is [System.Management.Automation.Language.MemberExpressionAst])
                {
                    [System.Management.Automation.Language.MemberExpressionAst]$meAst = $ast;
                    if ($meAst.Member -is [System.Management.Automation.Language.StringConstantExpressionAst])
                    {
                        [System.Management.Automation.Language.StringConstantExpressionAst]$sceAst = $meAst.Member;
                        if ($sceAst.Value -eq "isinrole")
                        {
                            $returnValue = $true;
                        }
                    }
                }

                return $returnValue
            }

            # Finds specific value, [system.security.principal.windowsbuiltinrole]::administrator.
            [ScriptBlock]$predicate2 = {
                param ([System.Management.Automation.Language.Ast]$Ast)

                [bool]$returnValue = $false

                if ($ast -is [System.Management.Automation.Language.AssignmentStatementAst])
                {
                    [System.Management.Automation.Language.AssignmentStatementAst]$asAst = $Ast;
                    if ($asAst.Right.ToString() -eq "[system.security.principal.windowsbuiltinrole]::administrator")
                    {
                        $returnValue = $true
                    }
                }

                return $returnValue
            }

            #endregion

            #region Finds ASTs that match the predicates.

            [System.Management.Automation.Language.Ast[]]$methodAst     = $ScriptBlockAst.FindAll($predicate1, $true)
            [System.Management.Automation.Language.Ast[]]$assignmentAst = $ScriptBlockAst.FindAll($predicate2, $true)

            if ($null -ne $ScriptBlockAst.ScriptRequirements)
            {
                if ((!$ScriptBlockAst.ScriptRequirements.IsElevationRequired) -and
                    ($methodAst.Count -ne 0) -and ($assignmentAst.Count -ne 0))
                {
                    $result = New-Object `
                                -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                                -ArgumentList $Messages.MeasureRequiresRunAsAdministrator,$assignmentAst.Extent,$PSCmdlet.MyInvocation.InvocationName,Information,$null
                    $results += $result
                }
            }
            else
            {
                if (($methodAst.Count -ne 0) -and ($assignmentAst.Count -ne 0))
                {
                    $result = New-Object `
                                -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                                -ArgumentList $Messages.MeasureRequiresRunAsAdministrator,$assignmentAst.Extent,$PSCmdlet.MyInvocation.InvocationName,Information,$null
                    $results += $result
                }
            }

            return $results

            #endregion
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}

<#
.SYNOPSIS
    Uses #Requires -Modules instead of Import-Module.
.DESCRIPTION
    The #Requires statement prevents a script from running unless the Windows PowerShell version, modules, snap-ins, and module and snap-in version prerequisites are met.
    From Windows PowerShell 3.0, the #Requires statement let script developers specify Windows PowerShell modules that the script requires.
    To fix a violation of this rule, please consider to use #Requires -Modules { <Module-Name> | <Hashtable> } instead of using Import-Module.
.EXAMPLE
    Measure-RequiresModules -ScriptBlockAst $ScriptBlockAst
.INPUTS
    [System.Management.Automation.Language.ScriptBlockAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    None
#>
function Measure-RequiresModules
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    Process
    {
        $results = @()

        try
        {
            #region Define predicates to find ASTs.

            # Finds specific command name, import-module.
            [ScriptBlock]$predicate = {
                param ([System.Management.Automation.Language.Ast]$Ast)

                [bool]$returnValue = $false

                if ($Ast -is [System.Management.Automation.Language.CommandAst])
                {
                    [System.Management.Automation.Language.CommandAst]$cmdAst = $Ast;
                    if ($null -ne $cmdAst.GetCommandName())
                    {
                        if ($cmdAst.GetCommandName() -eq "import-module")
                        {
                            $returnValue = $true
                        }
                    }
                }

                return $returnValue
            }

            #endregion

            #region Finds ASTs that match the predicates.

            [System.Management.Automation.Language.Ast[]]$asts = $ScriptBlockAst.FindAll($predicate, $true)

            if ($null -ne $ScriptBlockAst.ScriptRequirements)
            {
                if (($ScriptBlockAst.ScriptRequirements.RequiredModules.Count -eq 0) -and
                    ($null -ne $asts))
                {
                    foreach ($ast in $asts)
                    {
                        $result = New-Object `
                                -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                                -ArgumentList $Messages.MeasureRequiresModules,$ast.Extent,$PSCmdlet.MyInvocation.InvocationName,Information,$null

                        $results += $result
                    }
                }
            }
            else
            {
                if ($null -ne $asts)
                {
                    foreach ($ast in $asts)
                    {
                        $result = New-Object `
                                -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                                -ArgumentList $Messages.MeasureRequiresModules,$ast.Extent,$PSCmdlet.MyInvocation.InvocationName,Information,$null

                        $results += $result
                    }
                }
            }

            return $results

            #endregion
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}


# The two rules in the following if block use StaticParameterBinder class.
# StaticParameterBinder class was introduced in PSv4.
if ($PSVersionTable.PSVersion -ge [Version]'4.0.0')
{
	<#
	.SYNOPSIS
		You can store the type name in a variable or using -f operator to reduce the amount of redundant information in your script.
	.DESCRIPTION
		When interacting with classes that have long type names, you want to reduce the amount of redundant information in your script.
		To fix a violation of this rule, please store the type name in a variable or using -f operator. For example:
		$namespace = "System.Collections.{0}"; $arrayList = New-Object ($namespace -f "ArrayList"); $queue = New-Object ($namespace -f "Queue")
	.EXAMPLE
		Measure-LongClassName -CommandAst $CommandAst
	.INPUTS
		[System.Management.Automation.Language.CommandAst]
	.OUTPUTS
		[Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
	.NOTES
		Reference: 3.11. Reduce Typying for Long Class Names, Windows PowerShell Cookbook, Third Edition
	#>
	function Measure-LongClassName
	{
		[CmdletBinding()]
		[OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
		Param
		(
			[Parameter(Mandatory = $true)]
			[ValidateNotNullOrEmpty()]
			[System.Management.Automation.Language.CommandAst]
			$CommandAst
		)

		Process
		{
			$results = @()

			# The StaticParameterBinder help us to find the argument of TypeName.
			$spBinder = [System.Management.Automation.Language.StaticParameterBinder]

			# Checks New-Object without ComObject parameter command only.
			if ($null -ne $CommandAst.GetCommandName())
			{
				if ($CommandAst.GetCommandName() -ne "new-object")
				{
					return $results
				}
			}
			else
			{
				return $results
			}

			try
			{
				[System.Management.Automation.Language.StaticBindingResult]$sbResults = $spBinder::BindCommand($CommandAst, $true)
				foreach ($sbResult in $sbResults)
				{
					# TypeName cannot be found if user run command like, New-Object -ComObject Scripting.FileSystemObject.
					if ($null -eq $sbResult.BoundParameters["TypeName"].ConstantValue) { continue }

					if ($sbResult.BoundParameters["TypeName"].ConstantValue.ToString().Split('.').Length -ge 3)
					{
						# $sbResult.BoundParameters["TypeName"].Value is a CommandElementAst, so we can return an extent.
						$result = New-Object `
									-Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
									-ArgumentList $Messages.MeasureLongClassName,$sbResult.BoundParameters["TypeName"].Value.Extent,$PSCmdlet.MyInvocation.InvocationName,Information,$null

						$results += $result
					}
				}

				return $results
			}
			catch
			{
				$PSCmdlet.ThrowTerminatingError($PSItem)
			}


		}
	}

		<#
	.SYNOPSIS
		Please do not use COM objects when calling New-Object.
	.DESCRIPTION
		If you can't use just PowerShell, use .NET, external commands or COM objects, in that order of preference. COM objects are rarely well-documented, making them harder for someone else to research and understand.
		They do not always work flawlessly in PowerShell, as they must be used through .NET's Interop layer, which isn't 100% perfect.
		To fix a violation of this rule, please do not use COM objects when calling New-Object.
	.EXAMPLE
		Measure-ComObject -CommandAst $CommandAst
	.INPUTS
		[System.Management.Automation.Language.CommandAst]
	.OUTPUTS
		[Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
	.NOTES
		Reference: The Purity Laws, The Community Book of PowerShell Practices.
	#>
	function Measure-ComObject
	{
		[CmdletBinding()]
		[OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
		Param
		(
			[Parameter(Mandatory = $true)]
			[ValidateNotNullOrEmpty()]
			[System.Management.Automation.Language.CommandAst]
			$CommandAst
		)

		Process
		{
			$results = @()

			# The StaticParameterBinder help us to find the argument of TypeName.
			$spBinder = [System.Management.Automation.Language.StaticParameterBinder]

			# Checks New-Object without ComObject parameter command only.
			if ($null -ne $CommandAst.GetCommandName())
			{
				if ($CommandAst.GetCommandName() -ne "new-object")
				{
					return $results
				}
			}
			else
			{
				return $results
			}

			try
			{
				[System.Management.Automation.Language.StaticBindingResult]$sbResults = $spBinder::BindCommand($CommandAst, $true)
				foreach ($sbResult in $sbResults)
				{
					if ($sbResults.BoundParameters.ContainsKey("ComObject"))
					{
						# $sbResult.BoundParameters["TypeName"].Value is a CommandElementAst, so we can return an extent.
						$result = New-Object `
									-Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
									-ArgumentList $Messages.MeasureComObject,$sbResult.BoundParameters["ComObject"].Value.Extent,$PSCmdlet.MyInvocation.InvocationName,Warning,$null

						$results += $result
					}
				}

				return $results
			}
			catch
			{
				$PSCmdlet.ThrowTerminatingError($PSItem)
			}


		}
	}

} # end if ($PSVersionTable.PSVersion -ge [Version]'4.0')


<#
.SYNOPSIS
    Do not use deprecated WMI class in your script.
.DESCRIPTION
    With the release of new Microsoft Windows, some WMI classes are marked as deprecated. When writing Windows PowerShell scripts, you should not use these WMI classes.
    You can run this command to get the deprecated WMI classes list, "Get-CimClass * -QualifierName deprecated"
    To fix a violation of this rule, please do not use the deprecated WMI classes in your script.
.EXAMPLE
    Measure-DeprecatedWMIClass -StringConstantExpressionAst $StringConstantExpressionAst
.INPUTS
    [System.Management.Automation.Language.StringConstantExpressionAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: Filtering classes by qualifier, Windows PowerShell Best Practics
#>
function Measure-DeprecatedWMIClass
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.StringConstantExpressionAst]
        $StringConstantExpressionAst
    )

    Process
    {
        $results = @()

        $deprecatedWMIClasses = @("Win32_PageFile", "Win32_DisplayConfiguration", "Win32_DisplayControllerConfiguration",
                                  "Win32_VideoConfiguration", "Win32_AllocatedResource")

        try
        {
            if ($StringConstantExpressionAst.Value -in $deprecatedWMIClasses)
            {
                $result = New-Object `
                            -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                            -ArgumentList $Messages.MeasureDeprecatedWMIClass,$StringConstantExpressionAst.Extent,$PSCmdlet.MyInvocation.InvocationName,Information,$null

                $results += $result
            }

            return $results
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}

<#
.SYNOPSIS
    Adds end-of-line comment after closing curly bracket for deeply nested structures.
.DESCRIPTION
    In general, you should avoid creating deeply nested structures, but sometimes they cannot be avoided.
    The use of end-of-line comments with closing curly brackets can greatly improve the readability and maintainability of your script.
    To fix a violation of this rule, please add comment after closing curly bracket for deeply nested structures.
.EXAMPLE
    Measure-CurlyBracket -ScriptBlockAst $ScriptBlockAst
.INPUTS
    [System.Management.Automation.Language.ScriptBlockAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: Document nested structures, Windows PowerShell Best Practices.
#>
function Measure-CurlyBracket
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    Process
    {
        $results = @()

        try
        {
            #region Define predicates to find ASTs.

            # Finds specific command name, import-module.
            [ScriptBlock]$predicate1 = {
                param ([System.Management.Automation.Language.Ast]$Ast)

                [bool]$returnValue = $false

                if (($Ast -is [System.Management.Automation.Language.DoWhileStatementAst]) -or
                    ($Ast -is [System.Management.Automation.Language.DoUntilStatementAst]) -or
                    ($Ast -is [System.Management.Automation.Language.ForStatementAst])     -or
                    ($Ast -is [System.Management.Automation.Language.ForEachStatementAst]) -or
                    ($Ast -is [System.Management.Automation.Language.IfStatementAst])      -or
                    ($Ast -is [System.Management.Automation.Language.SwitchStatementAst])  -or
                    ($Ast -is [System.Management.Automation.Language.TryStatementAst])     -or
                    ($Ast -is [System.Management.Automation.Language.WhileStatementAst]))
                {
                    $returnValue = $true
                }

                return $returnValue
            }

            #endregion

            #region Finds ASTs that match the predicates.

            [System.Management.Automation.Language.Ast[]]$asts = $ScriptBlockAst.FindAll($predicate1, $true)

            foreach ($ast in $asts)
            {
                # Checks nesting structures
                $nestingASTs = $asts | Where-Object {($PSItem.Extent.StartLineNumber -gt $ast.Extent.StartLineNumber) -and
                                              ($PSItem.Extent.EndLineNumber   -lt $ast.Extent.EndLineNumber)}

                # If one AST have end-of-line comments, we should skip it.
                [bool]$needComment = $ast.Extent.EndScriptPosition.Line.Trim().EndsWith("}")

                if ($needComment -and $nestingASTs)
                {
                    $result = New-Object `
                                -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                                -ArgumentList $Messages.MeasureCurlyBracket,$ast.Extent,$PSCmdlet.MyInvocation.InvocationName,Information,$null

                    $results += $result
                }
            }

            return $results

            #endregion
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }


    }
}

<#
.SYNOPSIS
    Removes these unnecessary comments.
.DESCRIPTION
    Don't precede each line of code with a comment. Doing so breaks up the code and makes it harder to follow. A well-written PowerShell command, with full command and parameter names, can be pretty self-explanatory.
    Don't comment-explain it unless it isn't self-explanatory.To fix a violation of this rule, please remove these unnecessary comments.
.EXAMPLE
    Measure-OverComment -Token $Token
.INPUTS
    [System.Management.Automation.Language.Token[]]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: DOC-07 Don't over-comment, The Community Book of PowerShell Practices.
#>
function Measure-OverComment
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.Token[]]
        $Token
    )

    Process
    {
        $results = @()

        try
        {
            # Calculates comment tokens length
            foreach ($subToken in $Token)
            {
                $allTokensLength += $subToken.Text.Length
                if ($subToken.Kind -eq [System.Management.Automation.Language.TokenKind]::Comment)
                {
                    $commentTokensLength += $subToken.Text.Length
                }
                else
                {
                    $otherTokensLength   += $subToken.Text.Length
                }
            }

            $actualPercentage = [int]($commentTokensLength / $allTokensLength * 100)

            if ($actualPercentage -ge 80)
            {
                $result = New-Object `
                            -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                            -ArgumentList $Messages.MeasureOverComment,$Token[0].Extent,$PSCmdlet.MyInvocation.InvocationName,Warning,$null

                $results += $result
            }

            return $results
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}

<#
.SYNOPSIS
    Removes backticks from your script and use "splatting" instead.
.DESCRIPTION
    In general, the community feels you should avoid using those backticks as “line continuation characters” when possible.
    They’re hard to read, easy to miss, and easy to mis-type. Also, if you add an extra whitespace after the backtick in the above example, then the command won’t work.
    To fix a violation of this rule, please remove backticks from your script and use "splatting" instead. You can run "Get-Help about_splatting" to get more details.
.EXAMPLE
    Measure-Backtick -Token $Token
.INPUTS
    [System.Management.Automation.Language.Token[]]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: Document nested structures, Windows PowerShell Best Practices.
#>
function Measure-Backtick
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.Token[]]
        $Token
    )

    Process
    {
        $results = @()

        try
        {
            # Finds LineContinuation tokens
            $lcTokens = $Token | Where-Object {$PSItem.Kind -eq [System.Management.Automation.Language.TokenKind]::LineContinuation}

            foreach ($lcToken in $lcTokens)
            {
                $result = New-Object `
                            -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                            -ArgumentList $Messages.MeasureBacktick,$lcToken.Extent,$PSCmdlet.MyInvocation.InvocationName,Warning,$null

                $results += $result
            }

            return $results
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}

<#
.SYNOPSIS
    You should never use Write-Host to create any script output whatsoever.
.DESCRIPTION
    It is generally accepted that you should never use Write-Host to create any script output whatsoever, unless your script (or function, or whatever) uses the Show verb (as in, Show-Performance).
    That verb explicitly means “show on the screen, with no other possibilities.” Like Show-Command.
    To fix a violation of this rule, please replace Write-Host with Write-Output in most scenarios.
.EXAMPLE
    Measure-WriteHost -CommandAst $CommandAst
.INPUTS
    [System.Management.Automation.Language.CommandAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: Output, The Community Book of PowerShell Practices.
#>
function Measure-WriteHost
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.CommandAst]
        $CommandAst
    )

    Process
    {
        $results = @()

        try
        {
            # Checks command name, if the command name matches Write-Host or
            # user-defined aliases, this rule is triggered.

            if ($null -ne $CommandAst.GetCommandName())
            {
                $alias = (Get-Alias -Definition "Write-Host" -ErrorAction SilentlyContinue).Name

                if (($CommandAst.GetCommandName() -eq "Write-Host") -or
                    ($CommandAst.GetCommandName() -eq $alias))
                {
                    $result = New-Object `
                                -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                                -ArgumentList $Messages.MeasureWriteHost,$CommandAst.Extent,$PSCmdlet.MyInvocation.InvocationName,Warning,$null

                    $results += $result
                }
            }

            return $results
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}

<#
.SYNOPSIS
    Resets $ErrorActionPreference to continue.
.DESCRIPTION
    When executing something other than a cmdlet, set $ErrorActionPreference='Stop' before executing, and reset to Continue afterwards.
    Please don’t change $ErrorActionPreference persistently. To fix a violation of this rule, please reset $ErrorActionPreference to continue.
.EXAMPLE
    Measure-ErrorActionPreference -ScriptBlockAst $ScriptBlockAst
.INPUTS
    [System.Management.Automation.Language.ScriptBlockAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: Trapping and Capturing Errors, Windows PowerShell Best Practices.
#>
function Measure-ErrorActionPreference
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    Process
    {
        $results = @()

        try
        {
            #region Define predicates to find ASTs.

            # Finds specific AssignmentStatementAst.
            [ScriptBlock]$predicate1 = {
                param ([System.Management.Automation.Language.Ast]$Ast)

                [bool]$returnValue = $false

                if ($Ast -is [System.Management.Automation.Language.AssignmentStatementAst])
                {
                    if ($Ast.Left.VariablePath.ToString() -eq "ErrorActionPreference")
                    {
                        return $true
                    }
                }

                return $returnValue
            }

            #endregion

            #region Finds ASTs that match the predicates.

            [System.Management.Automation.Language.Ast[]]$asts = $ScriptBlockAst.FindAll($predicate1, $true)

            if ($asts.Count % 2 -ne 0)
            {
                $result = New-Object `
                            -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                            -ArgumentList $Messages.MeasureErrorActionPreference,$asts[0].Extent,"Measure-ErrorActionPreference",Warning,$null

                $results += $result
            }

            return $results

            #endregion
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }


    }
}

<#
.SYNOPSIS
    Considers to use try-catch-finally statements instead of using $?.
.DESCRIPTION
    When you need to examine the error that occurred, try to avoid using $?. It actually doesn’t mean an error did or did not occur; it’s reporting whether or not the last-run command considered itself to have completed successfully.
    You get no details on what happened. To fix a violation of this rule, please consider to use try-catch-finally statements.
.EXAMPLE
    Measure-QuestionVariable -ScriptBlockAst $ScriptBlockAst
.INPUTS
    [System.Management.Automation.Language.ScriptBlockAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: Trapping and Capturing Errors, Windows PowerShell Best Practices.
#>
function Measure-QuestionVariable
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    Process
    {
        $results = @()

        try
        {
            $tokens = $null
            $errors = $null

            # Parses the input to get tokens.
            $sbAst = [System.Management.Automation.Language.Parser]::ParseInput($ScriptBlockAst, [ref]$tokens, [ref]$errors)

            # Gets question variables
            $questionVariables = $tokens | Where-Object {$PSItem.Name -eq '?'}

            foreach ($questionVariable in $questionVariables)
            {
                $result = New-Object `
                            -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                            -ArgumentList $Messages.MeasureQuestionVariable,$questionVariable.Extent,$PSCmdlet.MyInvocation.InvocationName,Warning,$null

                $results += $result
            }

            return $results
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}

<#
.SYNOPSIS
    Adds a .NOTE keyword in your comment-based help.
.DESCRIPTION
    Comment-based help is written as a series of comments. You can write comment-based help topics for end users to better understand your functions. Additionally, it’s better to explain the detail about how the function works.
    To fix a violation of this rule, add a .NOTE keyword in your comment-based help. You can get more details by running “Get-Help about_Comment_Based_Help” command in Windows PowerShell.
.EXAMPLE
    Measure-HelpNote -FunctionDefinitionAst $FunctionDefinitionAst
.INPUTS
    [System.Management.Automation.Language.FunctionDefinitionAst]
.OUTPUTS
    [Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
.NOTES
    Reference: Writing Help and Comments, Windows PowerShell Best Practices.
#>
function Measure-HelpNote
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.FunctionDefinitionAst]
        $FunctionDefinitionAst
    )

    Process
    {
        $results = @()

        try
        {
            #region Define predicates to find ASTs.

            # Finds CmdletBinding attribute.
            [ScriptBlock]$predicate = {
                param ([System.Management.Automation.Language.Ast]$Ast)

                [bool]$returnValue = $false

                if ($Ast -is [System.Management.Automation.Language.AttributeAst])
                {
                    [System.Management.Automation.Language.AttributeAst]$attrAst = $ast;
                    if ($attrAst.TypeName.Name -eq 'CmdletBinding')
                    {
                        $returnValue = $true
                    }
                }

                return $returnValue
            }

            #endregion

            # Return directly if function is not an advanced function.
            [System.Management.Automation.Language.AttributeAst[]]$attrAsts = $FunctionDefinitionAst.Find($predicate, $true)
            if ($FunctionDefinitionAst.IsWorkflow -or !$attrAsts)
            {
                return $results
            }

            if (!$FunctionDefinitionAst.GetHelpContent().Notes)
            {
                $result = New-Object `
                            -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
                            -ArgumentList $Messages.MeasureHelpNote,$FunctionDefinitionAst.Extent,$PSCmdlet.MyInvocation.InvocationName,Warning,$null

                $results += $result
            }

            return $results
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}

Export-ModuleMember -Function Measure*
