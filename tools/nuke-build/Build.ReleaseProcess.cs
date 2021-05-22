using Nuke.Common;
using Nuke.Components;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using System.IO;
using Nuke.Common.Git;
using System;

partial class Build
{
    public string MajorMinorPatchVersion => GitVersion.MajorMinorPatch;

    Target Changelog => _ => _
        .Unlisted()
        .Requires(() => GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            var changelogFile = From<IHaveChangeLog>().ChangelogFile;
            FinalizeChangelog(changelogFile, MajorMinorPatchVersion, GitRepository);
            Logger.Info("Please review CHANGELOG.md and press any key to continue...");
            System.Console.ReadKey();

            Git($"add {changelogFile}");
            Git($"commit -m \"Finalize {Path.GetFileName(changelogFile)} for {MajorMinorPatchVersion}\"");
        })
        ;
    Target Release => _ => _
        .Unlisted()
        .DependsOn(Changelog)
        .Requires(() => GitHasCleanWorkingCopy() && IsMainBranch)
        .Executes(() =>
        {
            FinalizeRelease<SimpleRelease>();
        })
        ;

    private void FinalizeRelease<TRelease>()
        where TRelease : IReleaseStrategy, new()
    {
        var release = new TRelease();

        release.Release(GitRepository, MajorMinorPatchVersion);
    }

    private interface IReleaseStrategy
    {
        public void Release(GitRepository repository, string MajorMinorPatchVersion);
    }

    private class SimpleRelease : IReleaseStrategy
    {
        public void Release(GitRepository repository, string MajorMinorPatchVersion)
        {
            if (repository.Branch != MainBranch)
            {
                throw new InvalidOperationException($"Release can only be run from branch '{MainBranch}'");
            }

            Git($"tag {MajorMinorPatchVersion}");
            Git($"push origin {MainBranch} {MajorMinorPatchVersion}");
        }
    }
}
