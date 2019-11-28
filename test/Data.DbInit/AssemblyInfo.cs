using DevZest.Data.DbInit;
using DevZest.Samples.AdventureWorksLT;

[assembly:DefaultDbGen(typeof(_DbProvider), DbInitializerType = typeof(DbGenerator))]