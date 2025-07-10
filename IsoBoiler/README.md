# IsoBoiler (Last Updated For: 8.1.14)
This package makes basic configuration of Azure Function Apps (isolated) easier. 

## Using <code>HostRunner.cs</code> within <code>Program.cs</code>

### Application Insights:  
*All HostRunner methods will register Application Insights, but it won't trace anything unless a APPLICATIONINSIGHTS_CONNECTION_STRING is found in the environment variables. No error is thrown if it is not found.*

### .RunBasicWithServices()
```C#
//.RunBasicWithServices()
await HostRunner.RunWithServices((context, services) =>
{
    services.Configure<AppSettings>(context.Configuration.GetSection("MyConfigFilter:AppSettings"));
    services.AddHealthChecks();
    services.AddScoped<IMyService, MyService>();
});
```

### .RunWithServices()
```C#
//.RunWithServices() -> Requires APP_CONFIG_ENDPOINT environment variable. Errors if not found
await HostRunner.RunWithServices((context, services) =>
{
    services.Configure<AppSettings>(context.Configuration.GetSection("MyConfigFilter:AppSettings"));
    services.AddHealthChecks();
    services.AddScoped<IMyService, MyService>();
});

//May provide a Configuration Filter with .UseConfigurationFilter(), or a Configuration Snapshot with .UseConfigurationSnapshot()
await HostRunner.UseConfigurationFilter("MyFilter")
                .UseConfigurationSnapshot("MySnapshot")
                .RunWithServices((context, services) => { /* ...registering services... */});
```  
       
        
## Using <code>ConfigHelper.cs</code> for Testing:

```C#
//Automatically uses the last word in the project name as the Configuration Filter, i.e. "My.Glorious.Project" would use "Project"
var configuration = ConfigHelper.BuildConfiguration("https://appcs-myappconfigresource-env.azconfig.io");

//Provide a manual Configuration Filter
var configuration = ConfigHelper.BuildConfiguration("https://appcs-myappconfigresource-env.azconfig.io", "MyConfigurationFilter");

//Provide a Configuration Snapshot (must manually provide a Configuration Filter)
var configuration = ConfigHelper.BuildConfiguration("https://appcs-myappconfigresource-env.azconfig.io", "MyConfigurationFilter", "MyConfigurationSnapshot");

//Create App Settings model from a configuration
var appSettings = configuration.GetSettings<AppSettings>("Packing:AppSettings");

//All-in-one method to get settings (builds configuration within)
var appSettings = ConfigHelper.GetSettings<AppSettings>("https://appcs-myappconfigresource-env.azconfig.io", "Packing:AppSettings");

//Get the default service provider. This can be useful to get the default configured services, such as IObjectSerializer
var defaultServiceProvider = ConfigHelper.GetDefaultServiceProvider();

//Get a service provider with a custom configuration delegate so you can register services. May be useful when testing Dependency Injection
var serviceProvider = ConfigHelper.GetServiceProvider((context, services) =>
{
    services.Configure<AppSettings>(context.Configuration.GetSection("MyProjectName:AppSettings"));
    services.AddScoped<IMyService, MyService>();
});

//UseJsonAsConfiguration lets you provide configuration values as a string prior to creating your ServiceProvider.
var myConnectionString = "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;";
var appSettingsJson = $"{{\r\n  \"AppSettings\": {{\r\n    \"MyConnectionString\": \"{myConnectionString}\"\r\n  }}\r\n}}";
var serviceProvider = ConfigHelper.UseJsonAsConfiguration(appSettingsJson).GetServiceProvider((context, services) =>
{
    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
});
var injectedAppSettings = serviceProvider.GetRequiredService<IOptionsSnapshot<AppSettings>>()!.Value;
```

## Using <code>MemoryCache.Extensions.cs</code>:

```C#
//The Method name will automatically be used as the cacheKey, but one can be manually provided as well. This does not work (currently?) with Methods that require parameters in order to make sure that the function is executed lazily.
return await _memoryCache.Get(MyMethodName);
return await _memoryCache.Get("MyCacheKey", MyMethodName);

//Provide a lambda function that will produce the results you want. You must provide a cacheKey value to do this.
return await _memoryCache.Get("MyCacheKey", async () =>
{
    using (var connection = new SqlConnection("MyConnectionString"))
    using (var command = new SqlCommand()
    {
        CommandType = CommandType.StoredProcedure,
        Connection = connection,
        CommandText = "MyStoredProc"
    })
    {
        var returnList = new List<MyDataModel>();

        await connection.OpenAsync();

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                returnList.Add(new MyDataModel(reader));
            }
        }

        await connection.CloseAsync();
        return returnList;
    }
});