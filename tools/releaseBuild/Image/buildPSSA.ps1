push-location C:/PSScriptAnalyzer
import-module C:/PSScriptAnalyzer/Utils/ReleaseMaker.psm1
New-ReleaseBuild
Copy-Item -Recurse C:/PSScriptAnalyzer/out C:/
