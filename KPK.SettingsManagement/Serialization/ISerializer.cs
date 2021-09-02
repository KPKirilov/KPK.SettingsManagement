namespace KPK.SettingsManagement.Serialization
{
    /// <summary>
    /// Defines members for a serializer.
    /// </summary>
    /// <typeparam name="T">The type the serializer is for.</typeparam>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>A byte array representing the serialized object.</returns>
        byte[] Serialize(T obj);

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <param name="bytes">Byte array representing the serialized object.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize(byte[] bytes);
    }
}