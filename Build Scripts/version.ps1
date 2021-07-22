param([String]$Path=$PWD.Path)

# Use this to check if a commit has a tag (i.e. is a release)
git describe --exact-match HEAD --tags > $null
if ($LASTEXITCODE -ne 0)
{
    # This is not a tag, i.e. not release version.
    # Set version string to branch and commit hash.
    $version = "$(git branch --show-current)-$(git rev-parse --short HEAD)"
    $codeversion = $version
}
else
{
    # Tagged commit, so this is a release. Use the latest tag.
    # Substring out the first letter as we don't use the v part.
    $version = git describe --tags --abbrev=0
    $codeversion = $version.Substring(1)
}

$output = "namespace DistanceRando { internal static partial class Metadata { public const string RandomizerVersion = `"$codeversion`"; } }"
$outputfile = (Join-Path $Path DistanceRando-Spectrum/VersionNumber.cs)

Write-Output $version
Write-Output $output | Out-File -Encoding utf8 -FilePath $outputfile