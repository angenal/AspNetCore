CREATE TABLE [dbo].[subscribes] (
	[Id] varchar(36) NOT NULL PRIMARY KEY CLUSTERED, 
	[Name] varchar(100) NOT NULL, 
	[Spec] varchar(2) NOT NULL, 
	[Func] varchar(100) NOT NULL, 
	[Content] text, 
	[CacheDir] varchar(100) NOT NULL, 
	[MsgLimit] int NOT NULL, 
	[BytesLimit] int NOT NULL, 
	[Amount] int NOT NULL, 
	[Bulk] int NOT NULL, 
	[Interval] int NOT NULL, 
	[Version] int NOT NULL DEFAULT ((1)), 
	[Created] bigint NOT NULL DEFAULT (datediff(second,'1970-01-01 00:00:00',getdate())), 
	[Deleted] bit NOT NULL DEFAULT ((0))
) ON [PRIMARY];

GO
CREATE TRIGGER [dbo].[trigger_update_subscribes] ON [subscribes] AFTER UPDATE AS BEGIN
	SET NOCOUNT ON;
	UPDATE [subscribes] SET [Version] = [Version] + 1 WHERE [Id] IN (SELECT [Id] FROM inserted);
END;

GO
CREATE TABLE [dbo].[Sys_Log] (
	[Id] bigint NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED, 
	[Code] varchar(50), 
	[Type] int, 
	[Message] varchar(4000) NOT NULL, 
	[Exception] varchar(2000), 
	[ActionName] varchar(500) NOT NULL, 
	[Data] nvarchar(4000), 
	[CreateTime] datetime NOT NULL, 
	[CreateUser] varchar(36), 
	[AccountName] varchar(30)
) ON [PRIMARY];


INSERT INTO [subscribes] ([Id],[Name],[Spec],[Func],[Content],[CacheDir],[MsgLimit],[BytesLimit],[Amount],[Bulk],[Interval]) VALUES (LOWER(NEWID()),N'001',N'+',N'001',N'//扩展方法
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
    console.log(s);
}',N'001',100000000,1024,0,200,2000);
