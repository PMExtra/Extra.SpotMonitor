using JetBrains.Annotations;

namespace Extra.SpotMonitor.AliCloud.Models;

public class AliCloudLanguage
{
    /// <summary>
    ///     en-US
    /// </summary>
    public static readonly AliCloudLanguage English = new("en-US", "English");

    /// <summary>
    ///     zh-CN
    /// </summary>
    public static readonly AliCloudLanguage Chinese = new("zh-CN", "简体中文");

    /// <summary>
    ///     ja
    /// </summary>
    public static readonly AliCloudLanguage Japanese = new("ja", "日本語");

    public AliCloudLanguage(string value)
    {
        Value = value;
    }

    public AliCloudLanguage(string value, string name)
    {
        Value = value;
        Name = name;
    }

    [CanBeNull]
    public string Name { get; }

    [NotNull]
    public string Value { get; }

    public static AliCloudLanguage[] GetAllLanguages()
    {
        return new[] { English, Chinese, Japanese };
    }

    public override string ToString() => Value;

    public static implicit operator string(AliCloudLanguage language) => language.Value;

    public static implicit operator AliCloudLanguage(string value) => new(value);
}
