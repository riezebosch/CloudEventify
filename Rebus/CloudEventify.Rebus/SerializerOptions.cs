namespace CloudEventify.Rebus;

public record SerializerOptions
{
    internal string Source { get; private set; } = "cloudeventify:rebus";
    internal SerializerOptions()
    {
    }

    public SerializerOptions WithSource(string source)
    {
        if(string.IsNullOrWhiteSpace(source))
            throw new ArgumentOutOfRangeException(nameof(source), "source is required");

        Source = source;
        return this;
    }
};