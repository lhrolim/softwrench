using System;
using System.Collections.Generic;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;

namespace softWrench.Mobile.Applications.MatUseTrans {
    internal class MatUseTransBehavior : ApplicationBehavior {
        public MatUseTransBehavior(IEnumerable<IApplicationCommand> commands)
            : base(commands) {
        }

        /// <summary>
        ///     Ensures the `quantity` field of a given data map
        ///     has the specified sign. If it does not, inverts
        ///     the value.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <param name="signal">The sign to override with (+1 | -1).</param>
        private static void OverrideQuantitySign(DataMap dataMap, int signal) {
            signal = signal < 0 ? -1 : +1;

//            try {
//                var quantity = dataMap.Value<decimal>("quantity");
//                dataMap.Value("quantity", Math.Abs(quantity) * signal);
//            } catch {
//                //POG, because the field name on maximo 7.5
//                var quantity = dataMap.Value<decimal>("qtyrequested");
//                dataMap.Value("qtyrequested", Math.Abs(quantity) * signal);
//            }
        }

        public override void OnNew(OnNewContext context, DataMap dataMap) {
            base.OnNew(context, dataMap);

            dataMap.Value("transdate", DateTime.Now);
            dataMap.Value("refwo", context.Composite.Composite.Value("wonum"));
        }

        public override void OnLoad(OnLoadContext context, DataMap dataMap) {
            base.OnLoad(context, dataMap);

            // Maximo stores material quantities using
            // negative values. Go figure...
            OverrideQuantitySign(dataMap, +1);
        }

        public override void OnBeforeSave(OnBeforeSaveContext context, DataMap dataMap) {
            base.OnBeforeSave(context, dataMap);

            // Maximo stores material quantities using
            // negative values. Go figure...
            OverrideQuantitySign(dataMap, -1);
        }
    }
}
