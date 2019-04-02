Run `Build.bat` to build NuGet packages:
- `DevZest.Data.AspNetCore`

Build.bat must be run under DevZest.Data.AspNetCore sub directory, then run the following command:
```
..\Build.bat 1.0.0 beta-*
```

The result `DevZest.Data.AspNetCore.1.0.0-beta-20180525.nupkg` will be built under `DevZest.Data.AspNetCore\bin\Release` directory.

Note: 
- When repo is newly cloned, the DevZest.Data.AspNetCore sub directory does not exist. You need to run `Sync.bat` to create these folders automatically.
- When building the package, the dependent package with same package version must be present. For example, when building pakcage
`DevZest.Data.AspNetCore.1.0.0-beta-20180525`, the package `DevZest.Data.1.0.0-beta-20180525.nupkg` must first be built and deployed.
- The best practice is to deploy the packages to local NuGet repository, then deploy to nuget.org after testing.
