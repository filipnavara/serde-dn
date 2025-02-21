
using System.Collections.Immutable;

namespace Serde.Json
{
    internal abstract partial record JsonValue : IDeserialize<JsonValue>
    {
        static JsonValue IDeserialize<JsonValue>.Deserialize(IDeserializer deserializer)
        {
            return deserializer.DeserializeAny<JsonValue, Visitor>(new Visitor());
        }

        private sealed class Visitor : IDeserializeVisitor<JsonValue>
        {
            public string ExpectedTypeName => nameof(JsonValue);

            public JsonValue VisitEnumerable<D>(ref D d)
                where D : IDeserializeEnumerable
            {
                var builder = ImmutableArray.CreateBuilder<JsonValue>(d.SizeOpt ?? 3);
                while (d.TryGetNext<JsonValue, JsonValue>(out var next))
                {
                    builder.Add(next);
                }
                return new Array(builder.ToImmutable());
            }

            public JsonValue VisitDictionary<D>(ref D d)
                where D : IDeserializeDictionary
            {
                var builder = ImmutableDictionary.CreateBuilder<string, JsonValue>();
                while (d.TryGetNextEntry<string, JsonValue, StringWrap, JsonValue>(out var next))
                {
                    builder.Add(next.Item1, next.Item2);
                }
                return new Object(builder.ToImmutable());
            }

            public JsonValue VisitBool(bool b) => new Bool(b);
            public JsonValue VisitI64(long i) => new Number(i);
            public JsonValue VisitString(string s) => new String(s);
        }
    }
}