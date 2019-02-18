using System.Runtime.CompilerServices;

#if !DEPLOY

[assembly: InternalsVisibleTo("Data.MySql.UnitTests")]

// for Moq
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#endif
