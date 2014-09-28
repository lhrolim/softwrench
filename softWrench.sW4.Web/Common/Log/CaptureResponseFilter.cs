using System.IO;
using System.Text;

namespace softWrench.sW4.Web.Common.Log {
    class CapturingResponseFilter : Stream {
        private Stream _sink;
        private MemoryStream mem;

        public LogInfo AccessLogItem { get; set; }

        public CapturingResponseFilter(Stream sink, LogInfo item) {
            _sink = sink;
            AccessLogItem = item;
            mem = new MemoryStream();
        }

        public override bool CanRead {
            get { return _sink.CanRead; }
        }

        public override bool CanSeek {
            get { return _sink.CanSeek; }
        }

        public override bool CanWrite {
            get { return _sink.CanWrite; }
        }

        public override long Length {
            get { return _sink.Length; }
        }

        public override long Position {
            get {
                return _sink.Position;
            }
            set {
                _sink.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin direction) {
            return _sink.Seek(offset, direction);
        }

        public override void SetLength(long length) {
            _sink.SetLength(length);
        }

        public override void Close() {
            _sink.Close();
            mem.Close();
        }

        public override void Flush() {
            _sink.Flush();

            AccessLogItem.ResponseBody = GetContents(new UTF8Encoding(false));
            //YOU CAN STORE YOUR DATA TO YOUR DATABASE HERE
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _sink.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            mem.Write(buffer, 0, count);
            _sink.Write(buffer, offset, count);
        }

        public string GetContents(Encoding enc) {
            var buffer = new byte[mem.Length];
            mem.Position = 0;
            mem.Read(buffer, 0, buffer.Length);
            return enc.GetString(buffer, 0, buffer.Length);
        }
    }
}