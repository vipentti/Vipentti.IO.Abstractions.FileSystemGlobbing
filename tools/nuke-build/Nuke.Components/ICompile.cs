// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Nuke.Components
{
    [PublicAPI]
    public interface ICompile : IRestore, IHaveConfiguration
    {
        Target Compile => _ => _
            .DependsOn(Restore)
            .WhenSkipped(DependencyBehavior.Skip)
            .Executes(() =>
            {
                ReportSummary(_ => _
                    .WhenNotNull((this as IHaveGitVersion)!, (_, o) => _
                        .AddPair("Version", o.Versioning.FullSemVer))
                    );

                DotNetBuild(_ => _
                    .Apply(CompileSettingsBase)
                    .Apply(CompileSettings));

                DotNetPublish(_ => _
                        .Apply(PublishSettingsBase)
                        .Apply(PublishSettings)
                        .CombineWith(PublishConfigurations, (_, v) => _
                            .SetProject(v.Project)
                            .SetFramework(v.Framework)),
                    PublishDegreeOfParallelism);
            });

        sealed Configure<DotNetBuildSettings> CompileSettingsBase => _ => _
            .SetProjectFile(Solution)
            .SetConfiguration(Configuration)
            .When(IsServerBuild, _ => _
                .EnableContinuousIntegrationBuild())
            .SetNoRestore(SucceededTargets.Contains(Restore))
            .WhenNotNull((this as IHaveGitRepository)!, (_, o) => _
                .SetRepositoryUrl(o.GitRepository.HttpsUrl))
            .WhenNotNull((this as IHaveGitVersion)!, (_, o) => _
                .SetAssemblyVersion(o.Versioning.AssemblySemVer)
                .SetFileVersion(o.Versioning.AssemblySemFileVer)
                .SetInformationalVersion(o.Versioning.InformationalVersion));


        sealed Configure<DotNetPublishSettings> PublishSettingsBase => _ => _
            .SetConfiguration(Configuration)
            .EnableNoBuild()
            .EnableNoLogo()
            .When(IsServerBuild, _ => _
                .EnableContinuousIntegrationBuild())
            .WhenNotNull((this as IHaveGitRepository)!, (_, o) => _
                .SetRepositoryUrl(o.GitRepository.HttpsUrl))
            .WhenNotNull((this as IHaveGitVersion)!, (_, o) => _
                .SetAssemblyVersion(o.Versioning.AssemblySemVer)
                .SetFileVersion(o.Versioning.AssemblySemFileVer)
                .SetInformationalVersion(o.Versioning.InformationalVersion));

        Configure<DotNetBuildSettings> CompileSettings => _ => _;
        Configure<DotNetPublishSettings> PublishSettings => _ => _;

        IEnumerable<(Project Project, string Framework)> PublishConfigurations
            => Array.Empty<(Project Project, string Framework)>();

        int PublishDegreeOfParallelism => 10;
    }
}
