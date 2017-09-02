namespace N
open NUnit.Framework
module M =
  type Thing = { Thing: string } with
    member this.bytes () = System.Text.Encoding.UTF8.GetBytes(this.Thing)
  let makeThing s = { Thing = s }

  [<Test>]
  let testMakeThing() =
    Assert.AreEqual("s", (makeThing "s").Thing)
    Assert.AreEqual(5, (makeThing "aeiou").bytes().Length)

module DU =
    type MyUnion =
        | Foo of int
        | Bar of string
    with member this.as_bar() = match this with
                                | Foo n -> Bar (string n)
                                | bar -> bar

    let returnFoo v = Foo v

    let returnBar v = Bar v

    [<Test>]
    let testMakeUnion() =
        Assert.AreEqual(returnFoo 10, Foo 10)
        Assert.AreEqual(returnBar "s", Bar "s")
        Assert.AreEqual(Bar "10", (Foo 10).as_bar())
