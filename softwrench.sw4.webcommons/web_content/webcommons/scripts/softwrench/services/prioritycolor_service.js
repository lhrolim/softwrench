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
                return 'transparent';
            }

            //create the default priority ranges
            var priorityRanges = {
                high: '1',
                medium: '2,3',
                low: '4,5'
            }

            //override default ranges from metadata
            if (parameters.high) {
                priorityRanges.high = parameters.high;
            }

            if (parameters.medium) {
                priorityRanges.medium = parameters.medium;
            }

            if (parameters.low) {
                priorityRanges.low = parameters.low;
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