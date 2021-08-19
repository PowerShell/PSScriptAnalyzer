# Double typing is not allowed
function F10 ([int] [switch] $s1, [int] $p1){}

# Double typing is not allowed even for switch and boolean, because:
# switch maps to System.Management.Automation.SwitchParameter
# boolean maps to System.Boolean
function F11 ([switch][boolean] $s1, [int] $p1){}