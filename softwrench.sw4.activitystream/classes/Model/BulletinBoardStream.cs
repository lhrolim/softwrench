using System.Collections.Generic;

namespace softwrench.sw4.activitystream.classes.Model {
    public class BulletinBoardStream {

        private List<BulletinBoard> _stream = new List<BulletinBoard>();
        // using old-fashioned lock to controll concurrent access (no ConcurentList in .NET)
        private readonly object _lock = new object();

        public List<BulletinBoard> Stream {
            get {
                lock (_lock) {
                    return _stream;
                }
            }
            set {
                lock (_lock) {
                    _stream = value;
                }
            }
        }
    }
}