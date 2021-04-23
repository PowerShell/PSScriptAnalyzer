Write-Warning
Wrong-Cmd
Write-Verbose -Message "Write Verbose"
Write-Verbose "Warning" -OutVariable $test
Write-Verbose "Warning" | PipeLineCmdlet