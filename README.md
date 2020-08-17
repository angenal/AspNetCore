# [ASP.NET Core Docs](https://github.com/dotnet/AspNetCore.Docs)
ASP.NET Core 是一个跨平台的高性能开源框架，用于生成基于云且连接 Internet 的新式应用程序。可用于建立Web应用、 IoT物联网、移动后端等。

#### CLI命令行
    安装模板，创建项目
 > dotnet new --install Microsoft.AspNetCore.SpaTemplates::*<br>
   dotnet new angular # 创建新的项目使用 SPA 模板

~~~shell
# <PM> 从数据库至代码MODEL / DbFirst
> Install-Package Microsoft.EntityFrameworkCore.SqlServer
> Install-Package Microsoft.EntityFrameworkCore.Tools
> Scaffold-DbContext "Server=(localdb)\mssqllocaldb;Database=DbName;Trusted_Connection=True;" \
    Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models  # 脚手架工具Scaffold 用于生成模板代码

# <PM> 从代码MODEL至数据库，名称“Initial”是任意的，用于对迁移文件进行命名  / CodeFirst
> Add-Migration Initial
> Update-Database
$ dotnet ef migrations -h  # 使用命令行

# <PM> WEB页面与代码生成器
> Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 2.0.3
> dotnet aspnet-codegenerator razorpage -m Movie -dc MovieContext -udl -outDir Pages\Movies \
    --referenceScriptLibraries
~~~

~~~shell
# 查看在线.NET Core项目依赖包
> dotnet nuget locals all --list  
  # 复制 http-cache: C:\Users\Administrator\AppData\Local\NuGet\v3-cache
  # 复制 global-packages: C:\Users\Administrator\.nuget\packages\
# 离线还原.NET Core项目依赖包
> dotnet restore --source C:\Users\Administrator\.nuget\packages\
> dotnet build --no-restore   # 生成项目
> dotnet run --no-restore     # 运行项目

