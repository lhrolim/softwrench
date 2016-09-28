$(function () {

    if (typeof jScrollPane === 'undefined') {
        return;
    }

    var api = $('.menu-primary').jScrollPane({ maintainPosition: true }).data('jsp');
    var throttleTimeout;

    $(window).bind(
        'resize',
        function () {
            // IE fires multiple resize events while you are dragging the browser window which
            // causes it to crash if you try to update the scrollpane on every one. So we need
            // to throttle it to fire a maximum of once every 50 milliseconds...
            if (typeof api !== 'undefined') {
                if (!throttleTimeout) {
                    throttleTimeout = setTimeout(
                        function () {

                            //HAP-876 - resize the nav, to make sure it is scrollable
                            $('.menu-primary').height($(window).height());

                            api.reinitialise();
                            throttleTimeout = null;
                        },
                        50
                    );
                }
            }
        }
    );

    //prevent window scrolling after reaching end of navigation pane 
    $(document).on('mousewheel', '.menu-primary',
      function (e) {
          var delta = e.originalEvent.wheelDelta;
          this.scrollTop += (delta < 0 ? 1 : -1) * 30;
          e.preventDefault();
      });
});