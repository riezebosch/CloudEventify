namespace CloudEventify;

public record Map<T>(string Type, Func<T, string?> Subject);
public record Map(string Type, Func<object, string?> Subject);

