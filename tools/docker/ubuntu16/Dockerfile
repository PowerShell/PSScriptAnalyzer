FROM mcr.microsoft.com/powershell:ubuntu-16.04

ENV __InContainer 1

RUN apt update -qq && apt install -q -y wget git apt-transport-https
RUN wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb && dpkg -i packages-microsoft-prod.deb

RUN apt update -qq && \
    cd / && \
    git clone https://github.com/PowerShell/PSScriptAnalyzer

RUN pwsh -c 'save-module -name platyps,pester -path $PSHOME/Modules'
