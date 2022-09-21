namespace CloudEventify.Rebus;

public interface ICloudEvents : IConfigure<ICloudEvents>
{
    /// <summary>
    /// Configure the Wrapper to use this Uri as the source on each outgoing message
    /// </summary>
    /// <param name="source">Source <see cref="Uri"/> used on each outgoing message</param>
    /// <returns></returns>
    ICloudEvents WithSource(Uri source);
}