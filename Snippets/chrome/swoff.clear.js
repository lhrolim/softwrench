(function() {

  var dao = angular.element(document.body).injector().get("swdbDAO");
  dao.dropDataBase()
    .then(() => {
      console.log("database dropped");
      localStorage.clear();
      console.log("localStorage cleared");
      sessionStorage.clear();
      console.log("sessionStorage cleared", "reloading application");
      location.reload();
    })
    .catch(e => console.error("Failed to drop database", e));
        
})();