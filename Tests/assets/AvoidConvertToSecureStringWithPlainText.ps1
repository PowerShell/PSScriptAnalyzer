#[SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Test/NotASecret.")]
$supersecure = convertto-securestring "sdfdsfd" -asplaintext -force

New-Object System.Management.Automation.PSCredential -ArgumentList "username", (ConvertTo-SecureString "really secure" -AsPlainText -Force)

$sneaky = ctss "sneaky convert" -asplainText -force
