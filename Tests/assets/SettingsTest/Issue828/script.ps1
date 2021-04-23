# This script has to be like that in order to reproduce the issue
$MyObj | % { @{$_.Name = $_.Value} }