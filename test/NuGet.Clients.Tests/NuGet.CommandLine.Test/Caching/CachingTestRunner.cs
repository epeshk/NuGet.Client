﻿using System;
using System.Threading.Tasks;
using NuGet.Test.Utility;

namespace NuGet.CommandLine.Test.Caching
{
    public static class CachingTestRunner
    {
        public static async Task<CachingValidations> ExecuteAsync(ICachingTest test, ICachingCommand command, INuGetExe nuGetExe, CachingType caching, ServerType server)
        {
            using (var testFolder = TestFileSystemUtility.CreateRandomTestFolder())
            using (var mockServer = new MockServer())
            {
                var tc = new CachingTestContext(testFolder, mockServer, nuGetExe);

                // Enable this flag to launch the debugger when the nuget.exe process starts. This also increases
                // logging verbosity and command timeout.
                //
                // tc.Debug = true;

                tc.NoCache = caching.HasFlag(CachingType.NoCache);
                tc.DirectDownload = caching.HasFlag(CachingType.DirectDownload);
                tc.CurrentSource = server == ServerType.V2 ? tc.V2Source : tc.V3Source;

                tc.ClearHttpCache();

                var args = await test.PrepareTestAsync(tc, command);

                var result = tc.Execute(args);

                return test.Validate(tc, command, result);
            }
        }

        public static async Task<CachingValidations> ExecuteAsync(Type testType, Type commandType, INuGetExe nuGetExe, CachingType caching, ServerType server)
        {
            var test = (ICachingTest)Activator.CreateInstance(testType);
            var command = (ICachingCommand)Activator.CreateInstance(commandType);

            return await ExecuteAsync(
                test,
                command,
                nuGetExe,
                caching,
                server);
        }
    }
}
