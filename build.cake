string target = Argument("target", "Default");
string configuration = Argument("configuration", "Release");

string version = "5.0.0";
string modifier = "-alpha2";

string dbgSuffix = configuration == "Debug" ? "-dbg" : "";
string packageVersion = version + modifier + dbgSuffix;
string assemblyVersion = version + ".00000";

string PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";

string NHIBERNATE_DATABASE_TEMPLATE = EnvironmentVariable("NHIBERNATE_DATABASE_TEMPLATE") ?? "MSSQL.cfg.xml";
string NHIBERNATE_DIALECT = EnvironmentVariable("NHIBERNATE_DIALECT"); // if null, don't change from default
string NHIBERNATE_CONNECTION_STRING = EnvironmentVariable("NHIBERNATE_CONNECTION_STRING"); // if null, don't change from default
string DOTNET_FRAMEWORK = EnvironmentVariable("DOTNET_FRAMEWORK") ?? "net461";
string TARGET_PLATFORM = EnvironmentVariable("TARGET_PLATFORM") ?? "x86";

var ErrorDetail = new List<string>();

bool isDotNetCoreInstalled = false;
Setup(context =>
{
    if (BuildSystem.IsRunningOnAppVeyor)
    {
        var tag = AppVeyor.Environment.Repository.Tag;

        if (tag.IsTag)
        {
            packageVersion = tag.Name;
        }
        else
        {
            var buildNumber = AppVeyor.Environment.Build.Number.ToString("00000");
            var branch = AppVeyor.Environment.Repository.Branch;
            var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

            if (branch == "master" && !isPullRequest)
            {
                packageVersion = version + "-dev-" + buildNumber + dbgSuffix;
            }
            else
            {
                var suffix = "-ci-" + buildNumber + dbgSuffix;

                if (isPullRequest)
                    suffix += "-pr-" + AppVeyor.Environment.PullRequest.Number;
                else if (AppVeyor.Environment.Repository.Branch.StartsWith("release", StringComparison.OrdinalIgnoreCase))
                    suffix += "-pre-" + buildNumber;
                else
                    suffix += "-" + System.Text.RegularExpressions.Regex.Replace(branch, "[^0-9A-Za-z-]+", "-");

                // Nuget limits "special version part" to 20 chars. Add one for the hyphen.
                if (suffix.Length > 21)
                    suffix = suffix.Substring(0, 21);

                packageVersion = version + suffix;
            }

            assemblyVersion = version + "." + buildNumber;
        }

        AppVeyor.UpdateBuildVersion(packageVersion);
    }

    Information("Building version {0} of NHibernate.", packageVersion);
    isDotNetCoreInstalled = CheckIfDotNetCoreInstalled();
});

Task("Init")
	.Description("Outputs SharedAssemblyInfo.* files")
	.Does(() =>
	{
		WriteSharedAssemblyFiles(PROJECT_DIR + "src/", assemblyVersion, packageVersion);
	});

Task("Build")
    .Description("Builds the .NET 4.61 version of NHibernate")
	.IsDependentOn("Init")
    .Does(() =>
    {
        if(!isDotNetCoreInstalled)
        {
            Error("Was not built because .NET Core SDK is not installed");
            return;
        }
        BuildProject("src/NHibernate.sln", configuration);
    });

Task("CopyConfiguration")
    .Description("Setup Test Configuration")
    .OnError(exception => { ErrorDetail.Add(exception.Message); })
    .Does(() => 
    {
        var hibernateConfigPath = new FilePath(PROJECT_DIR + "current-test-configuration/hibernate.cfg.xml");
        CreateDirectory(PROJECT_DIR + "current-test-configuration");
        CopyFile(PROJECT_DIR + "src/NHibernate.Config.Templates/" + NHIBERNATE_DATABASE_TEMPLATE, hibernateConfigPath);


        var pokeSettings = new XmlPokeSettings();
        pokeSettings.Namespaces["nhc"] = "urn:nhibernate-configuration-2.2";

        if (!string.IsNullOrEmpty(NHIBERNATE_DIALECT))
            XmlPoke(hibernateConfigPath, "//*/nhc:property[@name='dialect']", NHIBERNATE_DIALECT, pokeSettings);

        if (!string.IsNullOrEmpty(NHIBERNATE_CONNECTION_STRING))
            XmlPoke(hibernateConfigPath, "//*/nhc:property[@name='connection.connection_string']", NHIBERNATE_CONNECTION_STRING, pokeSettings);
    });

