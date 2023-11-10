// Copyright 2021-2023 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

#pragma warning disable IDE0051 // Remove unused private members
partial class Build : NukeBuild
    , IHazSolution
    , IHazChangelog
    , IHazGitRepository
    , IHazGitVersion
    , IRestore
    , ICompile
    , ITest
    , IPack
    , IPublish
    , ICreateGitHubRelease
{
    public const string MainBranch = "main";
    public const string DevelopBranch = "develop";
    public const string RepositoryName = "Vipentti.IO.Abstractions.FileSystemGlobbing";

    // Support plugins are available for:
    //   - JetBrains ReSharper        https://nuke.build/resharper
    //   - JetBrains Rider            https://nuke.build/rider
    //   - Microsoft VisualStudio     https://nuke.build/visualstudio
    //   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main()
    {
        return Execute<Build>(x => ((ICompile)x).Compile);
    }

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Owner of the repository")] readonly string GitHubRepositoryOwner = null!;

    [Parameter("Name of the remote - Default is 'origin'")] readonly string RemoteName = "origin";

    [Parameter("")][Secret] readonly string PublicNuGetApiKey = null!;
    [Parameter("")][Secret] readonly string GitHubRegistryApiKey = null!;

    [Parameter("Reproducible build")] readonly bool ForceReproducible;

    GitVersion GitVersion => From<IHazGitVersion>().Versioning;
    GitRepository GitRepository => From<IHazGitRepository>().GitRepository;

    public static readonly Regex ValidTagRegex = new(@"\d+\.\d+\.\d+", RegexOptions.Compiled);
    string TagVersion => GitRepository.Tags?.SingleOrDefault(ValidTagRegex.IsMatch);
    bool IsTaggedBuild => !string.IsNullOrWhiteSpace(TagVersion);

    bool IsMainBranch => GitRepository.Branch?.Equals(MainBranch, StringComparison.OrdinalIgnoreCase) ?? false;

    bool IsOriginalRepository => GitRepository.Identifier == $"{GitHubRepositoryOwner}/{RepositoryName}";

    protected override void OnBuildInitialized()
    {
        var version = GitVersion.InformationalVersion;

        Serilog.Log.Information("BUILD SETUP");
        Serilog.Log.Information("Configuration:  {Configuration}", Configuration);
        Serilog.Log.Information("Version:        {VersionSuffix}", version);
        Serilog.Log.Information("TagVersion:     {TagVersion}", TagVersion ?? "(null)");
    }

    string PublicNuGetSource => "https://api.nuget.org/v3/index.json";

    [CI] readonly GitHubActions GitHubActions = null!;

    string GitHubRegistrySource => GitHubActions switch
    {
        not null => $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner}/index.json",
        null => $"https://nuget.pkg.github.com/{GitHubRepositoryOwner}/index.json",
    };

    bool UsePublicNuGet => false;

    string IPublish.NuGetApiKey => UsePublicNuGet ? PublicNuGetApiKey : GitHubRegistryApiKey;
    string IPublish.NuGetSource => UsePublicNuGet ? PublicNuGetSource : GitHubRegistrySource;

    Solution Solution => From<IHazSolution>().Solution;

    SolutionFolder SolutionSrc => Solution.GetSolutionFolder("src");
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => From<IHazArtifacts>().ArtifactsDirectory;
    AbsolutePath PackagesDirectory => From<IPack>().PackagesDirectory;
    AbsolutePath TestResultsDirectory => From<ITest>().TestResultDirectory;

    IEnumerable<AbsolutePath> NuGetPackageFiles
        => From<IPack>().PackagesDirectory.GlobFiles("*.nupkg");

    IEnumerable<Project> ProjectsToPublish => new[]
    {
        SolutionSrc.GetProject("Vipentti.IO.Abstractions.FileSystemGlobbing"),
    };

    IEnumerable<(Project Project, string Framework)> PublishConfigurations =>
        from project in ProjectsToPublish
        from framework in project.GetTargetFrameworks()
        select (project, framework);

    IEnumerable<(Project Project, string Framework)> ICompile.PublishConfigurations => PublishConfigurations;

    public IEnumerable<Project> TestProjects => Solution.GetAllProjects("*Tests*");

    Target ValidateFormat => _ => _
        .AssuredAfterFailure()
        .After<ITest>(x => x.Test)
        .Before<IPack>(x => x.Pack)
        .Executes(() =>
        {
            DotNetFormat(_ => _
                .Apply(FormatSettings));
        });

    Configure<DotNetFormatSettings> FormatSettings => _ => _
        .SetVerifyNoChanges(true);

    Target Clean => _ => _
        .Before<IRestore>()
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
            TestsDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target CleanPackages => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Cleaning packages {directory}", PackagesDirectory);
            PackagesDirectory.CreateOrCleanDirectory();
        });

    Target CleanTestResults => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Cleaning test-results {directory}", TestResultsDirectory);
            TestResultsDirectory.CreateOrCleanDirectory();
        });

    Target ITest.Test => _ => _
        .Inherit<ITest>()
        .DependsOn(CleanTestResults);

    Configure<DotNetTestSettings, Project> ITest.TestProjectSettings => (_, v) => _
        .RemoveLoggers($"trx;LogFileName={v.Name}.trx")
        .AddLoggers($"trx;LogFilePrefix={v.Name}");

    Target IPack.Pack => _ => _
        .DependsOn<ICompile>(x => x.Compile)
        .DependsOn<ITest>(x => x.Test)
        .Produces(PackagesDirectory / "*.nupkg")
        .DependsOn(CleanPackages)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .Apply(From<IPack>().PackSettingsBase)
                .Apply(From<IPack>().PackSettings)
                .CombineWith(ProjectsToPublish, (_, v) => _
                    .SetProject(v)
                ));

            ReportSummary(_ => _
                .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });

    Target ValidatePackages => _ => _
        .DependsOn<IPack>(x => x.Pack)
        .After<IPack>(x => x.Pack)
        .Executes(() =>
        {
            DotNetToolUpdate(_ => _
                .SetGlobal(true)
                .SetPackageName("Meziantou.Framework.NuGetPackageValidation.Tool")
            );

            static void ValidatePackage(AbsolutePath path)
            {
                ProcessTasks.StartProcess("meziantou.validate-nuget-package", arguments: path, logInvocation: true).AssertZeroExitCode();
            }

            PackagesDirectory.GlobFiles("*.nupkg")
                .ForEach(ValidatePackage);
        });

    string UseReproducibleBuild => ForceReproducible || IsServerBuild ? "true" : "false";

    Configure<DotNetRestoreSettings> IRestore.RestoreSettings => _ => _
        .AddProperty("UseReproducibleBuild", UseReproducibleBuild)
        ;

    Configure<DotNetBuildSettings> ICompile.CompileSettings => _ => _
        .AddProperty("RepositoryCommit", GitRepository.Commit)
        .AddProperty("RepositoryBranch", GitRepository.Branch)
        .AddProperty("UseReproducibleBuild", UseReproducibleBuild)
        ;

    Configure<DotNetPublishSettings> ICompile.PublishSettings => _ => _
        .AddProperty("RepositoryCommit", GitRepository.Commit)
        .AddProperty("RepositoryBranch", GitRepository.Branch)
        .AddProperty("UseReproducibleBuild", UseReproducibleBuild)
        ;

    Configure<DotNetPackSettings> IPack.PackSettings => _ => _
        .AddProperty("RepositoryCommit", GitRepository.Commit)
        .AddProperty("RepositoryBranch", GitRepository.Branch)
        .AddProperty("UseReproducibleBuild", UseReproducibleBuild)
        ;

    string ICreateGitHubRelease.Name => MajorMinorPatchVersion;
    IEnumerable<AbsolutePath> ICreateGitHubRelease.AssetFiles => NuGetPackageFiles;

    T From<T>()
        where T : INukeBuild
        => (T)(object)this;
}
