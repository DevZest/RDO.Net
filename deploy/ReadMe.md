To build NuGet package `DevZest.Data.WPF`:

- Make sure current directory is `DevZest.Data.WPF`, then run `Build.bat`:
 
  ```
  ..\Build.bat 1.0.0 beta-*
  ```
  The result `DevZest.Data.WPF.1.0.0-beta-20180525.nupkg` will be built under `.\bin\Release` directory.

Note: 
- When building the package, the dependent package `DevZest.Data` with same package version must be present. For example, when building version
`1.0.0-beta-20180525`, the package `DevZest.Data.1.0.0-beta-20180525.nupkg` must first be built and deployed.
- The best practice is to deploy the packages to local NuGet repository first, then deploy to nuget.org after testing.
