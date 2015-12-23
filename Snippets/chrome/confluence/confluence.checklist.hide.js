(function hideChecked (window) {
    // hides checked list elements
    var hideCheckedListItems = function() {
    	Array.prototype.slice.call(
          document.getElementsByClassName('checked')
        )
        .forEach(function(el) {
          el.style.display = 'none';
        });
    };
		// repeat calls to handle toggling between edit/read mode
		var intervalId = setInterval(hideCheckedListItems, 1000);
		// register global timeout variable so it can be cleared from the outside
		window["confluence:hidecheked:intervalid"] = intervalId;

})(window);