--------------------------
1.部署IIS on Windows
--------------------------
  先打开网址>> https://dotnet.microsoft.com/download/dotnet/5.0
  下载运行时>> ASP.NET Core Runtime & IIS runtime support (ASP.NET Core Module v2)
            >> Windows "Hosting Bundle"
            >> https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-aspnetcore-5.0.9-windows-hosting-bundle-installer
  安装运行时>> dotnet-hosting-5.x.x-win.exe
  发布该项目>> 新建:发布 >> 目标：文件夹 >> 设置文件夹位置>完成
            >> 包含发布文件*修改：web.config 文件属性(复制到输出目录) <system.webServer><handlers>..modules="AspNetCoreModuleV2"..
            >> 点击:发布 >> 配置：Release; 目标运行时：可移植; 目标框架：net5.0; 部署模式：框架依赖(非独立*需提前安装运行时)
  设置IIS >> 添加网站 >> 内容目录 >> 物理路径(指定为发布的输出目录)
          >> 绑定域名: https://api.xxx.com:443;http://api.xxx.com:80 或端口
          >> 应用程序池 >> 基本设置 >> .NET CLR 版本：无托管代码 >>依赖IIS模块AspNetCoreModuleV2<<目标框架：.NET 5.0 >> 不再属于.NET Framework
          >> 应用程序池 >> 高级设置 >> 进程模型 >> 标识(应用程序标识) >> 内置账户：LocalSystem (用于提高调用系统资源的权限)
          >> 重启网站

--------------------------
2.命令行 on Windows for Server: Kestrel
--------------------------
set output=./bin/Release/net5.0/publish
if exist "%output%" rd /S /Q "%output%"
dotnet publish -c Release /p:PublishSingleFile=false /p:PublishTrimmed=false -f net5.0 -r win-x64 -o "%output%/win-x64"
# dotnet build ./ApiDemo.NET5.csproj -c Release && dotnet publish ./ApiDemo.NET5.csproj -c Release -f net5.0 -r win-x64 -o "%output%/win-x64"
--------------------------
>> 点击:发布 >> 配置：Release; 部署模式：独立
cd /publish
>> 设置urls
set ASPNETCORE_URLS=https://localhost:35443;http://localhost:35000
>> 开始运行
Chat.Web.exe
>> 结束: Press Ctrl+C to shut down.

--------------------------
3.部署Linux https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-ubuntu
--------------------------
set output=./bin/Release/net5.0/publish
if exist "%output%" rd /S /Q "%output%"
dotnet publish -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -f net5.0 -r linux-x64 -o "%output%/linux-x64"
--------------------------


--------------------------
I.使用ngrok部署“外网访问”
--------------------------
# 先获取您的授权码 https://dashboard.ngrok.com
ngrok -help
ngrok authtoken [授权码]
ngrok http 35000 -host-header="localhost:35000"
# 查看运行状态
start https://dashboard.ngrok.com/endpoints/status
# 查看访问流量
start http://localhost:4040/inspect/http


--------------------------
其他相关资源
--------------------------
ASP.NET Core SignalR JavaScript 客户端 https://docs.microsoft.com/zh-cn/aspnet/core/signalr/javascript-client?view=aspnetcore-5.0
配置文档参考 https://docs.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-5.0&tabs=javascript#configure-additional-options
--------------------------
// javascript 参考 WebApi/ApiDemo.NET5/wwwroot/signalr/index.html
