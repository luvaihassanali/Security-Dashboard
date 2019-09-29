//animate tables displayed on dashboard page to move with scrolling
$(window).scroll(function () {
    $("#hiddenDiv").stop().animate({ "marginTop": $(window).scrollTop() + "px", "marginLeft": $(window).scrollLeft() + "px" }, "slow");
});

//global vars for pie charts
var activeClientData, threatEventData, agentcomplianceData, healthData, passData, productData, detectionData, rebootData, rebootSeven, rebootFourteen, rebootMonth, rebootTMonth, rebootSMonth, reboot12Month, inactivepass, inactiveunknown, activeunknown, activefail, inactivefail, unhealthyMachines, agentString, threatString, detectionString, productString;

//when document loads
$(document).ready(
    function () {
        //initialize json to make post
        var data = {
            TextBoxStringData: "dashboards"
        };
        console.log("before post");
        //post request to server will return all relevant dashboard data
        $.post("/Form/FormTwo", { formData: data },
            function (data) {
                console.log("in post");
                //spilt data into own variable and render pie chart
                data = data.split("#");
                activeClientData = data[0];
                activeClientData = activeClientData.substr(1);
                activeClientData = activeClientData.substr(0, activeClientData.length - 1);
                activeClientData = JSON.parse(activeClientData);
                activeClientChart();
                threatEventData = data[1];
                threatEventData = JSON.parse(threatEventData);
                threatEventChart();
                agentcomplianceData = data[2];
                agentcomplianceData = JSON.parse(agentcomplianceData);
                agentChart();
                healthData = data[3];
                healthData = JSON.parse(healthData);
                healthChart();
                passData = data[4];
                passData = JSON.parse(passData);
                passChart();
                productData = data[5];
                productData = JSON.parse(productData);
                productChart();
                detectionData = data[6];
                detectionData = JSON.parse(detectionData);
                detectionChart();
                rebootData = data[7];
                rebootData = JSON.parse(rebootData);
                rebootChart();
                rebootSeven = data[8];
                rebootFourteen = data[9];
                rebootMonth = data[10];
                rebootTMonth = data[11];
                rebootSMonth = data[12];
                reboot12Month = data[13];
                inactivepass = data[14];
                inactiveunknown = data[15];
                activeunknown = data[16];
                activefail = data[17];
                inactivefail = data[18];
                unhealthyMachines = data[19];
                agentString = data[20];
                threatString = data[21];
                detectionString = data[22];
                //productString = data[23];
                //hide loading message
                $("#waitMessage").empty();
                document.getElementById("loader").style.display = "none";
                $("#epoButton").show();
                $("#sccmButton").show();
                $("#legend").show();
            });
        console.log("done post");
        /* from this position up to the end of the document.ready function (line 278)
           each onclick function delcared here initializes the data to be rendered when a pie slice is clicked
           what data is to be shown is decided by what label of the pie slice is clicked
         */
        var activecanvas = document.getElementById("activeCanvas");
        activecanvas.onclick = function (evt) {
            var activePoints = activeChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                alert("Not available");
            }
        };

        var productcanvas = document.getElementById("productCanvas");
        productcanvas.onclick = function (evt) {
            var activePoints = productChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                var chartData = activePoints[0]['_chart'].config.data;
                var idx = activePoints[0]['_index'];
                var label = chartData.labels[idx];
                if (label === "Successful") alert("Data available for failed updates only");
                if (label === "Failed") {
                    alert("SIKE (Too many failed takes too long)");
                }
            }
        };

        var threatcanvas = document.getElementById("threatCanvas");
        threatcanvas.onclick = function (evt) {
            var activePoints = threatChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                var chartData = activePoints[0]['_chart'].config.data;
                var idx = activePoints[0]['_index'];

                var label = chartData.labels[idx];

                $("#hiddenDiv").empty();
                $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                $("#hiddenDiv").append(threatString);
                $('#threatString').DataTable();
                document.getElementById("hiddenDiv").style.display = "block";
            }
        };


        var agentcanvas = document.getElementById("agentCanvas");
        agentcanvas.onclick = function (evt) {
            var activePoints = agentChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                var chartData = activePoints[0]['_chart'].config.data;
                var idx = activePoints[0]['_index'];

                var label = chartData.labels[idx];
                if (label === "Compliant") alert("Data only available for Non-Compliant machines");
                if (label === "Non-Compliant") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(agentString);
                    $('#agentString').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
            }
        };

        var healthcanvas = document.getElementById("healthCanvas");
        healthcanvas.onclick = function (evt) {
            var activePoints = healthChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                var chartData = activePoints[0]['_chart'].config.data;
                var idx = activePoints[0]['_index'];

                var label = chartData.labels[idx];
                if (label === "Healthy") {
                    alert("Data only available for unhealthy machines");
                }
                if (label === "Unhealthy") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(unhealthyMachines);
                    $('#unhealthyMachines').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
            }
        };

        var passcanvas = document.getElementById("passCanvas");
        passcanvas.onclick = function (evt) {
            var activePoints = passChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                var chartData = activePoints[0]['_chart'].config.data;
                var idx = activePoints[0]['_index'];

                var label = chartData.labels[idx];
                if (label === "Active/Pass") {
                    alert("Data not available for Active/Pass");
                }
                if (label === "Inactive/Pass") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(inactivepass);
                    $('#inactivepass').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Inactive/Unknown") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(inactiveunknown);
                    $('#inactiveunknown').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Active/Unknown") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(activeunknown);
                    $('#activeunknown').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Active/Fail") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(activefail);
                    $('#activefail').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Inactive/Fail") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(inactivefail);
                    $('#inactivefail').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
            }
        };

        var rebootcanvas = document.getElementById("rebootCanvas");
        rebootcanvas.onclick = function (evt) {
            var activePoints = rebootChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                var chartData = activePoints[0]['_chart'].config.data;
                var idx = activePoints[0]['_index'];

                var label = chartData.labels[idx];
                if (label === "Last Reboot within 7 days") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(rebootSeven);
                    $('#rebootSeven').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Last Reboot within 14 days") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(rebootFourteen);
                    $('#rebootFourteen').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Last Reboot within 1 month") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(rebootMonth);
                    $('#rebootMonth').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Last Reboot within 3 months") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(rebootTMonth);
                    $('#rebootTMonth').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Last Reboot within 6 months") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(rebootSMonth);
                    $('#rebootSMonth').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }
                if (label === "Last Reboot within 12 months") {
                    $("#hiddenDiv").empty();
                    $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                    $("#hiddenDiv").append(reboot12Month);
                    $('#reboot12Month').DataTable();
                    document.getElementById("hiddenDiv").style.display = "block";
                }


            }
        };

        var detectioncanvas = document.getElementById("detectionCanvas");
        detectioncanvas.onclick = function (evt) {
            var activePoints = detectionChartDisplay.getElementsAtEvent(evt);
            if (activePoints[0]) {
                var chartData = activePoints[0]['_chart'].config.data;
                var idx = activePoints[0]['_index'];

                var label = chartData.labels[idx];
                $("#hiddenDiv").empty();
                $("#hiddenDiv").append("<input style='display: block' id='closeHiddenDiv' type='button' onclick='closeHiddenDiv()' value='Close'> <br>");
                $("#hiddenDiv").append(detectionString);
                $('#detectionString').DataTable();
                document.getElementById("hiddenDiv").style.display = "block";
            }
        };
});
//end of document ready 

