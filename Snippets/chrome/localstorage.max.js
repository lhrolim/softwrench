(function() {
	if (String.prototype.byteSize !== "function") {
    /**
     * @returns String's size in bytes
     */
    String.prototype.byteSize = function () {
      return encodeURI(this).split(/%(?:u[0-9A-F]{2})?[0-9A-F]{2}|./).length - 1;
    };
	}

	for (var i = 0, data = "m"; i < 40; i++) {
    try { 
      localStorage.setItem("DATA", data);
      data = data + data;
    } catch(e) {
      var storageSize = JSON.stringify(localStorage).byteSize() / 1024;
      console.log("[localStorage] total space: ", storageSize, "KB");
      console.log(e);
      break;
    }
	}
	localStorage.removeItem("DATA");
})();