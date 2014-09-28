$(function () {
    //display/hide the menu when the expand button is clicked
    $('.menu-expand').click(function () {
        jQuery(this).toggleClass('menu-open');
    });

    /*SM - 07/12 - REMOVE - Start remove JS (and fixheader_service.js) code in favor of CSS only*/
    //$(window).resize(function () {
    //    if ($('.site-header').css('position') == 'fixed') {
    //        var headerHeight = $('.site-header').height();
    //        var paginationHeight = $('.affix-pagination').height();
    //        var theaderHeight = $('.listgrid-thead').height();

    //        $('.content').css('margin-top', headerHeight);
    //        $('.affix-pagination').css('top', headerHeight);
    //        $('.listgrid-thead').css('top', headerHeight + paginationHeight);
    //        $('.listgrid-table').css('margin-top', paginationHeight + theaderHeight - 1);
    //    }
    //    else {
    //        $('.content').css('margin-top', 'auto');
    //        $('.affix-pagination').css('top', 'auto');
    //        $('.listgrid-thead').css('top', 'auto');
    //        $('.listgrid-table').css('margin-top', 'auto');
    //    }
    //});
    /*SM - 07/12 - REMOVE - Add remove JS (and fixheader_service.js) code in favor of CSS only*/
});