#addin Cake.Coveralls

#tool coveralls.net
#tool coveralls.io
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=GitReleaseNotes"
#tool "nuget:?package=GitVersion.CommandLine"
// #tool "nuget:?package=gitlink"


Setup((context) =>
{
    Information("");    
    Information("");    
    Information("                                `.--:////////:--.  .--:////////:--.`                                ");
    Information("                          `-/+ooo/::-.......-+shhsshhs+-.......-::/ooo+/-`                          ");
    Information("                      `-+oo/-.`          ./+o+:.````.:+o+/.          `.-/oo+-`                      ");
    Information("                   `:oo/.`            ./o+:.            .:+o/.            `./oo:`                   ");
    Information("                 .+s/.             `:so-`                  `-os:`             ./s+.                 ");
    Information("               -oo-              `/s/.                        ./s/`              -oo-               ");
    Information("             .oo.              `/y/`                            `/y/`              .oo.             ");
    Information("           `+y-               -y+`                                `+y-               -y+`           ");
    Information("          .yo`               +y-                                    -y+               `oy.          ");
    Information("         -h:               `ss`                                      `ss`               :h-         ");
    Information("        -d.               `yo                                          oy`               .d-        ");
    Information("       .d-                ss                                            ss                -d.       ");
    Information("       d:                /h                                              h/                :d       ");
    Information("      os                `m.                                              .m`                so      ");
    Information("     `m.                oo                                                oo                .m`     ");
    Information("     +y                 N.                                                .N                 y+     ");
    Information("     y/     -yy.       -m     `yhh-           syy-   +ys       .ssssso:`   m-   ssssso+.     /y     ");
    Information("     m-     /NN-       /y     yNNNm`          dNNN/  yNd       -NNhyymNm+  y/   NNdyyhNNs    -m     ");
    Information("     N.     /NN-       +y    +NN+NNy          dNmNNs yNd       -NN/   +NN- y+   NNo   /NN:   .N     ");
    Information("     m-     /NN-       /y   -NNh:sNN/         dNy-mNddNd       -NN/   .NN/ y/   NNs.-:yNm`   -m     ");
    Information("     y/     /NN/----`  -m  `dNNNNNNNN.        dNy `hNNNd       -NN+--/dNd` m-   NNNNNNNd.    /y     ");
    Information("     +y     /NNNNNNN:   N. yNN.````dNd`       dNy   sNNd       -NNNNNmho` .N    NNo``+NNs`   y+     ");
    Information("     `m.     ```````    oo ```     ```        ```    ```        ``````    oo    ```   ```   .m`     ");
    Information("      os                `m.                                              .m`                so      ");
    Information("       d:                /h                                              h/                :d       ");
    Information("       .d-                ss                                            ss                -d.       ");
    Information("        -d.               `yo                                          oy`               .d-        ");
    Information("         -h:               `ss`                                      `ss`               :h-         ");
    Information("          .yo`               +y-                                    -y+               `oy.          ");
    Information("           `+y-               -y+`                                `+y-               -y+`           ");
    Information("             .oo.              `/y/`                            `/y/`              .oo.             ");
    Information("               -oo-              `/s/.                        ./s/`              -oo-               ");
    Information("                 .+s/.             `:so-`                  `-os:`             ./s+.                 ");
    Information("                   `:oo/.`            ./o+:.            .:+o/.            `./oo:`                   ");
    Information("                      `-+oo/-.`          ./+o+:.````.:+o+/.          `.-/oo+-`                      ");
    Information("                          `-/+ooo/::-.......-+shhsshhs+-.......-::/ooo+/-`                          ");
    Information("                                `.--:////////:--.  .--:////////:--.`                                "); 
    Information("");    
    Information("");    
}); 

var target = Argument("target", "Default");
var outputDir = "./artifacts/";

Task("Clean")
    .Does(() => {
        if (DirectoryExists(outputDir))
        {
            DeleteDirectory(outputDir, recursive:true);
        }
        CreateDirectory(outputDir);
    });

