Run `Build.bat` to build NuGet packages:
- `DevZest.Data.MySql`

Build.bat must be run under DevZest.Data.MySql directory:
```
..\Build.bat 1.0.0 beta-*
```

The result `DevZest.Data.MySql.1.0.0-beta-20180525.nupkg` will be built under `DevZest.Data.MySql\bin\Release` directory.

Note: 
- When repo is newly cloned, the respective sub directory does not exist. You need to run `Sync.bat` to create these folders automatically.
- When building the package, the dependent package with same package version must be present. For example, when building pakcage
`DevZest.Data.MyServer.1.0.0-beta-20180525`, the package `DevZest.Data.1.0.0-beta-20180525.nupkg` must first be built and deployed.
