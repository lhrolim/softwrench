﻿using cts.commons.portable.Util;
using softWrench.sW4.Data.Pagination;
using cts.commons.simpleinjector;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {
    class WoDataSetBatchHelper :ISingletonComponent {

        public static void AppendParametersIfNeeded(PaginatedSearchRequestDto searchDto) {
            if (searchDto.ValuesDictionary == null) {
                //let´s fill this only for the first call
                var endToday = DateUtil.EndOfToday();
                var beginDate = DateUtil.ParsePastAndFuture("14days", -1);
                searchDto.AppendSearchEntry("schedstart", ">=" + beginDate.ToShortDateString());
                searchDto.AppendSearchEntry("schedfinish", "<=" + endToday.ToShortDateString());
            }
        }

    }
}