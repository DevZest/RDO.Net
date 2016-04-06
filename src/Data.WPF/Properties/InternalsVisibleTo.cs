using System.Runtime.CompilerServices;

#if !DEPLOY

[assembly: InternalsVisibleTo("DevZest.Data.Wpf.UnitTests")]

// for Moq
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#endif
