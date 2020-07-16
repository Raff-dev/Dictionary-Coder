var selectedFiles;
var validEncodeExtentions = [".txt", ".jpg", ".jpeg", ".png"]
var validDecodeExtentions = [".lz77"]

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

    file = selectedFiles[0];
    console.log('name: ' + file.name);
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