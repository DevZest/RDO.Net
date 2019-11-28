using CommandLine;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class ProgramTests
    {
        private class ConsoleOutput : IDisposable
        {
            private StringWriter stringWriter;
            private TextWriter originalOutput;

            public ConsoleOutput()
            {
                stringWriter = new StringWriter();
                originalOutput = Console.Out;
                Console.SetOut(stringWriter);
            }

            public string GetOuput()
            {
                return stringWriter.ToString();
            }

            public void Dispose()
            {
                Console.SetOut(originalOutput);
                stringWriter.Dispose();
            }
        }

        [TestMethod]
        public void Program_Run_DbGen()
        {
            Program_Run_DbGen(null);
            Program_Run_DbGen(Guid.NewGuid().ToString());
        }

        private void Program_Run_DbGen(string pipeName)
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var dbGenOption = new DbGenOptions()
                {
                    CancellationPipeName = pipeName,
                    DbSessionProviderType = typeof(_DbProvider).FullName
                };
                var args = Parser.Default.FormatCommandLine(dbGenOption).Split(' ');
                var result = args.RunDbInit();
                Assert.AreEqual(ExitCodes.Succeeded, result);
                var expected =
@"Executing DevZest.Data.DbInit.Program.Run...
Creating table [SalesLT].[Address]...
Creating table [SalesLT].[Customer]...
Creating table [SalesLT].[CustomerAddress]...
Creating table [SalesLT].[ProductCategory]...
Creating table [SalesLT].[ProductModel]...
Creating table [SalesLT].[ProductDescription]...
Creating table [SalesLT].[ProductModelProductDescription]...
Creating table [SalesLT].[Product]...
Creating table [SalesLT].[SalesOrderHeader]...
Creating table [SalesLT].[SalesOrderDetail]...
";
                Assert.AreEqual(expected, consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public async Task Program_Run()
        {
            new string[] { }.RunDbInit();

            using (var db = new DbProvider().Create(string.Empty))
            {
                var dataSet = await db.Address.ToDataSetAsync();
                Assert.AreEqual(1, dataSet.Count);
                Assert.AreEqual("Valley Cottage", dataSet._.City[0]);
            }
        }
    }
}
