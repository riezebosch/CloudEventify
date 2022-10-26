namespace CloudEventify;

public record TypeMap<T>(string Type, Func<T, string?> Subject);
public record TypeMap(string Type, Func<object, string?> Subject);

