$Global:DebugPreference


function Test {
    $initialized = "Initialized"
    $noglobal = "local"
    $env:ShouldNotRaiseError
}

$a = 3;

if ($true) {
    $a = 4;
    $c = 3;
} else {
    $b = 5;
    $c = 4;
}

$b = 6;
$a;
$b;

stop-process 12,23 -ErrorVariable ev -ErrorAction SilentlyContinue
if($null -ne $ev)
{
    Write-host $ev[0]
    # no error should be raised here
    Invoke-Command {$b}
}

get-process notepad | tee-object -variable proc 
$proc[0]

function Test-PreferenceVariable
{

   if (-not $PSBoundParameters.ContainsKey('Verbose')) {
       $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') -as 
        [System.Management.Automation.ActionPreference]
        }

        $VerbosePreference
    }