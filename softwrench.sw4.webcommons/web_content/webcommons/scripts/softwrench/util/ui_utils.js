﻿$(function () {

    //ensure that english is the current locale for moment.js
    moment.locale('en');

    $(window).resize(function () {
        //if the header is fixed to the top of the page, set the location of the content, context menu, grid header and filter bar
        if ($('.site-header').css('position') == 'fixed') {
            var headerHeight = $('.site-header').height();
            var paginationHeight = $('.affix-pagination').height();
            var theaderHeight = $('.listgrid-thead').height();

            $('.content').css('margin-top', headerHeight);

            //only adjust if table header is fixed 
            if ($('.affix-pagination').css('position') == 'fixed') {
                $('.affix-pagination').css('top', headerHeight);
            }

            //only adjust if table header is fixed
            if ($('.listgrid-thead').css('position') == 'fixed') {
                //move fixed listgrid header up in IE9
                var adjustment = 0;
                if (isIe9()) {
                    adjustment = 135;
                }

                var offsetTop = headerHeight + paginationHeight - adjustment - 1;
                var offsetMargin = paginationHeight + theaderHeight - 1;

                $('.listgrid-thead').css('top', offsetTop);
                $('.listgrid-table').css('margin-top', offsetMargin);
            }
        }
        //reset the lcoation of the content, context menu, grid header and filter bar
        else {
            $('.content').css('margin-top', 'auto');
            $('.affix-pagination').css('top', 'auto');
            $('.listgrid-thead').css('top', 'auto');
            $('.listgrid-table').css('margin-top', 'auto');
        }
    });

    //show or hide the menu when the expand button is clicked
    $('.menu-expand').click(function () {
        $(this).toggleClass('menu-open');
    });
});
