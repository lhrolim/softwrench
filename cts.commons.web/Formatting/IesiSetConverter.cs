using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iesi.Collections;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cts.commons.web.Formatting {
    public class IesiSetConverter<T> : JsonConverter {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            serializer.Serialize(writer,value);

//            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            Iesi.Collections.Generic.ISet<T> result = new Iesi.Collections.Generic.HashedSet<T>();

            return serializer.Deserialize< HashedSet<T>>(reader);

//            while (reader.Read()) {
//                if (reader.TokenType == JsonToken.StartArray) {
//                    // Load each object from the stream and do something with it
//                    return serializer.Deserialize<HashedSet<T>>(reader);
//                } else {
//                    return serializer.Deserialize<string>(reader);
//                }
//            }
//            return result;
        }

        public override bool CanConvert(Type objectType) {
            return false;
        }
    }
}
