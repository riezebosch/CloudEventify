namespace CloudEventify;

public interface IMap<in TFrom, out TTo>
{
    TTo this[TFrom type] { get; }
}

public interface ITryMap<in TFrom, TTo>
{
    bool TryGet(TFrom from, out TTo to);
}