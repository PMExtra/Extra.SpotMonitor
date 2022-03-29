using System;

namespace Extra.SpotMonitor.AliCloud;

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

    public AliCloudLanguage(string id, string? displayName = null)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));
        Id = id;
        DisplayName = displayName;
    }

    public virtual string Id { get; }

    public virtual string? DisplayName { get; }

    public static AliCloudLanguage[] GetAllLanguages()
    {
        return new[] { English, Chinese, Japanese };
    }

    public override string ToString() => Id;

    public static implicit operator string(AliCloudLanguage language) => language.Id;

    public static implicit operator AliCloudLanguage(string value) => new(value);
}
