// Copyright 2021-2023 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using Nuke.Common;
using Nuke.Common.Execution;

[DisableDefaultOutput<Terminal>(
    DefaultOutput.Logo
    //DefaultOutput.Timestamps,
    //DefaultOutput.TargetHeader,
    //DefaultOutput.ErrorsAndWarnings,
    //DefaultOutput.TargetOutcome,
    //DefaultOutput.BuildOutcome
    )]
public partial class Build
{
}

public class DisableDefaultOutputAttribute<T> : DisableDefaultOutputAttribute
    where T : Host
{
    public DisableDefaultOutputAttribute(params DefaultOutput[] disabledOutputs)
        : base(disabledOutputs)
    {
    }

    public override bool IsApplicable(INukeBuild build) => build.Host is T;
}

