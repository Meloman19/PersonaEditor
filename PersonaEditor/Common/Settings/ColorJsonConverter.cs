using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace PersonaEditor.Common.Settings
{
    internal sealed class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            byte?[] argb = new byte?[4];

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            for (int i = 0; i < argb.Length; i++)
            {
                ReadProperty(ref reader, argb);
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException();

            if (argb.Any(x => !x.HasValue))
                throw new JsonException();

            return Color.FromArgb(argb[0].Value, argb[1].Value, argb[2].Value, argb[3].Value);
        }

        private static void ReadProperty(ref Utf8JsonReader reader, byte?[] argb)
        {
            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            int index = -1;
            string propertyName = reader.GetString();
            switch (propertyName)
            {
                case "A":
                    index = 0;
                    break;
                case "R":
                    index = 1;
                    break;
                case "G":
                    index = 2;
                    break;
                case "B":
                    index = 3;
                    break;
                default:
                    throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException();

            argb[index] = reader.GetByte();
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("A", value.A);
            writer.WriteNumber("R", value.R);
            writer.WriteNumber("G", value.G);
            writer.WriteNumber("B", value.B);

            writer.WriteEndObject();
        }
    }
}