//array of colors used for pie slices
var colors = [
    "#4572A7", //blue
    "#AA4643", //red
    "#89A54E", //green
    "#80699B", //purple
    "#3D96AE", //cyan
    "#DB843D", //orange
    "#DA8D98", //pink
    "#B5CA92", //light green
    "#92A8CD", //grey
    "#A47D7C" //brown
];

//the rest of the code initializes the pie charts and pie tool tip 

var rebootChartDisplay;
function rebootChart() {
    var tempData = [];
    var tempData2 = [];
    var tempData3 = [];

    for (var i = 0; i < rebootData.length - 1; i++) {
        tempData.push(rebootData[i].Count);
        tempData2.push(rebootData[i].TimePeriod);
        tempData3.push(colors[i]);
    }
    document.getElementById("rebootLabel").innerText = "Total Machine Reboot Statistics (1 year) - SCCM";
    var rebootChart = document.getElementById("rebootCanvas");
    var ctx = rebootChart.getContext('2d');
    rebootChartDisplay = new Chart(ctx, {
        options: {

            responsive: true

        },
        type: 'pie',
        data: {
            datasets: [{
                data: tempData,
                backgroundColor: tempData3,
                label: 'Dataset 1'
            }],
            labels: tempData2
        }
    });
}
var detectionChartDisplay;
function detectionChart() {
    var tempData = [];
    var tempData2 = [];
    var tempData3 = [];

    for (var i = 0; i < detectionData.length; i++) {
        tempData.push(detectionData[i].count);
        tempData2.push(detectionData[i].AnalyzerName);
        tempData3.push(colors[i]);
    }
    document.getElementById("detectionLabel").innerText = "Today's Detection per Product - EPO";
    var detectionChart = document.getElementById("detectionCanvas");
    var ctx = detectionChart.getContext('2d');
    detectionChartDisplay = new Chart(ctx, {
        options: {
            responsive: true
        },
        type: 'pie',
        data: {
            datasets: [{
                data: tempData,
                backgroundColor: tempData3,
                label: 'Dataset 1'
            }],
            labels: tempData2
        }
    });
}
var productChartDisplay;
function productChart() {
    document.getElementById("productLabel").innerText = "Product updates within last 24 hours - EPO";
    var productChart = document.getElementById("productCanvas");
    var ctx = productChart.getContext('2d');
    productChartDisplay = new Chart(ctx, {
        options: {
            responsive: true
        },
        type: 'pie',
        data: {
            labels: ["Successful", "Failed"],
            datasets: [{
                backgroundColor: [
                    "#89A54E", //green
                    "#AA4643" //red
                ],
                data: [productData[0].count, productData[1].count]
            }]
        }
    });
}
var passChartDisplay;
function passChart() {
    var tempData = [];
    var tempData2 = [];

    for (var i = 0; i < passData.length; i++) {
        tempData.push(passData[i].ClientState);
        tempData2.push(passData[i].NumberofClients);
    }
    document.getElementById("passLabel").innerText = "Client Health - SCCM";
    var passChart = document.getElementById("passCanvas");
    var ctx = passChart.getContext('2d');
    passChartDisplay = new Chart(ctx, {
        options: {
            responsive: true
        },
        type: 'pie',
        data: {
            datasets: [{
                data: tempData2,
                backgroundColor: colors,
                label: 'Dataset 1'
            }],
            labels: tempData
        }
    });

}
var healthChartDisplay;
function healthChart() {
    document.getElementById("healthLabel").innerText = "Total machines: " + healthData[0].TotalMachines + " with " + Math.round(healthData[0].HealthyPercent) + "% health - SCCM";
    var d = document.getElementById("healthCanvas");
    var ctx = d.getContext("2d");
    healthChartDisplay = new Chart(ctx, {
        options: {
            responsive: true
        },
        type: 'pie',
        data: {
            labels: ["Healthy", "Unhealthy"],
            datasets: [{
                backgroundColor: [
                    "#89A54E", //green
                    "#AA4643" //red
                ],
                data: [healthData[0].Healthy, healthData[0].UnHealthy]
            }]
        }
    });
}
var agentChartDisplay;
function agentChart() {
    document.getElementById("agentLabel").innerText = "Agent Communication Summary - EPO";
    var agentChart = document.getElementById("agentCanvas");
    var ctx = agentChart.getContext('2d');
    agentChartDisplay = new Chart(ctx, {
        options: {
            responsive: true
        },
        type: 'pie',
        data: {
            labels: ["Compliant", "Non-Compliant"],
            datasets: [{
                backgroundColor: [
                    "#89A54E", //green
                    "#AA4643" //red
                ],
                data: [agentcomplianceData[0].count, agentcomplianceData[1].count]
            }]
        }
    });
}

