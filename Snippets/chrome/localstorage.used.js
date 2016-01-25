(function() {

	if (String.prototype.byteSize !== "function") {
    /**
     * @returns String's size in bytes
     */
    String.prototype.byteSize = function () {
      return encodeURI(this).split(/%(?:u[0-9A-F]{2})?[0-9A-F]{2}|./).length - 1;
    };
	}

	var used = Object.keys(localStorage).map(k => localStorage[k].byteSize()).reduce((accumulated, current) => current + accumulated, 0) / 1024;
	console.log("[localStorage] space used: ", used, "KB");
})();