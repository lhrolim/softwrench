using System.Text.RegularExpressions;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Core {
    public class RedisLookupRowstampChunkHash {

        public string RealKey;
        public int Count;

        public long MinUid { get; set; }
        public long MaxUid { get; set; }

        public void CopyKeyIncrementingChunkNumber(RedisLookupRowstampChunkHash lastChunk, int chunkNumber) {
            RealKey = Regex.Replace(lastChunk.RealKey, @"chunk:\d+", @"chunk:" + chunkNumber);
        }

        protected bool Equals(RedisLookupRowstampChunkHash other) {
            return string.Equals(RealKey, other.RealKey);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RedisLookupRowstampChunkHash)obj);
        }

        public override int GetHashCode() {
            return (RealKey != null ? RealKey.GetHashCode() : 0);
        }
    }
}
