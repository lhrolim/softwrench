using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.integration {
    public interface IErrorDto {

        string WarnMessage {
            get; set;
        }

        string ErrorMessage {
            get; set;
        }
        string ErrorStack {
            get; set;
        }
        string FullStack {
            get; set;
        }

        string ErrorType {
            get;

        }
        string OutlineInformation {
            get; set;
        }

    }
}
