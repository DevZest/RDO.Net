To build NuGet package `DevZest.Data.WPF`:

- Run `VerGen.bat` under `DevZest.Data.WPF` to generate files contain version numbers:
 
  ```
  ..\VerGen.bat 1.0.0 beta-*
  ```
  Source code files will be synchronized and generated into `Sync.DevZest.Data.WPF` directory.

- Open `ROD.WPF.Deploy.sln` in Visual Studio and build the solution in `Release` mode,
the result `DevZest.Data.WPF.1.0.0-beta-20180525.nupkg` will be built under `DevZest.Data.WPF\bin\Release` directory.

Note: 
- When building the package, the dependent package `DevZest.Data` with same package version must be present. For example, when building version
`1.0.0-beta-20180525`, the package `DevZest.Data.1.0.0-beta-20180525.nupkg` must first be built and deployed.
- The best practice is to deploy the packages to local NuGet repository first, then deploy to nuget.org after testing.
