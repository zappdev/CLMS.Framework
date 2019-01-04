﻿using System.IO;
using System.Xml;
using CLMS.Framework.Utilities;
using Microsoft.Extensions.Configuration;

namespace CLMS.Framework.Configuration
{   
    public class ConfigurationHandler
    {
        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddXmlFile("Web.config", true, true)
                .Add(new LegacyConfigurationProvider())
                .Build();
        }

        public static MailSettings GetSmtpSettings()
        {
            return GetInjectedConfig().GetSection("system.net:mailSettings")
                .Get<MailSettings>();
        }

        public static AppConfiguration GetAppConfiguration()
        {
            return GetInjectedConfig().Get<AppConfiguration>();
        }
        
        public static ConfigurationBuilder SetUpConfigurationBuilder(ConfigurationBuilder config)
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.Add(new LegacyConfigurationProvider());
            return config;
        }
        
        private static IConfiguration GetInjectedConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfiguration>();
            return (config ?? GetConfiguration());
        }
    }

    public class LegacyConfigurationProvider : ConfigurationProvider, IConfigurationSource
    {
        public override void Load()
        {
            var doc = new XmlDocument();

            doc.Load(@".\App.config");

            var selectNodes = doc.SelectNodes("//configuration/connectionStrings/add");
            if (selectNodes != null)
            {
                foreach (XmlNode connection in selectNodes)
                {
                    if (connection.Attributes == null) continue;

                    Data.Add($"ConnectionStrings:{connection.Attributes["name"].Value}:connectionString",
                        connection.Attributes["connectionString"].Value);
                    Data.Add($"ConnectionStrings:{connection.Attributes["name"].Value}:name",
                        connection.Attributes["name"].Value);
                    Data.Add($"ConnectionStrings:{connection.Attributes["name"].Value}:providerName",
                        connection.Attributes["providerName"].Value);
                }
            }

            var appSettNodeList = doc.SelectNodes("//configuration/appSettings/add");
            if (appSettNodeList == null) return;

            foreach (XmlNode appSett in appSettNodeList)
            {
                if (appSett.Attributes == null) continue;

                    Data.Add($"AppSettings:{appSett.Attributes["key"].Value}", appSett.Attributes["value"].Value);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }

}
