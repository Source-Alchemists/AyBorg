namespace AyBorg.Data.Mapper;
public interface IPortMapper<T> : IPortMapper
{
    T ToNativeType(object value, Type? type = null);
}
