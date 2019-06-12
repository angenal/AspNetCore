# AspNetCore
ASP.NET Core 是一个跨平台的高性能开源框架，用于生成基于云且连接 Internet 的新式应用程序。可用于建立Web应用、 IoT物联网、移动后端等。

#### 比较全面的c#帮助类，各种功能性代码。
 > https://github.com/Jimmey-Jiang/Common.Utility

#### Util是一个.net core平台下的应用框架，旨在提升小型团队的开发输出能力，由常用公共操作类(工具类)、分层架构基类、Ui组件，第三方组件封装，第三方业务接口封装，配套代码生成模板，权限等组成。
 > https://github.com/dotnetcore/util/

#### 新生命开发团队
    学无先后达者为师！技术改变生活！.Net群1600800，嵌入式物联网群1600838
 > https://github.com/NewLifeX

#### CLI命令行
    安装模板，创建项目
 > dotnet new --install Microsoft.AspNetCore.SpaTemplates::*<br>
   dotnet new angular # 创建新的项目使用 SPA 模板

  // 从数据库至代码MODEL
  // PM> Install-Package Microsoft.EntityFrameworkCore.SqlServer
  // PM> Install-Package Microsoft.EntityFrameworkCore.Tools
  // PM> Scaffold-DbContext "Server=(localdb)\mssqllocaldb;Database=DbName;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models

  // 从代码MODEL至数据库，名称“Initial”是任意的，用于对迁移文件进行命名
  // PM> Add-Migration Initial
  // PM> Update-Database

  // WEB页面与代码生成器
  // PM> Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 2.0.3
  > dotnet aspnet-codegenerator razorpage -m Movie -dc MovieContext -udl -outDir Pages\Movies --referenceScriptLibraries
  
----
