using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Nuke.Components;
using System.Collections.Generic;

[GitHubActions(
    "ubuntu-latest",
    GitHubActionsImage.UbuntuLatest,
    OnPullRequestBranches = new[] { MainBranch, DevelopBranch },
    PublishArtifacts = false,
    FetchDepth = 0, // fetch full history
    InvokedTargets = new[] { nameof(ITest.Test), nameof(ValidateFormat), nameof(ValidatePackages) }
    )]
[GitHubActions(
    "windows-latest",
    GitHubActionsImage.WindowsLatest,
    OnPullRequestBranches = new[] { MainBranch, DevelopBranch },
    PublishArtifacts = false,
    FetchDepth = 0, // fetch full history
    InvokedTargets = new[] { nameof(ITest.Test), nameof(ValidateFormat), nameof(ValidatePackages) }
    )]
[GitHubActions(
    "macos-latest",
    GitHubActionsImage.MacOsLatest,
    OnPullRequestBranches = new[] { MainBranch, DevelopBranch },
    PublishArtifacts = false,
    FetchDepth = 0, // fetch full history
    InvokedTargets = new[] { nameof(ITest.Test), nameof(ValidateFormat), nameof(ValidatePackages) }
    )]
[CustomGithubActions(
    "publish",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.MacOsLatest,
    OnPushBranches = new[] { MainBranch },
    OnPushTags = new[] { "*.*.*" },
    PublishArtifacts = true,
    PublishCondition = "${{ runner.os == 'Windows' }}",
    EmptyWorkflowTrigger = true,
    FetchDepth = 0, // fetch full history
    InvokedTargets = new[] { nameof(ITest.Test), nameof(ValidateFormat), nameof(IPack.Pack), nameof(ValidatePackages) }
    )]
public partial class Build
{
}

class CustomGithubActionsAttribute : GitHubActionsAttribute
{
    public CustomGithubActionsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images)
    {
    }

    public bool EmptyWorkflowTrigger { get; set; }

    protected override IEnumerable<GitHubActionsDetailedTrigger> GetTriggers()
    {
        foreach (var trigger in base.GetTriggers())
        {
            yield return trigger;
        }

        if (EmptyWorkflowTrigger)
        {
            yield return new EmptyGitHubActionsWorkflowDispatchTrigger();
        }
    }

    class EmptyGitHubActionsWorkflowDispatchTrigger : GitHubActionsDetailedTrigger
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("workflow_dispatch: # Allow running the workflow manually from the GitHub UI");
        }
    }
}
