$(function () {

    //ensure that english is the current locale for moment.js
    moment.locale('en');

    function setHeaderPosition () {
        //console.log('resize');
        var siteHeaderElements = $('.site-header');
        var affixPaginationElements = $('.affix-pagination');
        var listTheadElement = $('.listgrid-thead');

        if (siteHeaderElements.css('position') === 'fixed') {
            //if the header is fixed to the top of the page, set the location of the content, context menu, grid header and filter bar
            var headerHeight = siteHeaderElements.height();
            var paginationHeight = affixPaginationElements.height();
            var theaderHeight = listTheadElement.height();
            var offsetMargin = paginationHeight + theaderHeight - 1;

            $('.content').css('margin-top', headerHeight);

            //only adjust if toolbar is fixed 
            if (affixPaginationElements.css('position') === 'fixed') {
                affixPaginationElements.css('top', headerHeight);
                $('#crudbodyform:not([data-modal="true"])').css('margin-top', offsetMargin);
            }

            //only adjust if table header is fixed
            if (listTheadElement.css('position') === 'fixed') {
                //move fixed listgrid header up in IE9
                var adjustment = 0;
                if (isIe9()) {
                    adjustment = 135;
                }

                var offsetTop = headerHeight + paginationHeight - adjustment - 1;

                listTheadElement.css('top', offsetTop);
                $('.listgrid-table').css('margin-top', offsetMargin);
            }
        } else {
            //reset the lcoation of the content, context menu, grid header and filter bar
            $('.content').css('margin-top', 'auto');
            affixPaginationElements.css('top', 'auto');
            listTheadElement.css('top', 'auto');
            $('.listgrid-table').css('margin-top', 'auto');
        }
    };

    function setFooterPosition () {
        //make sure the height includes the footer
        $('.site-footer').css('position', 'static');

        var containerHeight = $('[ng-controller="LayoutController"]').height();
        var windowHeight = $(window).height();

        //adjust footer position
        if (containerHeight > windowHeight) {
            $('.site-footer').css('position', 'static');
        } else {
            $('.site-footer').css('position', 'absolute');
        }
    };

    setHeaderPosition();
    setFooterPosition();

    //register layout functions, debounced to stop repeated calls while resizing the browser window
    $(window).resize(window.debounce(setHeaderPosition, 300));
    $(window).resize(window.debounce(setFooterPosition, 300));

    //show or hide the menu when the expand button is clicked
    $('.menu-expand').click(function () {
        $(this).toggleClass('menu-open');
    });
});
