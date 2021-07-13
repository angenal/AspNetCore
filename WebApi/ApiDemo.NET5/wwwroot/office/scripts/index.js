//生成预览
function viewDoc(filename) {
    showLoading('body', '正在生成预览...');

    $.ajax({
        url: '/api/Office/PreviewA?filename=' + filename,
        success: function (url) {
            closeLoading();
            if (!url) return alert('生成失败');
            var d = new Dialog();
            d.URL = url;
            d.Title = '';
            d.Height = 720;
            d.Width = 1010;
            d.show();
            //打开新的网页
            //$('#hidePopupDialog').attr('href', url + '?iframe=true&height=720&width=1010').click();
        },
        error: function () {
            closeLoading();
            alert('生成失败');
        }
    });
}

//加载遮罩
var showLoading = function (elementTag, message) {
    var msg = message ? message : '努力加载中...';
    $('<div class=\"datagrid-mask\"></div>').css({ display: 'block', width: '100%', height: $(elementTag).height() }).appendTo(elementTag);
    $('<div class=\"datagrid-mask-msg\"></div>').html(msg).appendTo(elementTag).css({ display: 'block', left: '30%', top: ($(elementTag).height() - 45) / 2 });
};

//关闭遮罩
var closeLoading = function () {
    $('.datagrid-mask').remove();
    $('.datagrid-mask-msg').remove();
};
