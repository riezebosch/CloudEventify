namespace CloudEventify;

public interface ITypeMap<in TFrom, out TTo>
{
    TTo this[TFrom type] { get; }
}