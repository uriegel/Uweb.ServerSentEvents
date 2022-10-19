namespace Uweb.ServerSentEvents

open System
open System.IO
open System.Net.Http
open System.Net.Http.Json

module Client =
    open System.Threading.Tasks
    
    let test () = task { 
        let client = new HttpClient()
        client.Timeout <- TimeSpan.FromSeconds(30)
        let url = "http://localhost:5000/sse"

        while true do
            try 
                printfn "Establishing connection"
                let! stream = client.GetStreamAsync url
                use streamReader = new StreamReader(stream)
                while not streamReader.EndOfStream do
                    let! msg = streamReader.ReadLineAsync()
                    printfn "Event: %s" msg
            with
                | :? TaskCanceledException -> ()
                | _ as e -> eprintfn "%s" <| e.ToString ()
    }