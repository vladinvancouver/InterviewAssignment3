function parseIsoDate(dateIsoString) {
	//TODO: error checking
	var parts = dateIsoString.split('T').join('-').split(':').join('-').split('-');
	var date = {
		year: parseInt(parts[0]),
		month: parseInt(parts[1]),
		day: parseInt(parts[2]),
		hour: parseInt(parts[3]),
		minute: parseInt(parts[4]),
		second: parseInt(parts[5]),
	};

	return date;
}

function formatIsoDate(dateIsoString, format) {
	//TODO: add support for other formats
	//

	var date = parseIsoDate(dateIsoString);

	var output = date.year.toString() + '-' + date.month.toString().padStart(2, '0') + '-' + date.day.toString().padStart(2, '0') + ' ';
	if (date.hour == 0) {
		output += '12' + ':' + (date.minute).toString().padStart(2, '0') + ' AM';
	} else if (date.hour == 12) {
		output += '12' + ':' + (date.minute).toString().padStart(2, '0') + ' PM';
	} else if (date.hour > 12) {
		output += (date.hour - 12).toString().padStart(2, '0') + ':' + (date.minute).toString().padStart(2, '0') + ' PM';
	} else {
		output += (date.hour).toString().padStart(2, '0') + ':' + (date.minute).toString().padStart(2, '0') + ' AM';
	}

	return output;
}