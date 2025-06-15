$hashset1 = New-Object System.Collections.Generic.HashSet[String] # Issue #1

[Hashtable] $param = @{
    TypeName = 'System.Collections.Generic.HashSet[String]'
}

New-Object @param # Issue #2

function Test
{
    $hashset2 = New-Object -TypeName System.Collections.Generic.HashSet[String] # Issue #3
    New-Object -TypeName System.Collections.Generic.HashSet[String] # Issue #4

    [Hashtable] $param = @{
        TypeName = 'System.Collections.Generic.HashSet[String]'
    }

    New-Object @param # Issue #5

    Invoke-Command -ComputerName "localhost" -ScriptBlock {
        New-Object -TypeName System.Collections.Generic.HashSet[String] # Issue #6

        [Hashtable] $param = @{
            TypeName = 'System.Collections.Generic.HashSet[String]'
        }

        New-Object @param # Issue #7
    }

    function Test2
    {
        [Cmdletbinding()]
        Param ()

        New-Object -TypeName System.Collections.Generic.HashSet[String] # Issue #8

        [Hashtable] $param = @{
            TypeName = 'System.Collections.Generic.HashSet[String]'
        }

        New-Object @param # Issue #9

        Invoke-Command -ComputerName "localhost" -ScriptBlock {
            New-Object -TypeName System.Collections.Generic.HashSet[String] # Issue #10
        }
    }
}

class TestClass
{
    [System.Collections.Generic.HashSet[String]] $Set
    [System.Collections.Generic.HashSet[String]] $Set2 = (New-Object -TypeName System.Collections.Generic.HashSet[String]) # Issue #11

    TestClass ()
    {
        $this.Set = (New-Object -TypeName System.Collections.Generic.HashSet[String]) # Issue #12

        Invoke-Command -ComputerName "localhost" -ScriptBlock {
            New-Object -TypeName System.Collections.Generic.HashSet[String] # Issue #13

        }

        [Hashtable] $param = @{
            TypeName = 'System.Collections.Generic.HashSet[String]'
        }

        New-Object @param # Issue #14
    }
}

[Hashtable] $script:params = @{
    TypeName = 'System.Collections.Generic.HashSet[String]'
}

function Test-Scope
{
    New-Object @script:params # Issue #15

    $params2 = $script:params
    New-Object @params2 # Issue #16

    [Hashtable] $params3 = @{
        TypeName = 'System.Collections.Generic.HashSet[String]'
    }

    Invoke-Command -ComputerName "localhost" -ScriptBlock {
        New-Object @using:params3 # Issue #17
    }
}

$scripbBlockTest = {
    New-Object -TypeName System.Collections.Generic.HashSet[String] # Issue #18

    [Hashtable] $params4 = @{
        TypeName = 'System.Collections.Generic.HashSet[String]'
    }

    New-Object @params4 # Issue #19
}

$test = "co"
$value = "WScript.Shell"
[hashtable] $test1 = @{
    "$test" = $value
}

New-Object @test1 # Issue #20