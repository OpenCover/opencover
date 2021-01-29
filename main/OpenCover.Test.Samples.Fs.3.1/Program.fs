module Program

[<EntryPoint>]
let inline main _ =
    let sample = System.DateTime.UtcNow |> string
    printfn "%s" sample
    0
