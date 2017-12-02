namespace N
open NUnit.Framework
module M =
  type private Thing = { Thing: string } with
    member this.bytes () = System.Text.Encoding.UTF8.GetBytes(this.Thing)
  let private makeThing s = { Thing = s }

  [<Test>]
  let testMakeThing2() =
    Assert.AreEqual("s", (makeThing "s").Thing)
    Assert.AreEqual(5, (makeThing "aeiou").bytes().Length)

module DU =
    type private MyUnion =
        | Foo of int
        | Bar of string
    with member this.as_bar() = match this with
                                | Foo n -> Bar (string n)
                                | bar -> bar

    let private returnFoo v = Foo v

    let private returnBar v = Bar v

    [<Test>]
    let testMakeUnion2() =
        Assert.AreEqual(returnFoo 10, Foo 10)
        Assert.AreEqual(returnBar "s", Bar "s")
        Assert.AreEqual(Bar "10", (Foo 10).as_bar())