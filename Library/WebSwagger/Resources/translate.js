$(document).ready(function () {
    var res = resource_globalization;

    $.initialize('.swagger-ui .topbar .download-url-wrapper', function () {
        $(this).find('.select-label span').html(res["SelectDefinition"]);
    });

    $.initialize('section.models.is-open', function () {
        $(this).find('h4 > span').html(res["Schemas"]);
    });

    $.initialize('h4.opblock-title:not(.parameter__name)', function () {
        $(this).find('span').html(res["Description"]);
    });

    $.initialize('h4.opblock-title.parameter__name', function () {
        $(this).html(res["RequestBody"]);
    });

    $.initialize('table.parameters', function () {

        $(this).find('th.parameters-col_name').html(res["Name"]);
        $(this).find('th.parameters-col_description').html(res["Description"]);
    });

    $.initialize('button.try-out__btn', function () {
        $(this).html(res["TryItOut"]);
    });

    $.initialize('button.try-out__btn.cancel', function () {
        $(this).html(res["Cancel"]);
    });

    $.initialize('button.execute', function () {
        $(this).html(res["Execute"]);
    });

    $.initialize('button.btn-clear', function () {
        $(this).html(res["Clear"]);
    });

    $.initialize('button.authorize ', function () {
        $(this).find('span').html(res["Authorize"]);
    });

    $.initialize('button.authorize.modal-btn.auth', function () {
        $(this).html(res["Authorize"]);
        $(this).closest('.modal-ux-inner').find('.modal-ux-header > h3').html(res["AvailableAuth"]);
    });

    $.initialize('.btn-done.modal-btn.auth', function () {
        $(this).html(res["Close"]);
    });

    $.initialize('.parameter__enum ', function () {
        $(this).find('i').html(res["AvailableValues"]);
    });

    $.initialize('a.tablinks[data-name="example"]', function () {
        $(this).html(res["ExampleValue"]);
    });

    $.initialize('a.tablinks[data-name="model"]', function () {
        $(this).html(res["Schema"]);
    });

    $.initialize('.responses-wrapper', function () {
        $(this).find('.opblock-section-header > h4').html(res["Responses"]);
    });

    $.initialize('.request-url', function () {
        $(this).prev('h4').html(res["RequestUrl"]);
    });

    $.initialize('table.responses-table.live-responses-table', function () {
        $(this).prev('h4').html(res["ServerResponse"]);
        $(this).find('tr.responses-header > td.response-col_status').html(res["Code"]);
        $(this).find('tr.responses-header > td.response-col_description').html(res["Details"]);

        $(this).find('tr.response > td.response-col_description').find('h5:first').html(res["ResponseBody"]);
        $(this).find('tr.response > td.response-col_description').find('h5').eq(1).html(res["ResponseHeaders"]);

        $(this).parent('div').next('h4').html(res["Responses"]);
    });

    $.initialize('table.responses-table:not(.live-responses-table)', function () {
        $(this).find('tr.responses-header > td.response-col_status').html(res["Code"]);
        $(this).find('tr.responses-header > td.response-col_description').html(res["Description"]);
    });

    $.initialize('.response-control-media-type__title', function () {
        $(this).html(res["MediaType"]);
    });

    $.initialize('.download-contents', function () {
        $(this).html(res["Download"]);
    });

    $.initialize('.col_header.response-col_links', function () {
        $(this).html(res["Links"]);
    });

    $.initialize('.response-col_links:not(.col_header)', function () {
        $(this).find('i').html(res["NoLinks"]);
    });

});