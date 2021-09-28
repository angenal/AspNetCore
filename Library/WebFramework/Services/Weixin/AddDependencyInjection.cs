using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.CO2NET.AspNet;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Cache.Redis;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.TenPay;
using Senparc.Weixin.Work;
using Senparc.Weixin.WxOpen;
using System.IO;

namespace WebFramework.Services.Weixin
{
    /// <summary>
    /// 微信配置：公众号、小程序、企业微信、微信支付、开放平台、代理参数等
    /// </summary>
    public static class AddDependencyInjection
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddWeixin(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            // 微信CO2NET通用设置 https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/
            services.AddSenparcGlobalServices(config);
            #region appsettings.json
            /*
              //微信CO2NET设置
              "SenparcSetting": {
                "IsDebug": true,
                "DefaultCacheNamespace": "DefaultCache",
                "Cache_Redis_Configuration": "localhost:6379,defaultDatabase=0,password=senparc,connectTimeout=2000,connectRetry=2,syncTimeout=10000",
                "Cache_Memcached_Configuration": "",
                "SenparcUnionAgentKey": ""
              }
            */
            #endregion

            // 微信公众号小程序通用设置 https://github.com/JeffreySu/WeiXinMPSDK/blob/master/Samples/netcore3.1-mvc/Senparc.Weixin.Sample.NetCore3/
            services.AddSenparcWeixinServices(config);
            #region appsettings.json
            /*
              //微信CO2NET设置
              "SenparcSetting": {
                "IsDebug": true,
                "DefaultCacheNamespace": "DefaultCache",
                "Cache_Redis_Configuration": "localhost:6379,defaultDatabase=0,password=senparc,connectTimeout=2000,connectRetry=2,syncTimeout=10000",
                "Cache_Memcached_Configuration": "",
                "SenparcUnionAgentKey": ""
              },
              //微信通用设置
              "SenparcWeixinSetting": {
                "IsDebug": true,

                //公众号
                "Token": "",
                "EncodingAESKey": "",
                "WeixinAppId": "",
                "WeixinAppSecret": "",

                //小程序
                "WxOpenAppId": "",
                "WxOpenAppSecret": "",
                "WxOpenToken": "",
                "WxOpenEncodingAESKey": "",

                //企业微信
                "WeixinCorpId": "",
                "WeixinCorpAgentId": "",
                "WeixinCorpSecret": "",
                "WeixinCorpToken": "",
                "WeixinCorpEncodingAESKey": "",

                //微信支付
                "TenPayV3_AppId": "",
                "TenPayV3_AppSecret": "",
                "TenPayV3_SubAppId": "",
                "TenPayV3_SubAppSecret": "",
                "TenPayV3_MchId": "",
                "TenPayV3_SubMchId": "", //子商户，没有可留空
                "TenPayV3_Key": "",
                "TenPayV3_CertPath": "", //（新）支付证书物理路径，如：C:\\cert\\apiclient_cert.p12
                "TenPayV3_CertSecret": "", //（新）支付证书密码（原始密码和 MchId 相同）
                "TenPayV3_TenpayNotify": "", // https://YourDomainName/TenpayV3/PayNotifyUrl
                "TenPayV3_WxOpenTenpayNotify": "", // https://YourDomainName/TenpayV3/PayNotifyUrlWxOpen

                //开放平台
                "Component_Appid": "",
                "Component_Secret": "",
                "Component_Token": "",
                "Component_EncodingAESKey": "",

                //代理参数
                "AgentUrl": "",
                "AgentToken": "",
                "SenparcWechatAgentKey": "",

                //根据实际情况填写
                "Items": {
                  "第二个公众号": {
                    "Token": "",
                    "EncodingAESKey": "",
                    "WeixinAppId": "",
                    "WeixinAppSecret": ""
                  },
                  "第三个公众号": {
                    "Token": "",
                    "EncodingAESKey": "",
                    "WeixinAppId": "",
                    "WeixinAppSecret": ""
                  },
                  "第二个小程序": {
                    "WxOpenAppId": "",
                    "WxOpenAppSecret": "",
                    "WxOpenToken": "",
                    "WxOpenEncodingAESKey": ""
                  },
                  "第四个公众号+对应小程序+对应微信支付": {
                    //公众号
                    "Token": "",
                    "EncodingAESKey": "",
                    "WeixinAppId": "",
                    "WeixinAppSecret": "",
                    //小程序
                    "WxOpenAppId": "",
                    "WxOpenAppSecret": "",
                    "WxOpenToken": "",
                    "WxOpenEncodingAESKey": ""
                    //微信支付V3
                  }
                }
              },
            */
            #endregion

            //Senparc.WebSocket 注册（按需）
            //services.AddSenparcWebSocket<CustomNetCoreWebSocketMessageHandler>();

            //HttpClient 请求证书（按需）
            //services.AddCertHttpClient("name", "pwd", "path");

            //设置全局 Debug 状态
            //var isDebug = env.IsDevelopment();

            //API请求日志
            if (Senparc.CO2NET.Config.IsDebug)
            {
                string logPath = Path.Combine(env.ContentRootPath, "App_Data", "SenparcTraceLog");
                if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
            }

            //微信相关数据
            Data.MP.InitTextRequestReply();

            return services;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public static IApplicationBuilder UseWeixin(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            //启用 GB2312（按需）
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //引入EnableRequestRewind中间件（按需）
            //app.UseEnableRequestRewind();

            var senparcSetting = app.ApplicationServices.GetService<IOptions<SenparcSetting>>();
            var senparcWeixinSetting = app.ApplicationServices.GetService<IOptions<SenparcWeixinSetting>>(); //Senparc.Weixin.Config.SenparcWeixinSetting

            // 启动 CO2NET 全局注册，必须！
            //IRegisterService register = RegisterService.Start(senparcSetting.Value);
            //register.UseSenparcGlobal(true); // 自动扫描自定义扩展缓存（二选一）
            //register.UseSenparcGlobal(false, () => GetExCacheStrategies(senparcSetting.Value)); // 指定自定义扩展缓存（二选一）
            app.UseSenparcGlobal(env, senparcSetting.Value, register =>
            {
                //当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
                register.ChangeDefaultCacheNamespace($"CO2NET.{env.ApplicationName}");

                #region 全局缓存配置（按需）

                //配置全局使用Redis缓存（按需）
                var redisConfigurationStr = senparcSetting.Value.Cache_Redis_Configuration;
                if (!string.IsNullOrEmpty(redisConfigurationStr))
                {
                    /* 说明：
                     * 1、Redis 的连接字符串信息会从 Config.SenparcSetting.Cache_Redis_Configuration 自动获取并注册，如不需要修改，下方方法可以忽略
                     * 2、如需手动修改，可以通过下方 SetConfigurationOption 方法手动设置 Redis 链接信息（仅修改配置，不立即启用）
                     */
                    Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);

                    //以下会立即将全局缓存设置为 Redis
                    Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow();//键值对缓存策略（推荐）
                                                                              //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();//HashSet储存格式的缓存策略
                                                                              //也可以通过以下方式自定义当前需要启用的缓存策略
                                                                              //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//键值对
                                                                              //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
                }
                //配置Memcached缓存（按需）
                //var memcachedConfigurationStr = senparcSetting.Value.Cache_Memcached_Configuration;
                //if (!string.IsNullOrEmpty(memcachedConfigurationStr))
                //{
                //    app.UseEnyimMemcached();

                //    /* 说明：
                //    * 1、Memcached 的连接字符串信息会从 Config.SenparcSetting.Cache_Memcached_Configuration 自动获取并注册，如不需要修改，下方方法可以忽略
                //    * 2、如需手动修改，可以通过下方 SetConfigurationOption 方法手动设置 Memcached 链接信息（仅修改配置，不立即启用）
                //    */
                //    Senparc.CO2NET.Cache.Memcached.Register.SetConfigurationOption(redisConfigurationStr);

                //    //以下会立即将全局缓存设置为 Memcached
                //    Senparc.CO2NET.Cache.Memcached.Register.UseMemcachedNow();

                //    //也可以通过以下方式自定义当前需要启用的缓存策略
                //    CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);
                //}

                #endregion

                // 注册日志（按需）
                register.RegisterTraceLog(ConfigTraceLog); //配置TraceLog

                Senparc.CO2NET.APM.Config.EnableAPM = false;//默认开启，如果需要关闭，则设置为 false
                //Senparc.CO2NET.APM.Config.DataExpire = TimeSpan.FromMinutes(60);
            })
            // 启动 微信公众号小程序 全局注册，必须！
            .UseSenparcWeixin(senparcWeixinSetting.Value, register =>
            {
                #region 全局缓存配置（按需）

                //配置全局使用Redis缓存（按需）
                var redisConfigurationStr = senparcSetting.Value.Cache_Redis_Configuration;
                if (!string.IsNullOrEmpty(redisConfigurationStr))
                {
                    register.UseSenparcWeixinCacheRedis();//StackExchange.Redis
                }

                #endregion

                #region 注册公众号或小程序（按需）

                var items = senparcWeixinSetting.Value.Items;

                //注册公众号（可注册多个）
                if (!string.IsNullOrEmpty(senparcWeixinSetting.Value.WeixinAppId))
                {
                    register.RegisterMpAccount(senparcWeixinSetting.Value, "公众号");
                    foreach (string key in items.Keys)
                    {
                        var v = items[key]; if (!string.IsNullOrEmpty(v.WeixinAppId)) register.RegisterMpAccount(v, key);
                    }
                }

                //注册小程序（可注册多个）
                if (!string.IsNullOrEmpty(senparcWeixinSetting.Value.WxOpenAppId))
                {
                    register.RegisterWxOpenAccount(senparcWeixinSetting.Value, "小程序");
                    foreach (string key in items.Keys)
                    {
                        var v = items[key]; if (!string.IsNullOrEmpty(v.WxOpenAppId)) register.RegisterWxOpenAccount(v, key);
                    }
                }

                //除此以外，仍然可以在程序任意地方注册公众号或小程序
                //AccessTokenContainer.Register(appId, appSecret, name);//Senparc.Weixin.MP.Containers

                //注册企业微信（可注册多个）
                if (!string.IsNullOrEmpty(senparcWeixinSetting.Value.WeixinCorpId))
                {
                    register.RegisterWorkAccount(senparcWeixinSetting.Value, "企业微信");
                    foreach (string key in items.Keys)
                    {
                        var v = items[key]; if (!string.IsNullOrEmpty(v.WeixinCorpId)) register.RegisterWxOpenAccount(v, key);
                    }
                }

                //除此以外，仍然可以在程序任意地方注册企业微信
                //AccessTokenContainer.Register(corpId, corpSecret, name);//Senparc.Weixin.Work.Containers

                //注册最新微信支付版本（V3）（可注册多个）
                if (!string.IsNullOrEmpty(senparcWeixinSetting.Value.TenPayV3_AppId))
                {
                    register.RegisterTenpayV3(senparcWeixinSetting.Value, "微信支付V3");
                    /* 特别注意：
                        在 services.AddSenparcWeixinServices() 代码中，已经自动为当前的 
                        senparcWeixinSetting  对应的TenpayV3 配置进行了 Cert 证书配置，
                        如果此处注册的微信支付信息和默认 senparcWeixinSetting 信息不同，
                        请在 ConfigureServices() 方法中使用 services.AddCertHttpClient() 
                        添加对应证书。
                    */
                }

                #region 注册第三方平台（可注册多个）
                //if (!string.IsNullOrEmpty(senparcWeixinSetting.Value.Component_Appid))
                //{
                //    register.RegisterOpenComponent(senparcWeixinSetting.Value,
                //    //getComponentVerifyTicketFunc
                //    async componentAppId =>
                //    {
                //        //注意：当前使用本地文件缓存数据只是为了方便演示和部署，分布式系统中请使用其他储存方式！
                //        var dir = Path.Combine(ServerUtility.ContentRootMapPath("~/App_Data/OpenTicket"));
                //        if (!Directory.Exists(dir))
                //        {
                //            Directory.CreateDirectory(dir);
                //        }

                //        var file = Path.Combine(dir, string.Format("{0}.txt", componentAppId));
                //        using (var fs = new FileStream(file, FileMode.Open))
                //        {
                //            using (var sr = new StreamReader(fs))
                //            {
                //                var ticket = await sr.ReadToEndAsync();
                //                sr.Close();
                //                return ticket;
                //            }
                //        }
                //    },

                //    //getAuthorizerRefreshTokenFunc
                //    async (componentAppId, auhtorizerId) =>
                //    {
                //        var dir = Path.Combine(ServerUtility.ContentRootMapPath("~/App_Data/AuthorizerInfo/" + componentAppId));
                //        if (!Directory.Exists(dir))
                //        {
                //            Directory.CreateDirectory(dir);
                //        }

                //        var file = Path.Combine(dir, string.Format("{0}.bin", auhtorizerId));
                //        if (!File.Exists(file))
                //        {
                //            return null;
                //        }

                //        using (Stream fs = new FileStream(file, FileMode.Open))
                //        {
                //            var binFormat = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                //            var result = (RefreshAuthorizerTokenResult)binFormat.Deserialize(fs);
                //            fs.Close();
                //            return result.authorizer_refresh_token;
                //        }
                //    },

                //    //authorizerTokenRefreshedFunc
                //    (componentAppId, auhtorizerId, refreshResult) =>
                //    {
                //        var dir = Path.Combine(ServerUtility.ContentRootMapPath("~/App_Data/AuthorizerInfo/" + componentAppId));
                //        if (!Directory.Exists(dir))
                //        {
                //            Directory.CreateDirectory(dir);
                //        }

                //        var file = Path.Combine(dir, string.Format("{0}.bin", auhtorizerId));
                //        using (Stream fs = new FileStream(file, FileMode.Create))
                //        {
                //            //这里存了整个对象，实际上只存RefreshToken也可以，有了RefreshToken就能刷新到最新的AccessToken
                //            var binFormat = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                //            binFormat.Serialize(fs, refreshResult);
                //            fs.Flush();
                //            fs.Close();
                //        }
                //    }, "开放平台");
                //}

                //除此以外，仍然可以在程序任意地方注册开放平台
                //ComponentContainer.Register();//Senparc.Weixin.Open.Containers
                #endregion

                #endregion
            });


            #region 使用 MessageHadler 中间件，用于取代创建独立的 Controller（按需）
            //MessageHandler 中间件介绍：https://www.cnblogs.com/szw/p/Wechat-MessageHandler-Middleware.html

            //使用公众号的 MessageHandler 中间件（不再需要创建 Controller）
            //app.UseMessageHandlerForMp("/WeixinAsync", CustomMessageHandler.GenerateMessageHandler, options =>
            //{
            //    /* 说明：
            //     * 1、此代码块中演示了较为全面的功能点，简化的使用可以参考下面小程序和企业微信
            //     * 2、使用中间件也支持多账号，可以使用 URL 添加参数的方式（如：/Weixin?id=1），
            //     *    在options.AccountSettingFunc = context => {...} 中，从 context.Request 中获取 [id] 值，
            //     *    并反馈对应的 senparcWeixinSetting 信息
            //     */

            //    #region 配置 SenparcWeixinSetting 参数，以自动提供 Token、EncodingAESKey 等参数

            //    //此处为委托，可以根据条件动态判断输入条件（必须）
            //    options.AccountSettingFunc = context =>
            //    //方法一：使用默认配置
            //        senparcWeixinSetting.Value;

            //    //方法二：使用指定配置：
            //    //Config.SenparcWeixinSetting["<Your SenparcWeixinSetting's name filled with Token, AppId and EncodingAESKey>"]; 

            //    //方法三：结合 context 参数动态判断返回Setting值

            //    #endregion

            //    //对 MessageHandler 内异步方法未提供重写时，调用同步方法（按需）
            //    options.DefaultMessageHandlerAsyncEvent = DefaultMessageHandlerAsyncEvent.SelfSynicMethod;

            //    //对发生异常进行处理（可选）
            //    options.AggregateExceptionCatch = ex =>
            //    {
            //        //逻辑处理...
            //        return false;//系统层面抛出异常
            //    };
            //});

            ////使用 小程序 MessageHandler 中间件
            //app.UseMessageHandlerForWxOpen("/WxOpenAsync", CustomWxOpenMessageHandler.GenerateMessageHandler, options =>
            //{
            //    options.DefaultMessageHandlerAsyncEvent = DefaultMessageHandlerAsyncEvent.SelfSynicMethod;
            //    options.AccountSettingFunc = context => senparcWeixinSetting.Value;
            //}
            //);

            ////使用 企业微信 MessageHandler 中间件
            //app.UseMessageHandlerForWork("/WorkAsync", WorkCustomMessageHandler.GenerateMessageHandler,
            //                             o => o.AccountSettingFunc = c => senparcWeixinSetting.Value);//最简化的方式

            #endregion


            return app;
        }

