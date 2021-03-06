# [ASP.NET Core Docs](https://github.com/dotnet/AspNetCore.Docs)
ASP.NET Core 是一个跨平台的高性能开源框架，用于生成基于云且连接 Internet 的新式应用程序。<br>可用于建立Web应用、 IoT物联网、移动后端等。

#### CLI命令行

```shell
#! 安装工具
   dotnet tool install -g csharprepl # csharp console playground
   dotnet tool install -g dotnet-ef  # dotnet ef database-migrations tool
   dotnet tool install -g dotnet-trace # 监控.NET程序的GC; 获取所有进程: dotnet trace ps 诊断指定程序: dotnet gcmon -p 1024

#! 安装模板，创建项目
   dotnet new console                # Common/Console
   dotnet new classlib               # Common/Library
   dotnet new web                    # Web/Empty
   dotnet new webapi                 # Web/WebAPI
   dotnet new mvc                    # Web/MVC
   dotnet new sln                    # Solution
   dotnet new globaljson             # Config
   dotnet new nugetconfig            # Config
   dotnet new webconfig              # Config
   dotnet new --install Microsoft.AspNetCore.SpaTemplates::* # install templates from official repository
   dotnet new angular                # 创建新的项目使用 SPA 模板
   dotnet new react                  # Web/MVC/SPA
   dotnet new reactredux             # Web/MVC/SPA
   dotnet new --install [path-to-repository] # install templates from src (exists *dotnet-templates.nuspec)
   dotnet new avalonia.app           # ui/xaml from https://github.com/AvaloniaUI/avalonia-dotnet-templates
   dotnet new avalonia.mvvm          # ui/xaml
   dotnet new avalonia.usercontrol   # ui/xaml
   dotnet new avalonia.window        # ui/xaml
   dotnet new avalonia.resource      # ui/xaml
   dotnet new avalonia.styles        # ui/xaml
   ... ...                           # dotnet new --help
```
~~~shell
# <PM> 从数据库至代码MODEL / DbFirst
PM> Install-Package Microsoft.EntityFrameworkCore.SqlServer
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
# 脚手架工具Scaffold 用于生成模板代码
# Scaffold-DbContext {ConnectionString} Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models/Entities -Context {DbName}DbContext -ContextDir Models -DataAnnotations -Force
PM> Scaffold-DbContext "Server=(localdb)\mssqllocaldb;Database=DbName;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models

# <PM> 从代码MODEL至数据库，名称“Initial”是任意的，用于对迁移文件进行命名  / CodeFirst
PM> Add-Migration Initial    # 添加迁移版本的名称
PM> Update-Database          # 迁移至数据库
PS> dotnet ef migrations -h  # 使用命令行CMD或PowerShell
PS> dotnet ef migrations add Initial
PS> dotnet ef database update

# <PM> WEB页面与代码生成器
PM> Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 2.0.3
PM> dotnet aspnet-codegenerator razorpage -m Movie -dc MovieContext -udl -outDir Pages\Movies --referenceScriptLibraries

# <PM> Desktop桌面跨平台应用
PM> Install-Package Avalonia            # ui/xaml from https://github.com/AvaloniaUI/Avalonia
PM> Install-Package Avalonia.Desktop
~~~

#### 构建与发布项目
~~~shell
# 查看在线.NET Core项目依赖包
PS> dotnet nuget locals all --list
  # 复制 http-cache: C:\Users\Administrator\AppData\Local\NuGet\v3-cache
  # 复制 global-packages: C:\Users\Administrator\.nuget\packages\
