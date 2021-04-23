$test = "Test"
[Test]$test2 = "test"

if ($a=3) {
} else {
    $a = 5
}

# this should not raise usetypeatvariableassignment error
$input = 5