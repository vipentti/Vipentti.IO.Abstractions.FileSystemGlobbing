using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild,
    ICompile,
    IHaveChangeLog,
    IHaveConfiguration,
    IHaveGitRepository,
    IHaveGitVersion,
    IPack,
    IPublish,
    IRestore,
    ITest
{
    // Support plugins are available for:
    //   - JetBrains ReSharper        https://nuke.build/resharper
    //   - JetBrains Rider            https://nuke.build/rider
    //   - Microsoft VisualStudio     https://nuke.build/visualstudio
    //   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

    [Parameter] readonly string GitHubRepositoryOwner = null!;

    [CI] readonly GitHubActions GitHubActions = null!;

    [Solution] readonly Solution Solution = null!;
    Solution IHaveSolution.Solution => Solution;

    GitVersion GitVersion => From<IHaveGitVersion>().Versioning;
    GitRepository GitRepository => From<IHaveGitRepository>().GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";

    bool IsMainBranch => GitRepository.Branch?.Equals(MainBranch, StringComparison.OrdinalIgnoreCase) ?? false;
    Project MainProject => Solution.GetProject("Vipentti.IO.Abstractions.FileSystemGlobbing");

    Target Clean => _ => _
        .Before<IRestore>()
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    IEnumerable<(Project Project, string Framework)> ICompile.PublishConfigurations =>
        from project in new[] { MainProject }
        from framework in project.GetTargetFrameworks()
        select (project, framework);

    IEnumerable<Project> ITest.TestProjects => Solution.GetProjects("*Test*");

    IEnumerable<AbsolutePath> IPublish.PushPackageFiles => PackagesDirectory.GlobFiles("*.nupkg");

    string PublicNuGetSource => "https://api.nuget.org/v3/index.json";
    string GitHubRegistrySource => GitHubActions switch
    {
        not null => $"https://nuget.pkg.github.com/{GitHubActions.GitHubRepositoryOwner}/index.json",
        null => $"https://nuget.pkg.github.com/{GitHubRepositoryOwner}/index.json",
    };

    readonly string Repository = "Vipentti.IO.Abstractions.FileSystemGlobbing";
    bool IsOriginalRepository => GitRepository.Identifier == $"{GitHubRepositoryOwner}/{Repository}";
    public bool UsePublicNuGet => IsOriginalRepository;

    [Parameter] [Secret] readonly string PublicNuGetApiKey = null!;
    [Parameter] [Secret] readonly string GitHubRegistryApiKey = null!;

    string IPublish.NuGetApiKey => UsePublicNuGet ? PublicNuGetApiKey : GitHubRegistryApiKey;
    string IPublish.NuGetSource => UsePublicNuGet ? PublicNuGetSource : GitHubRegistrySource;

    Target IPublish.Publish => _ => _
        .Inherit<IPublish>()
        .Consumes(From<IPack>().Pack)
        .After(Changelog)
        .Before(Release)
        .Requires(() => GitHubRepositoryOwner)
        .WhenSkipped(DependencyBehavior.Execute);

    Configure<DotNetNuGetPushSettings> IPublish.PushSettings => _ => _;

    T From<T>()
        where T : INukeBuild
        => (T)(object)this;
}