Task("Restore")
    .Does(() => {
        DotNetCoreRestore();
    });

GitVersion versionInfo = null;
Task("Version")
    .Does(() => {
    
        GitVersion(new GitVersionSettings{
            OutputType = GitVersionOutput.BuildServer,              
        });

        versionInfo = GitVersion(new GitVersionSettings{
            OutputType = GitVersionOutput.Json,              
            LogFilePath = "./artifacts/git-version.log"
        });
       
        var projects = GetFiles("./**/project.json");
        foreach(var project in projects)
        {
            Information("---------------------------");
            Information("- Patching: " + project);
            Information("---------------------------");
            PatchVersionProjectJson(project.FullPath);
        }
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Restore")
    .Does(() => {
        MSBuild("./AppVeyorPoc.sln",new MSBuildSettings {
            Verbosity = Verbosity.Minimal,
            Configuration = "Release",
            });
    });

Task("ReleaseNotes")
    .IsDependentOn("Version")
    .Does(() => {
        
        GitReleaseNotes("./artifacts/releasenotes.md", new GitReleaseNotesSettings {
            WorkingDirectory         = ".",
            Verbose                  = true,
            // IssueTracker             = GitReleaseNotesIssueTracker.GitHub,
            // RepoUserName             = "bob",
            // RepoPassword             = "password",
            // RepoUrl                  = "http://myrepo.co.uk",
            RepoBranch               = "master",
            // IssueTrackerUrl          = "http://myissuetracker.co.uk",
            // IssueTrackerUserName     = "bob",
            // IssueTrackerPassword     = "password",
            // IssueTrackerProjectId    = "1234",
            // Categories               = "Category1",
            Version                  = versionInfo.MajorMinorPatch,
        });

        var notes = System.IO.File.ReadAllText("./artifacts/releasenotes.md") ?? "no release notes.";
         
        Information("====release notes:===");
        Information(notes);
        Information("=====================");
    });

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("ReleaseNotes")
    .Does(() => {
        PackageProject("AppVeyorPoc.Core", "./src/AppVeyorPoc.Core/project.json");
        PackageProject("AppVeyorPoc.Library", "./src/AppVeyorPoc.Library/project.json");
    });

Task("Test")
    .IsDependentOn("Package")
    .Does(() => {
            var settings = new DotNetCoreTestSettings
            {
                Configuration = "Release"
            };

            OpenCover(tool => tool.DotNetCoreTest("./tests/AppVeyorPoc.Tests/project.json",settings),
                new FilePath("./artifacts/coverage.xml"),
                new OpenCoverSettings()
                {
                    OldStyle = true,
                    Register = "user"
                    
                }
                .WithFilter("+[*]*")
                .WithFilter("-[xunit*]*")
            );         
			
			CoverallsIo("./artifacts/coverage.xml", new CoverallsIoSettings()
            {
                RepoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN"),
				FullSources = true
            });  
    });


private void PackageProject(string projectName, string projectJsonPath)
{
    var settings = new DotNetCorePackSettings
    {
        OutputDirectory = outputDir,
        NoBuild = true
    };

    DotNetCorePack(projectJsonPath, settings);

    System.IO.File.WriteAllLines(outputDir + "artifacts", new[]{
        "nuget:" + projectName + "." + versionInfo.NuGetVersion + ".nupkg",
        "nugetSymbols:" + projectName + "." + versionInfo.NuGetVersion + ".symbols.nupkg",
        "releaseNotes:releasenotes.md"
    });
}    

private void PatchVersionProjectJson(string projectJsonPath)
{
     // Update project.json
    var updatedProjectJson = System.IO.File.ReadAllText(projectJsonPath)
        .Replace("1.0.0-*", versionInfo.NuGetVersion);

    System.IO.File.WriteAllText(projectJsonPath, updatedProjectJson);
}

Task("Default")
    .IsDependentOn("Package")
    .IsDependentOn("Test")
    ;

RunTarget(target);