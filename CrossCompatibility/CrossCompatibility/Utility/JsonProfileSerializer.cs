using System.Collections.Generic;
using System.IO;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public class JsonProfileSerializer
    {
        private readonly JsonSerializer _serializer;

        public static JsonProfileSerializer Create()
        {
            return Create(Formatting.None);
        }

        public static JsonProfileSerializer Create(Formatting formatting)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = formatting,
                Converters = GetFormatConverters(),
            };

            var serializer = JsonSerializer.Create(settings);

            return new JsonProfileSerializer(serializer);
        }

        internal JsonProfileSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public string Serialize(object data)
        {
            var writer = new StringWriter();
            _serializer.Serialize(writer, data);
            return writer.ToString();
        }

        public void SerializeToFile(object data, string filePath)
        {
            using (var fileStream = File.OpenWrite(filePath))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                _serializer.Serialize(streamWriter, data);
            }
        }

        public CompatibilityProfileData DeserializeFromFile(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream))
            {
                return Deserialize(streamReader);
            }
        }

        public CompatibilityProfileData DeserializeFromFile(FileInfo file)
        {
            return Deserialize(file);
        }

        public CompatibilityProfileData Deserialize(string jsonString)
        {
            using (var stringReader = new StringReader(jsonString))
            {
                return Deserialize(stringReader);
            }
        }

        public CompatibilityProfileData Deserialize(FileInfo file)
        {
            using (var fileStream = file.OpenRead())
            using (var streamReader = new StreamReader(fileStream))
            {
                return Deserialize(streamReader);
            }
        }

        public CompatibilityProfileData Deserialize(TextReader textReader)
        {
            return _serializer.Deserialize<CompatibilityProfileData>(new JsonTextReader(textReader));
        }

        private static IList<JsonConverter> GetFormatConverters()
        {
            return new List<JsonConverter>()
            {
                new VersionConverter(),
                new StringEnumConverter()
            };
        }
    }
}