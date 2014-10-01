$(function () {
    //show or hide the menu when the expand button is clicked
    $('.menu-expand').click(function () {
        jQuery(this).toggleClass('menu-open');
    });

    //$(window).on("DOMContentLoaded", function () {
    //    console.log('DOMContentLoaded');
    //});

    $(window).resize(function () {
        //console.log('Resize Window');

        if ($('.site-header').css('position') == 'fixed') {
            var headerHeight = $('.site-header').height();
            var paginationHeight = $('.affix-pagination').height();
            var theaderHeight = $('.listgrid-thead').height();

            $('.content').css('margin-top', headerHeight);
            $('.affix-pagination').css('top', headerHeight);
            $('.listgrid-thead').css('top', headerHeight + paginationHeight);
            $('.listgrid-table').css('margin-top', paginationHeight + theaderHeight - 1);
        }
        else {
            $('.content').css('margin-top', 'auto');
            $('.affix-pagination').css('top', 'auto');
            $('.listgrid-thead').css('top', 'auto');
            $('.listgrid-table').css('margin-top', 'auto');
        }
    });
});