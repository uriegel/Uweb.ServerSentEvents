open FSharpTools
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open Sse
open Uweb.ServerSentEvents

let configureKestrel (options: KestrelServerOptions) = 
    options.ListenAnyIP 5000

let configureServices (services : IServiceCollection) = 
    services
        .AddCors()
        .AddSingleton(getJsonOptions ()) 
        .AddSingleton<Json.ISerializer, SystemTextJson.Serializer>() 
        .AddResponseCompression()
        .AddGiraffe()
        |> ignore

let configureLogging (builder : ILoggingBuilder) =
    // Set a logging filter (optional)
    let filter l = l.Equals LogLevel.Warning

    // Configure the logging factory
    builder.AddFilter(filter) // Optional filter
           .AddConsole()      // Set up the Console logger
           .AddDebug()        // Set up the Debug logger

           // Add additional loggers if wanted...
    |> ignore

let configureRoutes (app : IApplicationBuilder) = 
    let routes =
        choose [  
            route  "/sse" >=> warbler (fun _ -> sse ())
            route "/test"   >=> text "This is a Test Site for Uweb.ServerSentEvents "
        ]
    app
        .UseResponseCompression()
        .UseGiraffe routes     

let webHostBuilder (webHostBuilder: IWebHostBuilder) = 
    webHostBuilder
        .ConfigureKestrel(configureKestrel)
        .Configure(configureRoutes)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        |> ignore

Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webHostBuilder)
    .Build()    
    .Start()

Client.test () 
|> Async.AwaitTask 
|> Async.StartImmediate

while true do
    sendEvent <| System.Console.ReadLine ()




