namespace Uweb.ServerSentEvents

open System
open System.IO
open System.Net.Http
open System.Net.Http.Json
open System.Net.Http.Headers
open System.Threading.Tasks

module Client =
    
    
    let test () = task { 
        let client = new HttpClient()
        client.Timeout <- TimeSpan.FromSeconds(30)
        client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue("text/event-stream"))
        //let url = "http://localhost:5000/sse"
        let url = "http://192.168.178.42:8080/sse"
        //let url = "https://hacker-news.firebaseio.com/v0/updates.json"

        while true do
            try 
                printfn "Establishing connection"
                let! stream = client.GetStreamAsync url
                use streamReader = new StreamReader(stream)
                while not streamReader.EndOfStream do
                    let! msg = streamReader.ReadLineAsync()
                    if msg.StartsWith "data:" && not (msg = "data: null") then
                        printfn "%s" <| msg.Substring 5
                        printfn "---------------------------------------"
            with
                | :? TaskCanceledException -> ()
                | _ as e -> 
                    eprintfn "%s" <| e.ToString ()
                    do! Task.Delay 5000
    }