nuget install memberpage -Version 2.43.2 -OutputDirectory nuget_packages
nuget install DevZest.Data.DbInit -PreRelease -ExcludeVersion -DependencyVersion Ignore -OutputDirectory nuget_packages

docfx --serve
