using System.Runtime.CompilerServices;

#if !DEPLOY

[assembly: InternalsVisibleTo("DevZest.Data.DbInit.Test")]

// for Moq
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#endif
