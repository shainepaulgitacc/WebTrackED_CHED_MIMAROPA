$(function () {
    $.ajax({
        type: "GET",
        url: "/Application/Dashboard?handler=ApprovedAndDisApproved",
        dataType: "json",
        success: function (result) {
            console.log(result);
            const ctx = document.getElementById('myChart').getContext('2d');
            const data = {
                labels: ['Approved', 'Disapproved'],
                datasets: [{
                    label: 'Total',
                    data: [result.countApproved, result.countDisapproved],
                    backgroundColor: ['rgb(75, 192, 192)', 'rgb(255, 99, 132)']
                }]
            };
            new Chart(ctx, {
                type: 'polarArea',
                data: data,
                options: {}
            });

        },
        error: function (req, status, error) {
            console.log(status);
        }
    });
    $.ajax({
        type: "GET",
        url: "/Application/Dashboard?handler=EndedDocsPerMonth",
        dataType: "json",
        success: function (result) {
            console.log(result);
            const chart2 = document.getElementById('myChart2').getContext('2d');
            const labels = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
            const data2 = {
                labels: labels,
                datasets: [{
                    label: 'Ended Document',
                    data: [result.january, result.february, result.march, result.april, result.may, result.june, result.july, result.august, result.september, result.october, result.november, result.december],
                    fill: false,
                    borderColor: 'rgb(75, 192, 192)',
                    tension: 0.1
                }]
            };
            new Chart(chart2, {
                type: 'line',
                data: data2,
                options: {}
            });

        },
        error: function (req, status, error) {
            console.log(status);
        }
    });
});

