// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using static Nuke.Common.ValueInjection.ValueInjectionUtility;

namespace Nuke.Components
{
    [PublicAPI]
    public interface IHaveSolution : INukeBuild
    {
        [Solution] [Required] Solution Solution => TryGetValue(() => Solution);
    }
}
