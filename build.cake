#tool "GitVersion.Commandline"
#addin "Cake.Compression"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solution = File("Source/AlephNote.sln");

Task("Clean")
    .Does(() => 
{
    CleanDirectories("Bin/*");
    CleanDirectories("Lib/*");
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => NuGetRestore(solution));

Task("UpdateAssemblyInfo")
    .Does(() => 
{
    var version1 = GitVersion(new GitVersionSettings
    { 
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = "Source/AlephNote.App/Properties/AssemblyInfo.cs"
    });
    var version2 = GitVersion(new GitVersionSettings
    { 
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = "Source/AlephNote.Eto/Properties/AssemblyInfo.cs"
    });
    if (AppVeyor.IsRunningOnAppVeyor) AppVeyor.UpdateBuildVersion(version1.NuGetVersionV2);
    if (AppVeyor.IsRunningOnAppVeyor) AppVeyor.UpdateBuildVersion(version2.NuGetVersionV2);
});

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("UpdateAssemblyInfo")
    .Does(() => 
{
    MSBuild(solution, configurator =>
        configurator
            .SetConfiguration(configuration)
            .SetVerbosity(Verbosity.Minimal));
});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{    
    var rootWin  = "Bin/" + configuration + "/net46";
    var rootCore = "Bin/" + configuration + "/netstandard1.6";
    var filesToDelete =
        GetFiles("Bin/**/*.pdb") + 
        GetFiles("Bin/**/*.xml") + 
        GetFiles("Bin/**/*.vshost.exe") +
        GetFiles("Bin/**/*.manifest");
    DeleteFiles(filesToDelete);

    if (DirectoryExists(rootWin + "/.notes")) DeleteDirectory(rootWin + "/.notes", true);
    Zip(rootWin, "Bin/AlephNote.zip");

    if (DirectoryExists(rootCore + "/.notes")) DeleteDirectory(rootCore + "/.notes", true);
    Zip(rootCore, "Bin/AlephNote.zip");

    if (AppVeyor.IsRunningOnAppVeyor) AppVeyor.UploadArtifact("Bin/AlephNote.zip");
});

Task("Default")
    .IsDependentOn("Pack");

RunTarget(target);
