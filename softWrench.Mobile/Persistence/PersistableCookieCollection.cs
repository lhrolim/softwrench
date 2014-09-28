using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using SQLite;

namespace softWrench.Mobile.Persistence
{
    [Table("CookieCollection")]
    public class PersistableCookieCollection
    {       
        public static PersistableCookieCollection FromCookieCollection(CookieCollection collection)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, collection);

                return new PersistableCookieCollection(stream.GetBuffer());
            }
        }

        private PersistableCookieCollection(byte[] data)
        {
            Id = Guid.NewGuid();
            Data = data;
        }

        public PersistableCookieCollection()
        {
        }

        public CookieCollection ToCookieCollection()
        {
            using (var stream = new MemoryStream(Data))
            {
                var formatter = new BinaryFormatter();
                return (CookieCollection) formatter.Deserialize(stream);
            }
        }

        [PrimaryKey]
        public Guid Id { get; set; }

        [Indexed]
        public byte[] Data { get; set; }
    }
}