﻿// Copyright 2021-2024 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Components;
using Vipentti.Nuke.Components;
using static Vipentti.Nuke.Components.StandardNames;

namespace build;

[ExtendedGitHubActions(
    "pull-request",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.MacOsLatest,
    OnPullRequestBranches = new[] { MainBranch, DevelopBranch },
    PublishArtifacts = false,
    FetchDepth = 0 // fetch full history
    , SetupDotnetVersions = new[]
    {
        "6.x",
        "7.x",
        "8.x",
    }
    , InvokedTargets = new[]
    {
        nameof(IUseLocalDotNetTools.RestoreLocalTools),
        nameof(ITest.Test),
        nameof(IUseLinters.InstallLinters),
        nameof(IUseLinters.Lint),
        nameof(IValidatePackages.ValidatePackages),
    })]
[StandardPublishGitHubActions(
    "publish",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.MacOsLatest
    , OnPushBranches = new[] { MainBranch }
    , SetupDotnetVersions = new[]
    {
        "6.x",
        "7.x",
        "8.x",
    }
)]
class Build : StandardNukeBuild, IUseCsharpier
{
    public override string OriginalRepositoryName { get; } = "Vipentti.IO.Abstractions.FileSystemGlobbing";
    public override string MainReleaseBranch { get; } = MainBranch;
    public override IEnumerable<Project> ProjectsToPack => new[]
    {
        CurrentSolution.GetSolutionFolder("src").GetProject("Vipentti.IO.Abstractions.FileSystemGlobbing"),
    };

    public override IEnumerable<IProvideLinter> Linters => new[]
    {
        From<IUseDotNetFormat>().Linter,
        From<IUseCsharpier>().Linter,
    };

    public override IEnumerable<IProvideFormatter> Formatters => new[]
    {
        From<IUseDotNetFormat>().Formatter,
        From<IUseCsharpier>().Formatter,
    };

    bool IUseCsharpier.UseGlobalTool { get; } = false;

    public override bool SignReleaseTags { get; } = true;

    public override IEnumerable<Project> TestProjects => CurrentSolution.GetAllProjects("*Tests*");

    // Support plugins are available for:
    //   - JetBrains ReSharper        https://nuke.build/resharper
    //   - JetBrains Rider            https://nuke.build/rider
    //   - Microsoft VisualStudio     https://nuke.build/visualstudio
    //   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.From<ICompile>().Compile);
}
