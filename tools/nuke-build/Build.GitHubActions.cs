using Nuke.Common.CI.GitHubActions;
using Nuke.Components;

[GitHubActions(
    "ci",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    OnPushBranchesIgnore = new[] { DevelopBranch },
    OnPullRequestBranches = new[] { MainBranch },
    PublishArtifacts = false,
    CacheKeyFiles = new[] { "global.json", "source/**/*.csproj" },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) }
    )]
partial class Build
{
    const string MainBranch = "main";
    const string DevelopBranch = "develop";
}
