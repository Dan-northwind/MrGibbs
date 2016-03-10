﻿using System;
using System.Collections.Generic;

using Ninject;
using Ninject.Modules;

using MrGibbs.Configuration;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Infrastructure;

namespace MrGibbs
{
    /// <summary>
    /// Bootstrap for the MrGibbs application
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Initializing Mr. Gibbs Kernel...");

            IKernel kernel;
            try
            {
                kernel = Configure();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception during startup, "+ex.Message);
                System.Console.WriteLine(ex.StackTrace);
                return;
            }

            var logger = kernel.Get<ILogger>();
			logger.Info ("Kernel Configuration Complete");

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                logger.Fatal("Unhandled Application Exception", e.ExceptionObject);
            };

			logger.Info ("Configuring Supervisor");
			bool isFirst = true;
			using (var supervisor = kernel.Get<Supervisor> ()) {
                do
                {
                    logger.Info("Initializing Supervisor");
                    supervisor.Initialize();
					if(isFirst)
					{
						var config = ConfigurationHelper.GenerateDefaultConfiguration();
						logger.Info("Default Configuration"+System.Environment.NewLine+config);
						isFirst=false;
					}
                }
                while (supervisor.Run());//if run returns true then we need to restart, if false then it wants to close
            }

            logger.Info("Shutting down");
        }

        /// <summary>
        /// Configures the DI container
        /// </summary>
        /// <returns>configured kernel</returns>
        static IKernel Configure()
        {
            var kernel = new StandardKernel();

			//TODO: Move this to a module
			kernel.Bind<IClock> ().To<LinuxSystemClock> ().InSingletonScope ();

            //infrastructure modules
			kernel.Load(new List<NinjectModule>()
            {
				new LoggingModule(new List<string>(){"Log.config"})
            });

			var logger = kernel.Get<ILogger> ();

			logger.Info ("Loading Plugins...");
			//plugins
			kernel.Load (new string[] { "*.Plugin.dll" });

			logger.Info ("Loading Core...");
			//core services/controllers
            kernel.Bind<IRaceController>().To<RaceController>()
                .InSingletonScope()
                .WithConstructorArgument("autoRoundMarkDistanceMeters", AppConfig.AutoRoundMarkDistanceMeters);
			kernel.Bind<Supervisor>().ToSelf()
                .InSingletonScope()
                .WithConstructorArgument("sleepTime", AppConfig.SleepTime);
                
            return kernel;
        }
    }
}
