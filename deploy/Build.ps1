param (
  [string]$version = "",
  [string]$additionalLabel = "",
  [string]$projectDir = "",
  [string[]]$files
)

function showUsage()
{
	echo "Usage: Build Version [AdditionalLabel]"
	echo ""
	echo "Version: MAJOR.MINOR.PATCH"
	echo "  MAJOR version when you make incompatible API changes;"
	echo "  MINOR version when you add functionality in a backwards-compatible manner;"
	echo "  PATCH version when you make backwards-compatible bug fixes."
	echo ""
	echo "AdditionalLable:"
	echo "  Additional labels for pre-release and build metadata."
	echo "  If end '*' exists, it will be replaced with current date 'yyyyMMdd'."
	exit
}

function fileNotFound([string]$fileName)
{
	echo ""
	exit
}

function getAssemblyVersion([string]$major, [string]$minor)
{
	return $major + "." + $minor + ".0.0"
}

function getAssemblyFileVersion([string]$version)
{
	$span = New-TimeSpan -Start ([datetime]"2000/01/01") -End (Get-Date)
	return $version + "." + $span.Days
}

function getPackageVersion([string]$version, [string]$additionalLabel)
{
	if ($additionalLabel -eq "")
	{
		return $version
	}

	if ($additionalLabel.EndsWith("*"))
	{
		$additionalLabel = $additionalLabel.Substring(0, $additionalLabel.Length - 1)
		$additionalLabel = $additionalLabel + (Get-Date -format "yyyyMMdd")
	}

	return $version + "-" + $additionalLabel;
}

function safeRename([string]$from, [string]$to)
{
	if (Test-Path $to)
	{
		del $to > $null
	}
	Rename-Item $from $to
}

########################################################################
# Start of the script
########################################################################
for ($i=0; $i -lt $files.Count; $i++)
{
	$file = Join-Path $projectDir -ChildPath $files[$i]
	if (!(Test-Path $file))
	{
		echo "File $file not found!"
		exit
	}
}

$versions = $version.Split('.');
if ($versions.Count -ne 3)
{
	showUsage
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

for ($i=0; $i -lt $files.Count; $i++)
{
	$srcFile = Join-Path $projectDir -ChildPath $files[$i]
	$srcBakFile = $srcFile + '.bak'
	safeRename -from $srcFile -to $srcBakFile

	$templateFile = Join-Path $projectDir -ChildPath ('build.' + $files[$i])
	$content = [System.IO.File]::ReadAllText($templateFile).Replace('$ASSEMBLY_VERSION$', $assemblyVersion).Replace('$ASSEMBLY_FILE_VERSION$', $assemblyFileVersion).Replace('$PACKAGE_VERSION$', $packageVersion)
	[System.IO.File]::WriteAllText($srcFile, $content)
	$templateBakFile = $templateFile + '.bak'
	safeRename -from $templateFile -to $templateBakFile
}

dnu restore "$projectDir"
dnu pack "$projectDir" --configuration Release

for ($i=0; $i -lt $files.Count; $i++)
{
	$srcFile = Join-Path $projectDir -ChildPath $files[$i]
	$srcBakFile = $srcFile + '.bak'
	safeRename -from $srcBakFile -to $srcFile
	$templateFile = Join-Path $projectDir -ChildPath ('build.' + $files[$i])
	$templateBakFile = $templateFile + '.bak'
	safeRename -from $templateBakFile -to $templateFile
}
