using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace PersonaEditor.Common.Settings
{
    internal sealed class GridLengthJsonConverter : JsonConverter<GridLength>
    {
        public override GridLength Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            GridUnitType? unitType = null;
            double? value = null;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            ReadProperty(ref reader, ref unitType, ref value);

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            ReadProperty(ref reader, ref unitType, ref value);

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException();

            if (!unitType.HasValue || !value.HasValue)
                throw new JsonException();

            return new GridLength(value.Value, unitType.Value);
        }

        private static void ReadProperty(ref Utf8JsonReader reader, ref GridUnitType? unitType, ref double? value)
        {
            string propertyName = reader.GetString();
            if (propertyName == "GridUnitType")
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.String)
                    throw new JsonException();

                var unitTypeString = reader.GetString();
                unitType = Enum.Parse<GridUnitType>(unitTypeString);
            }
            else if (propertyName == "Value")
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                    throw new JsonException();

                value = reader.GetDouble();
            }
        }

        public override void Write(Utf8JsonWriter writer, GridLength value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var unitType = Enum.GetName<GridUnitType>(value.GridUnitType);
            writer.WriteString("GridUnitType", unitType);
            writer.WriteNumber("Value", value.Value);

            writer.WriteEndObject();
        }
    }
}
