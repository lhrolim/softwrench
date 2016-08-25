using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.services {

    public class ToshibaAuthUtils : ISingletonComponent{


        //TODO: findout what to with the person
        public virtual string TranslatePersonId(string userName) {
            var personid = userName.ToUpper();
            return personid;
        }

    }
}
