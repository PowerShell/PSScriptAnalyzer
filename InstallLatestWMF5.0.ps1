    # Use shortcode to find latest TechNet download site
    $confirmationPage = 'http://www.microsoft.com/en-us/download/' +  $((invoke-webrequest 'http://aka.ms/wmf5latest' -UseBasicParsing).links | ? Class -eq 'mscom-link download-button dl' | % href)    # Parse confirmation page and look for URL to file
    $directURL = (invoke-webrequest $confirmationPage -UseBasicParsing).Links | ? Class -eq 'mscom-link' | ? href -match "WindowsBlue-KB\d\d\d\d\d\d\d-x64.msu" | % href | select -first 1

    # Download file to local
    $download = invoke-webrequest $directURL -OutFile $env:Temp\wmf5latest.msu
    
    # Install quietly with no reboot
    if (test-path $env:Temp\wmf5latest.msu)
    {
        start -wait $env:Temp\wmf5latest.msu -argumentlist '/quiet /norestart'
    }
    
    # Clean up
    Remove-Item $env:Temp\wmf5latest.msu
