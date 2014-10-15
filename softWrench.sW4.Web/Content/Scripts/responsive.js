$(function () {
    //show or hide the menu when the expand button is clicked
    $('.menu-expand').click(function () {
        jQuery(this).toggleClass('menu-open');
    });

    $(window).resize(function () {
        //if the header is fixed to the top of the page, set the location of the content, context menu, grid header and filter bar
        if ($('.site-header').css('position') == 'fixed') {
            var headerHeight = $('.site-header').height();
            var paginationHeight = $('.affix-pagination').height();
            var theaderHeight = $('.listgrid-thead').height();

            $('.content').css('margin-top', headerHeight);
            $('.affix-pagination').css('top', headerHeight);
            $('.listgrid-thead').css('top', headerHeight + paginationHeight + 8);
            $('.listgrid-table').css('margin-top', paginationHeight + theaderHeight +7);
        }
            //reset the lcoation of the content, context menu, grid header and filter bar
        else {
            $('.content').css('margin-top', 'auto');
            $('.affix-pagination').css('top', 'auto');
            $('.listgrid-thead').css('top', 'auto');
            $('.listgrid-table').css('margin-top', 'auto');
        }
    });
});