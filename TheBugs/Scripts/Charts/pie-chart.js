$(document).ready(function () {
    google.charts.load('current', { packages: ['corechart', 'bar'] });
    google.charts.setOnLoadCallback(drawChart);

    function drawChart() {
        var elem = $('#pie_chart').get(0);
        var headerParts = elem.dataset.header.split(',');
        var data = [[headerParts[0], headerParts[1]]];
        var values = elem.dataset.values.split(';');
        var names = [];
        values.forEach(function (str, _, _) {
            var all = str.split(',');
            data.push([all[0], parseInt(all[1])]);
            names.push(all[0]);
        });

        var dataTable = google.visualization.arrayToDataTable(data);
        var options = {
            title: elem.dataset.title,
            pieSliceText: 'value',
        };

        var chart = new google.visualization.PieChart(elem);
        chart.draw(dataTable, options);

        google.visualization.events.addListener(chart, 'select', function () {
            var selectedItem = chart.getSelection()[0];
            if (selectedItem) {
                var name = names[selectedItem.row];
                $('#filter_assignee').attr('value', name);
                $('#filter_form').submit()
            }
        });
    }
});

