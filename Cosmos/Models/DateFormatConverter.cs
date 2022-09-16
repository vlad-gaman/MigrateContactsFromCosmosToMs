namespace MigrateContactsFromCosmosToMs.Cosmos.Models
{
    public class DateFormatConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return reader?.Value?.ToString() ?? "";
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var item = (string?)value;

            var dateTime = string.IsNullOrEmpty(item) ? DateTime.UtcNow : DateTime.Parse(item);
            writer.WriteValue(dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            writer.Flush();
        }
    }
}
