using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Buffer {
    class InMemoryNotificationBuffer {
        private string _roleKey;
        private List<Notification> _notificationList;
        public InMemoryNotificationBuffer(string roleKey) {
            roleKey = roleKey;
        }
    }
}
