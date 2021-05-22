// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using JetBrains.Annotations;
using Nuke.Common;
using static Nuke.Common.ChangeLog.ChangelogTasks;

namespace Nuke.Components
{
    [PublicAPI]
    public interface IHaveChangeLog : INukeBuild
    {
        // TODO: assert file exists
        string ChangelogFile => RootDirectory / "CHANGELOG.md";
        string NuGetReleaseNotes => GetNuGetReleaseNotes(ChangelogFile, (this as IHaveGitRepository)?.GitRepository);
    }
}
