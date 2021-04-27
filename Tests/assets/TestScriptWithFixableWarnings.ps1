# Produce PSAvoidUsingCmdletAliases and PSAvoidTrailingWhitespace warnings that should get fixed by replacing it with the actual command
gci . | % { } | ? { } 

# Produces PSAvoidUsingPlainTextForPassword warning that should get fixed by making it a [SecureString]
function Test-bar([string]$PasswordInPlainText){}
