/* chinese translate */
function swagger_translate() {
    $.initialize('.download-url-wrapper .select-label', function () {
        $(this).find('span').html(resource_globalization["SelectDefinition"]);
        // when different api groups are selected
        if (typeof downloadUrlChanged == 'function') $(this).find('select').each(function (i, sel) { sel.addEventListener('change', function () { setTimeout(downloadUrlChanged, 200) }) });
    });

    $.initialize('section.models.is-open', function () {
        $(this).find('h4 > span').html(resource_globalization["Schemas"]);
    });

    $.initialize('.opblock-summary', function () {
        var s = $.trim($(this).find('.opblock-summary-operation-id').text());
        if (!s || s === '匿名访问') $(this).find('.authorization__btn').hide();
    });

    $.initialize('h4.opblock-title:not(.parameter__name)', function () {
        $(this).find('span').html(resource_globalization["Description"]);
    });

    $.initialize('h4.opblock-title.parameter__name', function () {
        $(this).html(resource_globalization["RequestBody"]);
    });

    $.initialize('table.parameters', function () {
        $(this).find('th.parameters-col_name').html(resource_globalization["Name"]);
        $(this).find('th.parameters-col_description').html(resource_globalization["Description"]);
    });

    $.initialize('.try-out button', function () {
        var btn = this, cls = btn.className;
        btn.innerText = cls.indexOf('cancel') == -1 ? resource_globalization["TryItOut"] : resource_globalization["Cancel"];
        btn.addEventListener('click', function (e) {
            if (e.type !== 'click') return;
            var btn = e.srcElement, cls = btn.className;
            if (btn.tagName !== 'BUTTON' || !cls) return;
            setTimeout(function () {
                $(btn).html(cls.indexOf('cancel') == -1 ? resource_globalization["Cancel"] : resource_globalization["TryItOut"]);
                $.initialize('.execute-wrapper', function () {
                    var btn1 = $(this).find('button.execute').html(resource_globalization["Execute"]);
                    if (btn1.length != 1) return;
                    btn1[0].addEventListener('click', function (e1) {
                        if (e1.type !== 'click') return;
                        var btn1 = e1.srcElement;
                        if (btn1.tagName !== 'BUTTON') return;
                        $.initialize('button.btn-clear', function () { $(this).html(resource_globalization["Clear"]) });
                    });
                });
            }, 10);
        });
    });

    $.initialize('.tab-header', function () {
        $(this).find('.opblock-title').html(resource_globalization["Parameters"]);
    });

    $.initialize('button.authorize', function () {
        $(this).find('span').html(resource_globalization["Authorize"]);
    });

    $.initialize('button.authorize.modal-btn.auth', function () {
        $(this).html(resource_globalization["Authorize"]);
        //$(this).closest('.modal-ux-inner').find('.modal-ux-header > h3').html(resource_globalization["AvailableAuth"]);
    });

    $.initialize('.btn-done.modal-btn.auth', function () {
        $(this).html(resource_globalization["Close"]);
    });

    $.initialize('.parameter__enum', function () {
        $(this).find('i').html(resource_globalization["AvailableValues"]);
    });

    $.initialize('a.tablinks[data-name="example"]', function () {
        $(this).html(resource_globalization["ExampleValue"]);
    });

    $.initialize('a.tablinks[data-name="model"]', function () {
        $(this).html(resource_globalization["Schema"]);
    });

    $.initialize('.responses-wrapper', function () {
        $(this).find('.opblock-section-header > h4').html(resource_globalization["Responses"]);
        $(this).find('.opblock-section-header > label > span').html(resource_globalization["ResponseContentType"]);
    });

    $.initialize('.request-url', function () {
        $(this).prev('h4').html(resource_globalization["RequestUrl"]);
    });

    $.initialize('table.responses-table.live-responses-table', function () {
        $(this).prev('h4').html(resource_globalization["ServerResponse"]);
        $(this).find('tr.responses-header > td.response-col_status').html(resource_globalization["Code"]);
        $(this).find('tr.responses-header > td.response-col_description').html(resource_globalization["Details"]);

        $(this).find('tr.response > td.response-col_description').find('h5:first').html(resource_globalization["ResponseBody"]);
        $(this).find('tr.response > td.response-col_description').find('h5').eq(1).html(resource_globalization["ResponseHeaders"]);

        $(this).parent('div').next('h4').html(resource_globalization["Responses"]);
    });

    $.initialize('table.responses-table:not(.live-responses-table)', function () {
        $(this).find('tr.responses-header > td.response-col_status').html(resource_globalization["Code"]);
        $(this).find('tr.responses-header > td.response-col_description').html(resource_globalization["Description"]);
    });

    $.initialize('.response-control-media-type__title', function () {
        $(this).html(resource_globalization["MediaType"]);
    });

    $.initialize('.download-contents', function () {
        $(this).html(resource_globalization["Download"]);
    });

    $.initialize('.col_header.response-col_links', function () {
        $(this).html(resource_globalization["Links"]);
    });

    $.initialize('.response-col_links:not(.col_header)', function () {
        $(this).find('i').html(resource_globalization["NoLinks"]);
    });

}
