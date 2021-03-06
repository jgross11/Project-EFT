// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const TYPE_NAME = 0;
const TYPE_EMAIL = 1;
const TYPE_PASSWORD = 2;

/*
Checks if the given information is valid
example: if given an email address, it will check if the string contains an @ followed by a . ...
info: string that contains the information to check
type: the type of information obtained: NAME, EMAIL, ...
*/
function verifyInformation(info, type) {
    // empty / null check
    if (info === null || info.length == 0) {
        return false;
    }

    switch (type) {

        // name check
        case TYPE_NAME:
            for (i = 0; i < info.length; i++) {
                char = info.charAt(i);
                // TODO: find better way to check if character is alphabetic?
                // cheap, only works with latin-based languages...
                if (char.toLowerCase() == char.toUpperCase()) {
                    return false;
                }
            }
            return true;
            break;

        // email check
        case TYPE_EMAIL:
            var foundAt = false;
            for (i = 1; i < info.length; i++) {
                char = info.charAt(i);
                // TODO: find better way to check if email is of form ---@---.---?
                if (char == '@') {
                    foundAt = true;
                    console.log("found @ at position: " + i);
                }
                else if (foundAt && char == '.' && info.charAt(i - 1) != '@' && i != info.length - 1) {
                    return true;
                }
            }
            return false;
            break;
        // password check
        case TYPE_PASSWORD:
            let containsSymbol = false;
            let containsUppercase = false;
            let containsNumber = false;
            if (info.length < 8) return false;
            for (i = 0; i < info.length; i++) {
                char = info.charAt(i);
                if (!containsSymbol && isNaN(char) && char.toLowerCase() == char.toUpperCase()) {
                    containsSymbol = true;
                } else if (!containsNumber && !isNaN(char)) {
                    containsNumber = true;
                // cheap, only works with latin-based languages...
                // good enough for 481, so why not continue the badness?
                //                               true only when char is [latin] letter       true when char is uppercase
                } else if (!containsUppercase && char.toLowerCase() != char.toUpperCase() && char == char.toUpperCase()) {
                    containsUppercase = true;
                }
                if (containsUppercase && containsNumber && containsSymbol) {
                    return true;
                }
            }
            return false;
            break;
    }
}