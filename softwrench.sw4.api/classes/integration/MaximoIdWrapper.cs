using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.integration {

    /// <summary>
    /// to avoid a tuple
    /// </summary>
    public class MaximoIdWrapper {

        public MaximoIdWrapper() {

        }

        public MaximoIdWrapper(string id, string userId) {
            Id = id;
            UserId = userId;
        }

        public string Id { get; set; }
        public string UserId { get; set; }

    }
}
