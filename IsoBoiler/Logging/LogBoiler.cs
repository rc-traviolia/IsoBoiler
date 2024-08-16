﻿using IsoBoiler.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IsoBoiler.Logging
{
    public interface ILogBoiler
    {
        IDisposable? BeginScope<TState>(TState state) where TState : notnull;
        IDisposable? BeginScope(string key, string value);
        IDisposable? BeginScope(FunctionContext context);
        void Log(string message);
        void Log(object objectToSerialize, JsonSerializerOptions? jsonSerializerOptions = null);
        void Log(Exception exception);
        void Log(string message, Dictionary<string, object> customProperties);
        void Log(object objectToSerialize, Dictionary<string, object> customProperties, JsonSerializerOptions? jsonSerializerOptions = null);
        void Log(Exception exception, Dictionary<string, object> customProperties);
    }

    public class LogBoiler : ILogBoiler
    {
        public readonly ILogger _logger;
        public const string HTTP_TRIGGER = "HttpTrigger";
        public const string TIMER_TRIGGER = "TimerTrigger";

        public LogBoiler(ILogger<LogBoiler> logger)
        {
            _logger = logger;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public IDisposable? BeginScope(string key, string value)
        {
            return _logger.BeginScope(new Dictionary<string, object>() { { key, value } });
        }

        public IDisposable? BeginScope(FunctionContext context)
        {
            var split = context.FunctionDefinition.EntryPoint.Split('.');
            var projectName = split[split.Length - 3] == "Functions" ? split[split.Length - 4] : split[split.Length - 3];
            var functionName = context.FunctionDefinition.Name; //same as split[split.Length - 1];
            var functionType = context.FunctionDefinition.InputBindings.First().Value.Type;

            //Mutate camelCase to PascalCase
            var functionTypeCharacters = functionType.ToCharArray();
            functionTypeCharacters[0] = char.ToUpper(functionTypeCharacters[0]);
            functionType = new string(functionTypeCharacters);


            var scope = _logger.BeginScope(new Dictionary<string, object>() { { "FunctionProjectName", projectName }, { "FunctionName", functionName }, { "FunctionType", functionType } });
            return scope;
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }

        public void Log(object objectToSerialize, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            _logger.LogInformation(objectToSerialize.ToJson(jsonSerializerOptions));
        }

        public void Log(Exception exception)
        {
            if (exception is ExceptionBoiler exceptionBoiler)
            {
                Log(exceptionBoiler);
            }
            else
            {
                _logger.LogError(exception, exception.Message);
            }
        }

        private void Log(ExceptionBoiler exception)
        {
            using (BeginScope(exception.CustomProperties))
            {
                _logger.LogError(exception, exception.Message);
            }
        }
        public void Log(string message, Dictionary<string, object> customProperties)
        {
            using (BeginScope(customProperties))
            {
                _logger.LogInformation(message);
            }
        }
        public void Log(object objectToSerialize, Dictionary<string, object> customProperties, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            using (BeginScope(customProperties))
            {
                _logger.LogInformation(JsonSerializer.Serialize(objectToSerialize.ToJson(jsonSerializerOptions)));
            }
        }

        public void Log(Exception exception, Dictionary<string, object> customProperties)
        {
            using (BeginScope(customProperties))
            {
                _logger.LogError(exception, exception.Message);
            }
        }
    }
}