namespace Steemix.Library.Serializing
{
    public interface IUnmarshaller
    {
        T Process<T>(string response);
    }
}