New-Object -ComObject "WScript.Shell"

[Hashtable] $param = @{
    ComObject = "WScript.Shell"
}

New-Object @param

function Test
{
    New-Object -ComObject "WScript.Shell"

    [Hashtable] $param = @{
        ComObject = "WScript.Shell"
    }

    New-Object @param

    Invoke-Command -ComputerName "localhost" -ScriptBlock {
        New-Object -ComObject "WScript.Shell"

        [Hashtable] $param = @{
            ComObject = "WScript.Shell"
        }

        New-Object @param
    }

    function Test2
    {
        [Cmdletbinding()]
        Param ()

        New-Object -ComObject "WScript.Shell"

        [Hashtable] $param = @{
            ComObject = "WScript.Shell"
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

