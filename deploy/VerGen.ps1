param (
  [string]$version = '',
  [string]$additionalLabel = '',
  [string]$projectDir = '',
  [string[]]$files = @()
)

function showUsage()
{
	echo 'Usage: VerGen -version PackageVersion [-additionalLabel PreRelease] [-projectDir ProjectDir] [-files File1, [File2]...]'
	echo ''
	echo 'PackageVersion: MAJOR.MINOR.PATCH'
	echo '  MAJOR version when you make incompatible API changes;'
	echo '  MINOR version when you add functionality in a backwards-compatible manner;'
	echo '  PATCH version when you make backwards-compatible bug fixes.'
	echo ''
	echo 'PreRelease:'
	echo '  Additional labels for pre-release.'
	echo '  If ends with *, a date in yyyyMMdd will be appended.'
	echo ''
	echo 'ProjectDir'
	echo '  The project directory to build, relative to current directory.'
	echo '  If empty, "..\Sync.%CurrentDirectory%" will be used.'
	echo ''
	echo 'File1, [File2]...'
	echo '  List of files to process, relative to ProjectDir and seperated by comma.'
	echo '  If empty, default file "AssemblyVersion.cs" will be used:'.
	exit
}

function getAssemblyVersion([string]$major, [string]$minor)
{
	return $major + '.0.0.0'
}

function getAssemblyFileVersion([string]$major, [string]$minor)
{
	$span = New-TimeSpan -Start ([datetime]'2000/01/01') -End (Get-Date)
	return $major + '.' + $minor + '.' + $span.Days + '.0'
}

function getPackageVersion([string]$version, [string]$additionalLabel)
{
	if ($additionalLabel -eq '')
	{
		return $version
	}

	if ($additionalLabel.EndsWith('*'))
	{
		$additionalLabel = $additionalLabel.Substring(0, $additionalLabel.Length - 1)
		$additionalLabel = $additionalLabel + (Get-Date -format 'yyyyMMdd')
	}

	return $version + '-' + $additionalLabel;
}

function getTemplateFile([string]$file)
{
    $currentDir = (Get-Item -Path ".\").FullName
    $verGenDir = Join-Path $currentDir -ChildPath 'VerGen'
    $file = (Split-Path $file -Leaf)
	return Join-Path $verGenDir -ChildPath $file
}

function getDefaultProjectDir()
{
    return "..\Sync." + (Get-Item -Path ".\").Name
}

function generateFiles([string[]]$files, [string]$assemblyVersion, [string]$assemblyFileVersion, [string]$packageVersion)
{
	for ($i=0; $i -lt $files.Count; $i++)
	{
		$file = $files[$i]
		$srcFile = Join-Path $projectDir -ChildPath $file
		$templateFile = getTemplateFile -file $file
		$content = [System.IO.File]::ReadAllText($templateFile).Replace('$ASSEMBLY_VERSION$', $assemblyVersion).Replace('$ASSEMBLY_FILE_VERSION$', $assemblyFileVersion).Replace('$PACKAGE_VERSION$', $packageVersion)
		[System.IO.File]::WriteAllText($srcFile, $content)
		echo "File $srcFile generated."
	}
}


########################################################################
# Main Start
########################################################################

$versions = $version.Split('.');
if ($versions.Count -ne 3)
{
	showUsage
}

if ($projectDir -eq '')
{
	$projectDir = getDefaultProjectDir
}

$projectDir = Join-Path (Get-Item -Path ".\").FullName -ChildPath $projectDir

if ($files.Count -eq 0)
{
	$files = @('AssemblyVersion.cs')
}

for ($i=0; $i -lt $files.Count; $i++)
{
	$file = getTemplateFile -file $files[$i]
	if (!(Test-Path $file))
	{
		echo "File $file not found!"
		exit
	}
}

$major = $versions[0]
$minor = $versions[1]
$patch = $versions[2]
$assemblyVersion = getAssemblyVersion -major $major -minor $minor
$assemblyFileVersion = getAssemblyFileVersion -major $major -minor $minor
$packageVersion = getPackageVersion -version $version -additionalLabel $additionalLabel

echo "ASSEMBLY_VERSION=$assemblyVersion"
echo "ASSEMBLY_FILE_VERSION=$assemblyFileVersion"
echo "PACKAGE_VERSION=$packageVersion"
echo ""

generateFiles -files $files -assemblyVersion $assemblyVersion -assemblyFileVersion $assemblyFileVersion -packageVersion $packageVersion