~~~
 * [开源的 gRPC](https://www.nuget.org/profiles/grpc-packages)
~~~shell
# 使用 gRPC [ dotnet --version >= v3.0 ] 生命周期>
  # Client（发送请求）-> Client stub（压缩/解压）-> Client RPC Transfer（发送/接收）
  # -> Server RPC Transfer（接收/发送）-> Server stub（解压/压缩）-> Server（处理/响应）
> dotnet tool install -g dotnet-grpc # 安装 dotnet gRPC CLI工具(最新版)
> dotnet new grpc -n gRPC.Services # 新建Server工程 < *.proto文件 <2: option csharp_namespace = "gRPC.Services";
> dotnet-grpc add-file ../gRPC.Protos/*.proto -s Serve # 引入生成的protobuf文件到工程中\Serve\生成C#代码
  # 新建服务类 > 实现gRPC接口(继承) > 配置grpc服务类: endpoints.MapGrpcService<SmsService>();
> dotnet add package Google.Protobuf # 新建Client工程依赖
> dotnet-grpc add-file ../gRPC.Protos/*.proto -s Client # 引入生成的protobuf文件到工程中\Client\生成C#代码

~~~
 * [开源的 MessagePack](https://github.com/neuecc/MessagePack-CSharp)
~~~shell
# 使用 MsgPack [ dotnet --version >= v2.1 ] 项目中安装>
> Install-Package MessagePack
> Install-Package MessagePackAnalyzer
> Install-Package MessagePack.ImmutableCollection
> Install-Package MessagePack.ReactiveProperty
> Install-Package MessagePack.UnityShims
> Install-Package MessagePack.AspNetCoreMvcFormatter

~~~

----

#### [全面的c#帮助类](https://github.com/Jimmey-Jiang/Common.Utility)

 * [Powerful .NET library for benchmark](https://github.com/dotnet/BenchmarkDotNet)
 * [ASP.NET Core 2.x/3.x 综合应用示例](https://github.com/CoreDX9/IdentityServerDemo)
    * [Asp.Net Core 混合全球化与本地化支持](https://www.cnblogs.com/coredx/p/12271537.html)
    * [EntityFramework Core 2.x/3.x （ef core） 在迁移中自动生成数据库表和列说明](https://www.cnblogs.com/coredx/p/10026783.html)
    * [浏览器中的 .Net Core —— Blazor WebAssembly 初体验](https://www.cnblogs.com/coredx/p/12342936.html)
    * [Excel文档操作例子 —— npoi](https://github.com/tonyqus/npoi/tree/master/examples)
 * 类型反射
~~~
// 反射泛型方法，批量处理 Providers 继承于 AuthorizationProvider
foreach (var t in GetType().GetAssembly().GetTypes().Where(t => t.IsPublic && t.IsClass 
    && t.IsSubclassOf(typeof(Abp.Authorization.AuthorizationProvider))))
    Configuration.Authorization.Providers.GetType().GetMethods().FirstOrDefault(i => i.Name == "Add")
    ?.MakeGenericMethod(t).Invoke(Configuration.Authorization.Providers, new object[] { });
~~~
 * 同步`Synchronous`、异步`Asynchronous`
    * 同步：发送方发送请求，然后等待立即答复；可能通过异步I/O发生，但逻辑线程被保留；请求和应答之间的关联通常是隐式的，按请求的顺序执行；错误通常流回同一路径。
    * 异步：发送消息和进行其它事情；回复可能会在另一条路径上返回；消息可以由中介存储；可以多次尝试传递消息。
 ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/sync_async.png)
 * 通信方法`REST`<`RPC`<`Message Event Hub`
 * 消息事件总线`Message Event Hub`
    * 长时间任务/后台任务`Long-Running Work`(one producer, one consumer)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/long_running_work.png)
    * 负载平衡`Load Leveling`(multiple producers, one consumer)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/load_leveling.png)
    * 负载均衡和自动缩放`Load Balancing and Auto Scaling`(multiple producers, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/load_banancing_and_auto_scaling.png)
    * 发布订阅`Publish-Subscribe`(one producer, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/publish_subscribe.png)
    * 分区`Partitioning`(one producer, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/partitioning.png)
    * 多路复用/排他性消费者`Multiplexing with Exclusive Consumers`(multiple producers, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/multiplexing_with_exclusive_consumers.png)
    * 状态处理`Stateful Processing`
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/stateful_processing.png)
    * 稀疏连通性`Sparse Connectivity`
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/sparse_connectivity.png)

----

#### [Util应用框架](https://github.com/dotnetcore/util/)
    旨在提升小型团队的开发输出能力，由常用公共操作类(工具类)、分层架构基类、Ui组件，第三方组件封装，第三方业务接口封装，
     配套代码生成模板，权限等组成。

----

#### [52ABP企业级开发框架](https://www.52abp.com)
 > [ASP.NET Core与Angular开发 - 网易云课堂](https://study.163.com/course/courseMain.htm?courseId=1006191011&share=1&shareId=1151301279)

    一个快速响应，移动优先的符合现代UI设计和SOLID架构的强力开发框架；包含登录，身份验证，用户/角色/权限管理，本地化，
     设置系统，审计日志记录，多租户，UI组件，异常处理系统等功能。

----


#### [阿里云.接口文档](https://help.aliyun.com/learn/developer.html)
 * [.NET SDK](https://github.com/aliyun/aliyun-openapi-net-sdk)、[nuget](https://www.nuget.org/profiles/aliyun-openapi-sdk)

#### [腾讯云.接口文档](https://cloud.tencent.com/document/api/267/30661)
 * [.NET SDK](https://github.com/TencentCloud/tencentcloud-sdk-dotnet)、[nuget](https://www.nuget.org/packages/TencentCloudSDK)

#### [微信.开放平台](https://open.weixin.qq.com)
 * [Senparc.Weixin .NET SDK](https://github.com/JeffreySu/WeiXinMPSDK)
 * [Senparc.Weixin .Tool WeChatSampleBuilder](https://weixin.senparc.com/User)
 * [Senparc SCF框架：模块化SenparcCoreFramework](https://github.com/SenparcCoreFramework/SCF)
 * [Senparc SCF模块：微信管理后台](https://github.com/SenparcCoreFramework/Senparc.Xscf.WeixinManager)

#### [新生命开发团队](https://github.com/NewLifeX)
    学无先后达者为师！技术改变生活！.Net群1600800，嵌入式物联网群1600838

----
