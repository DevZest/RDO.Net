Run Build.bat to build NuGet packages:
- DevZest.Data
- DevZest.Data.SqlServer
- DevZest.Samples.AdventureWorksLT

Build.bat must be run under respective sub directory. For example, to build package DevZest.Data, make sure current directory is DevZest.Data, then run the following command:
```
..\Build.bat 1.0.0 beta-*
```

The result `Data.1.0.0-beta-20180525.nupkg` will be built under `DevZest.Data\bin\Release` directory.