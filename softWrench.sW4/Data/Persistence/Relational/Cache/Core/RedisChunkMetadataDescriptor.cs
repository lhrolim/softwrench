using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Core {

    public class RedisChunkMetadataDescriptor {

        public IList<RedisLookupRowstampChunkHash> Chunks { get; set; } = new List<RedisLookupRowstampChunkHash>();
        public long MaxRowstamp { get; set; }

        protected bool Equals(RedisChunkMetadataDescriptor other) {
            return MaxRowstamp == other.MaxRowstamp && Chunks.SequenceEqual(other.Chunks);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RedisChunkMetadataDescriptor)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (MaxRowstamp.GetHashCode() * 397) ^ (Chunks != null ? Chunks.GetHashCode() : 0);
            }
        }
    }
}
