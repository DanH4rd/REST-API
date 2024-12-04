using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToDoAPI.Models.Validation
{
    /// <summary>
    /// Custom json converter to work with date format "yyyy-MM-ddTHH:mm:ssZ".
    /// </summary>
    public class DateTimeFormatConverter : JsonConverter<DateTime>
    {
        // for simplicity we assume all dates as UTC
        private readonly string _dateFormat = "yyyy-MM-ddTHH:mm:ssZ";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateString = reader.GetString();

            if (DateTime.TryParseExact(dateString, _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime date))
            {
                return date;
            }

            // in case the date format is null or has wrong format we return the minimum possible date value,
            // which in the context of this project smoothly passes the control to the FutureDateAttribute custom validation attribute
            // on the ToDoItem.ExpiryDate validation 
            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_dateFormat, CultureInfo.InvariantCulture));
        }
    }
}
