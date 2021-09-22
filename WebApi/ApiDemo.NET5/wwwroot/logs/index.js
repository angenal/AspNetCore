// Gets log records
log0Load();
log1Load();

function log0Load() {
    var input = $('#logSearchInput'), btn = $('#log0MoreBtn'), info = $('#log0MoreInfo'), tbl = $('#log0ListTbl');
    var p = parseInt(btn.attr('page')), searchText = $.trim(input.val());
    if (isNaN(p) || p < 1) p = 1;
    if (searchText) searchText = '?search=' + encodeURIComponent(searchText);
    input.val(''); btn.attr('page', p).hide();
    $.ajax({
        type: 'GET',
        url: '/api/log/request/query/' + p + searchText,
        contentType: 'application/json',
        success: (data) => {
            //console.log(data);
            if (!data || data.records == 0) {
                info.html('记录为空').show();
                return;
            }
            info.html('已加载<span id="log00">' + (20 * (p > 1 ? p - 1 : 0) + data.rows.length) + '</span>条记录（共<span id="log01">' + data.records + '</span>条）').show();
            if (data.total > p) btn.attr('page', (p + 1)).show();
            if (searchText) tbl.html('');
            for (var i = 0; i < data.rows.length; i++) {
                var item = data.rows[i];
                var tpl = '<tr><td><span class="text-muted bg-info" style="cursor:pointer" onclick="$(\'#log0' + (item.id.replace('-', '')) + '\').toggle()">' + (item.path) + '</span><br><span class="text-info bg-warning">' + (item.trace) + '</span></td><td class="text-info vmiddle"><pre>' + (item.request) + '</pre></td><td class="text-muted vmiddle">' + (item.time) + '</td><td class="vmiddle"><button type="button" class="btn btn-xs btn-danger" page="1" onclick="log0Delete(this,\'' + (item.id) + '\')">删除</button></td></tr>';
                tpl += '<tr id="log0' + (item.id.replace('-', '')) + '" style="display:none"><td colspan="4" align="left"><pre>' + (item.response) + '</pre></td></tr>';
                //console.log(tpl);
                tbl.append($(tpl));
            }
        },
        error: (errMsg) => {
            console.log(errMsg);
        }
    });
}

function log1Load() {
    var input = $('#logSearchInput'), btn = $('#log1MoreBtn'), info = $('#log1MoreInfo'), tbl = $('#log1ListTbl');
    var p = parseInt(btn.attr('page')), searchText = $.trim(input.val());
    if (isNaN(p) || p < 1) p = 1;
    if (searchText) searchText = '?search=' + encodeURIComponent(searchText);
    input.val(''); btn.attr('page', p).hide();
    $.ajax({
        type: 'GET',
        url: '/api/log/exception/query/' + p + searchText,
        contentType: 'application/json',
        success: (data) => {
            //console.log(data);
            if (!data || data.records == 0) {
                info.html('记录为空').show();
                return;
            }
            info.html('已加载<span id="log10">' + (20 * (p > 1 ? p - 1 : 0) + data.rows.length) + '</span>条记录（共<span id="log11">' + data.records + '</span>条）').show();
            if (data.total > p) btn.attr('page', (p + 1)).show();
            if (searchText) tbl.html('');
            for (var i = 0; i < data.rows.length; i++) {
                var item = data.rows[i];
                var tpl = '<tr><td><span class="text-muted bg-info" style="cursor:pointer" onclick="$(\'#log1' + (item.id.replace('-', '')) + '\').toggle()">' + (item.path) + '</span><br><span class="text-danger bg-warning">' + (item.trace) + '</span></td><td class="text-danger vmiddle">' + (item.message) + '</td><td class="text-muted vmiddle">' + (item.time) + '</td><td class="vmiddle"><button type="button" class="btn btn-xs btn-danger" page="1" onclick="log1Delete(this,\'' + (item.id) + '\')">删除</button></td></tr>';
                tpl += '<tr id="log1' + (item.id.replace('-', '')) + '" style="display:none"><td colspan="4" align="left"><pre>' + (item.content) + '</pre></td></tr>';
                //console.log(tpl);
                tbl.append($(tpl));
            }
        },
        error: (errMsg) => {
            console.log(errMsg);
        }
    });
}

function logSearch(e) {
    if (e.keyCode != 13) return;
    $('#log0MoreBtn').attr('page', '1').hide();
    $('#log1MoreBtn').attr('page', '1').hide();
    log0Load();
    log1Load();
}

function log0Delete(btn, id) {
    var tr = $(btn).parent().parent(), tr1 = $('#log0' + id.replace('-', ''));
    $.ajax({
        type: 'DELETE',
        url: '/api/log/request/delete/' + id,
        contentType: 'application/json',
        success: (data) => {
            //console.log(data);
            tr.remove(); tr1.remove();
            $('#log00').text((parseInt($('#log00').text()) - 1));
            $('#log01').text((parseInt($('#log01').text()) - 1));
        },
        error: (errMsg) => {
            console.log(errMsg);
        }
    });
}

function log1Delete(btn, id) {
    var tr = $(btn).parent().parent(), tr1 = $('#log1' + id.replace('-', ''));
    $.ajax({
        type: 'DELETE',
        url: '/api/log/exception/delete/' + id,
        contentType: 'application/json',
        success: (data) => {
            //console.log(data);
            tr.remove(); tr1.remove();
            $('#log10').text((parseInt($('#log10').text()) - 1));
            $('#log11').text((parseInt($('#log11').text()) - 1));
        },
        error: (errMsg) => {
            console.log(errMsg);
        }
    });
}
