namespace CloudEventify;

public interface IMap<in TFrom, out TTo>
{
    TTo this[TFrom type] { get; }
}