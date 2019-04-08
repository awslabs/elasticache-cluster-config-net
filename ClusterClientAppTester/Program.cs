/*
 * Copyright 2014 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *  
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Amazon.ElastiCacheCluster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace ClusterClientAppTester
{
    static class Program
    {
        private static ILoggerFactory _loggerFactory;
        private static Form1 _form;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += MyHandler;

                _loggerFactory = CreateLoggerFactory();
                _form = new Form1(_loggerFactory, LoadClusterSettings());
                Application.Run(_form);
            }
            catch (Exception ex)
            {
                File.WriteAllText("log.txt", ex.Message);
                _form.ErrorAlert(ex.Message);
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            File.WriteAllText("log.txt", e.Message);
            _form.Name = e.Message;
            _form.ErrorAlert(e.Message);
        }

        static ILoggerFactory CreateLoggerFactory()
        {
            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
            return new LoggerFactory(new[] { new ConsoleLoggerProvider(optionsMonitor) }, new LoggerFilterOptions { MinLevel = LogLevel.Trace });
        }

        static ElastiCacheClusterConfig LoadClusterSettings()
        {
            var config = LoadConfiguration();
            var setting = new ClusterConfigSettings();
            config.GetSection("ClusterClient").Bind(setting);
            return new ElastiCacheClusterConfig(_loggerFactory, setting);
        }

        static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            return builder.Build();
        }
    }
}
