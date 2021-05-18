namespace SettingsManagement.Serialization
{
    using Newtonsoft.Json;
    using System.Text;

    public class NewtonsoftJsonSerializer<T>: ISerializer<T>
    {
        public byte[] Serialize(T obj)
        {
            var serializerSettings = this.GetDefaultSerializerSettings();
            string jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, serializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
            return bytes;
        }

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