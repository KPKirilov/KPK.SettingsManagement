namespace SettingsManagement.Serialization
{
    public interface ISerializer<T>
    {
        byte[] Serialize(T obj);
        T Deserialize(byte[] bytes);
    }
}