namespace SettingsManagement.Serialization
{
    using Newtonsoft.Json;
    using System.Text;

    /// <summary>
    /// Represents a serialized based on the Newtonsoft.json library.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NewtonsoftJsonSerializer<T>: ISerializer<T>
    {
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>A byte array representing the serialized object.</returns>
        public byte[] Serialize(T obj)
        {
            var serializerSettings = this.GetDefaultSerializerSettings();
            string jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, serializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
            return bytes;
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <param name="bytes">Byte array representing the serialized object.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize(byte[] bytes)
        {
            var serializerSettings = this.GetDefaultSerializerSettings();
            string jsonString = Encoding.UTF8.GetString(bytes);
            T obj = JsonConvert.DeserializeObject<T>(jsonString, serializerSettings);
            return obj;
        }

        private JsonSerializerSettings GetDefaultSerializerSettings()
        {
            JsonSerializerSettings serializerSettings = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            };

            return serializerSettings;
        }
    }
}