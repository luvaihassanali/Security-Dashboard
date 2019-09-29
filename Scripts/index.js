$(document).ready(function () {
    //automatically give cursor focus to text box on search page
    var input = document.getElementById("searchText");
    input.focus();
    //allow the submit button to be pressed using enter key
    input.addEventListener("keyup", function (event) {
        event.preventDefault();
        if (event.keyCode === 13) {
            document.getElementById("searchButton").click();
        }
    });
});

function reloadData() {
    //initialize json to make post
    var data = {
        TextBoxStringData: "reloadData"
    };
    //post request to server will return all relevant spreadsheet data
    $.post("/Form/FormTwo", { formData: data },
        function (data) {
            alert("All data from SCCM, SCSM, and EPO has been updated successfully.");
        });
}
//sends search query to webserver
function submit() {
    //display loader (small spinning icon)
    document.getElementById("loader").style.display = "block";
    $("#tableDiv").empty();
    //if input box is blank send warning
    if (document.getElementById("searchText").value === "") {
        alert("Cannot be blank.");
        document.getElementById("loader").style.display = "none";
        return;
    }
    //create json of query
    var data = {
        TextBoxStringData: $('#searchText').val()
    };
    //empty text box 
    document.getElementById("searchText").value = "";
    //post data to server
    $.post("/Form/FormOne", { formData: data },
        function (data) {
            //on return of post, remove loader and append tables onto page 
            $("#tableDiv").append(data);
            document.getElementById("loader").style.display = "none";
            //style each table with dataTables.js
            if ($('#sccmResult').length) {
                $('#sccmResult').DataTable();
            }
            if ($('#sccmResultComp').length) {
                $('#sccmResultComp').DataTable();
            }
            if ($('#epoResult').length) {
                $('#epoResult').DataTable();
            }
            if ($('#epoResultComp').length) {
                $('#epoResultComp').DataTable();
            }
            if ($('#adUser').length) {
                $('#adUser').DataTable();
            }
            if ($('#adComputer').length) {
                $('#adComputer').DataTable();
            }
            if ($('#scsmResult').length) {
                $('#scsmResult').DataTable();
            }
        });
}
