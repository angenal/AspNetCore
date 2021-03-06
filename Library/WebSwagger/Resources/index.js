/* when different api groups are selected */
function downloadUrlChanged() {
    var configObject = window.swagger_config;
    // Add a link
    $('.swagger-ui .topbar a').attr('href', location.href.split('#')[0].split('?')[0]);
    // Init logout operation
    if (configObject.customAuth && configObject.logoutUrl) {
        var btn = document.createElement('button');
        btn.className = 'btn '; btn.innerText = 'Logout';
        if (resource_globalization) btn.innerText = resource_globalization.Logout;
        btn.onclick = function () { location.href = configObject.logoutUrl; };
        document.getElementsByClassName('topbar-wrapper')[0].appendChild(btn);
    }
    // Init Authorize Listener
    if (configObject.ApiKeyStorage || configObject.BearerStorage) addAuthorizeListener(configObject);
    // Init filter operation
    var filterl = [], filterf = function () {
        filterl = $('.filter-container');
        if (filterl.length == 0) return setTimeout(filterf, 50);
        if (filterl.find('section.filter button').length > 0) return;
        filterl.parent().addClass('wrapper');
        filterl.find('.operation-filter-input').attr('placeholder', ' 筛选 / 过滤');
        var foldLen = $('.opblock-tag-section:not(.is-open)').length, openLen = $('.opblock-tag-section.is-open').length;
        var fbutton = $('<button class="btn">' + (foldLen > openLen ? '展开' : '折叠') + '</button>').click(function () {
            var btn = $(this), txt = btn.text();
            btn.text(txt == '展开' ? openOperations() : foldOperations());
        });
        filterl.find('section.filter').append(fbutton);
    }; filterf();
}

function foldOperations() { $('.opblock-tag-section').each(function (i, v) { var v1 = $(v), v2 = v1.hasClass('is-open'); if (v2) v1.find('h4').trigger('click'); }); return '展开'; }
function openOperations() { $('.opblock-tag-section').each(function (i, v) { var v1 = $(v), v2 = v1.hasClass('is-open'); if (!v2) v1.find('h4').trigger('click'); }); return '折叠'; }

function initTokenStorage(configObject) {
    if (configObject.ApiKeyStorage || configObject.BearerStorage) {
        if (configObject.ApiKeyStorage) configObject.ApiKeyStorage.authorized = false;
        if (configObject.BearerStorage) configObject.BearerStorage.authorized = false;
        configObject.onComplete = function () {
            var authorized = false;
            if (configObject.ApiKeyStorage) {
                var token = getApiKeyStorage(configObject);
                configObject.ApiKeyStorage.authorized = authorized = !!token;
                if (authorized) swagger_ui.preauthorizeApiKey(configObject.ApiKeyStorage.securityDefinition, token);
            }
            if (configObject.BearerStorage) {
                var token = getBearerStorage(configObject);
                configObject.BearerStorage.authorized = authorized = !!token;
                if (authorized) swagger_ui.preauthorizeApiKey(configObject.BearerStorage.securityDefinition, token);
            }
            configObject.afterAuthorizedButtonClick(authorized);
        };
        configObject.requestInterceptor = function (request) {
            if (configObject.ApiKeyStorage) {
                var token = getApiKeyStorage(configObject);
                if (token && configObject.ApiKeyStorage.securityScheme['in'] == 'header') {
                    request.headers[configObject.ApiKeyStorage.securityScheme.name] = token;
                }
            }
            if (configObject.BearerStorage) {
                var token = getBearerStorage(configObject);
                if (token && configObject.BearerStorage.securityScheme['in'] == 'header') {
                    request.headers[configObject.BearerStorage.securityScheme.name] = 'Bearer ' + token;
                }
            }
            return request;
        };
        configObject.afterAuthorizedButtonClick = function (saved) {
            if (!saved) return configObject.user = { id: "", role: "", permissions: [] };
            sendSwaggerRequest('getRolePermissions', function (data) {
                if (data && data.id) configObject.user = data;
            });
        };
    } else {
        //console.log('Init authorize cookie storage');
        configObject.requestInterceptor = (request) => {
            var token = getCookieValue('CSRF-TOKEN');
            if (token) request.headers.Authorization = 'Bearer ' + token;
            var csrfTokenCookie = getCookieValue('X-CSRF-TOKEN');
            if (csrfTokenCookie !== undefined && csrfTokenCookie !== null) request.headers['X-CSRF-TOKEN'] = csrfTokenCookie;
            return request;
        };
    }
}

