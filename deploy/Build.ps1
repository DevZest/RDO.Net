param (
  [string]$version = '',
  [string]$additionalLabel = '',
  [string]$projectDir = ''
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
	exit
}

function getAssemblyVersion([string]$major, [string]$minor)
{
    # Minor version number should be ignored:
    # https://codingforsmarties.wordpress.com/2016/01/21/how-to-version-assemblies-destined-for-nuget/
	return $major + '.0.0.0'
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

dotnet restore "$projectDir" /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$assemblyFileVersion /p:PackageVersion=$packageVersion
dotnet pack "$projectDir" --configuration Release /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$assemblyFileVersion /p:PackageVersion=$packageVersion
