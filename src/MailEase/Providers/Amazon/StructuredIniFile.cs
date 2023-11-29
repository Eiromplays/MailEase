using System.Text;

namespace MailEase.Providers.Amazon;

internal abstract class IniEntity;

internal class IniKeyValue(string key, string value, string? comment) : IniEntity
{
    public const string KeyValueSeparator = "=";

    public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));

    public string Value { get; set; } = value;

    public IniComment? Comment { get; } = comment == null ? null : new IniComment(comment);

    public static IniKeyValue? FromLine(string line, bool parseInlineComments)
    {
        var idx = line.IndexOf(KeyValueSeparator, StringComparison.CurrentCulture);
        if(idx == -1)
            return null;

        var key = line[..idx].Trim();
        var value = line[(idx + 1)..].Trim();
        string? comment = null;

        if (!parseInlineComments) return new IniKeyValue(key, value, comment);
        
        idx = value.LastIndexOf(IniComment.CommentSeparator, StringComparison.CurrentCulture);
        if (idx == -1) return new IniKeyValue(key, value, comment);
        
        comment = value[(idx + 1)..].Trim();
        value = value[..idx].Trim();

        return new IniKeyValue(key, value, comment);
    }

    public override string ToString()
    {
        return $"{Value}";
    }
}

internal class IniComment(string value) : IniEntity
{
    public const string CommentSeparator = ";";

    public string Value { get; set; } = value;

    public override string ToString() => Value;
}

internal class IniSection
{
    public const string SectionKeySeparator = ".";

    private readonly List<IniEntity> _entities = [];
    private readonly Dictionary<string, IniKeyValue> _keyToValue = new();

    /// <summary>
    /// Section name. Null name indicates global section (or no section, depending on the context)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name">Pass null to work with global section</param>
    public IniSection(string? name)
    {
        if (name != null)
        {
            if (name.StartsWith('['))
                name = name[1..];
            if (name.EndsWith(']'))
                name = name[..^1];
        }

        Name = name;
    }

    public void Add(IniEntity entity)
    {
        _entities.Add(entity);

        if (entity is IniKeyValue ikv)
        {
            _keyToValue[ikv.Key] = ikv;
        }
    }

    /// <summary>
    /// Get key names in this section
    /// </summary>
    public string[] Keys => _keyToValue.Select(p => p.Key).ToArray();

    public IniKeyValue? Set(string key, string? value)
    {
        IniKeyValue? ikv;
        if (value is null)
        {
            return _keyToValue.Remove(key, out ikv) ? ikv : null;
        }

        
        if (_keyToValue.TryGetValue(key, out ikv))
        {
            ikv.Value = value;
        }
        else
        {
            ikv = new IniKeyValue(key, value, null);
            Add(ikv);
        }
        
        return ikv;
    }

    public static void SplitKey(string fullKey, out string? sectionName, out string keyName)
    {
        var idx = fullKey.IndexOf(SectionKeySeparator, StringComparison.CurrentCulture);

        if (idx == -1)
        {
            sectionName = null;
            keyName = fullKey;
        }
        else
        {
            sectionName = fullKey[..idx];
            keyName = fullKey[(idx + 1)..];
        }
    }

    public void WriteTo(StreamWriter writer)
    {
        foreach (var entity in _entities)
        {
            switch (entity)
            {
                case IniKeyValue ikv:
                {
                    writer.Write($"{ikv.Key}{IniKeyValue.KeyValueSeparator}{ikv.Value}");
                    if (ikv.Comment is not null)
                    {
                        writer.Write(" ");
                        writer.Write(IniComment.CommentSeparator);
                        writer.Write(ikv.Comment.Value);
                    }
                    writer.WriteLine();
                    continue;
                }
                case IniComment comment:
                    writer.Write(IniComment.CommentSeparator);
                    writer.WriteLine(comment.Value);
                    break;
            }
        }
    }

    public override string ToString()
    {
        return Name ?? string.Empty;
    }
}

internal class StructuredIniFile
{
    private const string SectionBegin = "[";
    private const string SectionEnd = "]";

    private readonly IniSection _globalSection;
    private readonly List<IniSection> _sections = [];
    private readonly Dictionary<string, IniKeyValue> _fullKeyNameToValue = new(StringComparer.InvariantCultureIgnoreCase);

    public StructuredIniFile()
    {
        _globalSection = new IniSection(null);
        _sections.Add(_globalSection);
    }

    public string[] SectionNames =>
        _sections.Where(s => s.Name is not null).Select(s => s.Name!).ToArray();

    public string[]? GetSectionKeys(string sectionName)
    {
        var section = _sections.FirstOrDefault(s => s.Name == sectionName);
        return section?.Keys;
    }

    public string? this[string? key]
    {
        get
        {
            if (key is null)
                return null;

            return !_fullKeyNameToValue.TryGetValue(key, out IniKeyValue? value)
                ? null
                : value.Value;
        }
        set
        {
            if (key is null)
                return;

            IniSection.SplitKey(key, out var sectionName, out var keyName);
            var section =
                sectionName == null
                    ? _globalSection
                    : _sections.FirstOrDefault(s => s.Name == sectionName);
            if (section is null)
            {
                section = new IniSection(sectionName);
                _sections.Add(section);
            }
            var ikv = section.Set(keyName, value);

            //update the local cache
            if (ikv is null) return;
            
            if (value is null)
            {
                _fullKeyNameToValue.Remove(key);
            }
            else
            {
                _fullKeyNameToValue[key] = ikv;
            }
        }
    }

    public static StructuredIniFile FromString(string content, bool parseInlineComments = true)
    {
        using Stream input = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return FromStream(input, parseInlineComments);
    }

    public static StructuredIniFile FromStream(Stream inputStream, bool parseInlineComments = true)
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        var file = new StructuredIniFile();

        using var reader = new StreamReader(inputStream);
        
        var section = file._globalSection;

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();

            if (line.StartsWith(SectionBegin))
            {
                //start new section
                line = line.Trim();
                section = new IniSection(line);
                file._sections.Add(section);
            }
            else if (line.StartsWith(IniComment.CommentSeparator))
            {
                //whole line is a comment
                var comment = line[1..].Trim();
                section.Add(new IniComment(comment));
            }
            else
            {
                var ikv = IniKeyValue.FromLine(line, parseInlineComments);
                if (ikv is null)
                    continue;

                section.Add(ikv);
                var fullKey =
                    section.Name == null
                        ? ikv.Key
                        : $"{section.Name}{IniSection.SectionKeySeparator}{ikv.Key}";
                file._fullKeyNameToValue[fullKey] = ikv;
            }
        }

        return file;
    }

    public void WriteTo(Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(outputStream);

        using var writer = new StreamWriter(outputStream);
        foreach (var section in _sections)
        {
            if (section.Name is not null)
            {
                writer.WriteLine();
                writer.WriteLine($"{SectionBegin}{section.Name}{SectionEnd}");
            }

            section.WriteTo(writer);
        }
    }
}
