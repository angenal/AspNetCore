//配置订阅任务
subscribe = [
    {
        name: "001", // 订阅名(注："全局订阅前缀"参考yaml配置文件;如果未设置特殊符号,"完整订阅名"=name)
        spec: "+", // 特殊符号(注："+"表示"完整订阅名"="全局订阅前缀"+name 例如："test-001")
        func: function () { return this.name; } // function sql(records)订阅处理/所在目录(默认=name)
    },
    //{
    //    name: "002", // 定时任务名 Quartz.NET Job And Cron Expressions
    //    spec: "0/5 * * * * ?", // 表达式：second minute hour dayOfMonth month dayOfWeek [year?optional]
    //    func: function () { return this.name; }
    //},
];

//支持的功能列表 > 计划任务 function sql(records)

// DateTime.Now.js() => new Date()
// DateTime.Parse("2020-06-01").js()

// console.log(args)
// console.log('{0:G}',new Date)
// console.log('$nats.name:', $nats.name);
// console.log('$nats.prefix:', $nats.prefix);
// console.log('$nats.subject:', $nats.subject);

// $nats.pub('data'); $nats.pub('subj','data')
// $nats.req('data'); $nats.req('data',3); $nats.req('subj','data',3) // timeout:3s
// var res = $nats.req($nats.prefix, 'select') // select subscribes
// var res = $nats.req($nats.prefix, '{"act":"select","data":{"Name":"001"}}') // select subscribes.item

// console.log('$db.prefix:', $db.prefix);
// console.log('$db.subject:', $db.subject);

// var uuid = $db.uuid()
// var guid = $db.guid("N")
// var seconds = $db.timeout()
// $db.timeout(60)
// $db.q: return ResultObject or Array of all rows
// $db.q('select * from table1 where id=@id',{id:1})

// $db.q2: return Cache{ Memory = 0, Redis, Default } ResultObject or Array of all rows
// $db.q2(0,'select * from table1 where id=@id',{id:1})

// $db.g: return ResultValue of first column in first row
// $db.g('select name from table1 where id=@id',{id:1})

// $db.g2: return Cache{ Memory = 0, Redis, Default } ResultValue of first column in first row
// $db.g2(0,'select name from table1 where id=@id',{id:1})

// $db.i: return LastInsertId must int in number-id-column
// $db.i('insert into table1 values(@id,@name)',{id:1,name:'test'})

// $db.x: return RowsAffected all inserted,updated,deleted
// $db.x('update table1 set name=@name where id=@id',{id:1,name:'test'})

// var i = $.crc("text")
// var i = $.md5("text")
// var ok = $.v("n",1)
// var res = $.q("get",url)
// var res = $.q("get",url,param)
// var res = $.q("post",url,param,"json")
// var res = $.get(url)
// var res = $.get(url,param)
// var res = $.post(url,param,"json")

// var seconds = $.timeout()
// var fileName = $.download('https://www.baidu.com/favicon.ico')
// var fileName = $.download('https://www.baidu.com/favicon.ico','my-favicon')
// var tempdir = $.tempdir()
// var tmpfile = $.tempfile(), randomfile = $.tempfile("random")
// var tmpfile = $.tempfile('https://www.baidu.com/', '.html')

// var fs = $.loaddir(path)
// var ok = $.existsdir(path)
// var ok = $.existsfile(path)
// var ok = $.deletefile(path)
// var doc = $.loadfile(path)
// var doc = $.loadhtml(path)
// var doc = $.loadhtml(text)
// var doc = $.loadurl(url)
// doc.save(tmpfile)
// var htm = doc.html('#id1', true) // InnerHtml
// var htm = doc.html('#id1')       // OuterHtml
// var htm = doc.html(['#id1','#id2','#id3'])
// var data = doc.json('#id1 a')
// var data = doc.json(['#id1 a','#id2 a','#id3 a'])
// if (data.Length == 0) return;

// var res = $.compressimage('D:/Temp/01.png', 'pingo -s9 -pngpalette=50 -strip -q') // 压缩图片文件(文件路径,命令行参数,有损压缩模式=true,覆盖文件=true)
// var res = $.compressimage('D:/Temp/02.jpg', 'cjpeg -quality 60,40 -optimize -dct float -smooth 5')
// var res = $.compressimage('D:/Temp/03.gif', 'gifsicle -O3')

// console.log('$cache.prefix:', $cache.prefix);
// console.log('$cache.subject:', $cache.subject);
// $cache.get("key",0) // Memory:0, Cache{ Memory = 0, Redis, Default }
// $cache.set("key",123,60,1) // Expire 60 seconds, Redis:1, Cache{ Memory = 0, Redis, Default }
// $cache.del("key",2) // Default:2, Cache{ Memory = 0, Redis, Default }

// console.log('$redis.prefix:', $redis.prefix);
// console.log('$redis.subject:', $redis.subject);
// $redis.get("key")
// $redis.set("key",123,60)
// $redis.push("key",123,60)
// var list = $redis.pop("key",10)
// var seconds = $redis.expire("key")
// var ok = $redis.expire("key", 60)
// var seconds = $redis.idle("key")
// var ok = $redis.del("key")
