const express = require("express"),
	  path = require("path");

let expressApp = express();
expressApp.use('/maps', express.static(path.join(__dirname, 'maps')));

var listener = expressApp.listen(5054, () => {
	console.log(`\x1b[34m[INFO]\x1b[0m Server is running on port ${listener.address().port}!`);
});