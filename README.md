# IsoBoiler (8.1.13)
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