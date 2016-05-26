(function(modules) {
    "use strict";

    modules.webcommons.factory('prioritycolorService', [function () {
        var fallbackFunction = function (priority) {
            var color;

            switch (priority) {
                case 'high':
                    color = '#f65752'; //red
                    break;
                case 'medium':
                    color = '#e59323'; //orange
                    break;
                case 'low':
                    color = '#39b54a'; //green
                    break;
                default:
                    color = '#808080'; //gray
            }

            return color;
        };

    return {
        getColor: function (value, parameters) {
            if (!value) {
                return '#ccc';
            }

            //create the default priority ranges
            var priorityRanges = {
                high: '1',
                medium: '2',
                low: '3'
            }

            //override default ranges from metadata
            if (parameters.highpriority) {
                priorityRanges.high = parameters.highpriority;
            }

            if (parameters.mediumpriority) {
                priorityRanges.medium = parameters.mediumpriority;
            }

            if (parameters.lowpriority) {
                priorityRanges.low = parameters.lowpriority;
            }

            //find the priority range
            var priority = $.map(priorityRanges, function (values, key) {
                if (values.indexOf(value) >= 0) {
                    return key;
                }
            });

            return fallbackFunction(priority[0]);
        }
    };
}]);

})(modules);