        /// <summary>
        /// 配置全局跟踪日志
        /// </summary>
        static void ConfigTraceLog()
        {
            //这里设为Debug状态时，/App_Data/SenparcTraceLog/目录下会生成日志文件记录所有的API请求日志，正式发布版本建议关闭

            //如果全局的IsDebug（Senparc.CO2NET.Config.IsDebug）为false，此处可以单独设置true，否则自动为true
            Senparc.CO2NET.Trace.SenparcTrace.SendCustomLog("系统日志", "系统启动");//只在Senparc.CO2NET.Config.IsDebug = true的情况下生效

            //全局自定义日志记录回调
            Senparc.CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
            {
                //加入每次触发Log后需要执行的代码
            };

            Senparc.CO2NET.Trace.SenparcTrace.OnBaseExceptionFunc = ex =>
            {
                //加入每次触发BaseException后需要执行的代码

                //发送模板消息给管理员
                //new EventService().ConfigOnWeixinExceptionFunc(ex);
            };
        }

        ///// <summary>
        ///// 扩展缓存策略
        ///// </summary>
        ///// <returns></returns>
        //static IList<IDomainExtensionCacheStrategy> GetExCacheStrategies(SenparcSetting senparcSetting)
        //{
        //    senparcSetting ??= new SenparcSetting();
        //    var exContainerCacheStrategies = new List<IDomainExtensionCacheStrategy>();

        //    /*
        //    //判断Redis是否可用
        //    var redisConfiguration = senparcSetting.Cache_Redis_Configuration;
        //    if ((!string.IsNullOrEmpty(redisConfiguration) && redisConfiguration != "Redis配置"))
        //    {
        //        exContainerCacheStrategies.Add(RedisContainerCacheStrategy.Instance);//自定义的扩展缓存
        //    }
        //    //判断Memcached是否可用
        //    var memcachedConfiguration = senparcSetting.Cache_Memcached_Configuration;
        //    if ((!string.IsNullOrEmpty(memcachedConfiguration) && memcachedConfiguration != "Memcached配置"))
        //    {
        //        exContainerCacheStrategies.Add(MemcachedContainerCacheStrategy.Instance);//TODO:如果没有进行配置会产生异常
        //    }
        //    */

        //    //扩展自定义的缓存策略

        //    return exContainerCacheStrategies;
        //}
    }
}
