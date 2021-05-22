// Copyright 2021 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using JetBrains.Annotations;
using Nuke.Common.IO;

namespace Nuke.Components
{
    [PublicAPI]
    public interface IHaveReports : IHaveArtifacts
    {
        AbsolutePath ReportDirectory => ArtifactsDirectory / "reports";
    }
}