Task("Test")
    .Description("Tests NHibernate")
    .IsDependentOn("Build")
    .OnError(exception => { ErrorDetail.Add(exception.Message); })
    .Does(() =>
    {
        var framework = DOTNET_FRAMEWORK;
        Information("Testing framework: " + framework);

        var dir = PROJECT_DIR + "src/NHibernate.TestDatabaseSetup/";
        RunDotnetCoreTests(dir, "NHibernate.TestDatabaseSetup.csproj", framework, ref ErrorDetail);

        dir = PROJECT_DIR + "src/NHibernate.Test/";
        RunDotnetCoreTests(dir, "NHibernate.Test.csproj", framework, ref ErrorDetail);

        dir = PROJECT_DIR + "src/NHibernate.Test.VisualBasic/";
        RunDotnetCoreTests(dir, "NHibernate.Test.VisualBasic.vbproj", framework, ref ErrorDetail);
    });

Teardown(context => CheckForError(ref ErrorDetail));

bool CheckIfDotNetCoreInstalled()
{
    try
    {
        Information("Checking if .NET Core SDK is installed");
        StartProcess("dotnet", new ProcessSettings
        {
            Arguments = "--version"
        });
    }
    catch(Exception)
    {
        Warning(".NET Core SDK is not installed. It can be installed from https://www.microsoft.com/net/core");
        return false;
    }
    return true;
}

void CheckForError(ref List<string> errorDetail)
{
    if(errorDetail.Count != 0)
    {
        var copyError = new List<string>();
        copyError = errorDetail.Select(s => s).ToList();
        errorDetail.Clear();
        throw new Exception("One or more unit tests failed, breaking the build.\n"
                              + copyError.Aggregate((x,y) => x + "\n" + y));
    }
}

void BuildProject(string projectPath, string configuration)
{
    int rc = StartProcess(
        "dotnet",
        new ProcessSettings()
            .UseWorkingDirectory(PROJECT_DIR)
            .WithArguments(args => args
                .Append("build")
                .AppendQuoted(projectPath)
                .AppendSwitch("--configuration", configuration)
                .Append("/property:Version={0}", packageVersion)
                .Append("/property:AssemblyVersion={0}", assemblyVersion)
                .Append("/property:FileVersion={0}", assemblyVersion)
            )
        );
}

void RunDotnetCoreTests(DirectoryPath workingDir, FilePath testProject, string framework, ref List<string> errorDetail)
{
    string settingsPath = PROJECT_DIR + "src/NHibernate.Test/test-" + TARGET_PLATFORM + ".runsettings";
    int rc = StartProcess(
        "dotnet",
        new ProcessSettings()
            .UseWorkingDirectory(workingDir.FullPath)
            .WithArguments(args => {
                args
                    .Append("test")
                    .AppendQuoted(testProject.FullPath)
                    .AppendSwitch("--configuration", configuration)
                    .AppendSwitch("--framework", framework)
                    .AppendSwitchQuoted("--settings", settingsPath)
                    .Append("/property:Version={0}", packageVersion)
                    .Append("/property:AssemblyVersion={0}", assemblyVersion)
                    .Append("/property:FileVersion={0}", assemblyVersion);

                if (BuildSystem.IsRunningOnAppVeyor)
                {
                    // Report results in real-time: https://github.com/Microsoft/vstest-docs/blob/master/docs/report.md
                    args.Append("--logger:Appveyor");
                }
            })
    );

    if (rc > 0)
        errorDetail.Add(string.Format("{0}: {1} tests failed", framework, rc));
    else if (rc < 0)
        errorDetail.Add(string.Format("{0} returned rc = {1}", testProject.GetFilename(), rc));
}

void WriteSharedAssemblyFiles(DirectoryPath path, string version, string informationalVersion)
{
    const string csTemplate = 
@"using System; using System.Reflection; using System.Runtime.CompilerServices;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: AssemblyVersionAttribute(""{0}"")]
[assembly: AssemblyInformationalVersionAttribute(""{1}"")]
[assembly: AssemblyFileVersionAttribute(""{0}"")]

";

    const string vbTemplate = 
@"'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices
<Assembly: AssemblyVersionAttribute(""{0}""),  _
 Assembly: AssemblyInformationalVersionAttribute(""{1}""),  _
 Assembly: AssemblyFileVersionAttribute(""{0}"")> 

";

    System.IO.File.WriteAllText(System.IO.Path.Combine(path.FullPath, "SharedAssemblyInfo.cs"), string.Format(csTemplate, version, informationalVersion));
    System.IO.File.WriteAllText(System.IO.Path.Combine(path.FullPath, "SharedAssemblyInfo.vb"), string.Format(vbTemplate, version, informationalVersion));
}

Task("AppVeyor")
    .Description("Builds, tests and packages on AppVeyor")
    .IsDependentOn("Build")
    .IsDependentOn("CopyConfiguration")
    .IsDependentOn("Test");

Task("Travis")
    .Description("Builds and tests on Travis")
    .IsDependentOn("Build")
    .IsDependentOn("CopyConfiguration")
    .IsDependentOn("Test");

Task("Default")
    .Description("Builds all versions of NHibernate")
    .IsDependentOn("Build");

RunTarget(target);
