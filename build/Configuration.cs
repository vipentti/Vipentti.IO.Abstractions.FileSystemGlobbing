// Copyright 2021-2023 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using Nuke.Common.Tooling;
using System.ComponentModel;

[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    public static Configuration Debug = new Configuration { Value = nameof(Debug) };
    public static Configuration Release = new Configuration { Value = nameof(Release) };

    public static implicit operator string(Configuration configuration)
    {
        return configuration.Value;
    }
}
