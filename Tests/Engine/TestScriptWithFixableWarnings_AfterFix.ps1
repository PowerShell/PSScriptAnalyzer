# Produce PSAvoidUsingCmdletAliases and PSAvoidTrailingWhitespace warnings that should get fixed by replacing it with the actual command
Get-ChildItem . | ForEach-Object { } | Where-Object { }

# Produces PSAvoidUsingPlainTextForPassword warning that should get fixed by making it a [SecureString]
function Test-bar([SecureString]$PasswordInPlainText){}
