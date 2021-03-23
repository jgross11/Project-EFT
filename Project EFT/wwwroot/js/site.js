// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const TYPE_NAME = 0;
const TYPE_EMAIL = 1;
const TYPE_PASSWORD = 2;
const TYPE_USERNAME = 3;

const lowers = "qwertyuiopasdfghjklzxcvbnm";
const uppers = "MNBVCXZQWERTYUIOPHJKLFDSAG";
const numbers = "0512864973";
const symbols = ">/?=*@[}^&+.$\|]~-()`#!_%";

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
            if (info.length > 45) {
                return false;
            }
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
            if (info.length < 8 || info.length > 40) return false;
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
        case TYPE_USERNAME:
            return info.length > 0 && info.length < 46;
            break;
    }
}

// generate a password that fits the criteria outlined by verifyInformation.
function generateTemporaryPassword() {
    /*
        let containsSymbol = false;
        let containsUppercase = false;
        let containsNumber = false;
        if (info.length < 8 || info.length > 40) return false;
     */

    // number of each type of character
    let numEach = 4;

    // number of categories of characters
    let numCategoriesRemaining = 4;

    // number of characters remaining for each category
    let numRemaining = [numEach, numEach, numEach, numEach];

    let result = "";

    // array containing arrays of character types
    let chars = [lowers, uppers, numbers, symbols];

    for (let i = 0; i < numEach * 4; i++) {
        let categoryIndex = Math.round(Math.random() * (numCategoriesRemaining - 1));
        result += chars[categoryIndex].charAt(Math.round(Math.random() * (chars[categoryIndex].length - 1)));
        numRemaining[categoryIndex]--;
        if (numRemaining[categoryIndex] == 0) {
            let temp = numRemaining[numCategoriesRemaining - 1];
            numRemaining[numCategoriesRemaining - 1] = numRemaining[categoryIndex];
            numRemaining[categoryIndex] = temp;

            temp = chars[numCategoriesRemaining - 1];
            chars[numCategoriesRemaining - 1] = categoryIndex;
            chars[categoryIndex] = temp;
            numCategoriesRemaining--;
        }
    }
    return result;
}