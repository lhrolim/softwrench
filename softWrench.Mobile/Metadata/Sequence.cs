using System;
using SQLite;

namespace softWrench.Mobile.Metadata {
    public class Sequence {

        [PrimaryKey]
        public Guid LocalId { get; set; }

        public String Application { get; set; }

        public string Field { get; set; }

        public string Mask { get; set; }

        public int Next { get; set; }

        public string Format() {
            return Next == 0
                ? string.Format(Mask, "").Trim()
                : string.Format(Mask, Next);
        }
    }
}
