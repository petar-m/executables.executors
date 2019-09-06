$dotnet='"C:/Program Files/dotnet/dotnet.exe"'  
$userFolder=[System.Environment]::GetFolderPath('UserProfile')
$opencover= Join-Path -Path $userFolder -ChildPath  ".nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe"  
$reportgenerator= Join-Path -Path $userFolder -ChildPath ".nuget\packages\ReportGenerator\4.2.19\tools\netcoreapp2.0\ReportGenerator.dll"

$targetargs='"test M.Executables.Executors.NetCore.UnitTests.csproj"'
$filter='"+[M.Executables.Executors.NetCore*]* -[*.UnitTests]* -[xunit*]* -[FakeItEasy*]* -[M.Executables.Executors]* -[Microsoft*]*"'

$coveragefile="$PSScriptRoot\test_coverage\coverage.xml"
$coveragedir="$PSScriptRoot\test_coverage\coverage\"

Remove-Item "test_coverage" -Force -Recurse
New-Item -Path "test_coverage" -ItemType Directory

# Run code coverage analysis  
Start-Process $opencover "-oldStyle -register:user -target:$($dotnet) -output:$($coveragefile) -targetargs:$($targetargs) -filter:$($filter) -skipautoprops -hideskipped:All" -NoNewWindow -Wait

# Generate the report  
Start-Process dotnet "$reportgenerator -reports:$($coveragefile) -targetdir:$($coveragedir) -reporttypes:Html" -NoNewWindow -Wait

# Open the report  
Start-Process "$($coveragedir)\index.htm"  