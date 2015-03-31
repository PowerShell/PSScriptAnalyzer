$Global:1 = "Globalization?"

function NotGlobal {
    $localVars = "Localization?"
    $unitialized
    Write-Output $globalVars
}

if ($true) {
    $a = 5;
} else {
}

while ($false) {
    $d = 5;
}

# $a is not initialized here
$a;

# $d may not be initialized too
$d;