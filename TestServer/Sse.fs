module Sse
open FSharpTools.Functional
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

type SseEvent = 
    | Message of string
    | Nothing

let sseSubject = new Subject<SseEvent>()

let sendEvent text = 
    sseSubject.OnNext (Message text)
let sse () = Sse.create sseSubject <| getJsonOptions ()
