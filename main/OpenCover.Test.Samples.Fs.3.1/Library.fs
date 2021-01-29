namespace OpenCover.Test.Samples.Fs._3._1

open System

type ClassWithAutoProperty() =
    member val AutoProperty = 0 with get, set

module Issue807 =
    let example () =
        let sample = Console.Out |> string
        printfn "Hello %s" sample