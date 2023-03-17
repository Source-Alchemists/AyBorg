namespace AyBorg.Data.Mapper;
public interface IPortMapper<out T> : IPortMapper
{
    T ToNativeValue(object value, Type? type = null);
}
