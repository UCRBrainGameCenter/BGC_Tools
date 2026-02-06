using Newtonsoft.Json;
using System;

namespace BGC.Procedures
{
    /// <summary>
    /// Handles JSON serialization/deserialization for procedure states.
    /// Wraps Newtonsoft.Json with settings appropriate for our use case.
    /// </summary>
    public class ProcedureSerializer
    {
        private readonly JsonSerializerSettings _settings;
        private readonly JsonSerializerSettings _prettySettings;

        public ProcedureSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include
            };

            _prettySettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// Serialize an object to compact JSON.
        /// </summary>
        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        /// <summary>
        /// Serialize an object to indented JSON (for logging/debugging).
        /// </summary>
        public string SerializePretty<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _prettySettings);
        }

        /// <summary>
        /// Deserialize JSON to the specified type.
        /// </summary>
        public T Deserialize<T>(string json)
        {
            var result = JsonConvert.DeserializeObject<T>(json, _settings);

            if (result == null)
            {
                throw new ProcedureSerializationException(
                    $"Deserialization returned null for type {typeof(T).Name}");
            }

            return result;
        }

        /// <summary>
        /// Test that a value can be serialized and deserialized without data loss.
        /// Returns true if the round-trip produces an equal value.
        /// </summary>
        public bool TestRoundTrip<T>(T value) where T : ProcedureState
        {
            try
            {
                var json = Serialize(value);
                var restored = Deserialize<T>(json);
                return value.Equals(restored);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Exception thrown when serialization or deserialization fails.
    /// </summary>
    public class ProcedureSerializationException : Exception
    {
        public ProcedureSerializationException(string message) : base(message) { }
        public ProcedureSerializationException(string message, Exception inner) : base(message, inner) { }
    }
}