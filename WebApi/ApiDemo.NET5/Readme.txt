--------------------------
1.部署IIS on Windows
--------------------------
  先打开网址>> https://dotnet.microsoft.com/download/dotnet/5.0
  下载运行时>> ASP.NET Core Runtime & IIS runtime support (ASP.NET Core Module v2)
            >> Windows "Hosting Bundle"
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
2.命令行 on Windows
--------------------------
>> 点击:发布 >> 配置：Release; 部署模式：独立
cd /publish
>> 设置urls
set ASPNETCORE_URLS=https://localhost:35443;http://localhost:35000
>> 开始运行
Chat.Web.exe
>> 结束: Press Ctrl+C to shut down.
--------------------------
3.部署Linux
--------------------------
