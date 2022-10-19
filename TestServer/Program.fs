open FSharpTools
open FSharpTools.Functional
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.Reactive.Subjects
open System.Text.Encodings.Web;
open System.Text.Json
open System.Text.Json.Serialization

let getJsonOptions = 
    let getJsonOptions () = 
        let jsonOptions = JsonSerializerOptions()
        jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        jsonOptions.Encoder <- JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        jsonOptions.Converters.Add(JsonFSharpConverter())
        jsonOptions
    memoizeSingle getJsonOptions

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


type RendererEvent = 
    | ThemeChanged of string
    | Nothing

let rendererReplaySubject = new Subject<RendererEvent>()

let sse () = Sse.create rendererReplaySubject <| getJsonOptions ()

let configureRoutes (app : IApplicationBuilder) = 
    let routes =
        choose [  
            route  "/commander/sse" >=> warbler (fun _ -> sse ())
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
    .Run()



