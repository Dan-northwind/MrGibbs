﻿using System;

using Newtonsoft.Json;

namespace MrGibbs.Infrastructure
{
    /// <summary>
    /// wraps nlog with the logging interface
    /// </summary>
    public class NLogLogger : MrGibbs.Contracts.Infrastructure.ILogger
    {
        private NLog.ILogger _logger;

        public NLogLogger(NLog.ILogger logger)
        {
            _logger = logger;
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

		public void Debug(string message,params object[] parms)
		{
			_logger.Debug(message,parms);
		}

        public void Debug(string message, Exception exception)
        {
            _logger.Debug(message, exception);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

		public void Info(string message,params object[] parms)
		{
			_logger.Info(message,parms);
		}

        public void Info(string message, Exception exception)
        {
            _logger.Info(message, exception);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

		public void Warn(string message,params object[] parms)
		{
			_logger.Warn(message,parms);
		}

        public void Warn(string message, Exception exception)
        {
            _logger.Warn(message, exception);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

		public void Error(string message,params object[] parms)
		{
			_logger.Error(message,parms);
		}

        public void Error(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

		public void Fatal(string message,params object[] parms)
		{
			_logger.Fatal(message,parms);
		}

        public void Fatal(string message, Exception exception)
        {
            _logger.Fatal(message, exception);

        }

        private string DumpObjectToJson<T>(T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return json;
        }

        private string CreateObjectLogMessage<T>(string message, T obj)
        {
            return string.Format("{0} {1}:{2}", message, typeof(T).Name, DumpObjectToJson(obj));
        }

        public void Debug<T>(string message, T o) where T : class
        {
            _logger.Debug(CreateObjectLogMessage(message, o));
        }

        public void Info<T>(string message, T o) where T : class
        {
            _logger.Info(CreateObjectLogMessage(message, o));
        }

        public void Warn<T>(string message, T o) where T : class
        {
            _logger.Warn(CreateObjectLogMessage(message, o));
        }

        public void Error<T>(string message, T o) where T : class
        {
            _logger.Error(CreateObjectLogMessage(message, o));
        }

        public void Fatal<T>(string message, T o) where T : class
        {
            _logger.Fatal(CreateObjectLogMessage(message, o));
        }
    }
}
