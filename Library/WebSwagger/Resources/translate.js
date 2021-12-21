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
        var div = $(this), s = div.find('.opblock-summary-operation-id').text(), btn = div.find('.authorization__btn');
        var allowAnonymous = (!s || s.indexOf('匿名访问') != -1 || s.indexOf('授权访问') == -1);
        if (allowAnonymous) btn.css('visibility', 'hidden');
        var configObject = window.swagger_config;
        if (configObject != undefined && configObject != null) {
            var user = configObject.user;
            //如果已登录授权
            if (user && user.id) {
                var role1 = user.role ? user.role.split(',') : [], permissions1 = user.permissions || [];
                //角色
                var role2_regex = s.match(/\u89d2\u8272([^\u6743\u9650]+)/), role_ok = (role2_regex && role2_regex.length == 2);
                var role2 = role_ok ? role2_regex[1].split(',') : [];
                if (role_ok) {
                    role_ok = role1.filter(function (i) { return role2.indexOf(i) != -1; }).length > 0;
                } else {
                    role_ok = 1; //无限制
                }
                //权限
                var permissions2_regex = s.match(/\u6743\u9650([^u7b56\u7565]+)/), permission_ok = (permissions2_regex && permissions2_regex.length == 2);
                var permissions2 = permission_ok ? permissions2_regex[1] : '';
                if (permission_ok) {
                    var permissions = permissions2.replace(' ', '').replace(/\W+/, ',').split(','), permissionJs = '';
                    $.each(permissions, function (i, v) { if (v) permissionJs += 'var ' + v + '=' + (permissions1.indexOf(v) == -1 ? 'false' : 'true') + ';'; });
                    permission_ok = eval(permissionJs + permissions2);
                } else {
                    permission_ok = 1; //无限制
                }
                //策略
                var policy2_regex = s.match(/\u7b56\u7565(\S+)/), policy_ok = (policy2_regex && policy2_regex.length == 2);
                var policy2 = policy_ok ? policy2_regex[1] : '';
                console.log('角色', role2, '权限', permissions2, '策略', policy2);
                if (!role_ok || !permission_ok) div.parent().hide();
            } else {
            }
        }
        if (div.parent().hasClass('opblock-deprecated')) div.click(function () { return false }).parent().click(function () { return false });
        btn.click(function () { return false });
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

    $.initialize('.parameters-container', function () {
        $(this).find('.opblock-description-wrapper').hide();
    });

    $.initialize('table.headers', function () {
        var th = $(this).find('th');
        if (th.length != 3) return;
        th.eq(0).html(resource_globalization["ResponseHeaderName"]);
        th.eq(1).html(resource_globalization["ResponseHeaderDescription"]);
        th.eq(2).html(resource_globalization["ResponseHeaderType"]);
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

    $.initialize('.model-box', function () {
        var td = $(this).find('table.model tr.description td'), p = td.find('p');
        td.eq(0).html(resource_globalization["ResponseTypeFields"]);
        if (p.length == 1 && !$.trim(p.text())) p.html(resource_globalization["Description"]);
        $(this).find('table.model tr:last:not(.property-row)').remove();
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
