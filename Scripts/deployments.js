$(document).ready(
    function () {
        //initialize json to make post
        var data = {
            TextBoxStringData: "deployments"
        };
        //post request to server will return all relevant spreadsheet data
        $.post("/Form/FormTwo", { formData: data },
            function (data) {
                data = data.split("#");
                //append tables to page
                deployments_summary = data[0];
                app_deployments_summary = data[1];
                firefox_patches = data[2]
                $("#tableDiv").append("<strong><span style='color:#3498db'>Deployments Summary</span></strong>")
                $("#tableDiv").append(deployments_summary);
                $("#tableDiv").append("<strong><span style='color:#3498db'>App Deployments Summary</span></strong>")
                $("#tableDiv").append(app_deployments_summary);
                $("#tableDiv").append("<strong><span style='color:#3498db'>Firefox Patches Needed</span></strong>")
                $("#tableDiv").append(firefox_patches)
                $("#firefox_patches").DataTable({ "pageLength": 3 });
                $('#deployments_summary').DataTable({ "order": [[4, "desc"]], "pageLength": 3 });
                $('#app_deployments_summary').DataTable({ "order": [[4, "desc"]], "pageLength": 3 });
                //hide loading message
                $("#waitMessage").empty();
                document.getElementById("loader").style.display = "none";
            });
    });
