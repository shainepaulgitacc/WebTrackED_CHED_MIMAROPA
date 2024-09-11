$(function () {
    $("#button-excel").on("click", function () {
        var table_excel = new Table2Excel();
        table_excel.export($("#table-data"));
    });
    $("#button-print").on("click", function () {
                //start_loader();
                var head = $('head').clone();
                var p = $('#outprint').clone();
                var el = $('<div class="print-wrapper">');
                var pContent = $($('noscript#print-content').html()).clone();
                head.find('title').text("Report - Print View");
                el.append(head);
                el.append(pContent);
                var nw = window.open("", "_blank", "width=1010,height=1000,top=50,left=75");
                nw.document.write(el.html());
                nw.document.close();

        // Maximize the window before printing
                nw.moveTo(0, 0);
                nw.resizeTo(screen.width, screen.height);

                setTimeout(() => {
                    nw.print();
                    setTimeout(() => {
                        nw.close();
                        end_loader();
                    }, 200);
                }, 500);
    });
    $("#button-pdf").on("click", function () {
        var element = document.getElementById('print-content');
        var tableWidth = element.offsetWidth / 72;
        var tableHeight = element.offsetHeight / 72;

        // Remove text-nowrap class from all table cells
        var opt = {
            margin: 0.3,
            filename: 'myfile.pdf',
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: { scale: 2 },
            jsPDF: { unit: 'in', format: 'letter', orientation: 'landscape' }

        };
        html2pdf().from(element).save();
    });
    
});