CREATE TABLE `subscribes` (
  `Id` varchar(36) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Spec` varchar(2) NOT NULL,
  `Func` varchar(100) NOT NULL,
  `Content` mediumtext,
  `CacheDir` varchar(100) NOT NULL,
  `MsgLimit` int(11) NOT NULL,
  `BytesLimit` int(11) NOT NULL,
  `Amount` int(11) NOT NULL,
  `Bulk` int(11) NOT NULL,
  `Interval` int(11) NOT NULL,
  `Version` int(11) NOT NULL DEFAULT '1',
  `Created` bigint NOT NULL,
  `Deleted` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TRIGGER `trigger_update_subscribes` BEFORE UPDATE ON `subscribes` FOR EACH ROW BEGIN
	SET NEW.`Version` = OLD.`Version` + 1;
END;


CREATE TABLE `sys_log` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) DEFAULT NULL,
  `Type` int(11) NOT NULL,
  `Message` varchar(4000) NOT NULL,
  `Exception` varchar(2000) DEFAULT NULL,
  `ActionName` varchar(500) NOT NULL,
  `Data` varchar(4000) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `CreateUser` varchar(36) DEFAULT NULL,
  `AccountName` varchar(30) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


INSERT INTO `subscribes` (`Id`,`Name`,`Spec`,`Func`,`Content`,`CacheDir`,`MsgLimit`,`BytesLimit`,`Amount`,`Bulk`,`Interval`,`Created`) VALUES (UUID(),'001','+','001','//扩展方法
Date.prototype.Add = function (seconds) { var t = new Date(); t.setTime(this.getTime() + seconds * 1000); return t; };
Date.prototype.AddDate = Date.prototype.AddDays = function (days) { var t = new Date(); t.setTime(this.getTime() + days * 24 * 3600 * 1000); return t; };
Date.prototype.Date = function () { return this.toISOString().split("T")[0]; };
Date.prototype.Time = function () { return this.toISOString().split("T")[1].split(".")[0]; };
Date.prototype.DateTime = function () { return this.toISOString().replace("T", " ").split(".")[0]; };
String.prototype.Date = function () { return this.replace("T", " ").split(" ")[0]; };
String.prototype.Time = function () { return this.replace("T", " ").split(" ")[1].split(".")[0]; };
String.prototype.DateTime = function () { return this.replace("T", " ").split(".")[0]; };
function col(s) { if (!s) { return "NULL"; } return "''" + s.replace("''", "''''") + "''"; };

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

    var res = $.q("get", "https://postman-echo.com/time/now", { show: ''当前时间'' }); // query string
    if (res && res.error) console.log(res); else console.log(new Date(res).Add(8 * 3600).DateTime());
    res = $.q("post", "https://postman-echo.com/post", { strange: ''boom'' }); // form submit
    console.log(res);
    res = $.q("post", "https://postman-echo.com/post", { strange: ''boom'' }, "json");
    console.log(res);

    console.log(''nats.name:'', nats.name);
    console.log(''nats.subject:'', nats.subject);

    var Id = db.i(s) + (items.length-1);
    console.log(''INSERT ROWS:'', items.length, ''LAST INSERT ID:'', Id);

    var rows = db.q(''select * from Sys_Log where Id=@Id'', { Id: Id });
    console.log(''QUERY RESULT:'', rows[0].CreateTime);

    var createTime = db.g(''select CreateTime from Sys_Log where Id=@Id'', { Id: Id });
    console.log(''QUERY RESULT:'', createTime);

    var ids = []; for (var i = items.length - 1; i >= 0; i--) ids.push(Id - i);
    var effected = db.x(''delete from Sys_Log where Id in('' + ids.join('','') + '')'');
    console.log(''DELETE ROWS:'', effected, ids);

    cache.set("Sys_Log", rows[0]);
    var row = cache.get("Sys_Log");
    //console.log(''Cache ROW:'', row);
    console.log(''Cache ROW.Data:'', row.Data);

    //return s;
}','001',100000000,1024,0,200,2000,UNIX_TIMESTAMP());
