#########################################################################
# Ussage:
# VerGen -version Version -project Project
#
# For example:
# VerGen -version 1.0.* -project DevZest.Data.Tools
# If version ends with *, the * will be replaced with current date in
# yyyyMMdd format.
#########################################################################
param (
  [string]$version = '',
  [string]$project = ''
)

########################################################################
# Main Start
########################################################################

if ($version.EndsWith('*'))
{
	$version = $version.Substring(0, $version.Length - 1)
	$version = $version + (Get-Date -format 'yyyyMMdd')
}

$fileName = 'source.extension.vsixmanifest'

$currentDir = (Get-Item -Path ".\").FullName
$projectDir = Join-Path $currentDir -ChildPath $project
$sourceFile = Join-Path $currentDir -ChildPath $fileName
$targetFile = Join-Path $projectDir -ChildPath $fileName

if (!(Test-Path $sourceFile))
{
	Write-Error "File $sourceFile not found!"
	exit
}

$content = [System.IO.File]::ReadAllText($sourceFile).Replace('$VERSION$', $version)
[System.IO.File]::WriteAllText($targetFile, $content)
echo ''
echo "File $targetFile generated with version $version."