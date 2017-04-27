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
    var version = GitVersion(new GitVersionSettings
    { 
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = "Source/AlephNote.App/Properties/AssemblyInfo.cs"
    });
    if (AppVeyor.IsRunningOnAppVeyor) AppVeyor.UpdateBuildVersion(version.NuGetVersionV2);
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
    var root = "Bin/" + configuration;
    var filesToDelete =
        GetFiles("Bin/**/*.pdb") + 
        GetFiles("Bin/**/*.xml") + 
        GetFiles("Bin/**/*.vshost.exe") +
        GetFiles("Bin/**/*.manifest");
    DeleteFiles(filesToDelete);
    if (DirectoryExists(root + "/.notes"))
        DeleteDirectory(root + "/.notes", true);

    Zip(root, "Bin/AlephNote.zip");
    if (AppVeyor.IsRunningOnAppVeyor) AppVeyor.UploadArtifact("Bin/AlephNote.zip");
});

Task("Default")
    .IsDependentOn("Pack");

RunTarget(target);