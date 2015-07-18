﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

namespace PovertySail.Console
{
	public class Supervisor:IDisposable
    {
        private ILogger _logger;
        private PluginConfiguration _configuration;
        private int _sleepTime;

        public Supervisor(ILogger logger,IList<IPlugin> plugins, int sleepTime)
        {
            _sleepTime = sleepTime;
            _logger = logger;
            _configuration = new PluginConfiguration();
            _configuration.Plugins = plugins;
        }

        public void Initialize()
        {
            var failed = new List<IPlugin>();
            foreach (var plugin in _configuration.Plugins)
            {
                try
                {
                    _logger.Info("Initializing Plugin " + plugin.GetType().Name);
                    plugin.Initialize(_configuration);
                }
                catch (Exception ex)
                {
                    _logger.Fatal("Exception initializing plugin "+plugin.GetType().Name, ex);
                    failed.Add(plugin);
                }
            }
            foreach (var plugin in failed)
            {
                EvictPlugin(_configuration,plugin,false);
            }

            //remove any plugins that failed to initialize
            //_configuration.Plugins = _configuration.Plugins.Where(x => x.Initialized).ToList();
            foreach (var plugin in _configuration.Plugins)
            {
                _logger.Info(plugin.GetType().Name + " Initialized OK");
            }
        }

        public void Run()
        {
            var state = new State();
            bool run = true;
            int operationCount = 1;
            while (run && operationCount>0)
            {
                operationCount = 0;

                IList<IPlugin> erroredPlugins = new List<IPlugin>();

                foreach (var sensor in _configuration.Sensors)
                {
                    try
                    {
                        sensor.Update(state);
                        operationCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception updating sensor "+sensor.GetType().Name);
                        if (!erroredPlugins.Contains(sensor.Plugin))
                        {
                            erroredPlugins.Add(sensor.Plugin);
                        }
                    }
                }

                foreach (var calculator in _configuration.Calculators)
                {
                    try
                    {
                        calculator.Calculate(state);
                        operationCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception updating calculator " + calculator.GetType().Name);
                        if (!erroredPlugins.Contains(calculator.Plugin))
                        {
                            erroredPlugins.Add(calculator.Plugin);
                        }
                    }
                }

                //foreach (var recorder in _configuration.Recorders)
                //{
                //    try
                //    {
                //        operationCount++;
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.Error("Exception updating recorder " + recorder.GetType().Name);
                //        if (!erroredPlugins.Contains(recorder.Plugin))
                //        {
                //            erroredPlugins.Add(recorder.Plugin);
                //        }
                //    }
                //}

                foreach (var viewer in _configuration.DashboardViewers)
                {
                    try
                    {
                        viewer.Update(state);
                        operationCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception updating viewer " + viewer.GetType().Name);
                        if (!erroredPlugins.Contains(viewer.Plugin))
                        {
                            erroredPlugins.Add(viewer.Plugin);
                        }
                    }
                }

                //attempt to reinitialize any plugins that encountered errors
                foreach (var plugin in erroredPlugins)
                {
                    EvictPlugin(_configuration,plugin,true);
                }

                _logger.Debug("Sleeping");
                Thread.Sleep(_sleepTime);
            }
        }

        private void EvictPlugin(PluginConfiguration configuration,IPlugin plugin,bool reinitialize)
        {
            _logger.Warn("Evicting "+plugin.GetType().Name+" "+(reinitialize? "with" : "without")+" reinitialize");

            configuration.Sensors = configuration.Sensors.Where(x => x.Plugin != plugin).ToList();
            configuration.Calculators = configuration.Calculators.Where(x => x.Plugin != plugin).ToList();
            configuration.Recorders = configuration.Recorders.Where(x => x.Plugin != plugin).ToList();
            configuration.DashboardViewers = configuration.DashboardViewers.Where(x => x.Plugin != plugin).ToList();
            
            if (reinitialize)
            {
                //allow it to re-add the components and attempt to restart
                plugin.Initialize(configuration);

                if (!plugin.Initialized)
                {
                    EvictPlugin(configuration,plugin,false);
                }
            }
            else
            {
                //remove it entirely
                configuration.Plugins = configuration.Plugins.Where(x => x != plugin).ToList();
            }
        }

		public void Dispose()
		{
			if (_configuration != null) {
				_configuration.Dispose ();
			}
		}
    }
}
