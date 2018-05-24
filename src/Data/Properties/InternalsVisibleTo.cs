using System.Runtime.CompilerServices;

#if !DEPLOY

[assembly: InternalsVisibleTo("Data.UnitTests")]

// for Moq
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#endif
