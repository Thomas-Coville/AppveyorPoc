#tool "nuget:?package=GitReleaseNotes"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"

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
        DotNetCoreRestore("src");
    });

GitVersion versionInfo = null;
Task("Version")
    .Does(() => {
        GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo = true,
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
        MSBuild("./AppVeyorPoc.sln");
    });

Task("Package")
    .IsDependentOn("Build")
    .Does(() => {
        GitLink("./", new GitLinkSettings { ArgumentCustomization = args => args.Append("-include Specify,Specify.Autofac") });

        GenerateReleaseNotes();

        PackageProject("AppVeyorPoc.Core", "./src/AppVeyorPoc.Core/project.json");
        PackageProject("AppVeyorPoc.Library", "./src/AppVeyorPoc.Library/project.json");
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

private void GenerateReleaseNotes()
{
        var releaseNotesExitCode = StartProcess(
        @"tools\GitReleaseNotes\tools\gitreleasenotes.exe", 
        new ProcessSettings { Arguments = ". /o artifacts/releasenotes.md" });
    if (string.IsNullOrEmpty(System.IO.File.ReadAllText("./artifacts/releasenotes.md")))
        System.IO.File.WriteAllText("./artifacts/releasenotes.md", "No issues closed since last release");

    if (releaseNotesExitCode != 0) throw new Exception("Failed to generate release notes");
}

private void PatchVersionProjectJson(string projectJsonPath)
{
     // Update project.json
    var updatedProjectJson = System.IO.File.ReadAllText(projectJsonPath)
        .Replace("1.0.0-*", versionInfo.NuGetVersion);

    System.IO.File.WriteAllText(projectJsonPath, updatedProjectJson);
}

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);