# 离线还原.NET Core项目依赖包
PS> dotnet restore --source C:\Users\Administrator\.nuget\packages\ # 离线还原NuGet依赖包
PS> dotnet run --no-restore             # 运行项目(不还原NuGet依赖包)
PS> dotnet build --no-restore           # 生成项目(不还原NuGet依赖包)
PS> dotnet build ./*.csproj -c Release  # 生成项目(要还原NuGet依赖包) 接下来，发布项目。
PS> dotnet publish -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -f net5.0 -r linux-x64 -o "./bin/Release/net5.0/publish/linux-x64"
PS> dotnet publish -c Release /p:PublishSingleFile=false /p:PublishTrimmed=false -f net5.0 -r win-x64 -o "./bin/Release/net5.0/publish/win-x64"
PS> dotnet publish ./*.csproj -c Release -r win-x64 -o "./bin/Release/net5.0/publish/win-x64"
~~~

#### [`NuGet`](https://www.nuget.org/)包管理器 [官方NuGet程序包源](https://docs.microsoft.com/zh-cn/nuget)、[微软推荐托管](https://docs.microsoft.com/zh-cn/nuget/hosting-packages/overview)、[本地`NuGet.Server`托管](https://docs.microsoft.com/zh-cn/nuget/hosting-packages/nuget-server)、[开源`BaGet`托管](https://loic-sharma.github.io/BaGet/)
~~~shell
# 安装
docker run --name nuget-server -itd -p 8880:80 --restart=always --env-file baget.env -v "$(pwd)/baget-data:/var/baget" loicsharma/baget:latest
# 配置 baget.env # ApiKey=
# 打包
dotnet pack
# 发布
dotnet nuget push -s http://nuget.xxx.com/v3/index.json -k {ApiKey} {PackageId}.{Version}.nupkg
~~~


#### 开源框架及应用

 * [开源的 gRPC](https://www.nuget.org/profiles/grpc-packages)
~~~shell
# 使用 gRPC [ dotnet --version >= v3.0 ] 生命周期>
  # Client（发送请求）-> Client stub（压缩/解压）-> Client RPC Transfer（发送/接收）
  # -> Server RPC Transfer（接收/发送）-> Server stub（解压/压缩）-> Server（处理/响应）
> dotnet tool install -g dotnet-grpc # 安装 dotnet gRPC CLI工具(最新版)
> dotnet new grpc -n gRPC.Services # 新建Server工程< *.proto文件<: option csharp_namespace="gRPC.Services"
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

 * 开源的 应用框架
    - [.Net Core](https://docs.microsoft.com/zh-cn/aspnet/core/)
    - [ABP](http://aspnetboilerplate.com)
    - [Nop](https://www.nopcommerce.com)
    - [ServiceStack](https://servicestack.net)

 * 开源的 扩展框架
    - [ASP.NET Core 综合应用](https://github.com/CoreDX9/IdentityServerDemo)
       - [`ABAC`基于属性的访问控制(Attribute-Based Access Control)](https://casbin.org/)
       - [`RBAC`基于角色的访问控制(Role-Based Access Control)](https://casbin.org/)→`MongoDB`→[`goRBAC`](https://github.com/mikespook/gorbac)→[`AuthBoss`](https://github.com/volatiletech/authboss)
       - [`CBAC`基于声明的访问控制(Claim-Based Access Control)](https://docs.microsoft.com/zh-cn/openspecs/windows_protocols/ms-azod/e600249e-247b-469c-8979-e0c578adfbe6)
         - [`身份认证`](https://www.cnblogs.com/JulianHuang/p/13725873.html)
            - `Claim`*申明*身份的片段数据→`ClaimsIdentity`*单个身份*信息→`ClaimsPrincipal`*主体*身份的集合
         - [`访问控制(casbin)`](https://casbin.org/)→[`资源命名(upspin)`](https://github.com/upspin/upspin)→[`准入控制(AdmissionControl)`](https://juejin.cn/book/6844733753063915533/section/6844733753131008007)
         - [`访问权限` Asp.Net Core Identity](https://docs.microsoft.com/zh-cn/aspnet/core/security/authentication/identity?tabs=visual-studio%2Caspnetcore2x)、[Identity Server 4](https://identityserver4.readthedocs.io/en/latest/)
       - [Asp.Net Core 混合全球化与本地化支持](https://www.cnblogs.com/coredx/p/12271537.html)
       - [EntityFramework Core 2.x/3.x （ef core） 在迁移中自动生成数据库表和列说明](https://www.cnblogs.com/coredx/p/10026783.html)
       - [应用程序容器化](https://docs.microsoft.com/zh-cn/dotnet/core/docker/build-container)
    - [服务注入Ioc Autofac](https://autofaccn.readthedocs.io/zh/latest/)
    - [切面编程Aop AspectCore-Framework](https://github.com/dotnetcore/AspectCore-Framework)
    - [序列化Json Json.Net](https://www.newtonsoft.com/json)
    - [映射模型 AutoMapper](http://automapper.org/)
    - [高性能存储 microsoft.FASTER](https://github.com/microsoft/FASTER)、[FASTER.KV replace ConcurrentDictionary](https://microsoft.github.io/FASTER/docs/fasterkv-basics)、[FASTER.Log unique hybrid records](https://microsoft.github.io/FASTER/docs/fasterlog-basics/)
    - [日志 NLog](http://nlog-project.org/)、[Exceptionless](https://github.com/exceptionless)
    - [缓存 EasyCaching](https://github.com/dotnetcore/EasyCaching)
    - [事件总线 CAP](https://github.com/dotnetcore/CAP)
    - [二维码 QRCoder](https://github.com/codebude/QRCoder)
    - [开源OCR .Net wrapper](https://github.com/charlesw/tesseract) for [tesseract-ocr](https://github.com/tesseract-ocr/tesseract)
    - [开源Office组件`NPOI`](https://techbrij.com/export-excel-xls-xlsx-asp-net-npoi-epplus)
    - [破解Office组件`Spire`](https://www.board4all.biz/threads/e-iceblue-spire-office-platinum-v5-3-7-net4-0-netcore-netstandard-wpf-license-key.849643/)[`推荐`](https://www.board4all.biz/tags/e-iceblue-spireoffice/)、[破解Office组件`Aspose`](https://www.board4all.biz/threads/aspose-cells-words-slides-pdf-for-net.849528/)[`推荐`](https://www.board4all.biz/tags/aspose/)
    - [破解Office组件`DevExpress(推荐)`](https://github.com/angenalZZZ/AspNet/tree/master/src/Office),[下载`DevExpress`for`.doc,docx,txt,rtf,mht,odt,xml,epub,html`,`excel`,`pdf`](https://github.com/angenalZZZ/doc-zip/commit/7b8b2276f5cc07c587b433ef9b26e09f0ef04bfe)
    - [分布式主键ID生成器(雪花漂移算法500W/s)](https://github.com/yitter/IdGenerator)、[Id生成器 ECommon.Utilities.ObjectId](https://github.com/tangxuehua/ecommon/blob/master/src/ECommon/Utilities/ObjectId.cs)
    - [Linq扩展 System.Linq.Dynamic.Core](https://github.com/StefH/System.Linq.Dynamic.Core)
    - [科学计算 MathNet](https://github.com/mathnet/mathnet-numerics)
    - [定时任务 Quartz.Net](https://www.quartz-scheduler.net)
    - [包装调用本机 Windows API](https://github.com/dahall/vanara)
    - [单元测试/库 Verify](https://github.com/VerifyTests/Verify)
    - [基准/压测/库 Powerful .NET library for benchmark](https://github.com/dotnet/BenchmarkDotNet)
    - [基准/压测/工具 NBomber](https://github.com/PragmaticFlow/NBomber)

 * 开源的 Sql ORM
    - [EntityFrameworkCore](https://docs.microsoft.com/zh-cn/ef/core)
    - [Dapper](https://github.com/StackExchange/Dapper)
    - [FreeSql](https://github.com/dotnetcore/FreeSql/wiki)
    - [SqlSugar](https://github.com/sunkaixuan/SqlSugar/wiki)

 * 开源的 跨语言+跨平台+远程调用(RPC,gRPC等)
    - [NATS - 开源消息系统](https://nats.io/)
    - [ZeroMQ 跨语言,LGPLed解决方案](https://zeromq.org/)
    - [Hprose 跨语言,跨平台,无侵入式,高性能动态远程对象调用引擎库](https://hprose.com/)
    - [microsoft.ClearScript](https://github.com/microsoft/ClearScript) supports JavaScript (via [V8](https://developers.google.com/v8/) and [JScript](https://docs.microsoft.com/en-us/previous-versions//hbxc2t98(v=vs.85))) and [VBScript](https://docs.microsoft.com/en-us/previous-versions//t0aew7h6(v=vs.85)), [Examples](https://microsoft.github.io/ClearScript/Examples/Examples.html), [Tutorial](https://microsoft.github.io/ClearScript/Tutorial/FAQtorial.html)

 * 实时应用
    - [.NET Core SignalR 官方文档](https://docs.microsoft.com/zh-cn/aspnet/core/signalr)

 * `界面`UI`桌面软件`(WinFrom、WPF、DotnetCore^3)
    - [开源WPF控件库`MaterialDesignInXAML`](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)、[`PanuonUI`](https://github.com/Panuon/PanuonUI)、[`HandyControl`](https://github.com/HandyOrg/HandyControl)、[Beautiful WPF Control UI](https://github.com/aduskin/AduSkin)
    - [.NET Core Blazor 官方文档](https://docs.microsoft.com/zh-cn/aspnet/core/blazor)
    - [Ant Design 的 Blazor 组件库 `推荐`服务于企业级后台产品](https://ant-design-blazor.gitee.io/zh-CN/docs/i18n)
    - [Bootstrap 的 Blazor 组件库 `推荐`](https://gitee.com/LongbowEnterprise/BootstrapBlazor)、[官方文档](https://www.blazor.zone/)
    - [DevExpress 控件库,工具,框架 `推荐`](https://www.baidu.com/s?wd=devexpress%20%E7%A0%B4%E8%A7%A3)、[示例代码](https://github.com/DevExpress-Examples)
    - [`安装`制作 WiX Toolset & Visual Studio Extension](https://wixtoolset.org/releases/)
    - [`部署`发布 ClickOnce 应用程序](https://docs.microsoft.com/zh-cn/visualstudio/deployment/publishing-clickonce-applications?view=vs-2019)
    - [`工具` Advanced Installer v18.8 破解版](https://www.board4all.biz/threads/advanced-installer-architect-v18-8.860572/)
    - [`工具` setup factory v9.5.3 汉化版](http://www.xz7.com/downinfo/493875.html)

 * `物联网`应用
    - [UHF RFID「超高频射频识别&标签读取器」二次开发](https://gitee.com/angenal/RFID-Toolkit)

 * `代码`生成器
    - [CodeSmith](https://www.codesmithtools.com)

 * `扩展` Microsoft Visual Studio
    - [.NET Core Debugging with WSL 2](https://aka.ms/wsldebug)
    - [搭建公网测试→转发本地流量→ Ngrok Extensions](https://marketplace.visualstudio.com/items?itemName=DavidProthero.NgrokExtensions)

 * 快速开发`解决方案`
    - [Admin `后台`](https://github.com/zhontai/Admin.Core) + [`演示`](https://www.admin.zhontai.net) + [`文档`](https://www.zhontai.net)
    - [Bootstrap Blazor 组件库 `推荐`](https://www.blazor.zone)、[Bootstrap Blazor 开源项目](https://gitee.com/LongbowEnterprise/BootstrapBlazor)
    - [NetModular 后端](https://github.com/iamoldli/NetModular)、[前端](https://github.com/iamoldli/NetModular.UI)
    - [Furion 基于.NET5/6平台 `推荐`](https://dotnetchina.gitee.io/furion/)

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

----

#### [新生命开发团队](https://github.com/NewLifeX)
    学无先后达者为师！技术改变生活！.Net群1600800，嵌入式物联网群1600838

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

#### [C#帮助类](https://github.com/Jimmey-Jiang/Common.Utility)

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
 * 通信方法`REST` < `RPC` < `Message Event Hub` [dotnet presentations conf](https://github.com/dotnet-presentations/dotNETConf/tree/master/2020/FocusOnMicroservices/Technical), [`Easy.MessageHub`](https://github.com/NimaAra/Easy.MessageHub)
 * 消息事件总线`Message Event Hub`
    * 后台任务/长时间任务`Long-Running Work`(one producer, one consumer)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/long_running_work.png)
    * 负载平衡/自动缩放生产`Load Leveling`(multiple producers, one consumer)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/load_leveling.png)
    * 负载均衡/自动缩放消费`Load Balancing and Auto Scaling`(multiple producers, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/load_banancing_and_auto_scaling.png)
    * 发布订阅`Publish-Subscribe`(one producer, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/publish_subscribe.png)
    * 发布分流`Partitioning`(one producer, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/partitioning.png)
    * 多路复用/排它性消费`Multiplexing with Exclusive Consumers`(multiple producers, multiple consumers)
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/multiplexing_with_exclusive_consumers.png)
    * 状态处理`Stateful Processing`
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/stateful_processing.png)
    * 稀疏连通性`Sparse Connectivity`
    ![](https://github.com/angenalZZZ/AspNetCore/blob/master/screenshots/sparse_connectivity.png)

----
