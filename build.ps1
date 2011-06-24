param($buildConfiguration = 'Release')

#Copied from psake https://github.com/JamesKovacs/psake/blob/master/psake.psm1
function Assert
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)]$conditionToCheck,
        [Parameter(Position=1,Mandatory=1)]$failureMessage
    )
    if (!$conditionToCheck) { 
        throw ("Assert: " + $failureMessage) 
    }
}

#Copied from psake https://github.com/JamesKovacs/psake/blob/master/psake.psm1
function Configure-BuildEnvironment {
    if ($framework.Length -ne 3 -and $framework.Length -ne 6) {
        $framework = '4.0'
    }
    $versionPart = $framework.Substring(0, 3)
    $bitnessPart = $framework.Substring(3)
    $versions = $null
    switch ($versionPart) {
        '1.0' {
            $versions = @('v1.0.3705')
        }
        '1.1' {
            $versions = @('v1.1.4322')
        }
        '2.0' {
            $versions = @('v2.0.50727')
        }
        '3.0' {
            $versions = @('v2.0.50727')
        }
        '3.5' {
            $versions = @('v3.5', 'v2.0.50727')
        }
        '4.0' {
            $versions = @('v4.0.30319')
        }
        default {
            throw ("Unknown framework versionPart[{0}] framework[{1}]" -f $versionPart, $framework)
        }
    }

    $bitness = 'Framework'
    if ($versionPart -ne '1.0' -and $versionPart -ne '1.1') {
        switch ($bitnessPart) {
            'x86' {
                $bitness = 'Framework'
            }
            'x64' {
                $bitness = 'Framework64'
            }
            $null {
                $ptrSize = [System.IntPtr]::Size
                switch ($ptrSize) {
                    4 {
                        $bitness = 'Framework'
                    }
                    8 {
                        $bitness = 'Framework64'
                    }
                    default {
                        throw ("Unknown pointer size [{0}]" -f $ptrSize)
                    }
                }
            }
            default {
				throw ("Unknown bitness bitnessPart[{0}] framework[{1}]" -f $bitnessPart, $framework)
            }
        }
    }
    $frameworkDirs = $versions | foreach { "$env:windir\Microsoft.NET\$bitness\$_\" }

    $frameworkDirs | foreach { Assert (test-path $_) ($msgs.error_no_framework_install_dir_found -f $_)}

    $env:path = ($frameworkDirs -join ";") + ";$env:path"
    #if any error occurs in a PS function then "stop" processing immediately
    #this does not effect any external programs that return a non-zero exit code
    $global:ErrorActionPreference = "Stop"
}

function Get-Last-NuGet-Version($nuGetPackageId) {
    $feeedUrl = "http://packages.nuget.org/v1/FeedService.svc/Packages()?`$filter=Id%20eq%20'$nuGetPackageId'"
    $webClient = new-object System.Net.WebClient
    $queryResults = [xml]($webClient.DownloadString($feeedUrl))
    $queryResults.feed.entry | %{ $_.properties.version } | sort-object | select -last 1
}

function Increment-Version($version){
    $parts = $version.split('.')
    for($i = $parts.length-1; $i -ge 0; $i--){
        $x = ([int]$parts[$i]) + 1
        if($i -ne 0) {
            # Don't roll the previous minor or ref past 10
            if($x -eq 10) {
                $parts[$i] = "0"
                continue
            }
        }
        $parts[$i] = $x.ToString()
        break;
    }
    [System.String]::Join(".", $parts)
}


msbuild FileByter.sln /t:Rebuild /p:Configuration=$buildConfiguration /verbosity:normal /nologo
if($LASTEXITCODE -ne 0){
	throw "Build Failed"
}


Configure-BuildEnvironment

$version = Get-Last-NuGet-Version 'FileByter'

if(!$version){
    $version = "0.0"
}

$newVersion = Increment-Version $version

$nuget = ls .\packages\NuGet.CommandLine*\tools\NuGet.exe

$buildRoot = ".\NuGetBuild"
$buildDestination = "$buildRoot\lib"
rm $buildRoot -force -recurse -ErrorAction SilentlyContinue
mkdir $buildDestination | out-null
cp .\FileByter\bin\$buildConfiguration\FileByter.dll $buildDestination
$nuspecFile = "FileByter.$newVersion.nuspec"
cp .\FileByter.nuspec "$buildRoot\$nuspecFile"
pushd $buildRoot

    $nuspec = [xml](cat $nuspecFile)
    $nuspec.package.metadata.version = $newVersion
    $nuspec.Save((get-item $nuspecFile))
    
    & $nuget pack $nuspecFile

popd