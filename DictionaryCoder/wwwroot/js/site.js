var selectedFiles;
var validEncodeExtensions = [".TXT", ".JPG", ".JPEG", ".PNG"]
var validDecodeExtensions = [".LZ77"]

function OnDragEnter(e) {
    e.stopPropagation();
    e.preventDefault();
}

function OnDragOver(e) {
    e.stopPropagation();
    e.preventDefault();
}

function OnDrop(e) {
    e.stopPropagation();
    e.preventDefault();

    selectedFiles = e.dataTransfer.files;
    e.dataTransfer.clearData();

    let isValid = false;
    var message;
    const isEncode = this == document.getElementById("encode-drag-file");

    for (var file in selectedFiles) console.log('file: ' + file)
    console.log('files: ' + selectedFiles)
    console.log('lken: ' + selectedFiles.length)

    if (selectedFiles.length > 1) {
        isValid = false;
        const action = isEncode ? 'encode' : 'decode';
        message = 'You can only ' + action + ' one file at a time!';
    } else {
        const name = selectedFiles[0].name;
        const extensions = isEncode ? validEncodeExtensions : validDecodeExtensions;

        const successMesage = name + " selected for encoding!";
        let errorMesage = "Invalid extension of " + name + "\nAccepted extensions: ";

        for (var index in extensions) {
            errorMesage += extensions[index]
            if (index != extensions.length - 1)
                errorMesage += ', '
            else errorMesage += '.'
        }

        isValid = ExtensionIsValid(name, extensions);
        message = isValid ? successMesage : errorMesage;
    }

    DisplayMessage(this, message, isValid);
}

function DisplayMessage(divId, message, isValid) {
    const color = isValid ? '#8fdb56' : '#fc6d6d';
    div = $(divId);
    div.text(message)
    div.html(div.html().replace(/\n/g, '<br/>'));
    div.css('color', color);
}

function ExtensionIsValid(name, extensions) {
    let isValid = false;
    name = name.toUpperCase();

    for (var index in extensions) {
        const extension = extensions[index];
        isValid = name.endsWith(extension) || isValid;
        console.log('isValid: ' + isValid)
        console.log('endsWith: ' + name.endsWith(extension))
    }
    console.log('final isValid: ' + isValid)
    return isValid;
}
$(document).ready(function () {
    var file;
    file = document.getElementById("encode-drag-file");
    file.addEventListener("dragenter", OnDragEnter, false);
    file.addEventListener("dragover", OnDragOver, false);
    file.addEventListener("drop", OnDrop, false);
});

$("#encode-button").click(function () {
    var data = new FormData();
    for (var i = 0; i < selectedFiles.length; i++) {
        data.append(selectedFiles[i].name, selectedFiles[i]);
    }
    $.ajax({
        type: "POST",
        url: "FileHandler.ashx",
        contentType: false,
        processData: false,
        data: data,
        success: function (result) {
            alert(result);
        },
        error: function () {
            alert("There was error encode-buttoning files!");
        }
    });
});

console.log("koniaaa")