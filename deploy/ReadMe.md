Run `Build.bat` to build NuGet packages:
- `DevZest.Data`
- `DevZest.Data.SqlServer`
- `DevZest.Data.MySql`

Build.bat must be run under respective sub directory. For example, to build package DevZest.Data, make sure current directory is DevZest.Data, then run the following command:
```
..\Build.bat 1.0.0 beta-*
```

The result `DevZest.Data.1.0.0-beta-20180525.nupkg` will be built under `DevZest.Data\bin\Release` directory.

Note: 
- When repo is newly cloned, the respective sub directory does not exist. You need to run `Sync.bat` to create these folders automatically.
- When building the package, the dependent package with same package version must be present. For example, when building pakcage
`DevZest.Data.SqlServer.1.0.0-beta-20180525`, the package `DevZest.Data.1.0.0-beta-20180525.nupkg` must first be built and deployed.
- The best practice is to deploy the packages to local NuGet repository, testing with `RDO.Data.Samples.sln` (projects in this solution are overrided to reference latest nuget packages), then deploy to nuget.org.


To build NuGet package `DevZest.Data.WPF`:

Prerequisites:
Chocolatey packages installed: `vswhere`, `nuget.commandline`.

- Make sure current directory is `DevZest.Data.WPF`, then run `BuildWpf.bat`:
 
  ```
  ..\BuildWpf.bat 1.0.0 beta-*
  ```
  The result `DevZest.Data.WPF.1.0.0-beta-20180525.nupkg` will be built under `.\bin\Release` directory.

Note: 
- When building the package, the dependent package `DevZest.Data` with same package version must be present. For example, when building version
`1.0.0-beta-20180525`, the package `DevZest.Data.1.0.0-beta-20180525.nupkg` must first be built and deployed.
- The best practice is to deploy the packages to local NuGet repository first, then deploy to nuget.org after testing.
