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
function sql() {

    //var res = $.get("https://postman-echo.com/time/now", { show: '当前时间' }); // query string
    //if (res && res.error) console.log(res); else console.log(new Date(res).Add(8 * 3600).DateTime());
    //res = $.post("https://postman-echo.com/post", { strange: 'boom' }); // form submit
    //console.log(res);
    //res = $.q("post", "https://postman-echo.com/post", { strange: 'boom' }, "json");
    //console.log(res);

    console.log($nats.subject, ':', DateTime.Now.js().Time());

    // Trigger a new message
    //var msg = {"Id":0,"Code":"用户2","Type":2,"Message":"【002】登录","Exception":null,"ActionName":"Account.Login","Data":"","CreateTime":"2020-07-27 14:26:38","CreateUser":"a91fb982-6775-490c-9035-4735bdd06b26","AccountName":"002"};
    //$redis.push($nats.prefix+'001', msg, 60);
}
