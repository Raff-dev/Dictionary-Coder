var selectedFiles;
var validEncodeExtensions = [".TXT", ".JPG", ".JPEG", ".PNG"]
var validDecodeExtensions = [".LZ77"]

var animateArrow = false;

function OnDragEnter(e) {
    console.log('enter')

    e.stopPropagation();
    e.preventDefault();
    if (this == document) $(".arrow-div").children("img").addClass("bouncy");
}

function OnDragLeave(e) {
    console.log('leave')

    e.stopPropagation();
    e.preventDefault();
    if (this == document) $(".arrow-div").children("img").removeClass("bouncy");
    else $(this).css("color", "#69B6F1");
}


function OnDragOver(e) {
    console.log('over')

    e.stopPropagation();
    e.preventDefault();
    if (this == document) $(".arrow-div").children("img").addClass("bouncy");
    else $(this).css("color", "blue");
}

function OnDrop(e) {
    e.stopPropagation();
    e.preventDefault();
    $(".arrow-div").children("img").removeClass("bouncy");

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
    var elements = [document, document.getElementById("encode-drag-file"), document.getElementById("decode-drag-file")];
    for (index in elements) {
        const elem = elements[index];
        elem.addEventListener("dragenter", OnDragEnter, false);
        elem.addEventListener("dragover", OnDragOver, false);
        elem.addEventListener("dragleave", OnDragLeave, false);
        elem.addEventListener("drop", OnDrop, false);
    }

    var elements = [$("#encode-button"), $("#decode-button")];
    for (index in elements) {
        const elem = elements[index];
        elem.click(e => {
            if (selectedFiles.length <= 0) return;
            console.log("not returned")

            var data = new FormData();
            for (var i = 0; i < selectedFiles.length; i++) {
                data.append(selectedFiles[i].name, selectedFiles[i]);
            }
            $.ajax({
                type: "POST",
                url: "https://localhost:44359/home/Upload?value=" + elem.attr('value'),
                contentType: false,
                processData: false,
                data: data,
                success: function (result) {
                    console.log(result);
                    alert(result);
                },
                error: function (error) {
                    alert("There was error uploading files! " + error);

                }
            });
        });
    }
});
