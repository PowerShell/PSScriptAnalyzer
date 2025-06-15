New-Object -ComObject "WScript.Shell"

[Hashtable] $param = @{
    ComObject = "WScript.Shell"
}

New-Object @param

function Test
{
    $partialParameter = "CO"
    New-Object -"$partialParameter" "WScript.Shell"

    [Hashtable] $param = @{
        ComO = "WScript.Shell"
    }

    New-Object @param

    Invoke-Command -ComputerName "localhost" -ScriptBlock {
        New-Object -ComObject "WScript.Shell"

        [Hashtable] $param = @{
            CO = "WScript.Shell"
        }

        New-Object @param
    }

    function Test2
    {
        [Cmdletbinding()]
        Param ()

        New-Object -ComObject "WScript.Shell"

        [Hashtable] $param = @{
            C = "WScript.Shell"
        }

        New-Object @param

        Invoke-Command -ComputerName "localhost" -ScriptBlock {
            New-Object -ComObject "WScript.Shell"
        }
    }
}

class TestClass
{
    TestClass ()
    {
        New-Object -ComObject "WScript.Shell"

        Invoke-Command -ComputerName "localhost" -ScriptBlock {
            New-Object -ComObject "WScript.Shell"

        }

        [Hashtable] $param = @{
            ComObject = "WScript.Shell"
        }

        New-Object @param
    }
}

[Hashtable] $script:params = @{
    ComObject = "WScript.Shell"
}

function Test-Scope
{
    New-Object @script:params

    $params2 = $script:params
    New-Object @params2

    [Hashtable] $params3 = @{
        ComObject = "WScript.Shell"
    }

    Invoke-Command -ComputerName "localhost" -ScriptBlock {
        New-Object @using:params3
    }
}

$scripbBlockTest = {
    New-Object -ComObject "WScript.Shell"

    [Hashtable] $params4 = @{
        ComObject = 'WScript.Shell'
    }

    New-Object @params4
}

$partialKey = "COMO"
$value = "WScript.Shell"
[hashtable] $partialKeyParams = @{
    "$partialKey" = $value
}

New-Object @partialKeyParams