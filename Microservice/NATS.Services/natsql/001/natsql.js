//扩展方法
Date.prototype.Add = function (seconds) { var t = new Date(); t.setTime(this.getTime() + seconds * 1000); return t; };
Date.prototype.AddDate = Date.prototype.AddDays = function (days) { var t = new Date(); t.setTime(this.getTime() + days * 24 * 3600 * 1000); return t; };
Date.prototype.Date = function () { return this.toISOString().split("T")[0]; };
Date.prototype.Time = function () { return this.toISOString().split("T")[1].split(".")[0]; };
Date.prototype.DateTime = function () { return this.toISOString().replace("T", " ").split(".")[0]; };
String.prototype.Date = function () { return this.replace("T", " ").split(" ")[0]; };
String.prototype.Time = function () { return this.replace("T", " ").split(" ")[1].split(".")[0]; };
String.prototype.DateTime = function () { return this.replace("T", " ").split(".")[0]; };
function col(s) { if (!s) { return "NULL"; } return "'" + s.replace("'", "''") + "'"; };

//计划任务
function sql(records) {
    //console.log(records);
    if (!records || records.constructor.name != "Array") return "";
    var items = records.filter(function (item) { return item.constructor.name == "Object" && item.hasOwnProperty("Code") && item.hasOwnProperty("Type"); });
    if (items.length == 0) return "";
    var s = "INSERT INTO Sys_Log (Code, Type, Message, Exception, ActionName, Data, CreateTime, CreateUser, AccountName) VALUES"
        + items.map(function (item) {
            return "("
                + col(item.Code) + ","
                + item.Type + ","
                + col(item.Message) + ","
                + col(item.Exception) + ","
                + col(item.ActionName) + ","
                + col(item.Data) + ","
                + col(item.CreateTime.DateTime()) + ","
                + col(item.CreateUser) + ","
                + col(item.AccountName) + ")";
        }).join(",") + ";";
    //console.log(s);

    //var res = $.get("https://postman-echo.com/time/now", { show: '当前时间' }); // query string
    //if (res && res.error) console.log(res); else console.log(new Date(res).Add(8 * 3600).DateTime());
    //res = $.post("https://postman-echo.com/post", { strange: 'boom' }); // form submit
    //console.log(res);
    //res = $.q("post", "https://postman-echo.com/post", { strange: 'boom' }, "json");
    //console.log(res);

    //(function () {
    //    console.log('临时目录', $.loaddir('D:\Temp'));
    //    console.log('临时文件', $.tempdir(), $.tempfile(), $.tempfile("random"));
    //    var fileName = $.download('https://www.baidu.com/favicon.ico', $.tempfile());
    //    console.log(fileName);
    //    var i01 = $.compressimage('D:/Temp/01.png');
    //    if (i01) console.log('压缩图片文件:', i01.OriginalFileName, i01.Percent + '% compressed');
    //    var i02 = $.compressimage('D:/Temp/02.jpg');
    //    if (i02) console.log('压缩图片文件:', i02.OriginalFileName, i02.Percent + '% compressed');
    //    var i03 = $.compressimage('D:/Temp/03.gif');
    //    if (i03) console.log('压缩图片文件:', i03.OriginalFileName, i03.Percent + '% compressed');
    //})();

    (function () {
        var source = '中国网信网';
        var url = 'http://www.cac.gov.cn/';
        var path = $.tempfile(url, '.html'), ok = $.existsfile(path);
        var doc = ok ? $.loadhtml(path) : $.loadurl(url);
        if (!ok) {
            doc.save(path);
            console.log('doc.save:', path);
            return;
        }
        console.log(source, url);
        console.log('工作之窗');
        var n1 = '#tabN div.tab_box div:nth-child(3) div.con div.wlaq_fl_2 ul a';
        console.log(n1);
        console.log('网络安全');
        var n2 = '#tabN div.tab_box div:nth-child(4) div.con ul a';
        console.log(n2);
        console.log('信息化');
        var n3 = '#tabN div.tab_box div:nth-child(5) div.con ul a';
        console.log(n3);
        console.log('教育培训');
        var n4 = '#tabN div.tab_box div:nth-child(7) div.con div.wlaq_fl_2 ul a';
        console.log(n4);
        var data = doc.json(n1, n2, n3, n4);
        if (data.Length == 0) return;
        var item0 = data[0];
        console.log(source, '新增记录:', item0.text);
        //保存数据库
        var sql0 = "select Id from `sys_sitenews` where `Title`=@Title;";
        var old0 = $db.g(sql0, { Title: item0.text });
        if (old0) {
            console.log(source, '新增记录:', 0);
        } else {
            var doc1 = $.loadurl(item0.href);
            if (!doc1) {
                console.log(source, '新增记录:', 0, item0.href);
                return;
            }
            //console.log(doc1.html('#title'));
            var img1 = doc1.json('#BodyLabel img'), cnt1 = doc1.html('#BodyLabel', true);
            var item1 = { ImageUrl: img1.Length > 0 ? img1[0].src : "", Title: item0.text, Content: cnt1, Source: source };
            var sql1 = "INSERT INTO `sys_sitenews`(`Id`,`NodeId`,`ImageUrl`,`FileUrl`,`LinkUrl`,`Title`,`SubTitle`,`Summary`,`Content`,`Remark`,`Source`,`CreateTime`,`CreateUser`,`LastUpdateUserId`,`LastUpdateUser`,`LastUpdateTime`,`NodeType`,`DeleteMark`)";
            sql1 += "VALUES(UUID(), 216, @ImageUrl, NULL, NULL, @Title, NULL, NULL, @Content, NULL, @Source, CURRENT_TIMESTAMP(), '730BD098-1E53-49EB-ACE1-7174EEC76692', '730BD098-1E53-49EB-ACE1-7174EEC76692', '管理员', CURRENT_TIMESTAMP(), '0', b'0');";
            console.log(source, '新增记录:', $db.x(sql1, item1), item0.href);
        }
    })();

    //console.log('DateTime:', DateTime.Now.js().DateTime(), DateTime.Parse("2020-06-01").js().DateTime());
    //console.log('$nats.name:', $nats.name);
    //console.log('$nats.prefix:', $nats.prefix);
    //console.log('$nats.subject:', $nats.subject);

    //if (!$.v("n")) {
    //    while ($.v("n") < 4) {
    //        var n = $.v("n") + 1; $.v("n", n);
    //        console.log('increment.number:', n);
    //    }
    //    // Trigger a new message
    //    //$redis.push($redis.subject, items[0], 60);
    //}

    //setTimeout(function () {
    //    var par = { "act": "select", "data": { "Name": "001" } };
    //    var res = $nats.req($nats.prefix, JSON.stringify(par)) // select subscribes.item
    //    console.log('select subscribes.:', res);
    //}, 4000);

    //var bulkInsert = false;

    //var Id = bulkInsert ? $db.i(items, "Sys_Log", "Id") : db.i(s);
    //if (bulkInsert) console.log('INSERT ROWS:', items.length, 'Affected Rows:', Id);
    //else console.log('INSERT ROWS:', items.length, 'LAST INSERT ID:', Id);

    //var rows = $db.q('select * from Sys_Log where Id=@Id', { Id: Id });
    //if (rows && rows.length == 1) {
    //    console.log('QUERY RESULT:', rows[0].CreateTime);

    //    var createTime = $db.g('select CreateTime from Sys_Log where Id=@Id', { Id: Id });
    //    console.log('QUERY RESULT:', createTime);

    //    var ids = []; for (var i = items.length - 1; i >= 0; i--) ids.push(Id - i);
    //    var effected = $db.x('delete from Sys_Log where Id in(' + ids.join(',') + ')');
    //    console.log('DELETE ROWS:', effected, ids);

    //    // Redis Cache{ Memory = 0, Redis, Default }
    //    var key = "Sys_Log:" + $db.uuid();
    //    console.log('Cache Key:', key);
    //    $cache.set(key, rows[0], 60, 1); // Expire 60 seconds, Redis:1
    //    var row = $cache.get(key, 1);
    //    console.log('Cache ROW:', row);
    //    console.log('Cache ROW.Data:', row.Data);
    //}

    //return s;
}
