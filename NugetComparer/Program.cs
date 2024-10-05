using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NugetComparer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            invoke().GetAwaiter().GetResult();
        }

        const string BaseNugetServerUrl = "https://api.nuget.org/v3/index.json";
        const string BaseSiteUrl = "https://www.nuget.org/packages";

        static List<NugetResponseModel> VunrabilitiesList = new List<NugetResponseModel>();

        public class NugetModel
        {
            public string PackageName { get; set; }
            public NuGetVersion LocalVersion { get; set; }
        }

        public class NugetsResponse
        {
            public List<NugetResponseModel> Vunrabilities { get; set; }
            public string PackageName { get; set; }
        }


        static async Task invoke()
        {
            Console.WriteLine("Please enter nuget Packages (*.nupkg) folder:");
            string packages = Console.ReadLine();
            Console.WriteLine("Starting Scanning:");

            int batchSize = 10;
            List<NugetModel> NugetsInfoColl = new List<NugetModel>();
            List<string> nugets = Directory.GetDirectories(packages).ToList();

            /// get dlls info
            foreach (var nuget in nugets)
            {
                var nugetDLL = Directory.GetFiles(nuget, "*.nupkg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (nugetDLL != null)
                {
                    var packageName = Path.GetFileName(nuget);
                    var localVersion = GetLocalVersion(nugetDLL);
                    packageName = packageName.Replace($".{localVersion.ToString()}", "");
                    string url = $"{BaseSiteUrl}/{packageName}/{localVersion}";

                    NugetsInfoColl.Add(new NugetModel()
                    {
                        PackageName = packageName,
                        LocalVersion = localVersion
                    });
                }
            }

            List<NugetsResponse> ResponsesList = new List<NugetsResponse>();
            /// get http responses

            List<Task> tasks = new List<Task>();

            foreach (var item in NugetsInfoColl)
            //for (int i = 0; i < NugetsInfoColl.ToList().Count; i += batchSize)
            {
                string packageName = "";
                packageName = item.PackageName;
                tasks.Add(Task.Run(() => GetVunrabilities(item.LocalVersion.Version.ToString(), packageName)));

            }

            await Task.WhenAll(tasks);

            await PrintAnalysis(VunrabilitiesList, NugetsInfoColl);

            Console.WriteLine("Scan all completed!");
            Console.ReadLine();
        }

        static async Task GetVunrabilities(string Version, string PackageName)
        {
            var logger = new Logger();
            var cache = new SourceCacheContext();
            var rep = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var res = await rep.GetResourceAsync<PackageMetadataResource>();
            var identity = new PackageIdentity(PackageName, NuGetVersion.Parse(Version));
            var metadata = await res.GetMetadataAsync(identity, cache, logger, default);

            if (metadata != null)
            {
                if (metadata.Vulnerabilities != null)
                {
                    foreach (var vulnerability in metadata.Vulnerabilities)
                    {
                        NugetResponseModel nu = new NugetResponseModel();
                        nu.AdvisoryUrl = vulnerability.AdvisoryUrl.ToString();
                        nu.Severity = vulnerability.Severity;
                        nu.PackageName = PackageName;
                        VunrabilitiesList.Add(nu);
                    }
                }
                else
                {
                    VunrabilitiesList.Add(new NugetResponseModel() { PackageName = PackageName, Severity = 0 });
                }
            }
        }




        /// print analysis to console


        static async Task PrintAnalysis(List<NugetResponseModel> ResponsesList, List<NugetModel> NugetsInfoColl)
        {
            foreach (var res in NugetsInfoColl)
            {
                StringBuilder messages = new StringBuilder();
                var latestVersion = await GetLatestVersionFromNuGet(res.PackageName);
                bool vunrable = false;
                NuGetVersion localVer = NugetsInfoColl.Where(e => e.PackageName == res.PackageName).FirstOrDefault().LocalVersion;
                var vunList = ResponsesList.Where(e => e.PackageName == res.PackageName).ToList();

                messages.AppendLine($"Package Name: {res.PackageName}");
                messages.AppendLine($"Latest Version: {latestVersion}");
                messages.AppendLine($"Local Version: {localVer}");

                if (vunList.Count > 0 && vunList.Any(e => e.AdvisoryUrl != null))
                {
                    foreach (var vun in vunList)
                        if (vun.AdvisoryUrl != "")
                        {
                            messages.AppendLine($"Severity: {vun.Severity}: Analysis: {vun.AdvisoryUrl}");
                            vunrable = true;
                        }
                }

                string updateAvailable = "No";
                if (latestVersion > localVer)
                    updateAvailable = "Yes";

                messages.AppendLine($"Update available: {updateAvailable}");

                Console.ForegroundColor = ConsoleColor.Green;

                if (latestVersion > localVer)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                if (vunrable)
                    Console.ForegroundColor = ConsoleColor.Red;

                Console.Write(messages.ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("------------------------------------------");
            }

        }


        static NuGetVersion GetLocalVersion(string dllFile)
        {
            using (var packageStream = new FileStream($"{dllFile}.", FileMode.Open, FileAccess.Read))
            {
                using (var packageReader = new PackageArchiveReader(packageStream))
                {
                    var nugetReader = packageReader.NuspecReader;
                    var ver = nugetReader.GetVersion();
                    return ver;
                }
            }
        }


        public class NugetResponseModel
        {
            public string PackageName { get; set; }
            public string AdvisoryUrl { get; set; }
            public int Severity { get; set; }
        }

        class Logger : ILogger
        {
            public void Log(LogLevel level, string data)
            {
            }

            public void Log(ILogMessage message)
            {
            }

            public Task LogAsync(LogLevel level, string data)
            {
                throw new NotImplementedException();
            }

            public Task LogAsync(ILogMessage message)
            {
                throw new NotImplementedException();
            }

            public void LogDebug(string data)
            {
            }

            public void LogError(string data)
            {
            }

            public void LogInformation(string data)
            {
            }

            public void LogInformationSummary(string data)
            {
            }

            public void LogMinimal(string data)
            {
            }

            public void LogVerbose(string data)
            {
            }

            public void LogWarning(string data)
            {
            }
        }

        static async Task<NuGetVersion> GetLatestVersionFromNuGet(string packageName)
        {
            var repository = Repository.Factory.GetCoreV3(BaseNugetServerUrl);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var versions = await resource.GetAllVersionsAsync(packageName, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);
            var finalVersions = versions.Where(v => !v.IsPrerelease).ToArray();

            /// this was unknown or private dll (PE?)
            if (finalVersions == null) return null;
            return finalVersions.Max();
        }

    }
}
