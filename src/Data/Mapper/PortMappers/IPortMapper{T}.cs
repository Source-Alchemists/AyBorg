namespace AyBorg.Data.Mapper;
public interface IPortMapper<T> : IPortMapper
{
    T ToNativeValue(object value, Type? type = null);
}
