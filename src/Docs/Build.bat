nuget install DevZest.Data.DbInit -PreRelease -ExcludeVersion -DependencyVersion Ignore -OutputDirectory nuget_packages
nuget install DevZest.Data.WPF -PreRelease -ExcludeVersion -DependencyVersion Ignore -OutputDirectory nuget_packages

docfx --serve
