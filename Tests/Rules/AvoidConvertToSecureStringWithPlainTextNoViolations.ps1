$secure = read-host -assecurestring
$encrypted = convertfrom-securestring -securestring $secure
convertto-securestring -string $encrypted