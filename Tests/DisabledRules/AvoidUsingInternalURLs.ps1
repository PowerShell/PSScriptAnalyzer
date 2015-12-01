$scratch = "\\scratch2\scratch"
$internalSite = "//msw"
$externalSite = "http:\\msw"
if (-not $scratch.EndsWith("/")) {
	$scratch += "/";
}
