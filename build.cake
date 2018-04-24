#tool "nuget:https://www.nuget.org/api/v2?package=JetBrains.ReSharper.CommandLineTools&version=2018.1.0"
#tool "nuget:https://www.nuget.org/api/v2?package=coveralls.io&version=1.4.2"
#addin "nuget:https://www.nuget.org/api/v2?package=Cake.Coveralls&version=0.8.0"
#addin "nuget:https://www.nuget.org/api/v2?package=Cake.Incubator&version=2.0.1"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var projects = GetFiles("./**/*.csproj");
var projectPaths = projects.Select(project => project.GetDirectory().ToString());
var artifactsDir = "./Artifacts";
var coverageThreshold = 66;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information("Running tasks...");
});

Teardown(context =>
{
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
    var settings = new DeleteDirectorySettings {
        Recursive = true,
        Force = true
    };
    // Clean solution directories.
    foreach(var path in projectPaths)
    {
        Information($"Cleaning path {path} ...");
        var directoriesToDelete = new DirectoryPath[]{
            Directory($"{path}/obj"),
            Directory($"{path}/bin")
        };
        foreach(var dir in directoriesToDelete)
        {
            if (DirectoryExists(dir))
            {
                DeleteDirectory(dir, settings);
            }
        }
    }
    // Delete artifact output too
    if (DirectoryExists(artifactsDir))
    {
        Information($"Cleaning path {artifactsDir} ...");
        DeleteDirectory(artifactsDir, settings);
    }
});

Task("Restore")
    .Description("Restores all the NuGet packages that are used by the specified solution.")
    .Does(() =>
{
    // Restore all NuGet packages.
    foreach(var path in projectPaths)
    {
        Information($"Restoring {path}...");
        DotNetCoreRestore(path);
    }
});

Task("Build")
    .Description("Builds all the different parts of the project.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
     {
         Framework = "netcoreapp2.0",
         Configuration = "Release",
         OutputDirectory = artifactsDir
     };

     DotNetCoreBuild("./App/App.csproj", settings);
});

///////////////////////////////////////////////////////////////////////////////
// Unit Tests
///////////////////////////////////////////////////////////////////////////////

Task("Test")
    .Description("Run all unit tests within the project.")
    .Does(() =>
{
    // Calculate code coverage
    var settings = new DotNetCoreTestSettings
     {
         ArgumentCustomization = args => args.Append("/p:CollectCoverage=true")
                                             .Append("/p:CoverletOutputFormat=opencover")
                                             .Append($"/p:Threshold={coverageThreshold}")
     };
    DotNetCoreTest("./Test/Test.csproj", settings);
});

///////////////////////////////////////////////////////////////////////////////
// Validations
///////////////////////////////////////////////////////////////////////////////

Task("DupFinder")
    .Description("Find duplicates in the code")
    .Does(() =>
{
    var settings = new DupFinderSettings() {
        ShowStats = true,
        ShowText = true,
        OutputFile = $"{artifactsDir}/dupfinder.xml",
        ExcludeCodeRegionsByNameSubstring = new string [] { "DupFinder Exclusion" },
        ThrowExceptionOnFindingDuplicates = true
    };
    DupFinder("./App.sln", settings);
});

Task("InspectCode")
    .Description("Inspect the code using Resharper's rule set")
    .Does(() =>
{
    var settings = new InspectCodeSettings() {
        SolutionWideAnalysis = true,
        OutputFile = $"{artifactsDir}/inspectcode.xml",
        ThrowExceptionOnFindingViolations = true
    };
    InspectCode("./App.sln", settings);
});

Task("Validate")
    .Description("Validate code quality using Resharper CLI. tools.")
    .IsDependentOn("DupFinder")
    .IsDependentOn("InspectCode");

///////////////////////////////////////////////////////////////////////////////
// Third party tools
///////////////////////////////////////////////////////////////////////////////

Task("Upload-Coverage")
    .Does(() =>
{
    var isRunningOnAppveyor = EnvironmentVariable<bool>("APPVEYOR", false);
    if (!isRunningOnAppveyor)
        return;
    Information("Running on Appveyor, uploading coverage information to coveralls.io");
    CoverallsIo("./Test/coverage.xml", new CoverallsIoSettings()
    {
        RepoToken = EnvironmentVariable("coveralls_token")
    });
});

///////////////////////////////////////////////////////////////////////////////
// CI
///////////////////////////////////////////////////////////////////////////////

Task("CI")
    .Description("Build the code, test and validate")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Validate")
    .IsDependentOn("Upload-Coverage");

Task("CI-UNIX")
    .Description("Build the code, test and validate")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("Test")
    .IsDependentOn("Validate");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
