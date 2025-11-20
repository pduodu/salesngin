// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function showRequiredToastr(message, title) {

    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toastr-top-right",
        "preventDuplicates": true,
        "showDuration": "600",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    toastr.error(message, title);
}

function showSuccessToastr(message, title) {

    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toastr-top-right",
        "preventDuplicates": true,
        "showDuration": "600",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    toastr.success(message, title);
}

function toggleCheckBox(source, checkGroupName) {
    checkboxes = document.getElementsByClassName(checkGroupName);
    for (var i = 0, n = checkboxes.length; i < n; i++) {
        checkboxes[i].checked = source.checked;
        if (source.checked) {
            checkboxes[i].value = 'true';
        } else {
            checkboxes[i].value = 'false';
        }
    }
}

function roleClick(element) {
    if (element.checked) {
        $(element).attr("value", "true");
    } else {
        $(element).attr("value", "false");
    }
}

function isVisible(elementId) {
    if ($(elementId).is(":visible")) {
        return true;
    } else {
        return false;
    }
}

function addDivValidatorClass(elementId) {
    var element = document.getElementById(elementId);
    if (!element.classList.contains('borderInvalid')) {
        element.classList.add('borderInvalid');
    }
}

function removeDivValidatorClass(elementId) {
    var element = document.getElementById(elementId);
    if (element.classList.contains('borderInvalid')) {
        element.classList.remove('borderInvalid');
    }
}

function addValidatorClass(elementId) {
    var element = document.getElementById(elementId);
    if (!element.classList.contains('is-invalid')) {
        element.classList.add('is-invalid');
    }
}

function removeValidatorClass(elementId) {
    var element = document.getElementById(elementId);
    if (element.classList.contains('is-invalid')) {
        element.classList.remove('is-invalid');
    }
}

function requiredDropdown(selectName) {
    var select = document.getElementById(selectName);
    var selectedValue = select.options[select.selectedIndex].value;
    if (selectedValue === "" || selectedValue == "0") {
        return false;
    }
    else {
        return true;
    }
}

function passwordCompare(password, confirmPass) {
    var pw1 = document.getElementById(password).value;
    var pw2 = document.getElementById(confirmPass).value;
    if (pw1 == "" || pw1 == null || pw2 == "" || pw2 == null) {
        return false;
    }
    else {
        if (pw1 == pw2) {
            return true;
        }
        else { return false; }
    }

}

function requiredRadio(radioName) {
    if ($('input[name=' + radioName + ']:checked').length) {
        // at least one of the radio buttons was checked
        return true; // allow whatever action would normally happen to continue
    }
    else {
        // no radio button was checked
        return false; // stop whatever action would normally happen
    }
}

function requiredInput(inputtx) {

    var inputValue = document.getElementById(inputtx).value;
    if (inputValue == null || inputValue == "") {
        return false;
    }
    else {
        return true;
    }
}
