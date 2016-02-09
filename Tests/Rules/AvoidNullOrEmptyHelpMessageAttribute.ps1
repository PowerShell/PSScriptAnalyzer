function BadFuncNullHelpMessage
{
    [CmdletBinding()]
    [OutputType([String])]
    param(
        # this one null value
        [Parameter(HelpMessage=$null)]          
        [string] $Param1="String",
        
        # this parameter has no help message
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}

function BadFuncEmptyHelpMessage
{
    [CmdletBinding()]
    [OutputType([String])]
    param(
        # this has an empty string
        [Parameter(HelpMessage="")]          
        [string] $Param1="String",
        
        # this parameter has no default value
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}

function GoodFunc1($Param1)
{
    $Param1
}

# same as BadFunc but this one has no cmdletbinding
function BadFuncNullHelpMessageNoCmdletBinding
{
    param(
        # this one null value
        [Parameter(HelpMessage=$null)]          
        [string] $Param1="String",
        
        # this parameter has no help message
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}

# same as BadFunc but this one has no cmdletbinding
function BadFuncEmptyHelpMessageNoCmdletBinding
{    
    param(
        # this has an empty string
        [Parameter(HelpMessage="")]          
        [string] $Param1="String",
        
        # this parameter has no default value
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}


# same as BadFunc but this one has no cmdletbinding
function BadFuncEmptyHelpMessageNoCmdletBindingSingleQoutes
{    
    param(
        # this has an empty string
        [Parameter(HelpMessage='')]          
        [string] $Param1="String",
        
        # this parameter has no default value
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}

# same as BadFunc but this one has no cmdletbinding
function BadFuncEmptyHelpMessageNoCmdletBindingNoAssignment
{    
    param(
        # this has an empty string
        [Parameter(HelpMessage)]          
        [string] $Param1="String",
        
        # this parameter has no default value
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}