function addAuthorizeListener(configObject) {
    var authWrapper = document.querySelector('.scheme-container .auth-wrapper');
    if (!authWrapper) return setTimeout(function () { addAuthorizeListener(configObject); }, 50);
    var txtAuthorize = ['Authorize', resource_globalization ? resource_globalization.Authorize : '授权'];
    var txtLogout = ['Logout', resource_globalization ? resource_globalization.Logout : '退出'];
    var scheme1 = "ApiKey", scheme2 = "Bearer";// queryName1 = "apiKey", queryName2 = "token";
    authWrapper.addEventListener('click', function (e) {
        if (e.type !== 'click') return;
        var btn = e.srcElement, tagName = 'BUTTON', clsName = 'authorized';
        if (btn.tagName !== tagName) {
            if (btn.parentNode.tagName === tagName) btn = btn.parentNode;
            else if (btn.parentNode.parentNode.tagName === tagName) btn = btn.parentNode.parentNode;
        }
        if (btn.tagName === tagName) {
            var txt = btn.innerText, cls = btn.className;
            if (!txt || !cls) return;
            // authorize button
            if (cls.indexOf('btn authorize') == 0) {
                //console.log('click authorize button');
                var formf = function () {
                    var forms = $('.scheme-container .auth-container');
                    if (forms.length == 0) setTimeout(formf, 20);
                    $('.scheme-container .modal-ux-header > h3').html(resource_globalization["AvailableAuth"]);
                    // scheme1 form
                    var form1 = forms.filter(function (i, v) { return v.innerText.indexOf(scheme1) == 0; });
                    if (form1.length == 1) {
                        var btnQ = form1.find('.modal-btn').eq(0);
                        var token = getApiKeyStorage(configObject), authorized = !!token;
                        configObject.ApiKeyStorage.authorized = authorized;
                        btnQ.attr('scheme', scheme1).text(authorized ? txtLogout[1] : txtAuthorize[1]);
                        if (authorized) btnQ.addClass(clsName); else btnQ.removeClass(clsName);
                        form1.find('h4').html(form1.find('h4 code').html());
                        form1.find('h6').hide();
                    }
                    // scheme2 form
                    var form2 = forms.filter(function (i, v) { return v.innerText.indexOf(scheme2) == 0; });
                    if (form2.length == 1) {
                        //var btns = form2.find('.modal-btn'), el = form2.find('input');
                        //if (el.length == 1 && tokenValue) { el.val(tokenValue); btns.eq(0).trigger('click'); }
                        var btnQ = form2.find('.modal-btn').eq(0);
                        var token = getBearerStorage(configObject), authorized = !!token;
                        configObject.BearerStorage.authorized = authorized;
                        btnQ.attr('scheme', scheme2).text(authorized ? txtLogout[1] : txtAuthorize[1]);
                        if (authorized) btnQ.addClass(clsName); else btnQ.removeClass(clsName);
                        form2.find('h4').html(form2.find('h4 code').html());
                        form2.find('h6').hide();
                    }
                }; formf();
            }
            // authorize form auth button
            if (cls.indexOf('btn modal-btn') == 0) {
                var btnQ = $(btn), scheme = btnQ.attr('scheme');
                //console.log('click authorize form-button:', scheme);
                // scheme1 form button
                if (scheme === scheme1) {
                    var form1 = btnQ.parents('form');
                    if (txtAuthorize.indexOf(txt) != -1) {
                        var el = form1.find('input');
                        if (el.length == 0) return;
                        if ($.trim(el.val()) == '') return alert('输入内容不能为空');
                        configObject.ApiKeyStorage.authorized = true;
                        var authfn = function () { btnQ.text(txtLogout[1]); btnQ.addClass(clsName); form1.find('h6').hide(); configObject.afterAuthorizedButtonClick(true); foldOperations(); };
                        setTimeout(authfn, 20);
                        setApiKeyStorage(configObject, el.val());
                    } else if (txtLogout.indexOf(txt) != -1) {
                        configObject.ApiKeyStorage.authorized = false;
                        var authfn = function () { btnQ.text(txtAuthorize[1]); btnQ.removeClass(clsName); form1.find('h6').hide(); configObject.afterAuthorizedButtonClick(false); foldOperations(); };
                        setTimeout(authfn, 20);
                        removeApiKeyStorage(configObject);
                    }
                }
                // scheme2 form button
                if (scheme === scheme2) {
                    var form2 = btnQ.parents('form');
                    if (txtAuthorize.indexOf(txt) != -1) {
                        var el = form2.find('input');
                        if (el.length == 0) return;
                        if ($.trim(el.val()) == '') return alert('输入内容不能为空');
                        configObject.BearerStorage.authorized = true;
                        var authfn = function () { btnQ.text(txtLogout[1]); btnQ.addClass(clsName); form2.find('h6').hide(); configObject.afterAuthorizedButtonClick(true); foldOperations(); };
                        setTimeout(authfn, 20);
                        setBearerStorage(configObject, el.val());
                    } else if (txtLogout.indexOf(txt) != -1) {
                        configObject.BearerStorage.authorized = false;
                        var authfn = function () { btnQ.text(txtAuthorize[1]); btnQ.removeClass(clsName); form2.find('h6').hide(); configObject.afterAuthorizedButtonClick(false); foldOperations(); };
                        setTimeout(authfn, 20);
                        removeBearerStorage(configObject);
                    }
                }
            }
        }
    }, false);
}

