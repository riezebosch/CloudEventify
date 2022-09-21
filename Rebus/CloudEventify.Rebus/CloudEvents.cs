namespace CloudEventify.Rebus;

public interface CloudEvents : Types<CloudEvents>, JsonOptions<CloudEvents>
{
    CloudEvents WithSource(Uri source);
}