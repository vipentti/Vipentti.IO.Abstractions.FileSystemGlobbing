using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Components;
using System.IO;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.Git.GitTasks;

partial class Build
{
    public string MajorMinorPatchVersion => GitVersion.MajorMinorPatch;

    [PathVariable("code")]
    readonly Tool VsCode;

    Target IPublish.Publish => _ => _
        .Unlisted()
        .Inherit<IPublish>()
        .Consumes<IPack>(it => it.Pack)
        .Requires(() => IsOriginalRepository && IsMainBranch && Host is GitHubActions && GitHubActions.EventName == "release")
        .Requires(() => GitHubRepositoryOwner)
        .Before(Changelog)
        .Before(CreateRelease)
        .WhenSkipped(DependencyBehavior.Execute);

    Target Changelog => _ => _
        .Unlisted()
        .Requires(() => GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            var changelogFile = AbsolutePath.Create(From<IHazChangelog>().ChangelogFile);

            var tempFile = TemporaryDirectory / (Path.GetRandomFileName() + ".md");
            try
            {
                CopyFile(changelogFile, tempFile);

                FinalizeChangelog(tempFile, MajorMinorPatchVersion, GitRepository);

                Git($"diff --no-index {changelogFile} {tempFile}", logOutput: true, logInvocation: true, exitHandler: (_) => { });

                Serilog.Log.Information("Do you want to view the diff in VsCode (y/n)?");

                if (System.Console.ReadKey(intercept: true).KeyChar == 'y')
                {
                    VsCode(
                        arguments: $"--wait --diff {changelogFile} {tempFile}"
                        , logOutput: true
                        , logInvocation: true
                    );
                }

                Serilog.Log.Information("Preparing changelog {path}. Are you sure you want to continue (y/n)?", changelogFile.Name);
                var yesNo = System.Console.ReadKey(intercept: true).KeyChar;
                Assert.True(yesNo == 'y');

                // Finalize the actual changelog
                FinalizeChangelog(changelogFile, MajorMinorPatchVersion, GitRepository);
                Serilog.Log.Information("Please review {Path} and press any key to continue...", changelogFile);
                System.Console.ReadKey(intercept: true);

                Git($"add {changelogFile}");
                Git($"commit -m \"Finalize {Path.GetFileName(changelogFile)} for {MajorMinorPatchVersion}\"");

                return 0;
            }
            finally
            {
                if (tempFile.Exists())
                {
                    Serilog.Log.Information("Cleaning up {path}", tempFile);
                    tempFile.DeleteFile();
                }
            }
        });

    Target CreateRelease => _ => _
        .Unlisted()
        .DependsOn(Changelog)
        .Requires(() => IsMainBranch)
        .Requires(() => GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            FinalizeRelease();
        });

    private void FinalizeRelease()
    {
        Serilog.Log.Information("Using remote = {Remote}", RemoteName);
        Git($"tag {MajorMinorPatchVersion}");
        Git($"push {RemoteName} {MainBranch} {MajorMinorPatchVersion}");
    }
}