function getCookieValue(key) {
    var equalities = document.cookie.split('; ');
    for (var i = 0; i < equalities.length; i++) {
        if (!equalities[i]) continue;
        var splitted = equalities[i].split('=');
        if (splitted.length !== 2) continue;
        if (decodeURIComponent(splitted[0]) === key) return decodeURIComponent(splitted[1] || '');
    }
    return null;
}

function getApiKeyStorage(configObject) {
    var key = configObject.ApiKeyStorage.securityDefinition;
    return configObject.ApiKeyStorage.cacheType.toLowerCase().indexOf('local') == -1 ? sessionStorage.getItem(key) : localStorage.getItem(key);
}

function setApiKeyStorage(configObject, value) {
    var key = configObject.ApiKeyStorage.securityDefinition;
    return configObject.ApiKeyStorage.cacheType.toLowerCase().indexOf('local') == -1 ? sessionStorage.setItem(key, value) : localStorage.setItem(key, value);
}

function removeApiKeyStorage(configObject) {
    var key = configObject.ApiKeyStorage.securityDefinition;
    localStorage.removeItem(key); sessionStorage.removeItem(key);
}

function getBearerStorage(configObject) {
    var key = configObject.BearerStorage.securityDefinition;
    return configObject.BearerStorage.cacheType.toLowerCase().indexOf('local') == -1 ? sessionStorage.getItem(key) : localStorage.getItem(key);
}

function setBearerStorage(configObject, value) {
    var key = configObject.BearerStorage.securityDefinition;
    return configObject.BearerStorage.cacheType.toLowerCase().indexOf('local') == -1 ? sessionStorage.setItem(key, value) : localStorage.setItem(key, value);
}

function removeBearerStorage(configObject) {
    var key = configObject.BearerStorage.securityDefinition;
    localStorage.removeItem(key); sessionStorage.removeItem(key);
}

function sendSwaggerRequest(action, successFunction) {
    var configObject = window.swagger_config, requestObject = { headers: {} };
    var headers = configObject.requestInterceptor(requestObject).headers;
    $.ajax({
        dataType: "json",
        url: '/api/swagger/' + action,
        beforeSend: function (request) { for (var name in headers) request.setRequestHeader(name, headers[name]); },
        success: successFunction
    });
}
