(function() {

	var total = Array.prototype.slice.call(document.querySelectorAll("li[data-inline-task-id]")).length;
	var checked = Array.prototype.slice.call(document.querySelectorAll("li[data-inline-task-id].checked")).length
	var remaining = total - checked;
	var completionRate = ((checked / total) * 100);

	var msg = "** CHECK LIST STATS **\n" + 
	"total items: " + total + "\n" + 
	"solved items: " + checked + "\n" + 
	"remaining items: " + remaining + "\n" + 
	"completion rate: " + completionRate + "%\n"
	
	// change for alert if you want a more 'in your face' experience
	console.log(msg);

})();