var threatChartDisplay;
function threatEventChart() {
    var tempData = [];
    var tempData2 = [];
    var tempData3 = [];
    for (var i = 0; i < threatEventData.length; i++) {
        tempData.push(threatEventData[i].Name);
        tempData2.push(threatEventData[i].count);
        tempData3.push(colors[i]);
    }
    document.getElementById("threatLabel").innerText = "Top 360 threats detected within last 24 hours - EPO";
    var threatChart = document.getElementById("threatCanvas");
    var ctx = threatChart.getContext('2d');
    threatChartDisplay = new Chart(ctx, {
        options: {
            responsive: true
        },
        type: 'pie',
        data: {
            datasets: [{
                data: tempData2,
                backgroundColor: tempData3,
                label: 'Dataset 1'
            }],
            labels: tempData
        }
    });
}
var activeChartDisplay; 
function activeClientChart() {
    document.getElementById("activeClientLabel").innerText = "Total workstations: " + activeClientData.TotalClientInstalled + " with " + Math.round(activeClientData.ClientActivePercent) + "% active - SCCM";
    var d = document.getElementById("activeCanvas");
    var ctx = d.getContext("2d");
    activeChartDisplay = new Chart(ctx, {
        options: {
            responsive: true
        },
        type: 'pie',
        data: {
            labels: ["Active", "Inactive"],
            datasets: [{
                backgroundColor: [
                    "#89A54E", //green
                    "#AA4643" //red
                ],
                data: [activeClientData.ClientActive, activeClientData.ClientInActive]
            }]
        }
    });
}

//initialize tool tip for pie charts
Chart.defaults.global.tooltips.custom = function (tooltip) {
    var tooltipEl = document.getElementById('tail');
    if (tooltip.opacity === 0) {
        tooltipEl.style.opacity = 0;
        return;
    }
    if (tooltip.body) {
        var total = 0;
        var value = this._data.datasets[tooltip.dataPoints[0].datasetIndex].data[tooltip.dataPoints[0].index].toLocaleString();
        this._data.datasets[tooltip.dataPoints[0].datasetIndex].data.forEach(function (e) {
            total += e;
        });
        var parsedValue = parseFloat(value.replace(/,/g, ''));
        parsedValue = parsedValue / total * 100;
        //if value when rounded is 0.xx then multiply by 100 to get actual percentage
        if (Math.floor(parsedValue) === 0) {
                parsedValue = parsedValue * 100;
        }
        //remove decimals
        parsedValue = parsedValue.toFixed(2);
        tooltipEl.innerHTML = '<h1 style="font-size:16px;font-weight:bold; padding-bottom:5px;">' + Math.round(parsedValue) + '%</h1>';
    }
    tooltipEl.style.opacity = 1;
    //bind tool tip to mouse movement
    $(document).bind('mousemove', function (e) {
        $('#tail').css({
            left: e.pageX + 50,
            top: e.pageY
        });
    });

};

//close visible information tables 

function closeHiddenDiv() {
    $("#hiddenDiv").empty();
    document.getElementById('hiddenDiv').style.display = "none";
}
