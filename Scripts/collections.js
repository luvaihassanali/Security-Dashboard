$(document).ready(
    function () {
        //initialize json to make post
        var data = {
            TextBoxStringData: "spreadsheets"
        };
        //post request to server will return all relevant spreadsheet data
        $.post("/Form/FormTwo", { formData: data },
            function (data) {
                data = data.split("#");
                collections_overview = data[0];
                collections_changes = data[1];
                collections_notifications = data[2]
                //append tables to page
                $("#tableDiv").append("<strong><span style='color:#3498db'>Collections Overview</span></strong>")
                $("#tableDiv").append(collections_overview);
                $("#innerLeft").append("<strong><span style='color:#3498db'>Collections Changes</span></strong>")
                $("#innerLeft").append(collections_changes);
                $("#innerRight").append("<strong><span style='color:#3498db'>Collections Notifications</span></strong>")
                $("#innerRight").append(collections_notifications);
                $('#collections_overview').DataTable({ "pageLength": 3 });
                $('#collections_changes').DataTable({ "pageLength": 3 });
                $('#collections_notifications').DataTable({ "pageLength": 3 });
                //hide loading message
                $("#waitMessage").empty();
                document.getElementById("loader").style.display = "none";
            });
        
    });