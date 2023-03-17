function escapeHtml(unsafe) {
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/"/g, "&quot;")
         .replace(/'/g, "&#039;");
}

function bytesToSize(bytes) {
    let sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    if (bytes == 0) return '0 Byte';
    let i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
    return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
};

function makeId(length) {
    let text = "";
    let possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    for (let i = 0; i < length; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));

    return text;
}

function humanizeDuration(durationString) {
    let parts = durationString.split(':');
    let days = 0;
    let hours = 0;
    let minutes = 0;
    let seconds = 0;

    if (parts.length > 4) {
        return '?';
    }

    if (parts.length < 3) {
        return '?';
    }

    if (parts.length == 4) {
        days = Number.parseInt(parts[0]);
        hours = Number.parseInt(parts[1]);
        minutes = Number.parseInt(parts[2]);
        seconds = Math.round(parseFloat(parts[3]));
    } else {
        hours = Number.parseInt(parts[0]);
        minutes = Number.parseInt(parts[1]);
        seconds = Math.round(parseFloat(parts[2]));
    }

    let result = '';
    if (days > 0) {
        result += days + 'd';
    }

    if (days > 0 || hours > 0) {
        result += ' ' + hours + 'h';
    }

    if (days > 0 || hours > 0 || minutes > 0) {
        result += ' ' + minutes + 'm';
    }

    if (days > 0 || hours > 0 || minutes > 0 || seconds > 0) {
        result += ' ' + seconds + 's';
    }

    return result;
}

function isInt(value) {
    var x;
    if (isNaN(value)) {
        return false;
    }
    x = parseFloat(value);
    return (x | 0) === x;
}