using System.Runtime.CompilerServices;

#if !DEPLOY

[assembly: InternalsVisibleTo("Data.PresentationFramework.UnitTests")]

// for Moq
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#endif
