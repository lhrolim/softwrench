$(function () {
    var api = $('.vertical-menu .menu-primary').jScrollPane({ maintainPosition: true }).data('jsp');
    var throttleTimeout;

    $(window).bind('resize', function () {
        if (typeof api !== 'undefined') {
            var headerHeight = $('.site-header').height();

            //set the position of the nav bar
            if ($('.site-header').css('position') == 'fixed') {
                $('.vertical-menu').css('margin-top', headerHeight);
            } else {
                $('.vertical-menu').css('margin-top', 0);
            }

            // IE fires multiple resize events while you are dragging the browser window which
            // causes it to crash if you try to update the scrollpane on every one. So we need
            // to throttle it to fire a maximum of once every 50 milliseconds...
            if (!throttleTimeout) {
                throttleTimeout = setTimeout(function () {

                    //set the height of the nav bar
                    if ($('.site-header').css('position') == 'fixed') {
                        $('.vertical-menu .menu-primary').height($(window).height() - headerHeight);
                    } else {
                        $('.vertical-menu .menu-primary').height($(window).height());
                    }

                    api.reinitialise();
                    throttleTimeout = null;
                }, 50);
            }


            var menuWidth = $('.vertical-menu ul[role="menu"]').width();
            var gridPadding = 0;

            //if the header is fixed (desktop), add additional offset
            if ($('.site-header').css('position') == 'fixed') {
                gridPadding = 40;
            }

            var gridOffset = menuWidth + gridPadding;
            var headerOffset = menuWidth;
            var filterOffset = menuWidth + (gridPadding / 2);
            var contentOffset = menuWidth;

            //set the header/pagination position and width
            if ($('.site-header').css('position') !== 'fixed') {
                $('.site-header').width($('.site-header').css('width', 'calc(100% - ' + headerOffset + 'px)'));
                $('.site-header').css('left', headerOffset + 'px');

                $('#affixpagination').width($('#affixpagination').css('width', '100%'));
            } else {
                $('.site-header').width($('.site-header').css('width', '100%'));
                $('.site-header').css('left', '0');

                $('#affixpagination').width($('#affixpagination').css('width', 'calc(100% - ' + gridOffset + 'px)'));
            }

            $('.listgrid-thead').width($('.listgrid-thead').css('width', 'calc(100% - ' + gridOffset + 'px)'));
            $('.listgrid-thead').css('left', filterOffset + 'px');

            $('.content').css('padding-left', contentOffset + 'px');
        }
    });

    //prevent window scrolling after reaching end of navigation pane 
    $(document).on('mousewheel', '.vertical-menu .menu-primary',
      function (e) {
          var delta = e.originalEvent.wheelDelta;
          this.scrollTop += (delta < 0 ? 1 : -1) * 30;
          e.preventDefault();
      });
});