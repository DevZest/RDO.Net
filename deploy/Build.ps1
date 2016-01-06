param (
  [string]$version = '',
  [string]$additionalLabel = '',
  [string]$projectDir = '',
  [string[]]$files = @()
)

function showUsage()
{
	echo 'Usage: Build -version Version [-additionalLabel AdditionalLabel] [-projectDir ProjectDir] [-files Files...]'
	echo ''
	echo 'Version: MAJOR.MINOR.PATCH'
	echo '  MAJOR version when you make incompatible API changes;'
	echo '  MINOR version when you add functionality in a backwards-compatible manner;'
	echo '  PATCH version when you make backwards-compatible bug fixes.'
	echo ''
	echo 'AdditionalLable:'
	echo '  Additional labels for pre-release and build metadata.'
	echo '  If end "*" exists, it will be replaced with current date "yyyyMMdd".'
	echo ''
	echo 'ProjectDir'
	echo '  The project directory to build.'
	echo '  If empty, current directory will be used.'
	echo ''
	echo 'Files...'
	echo '  List of files to process.'
	echo '  If empty, two files will be used: project.json and project.cs'.
	exit
}

function fileNotFound([string]$fileName)
{
	echo ""
	exit
}

function getAssemblyVersion([string]$major, [string]$minor)
{
	return $major + '.' + $minor + '.0.0'
}

function getAssemblyFileVersion([string]$version)
{
	$span = New-TimeSpan -Start ([datetime]'2000/01/01') -End (Get-Date)
	return $version + "." + $span.Days
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

function getTemplateFile([string]$dir, [string]$file)
{
	return (Join-Path $dir -ChildPath ('build.' + $file))
}

function generateFiles([string[]]$files, [string]$assemblyVersion, [string]$assemblyFileVersion, [string]$packageVersion)
{
	for ($i=0; $i -lt $files.Count; $i++)
	{
		$srcFile = Join-Path $projectDir -ChildPath $files[$i]
		$templateFile = getTemplateFile -dir $projectDir -file $files[$i]
		$content = [System.IO.File]::ReadAllText($templateFile).Replace('$ASSEMBLY_VERSION$', $assemblyVersion).Replace('$ASSEMBLY_FILE_VERSION$', $assemblyFileVersion).Replace('$PACKAGE_VERSION$', $packageVersion)
		[System.IO.File]::WriteAllText($srcFile, $content)
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
	$projectDir = (Get-Item -Path ".\" -Verbose).FullName
}

if ($files.Count -eq 0)
{
	$files = @('project.json', 'project.cs')
}

for ($i=0; $i -lt $files.Count; $i++)
{
	$file = getTemplateFile -dir $projectDir -file $files[$i]
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
$assemblyFileVersion = getAssemblyFileVersion -version $version
$packageVersion = getPackageVersion -version $version -additionalLabel $additionalLabel

echo "Building..."
echo "ASSEMBLY_VERSION=$assemblyVersion"
echo "ASSEMBLY_FILE_VERSION=$assemblyFileVersion"
echo "PACKAGE_VERSION=$packageVersion"

generateFiles -files $files -assemblyVersion $assemblyVersion -assemblyFileVersion $assemblyFileVersion -packageVersion $packageVersion

dnu restore "$projectDir"
dnu pack "$projectDir" --configuration Release

generateFiles -files $files -assemblyVersion '0.0.0.0' -assemblyFileVersion '0.0.0.0' -packageVersion '0.0.0-dev'