(function showChecked(window) {
    // clear interval from that hides the checked items
    clearInterval(window["confluence:hidecheked:intervalid"]);
    
    // show the checked items
    Array.prototype.slice.call(
      document.getElementsByClassName('checked')
    )
    .forEach(function(el) {
      el.style.display = 'initial';
    });

})(window);