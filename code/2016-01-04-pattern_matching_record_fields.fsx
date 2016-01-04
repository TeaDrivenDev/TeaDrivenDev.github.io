(*** raw ***)
---
layout: page
title: Pattern Matching (not only) on Record Fields
---

(**
## Quintuple Ways Of Matching Tuples

Everyone who has used F# pattern matching knows how powerful it is, but interestingly it is often overlooked that all the different aspects are always equally applicable.

Destructuring tuples is something every F# programmer is probably well versed in in all its forms:
*)

// simple destructuring assignment
let getName (nameTuple : string * string) =
    let firstName, lastName = nameTuple // firstName and lastName now contain the two parts of the tuple
    sprintf "%s %s" firstName lastName

// destructuring directly in a method signature
let printName (firstName, lastName) =
    // again, firstName and lastName now contain the two parts of the tuple that was passed into the function
    printfn "Name: %s, %s" lastName firstName

// destructuring in a lambda
let printNameLambda = fun (firstName, lastName) -> printfn "Name: %s, %s" lastName firstName

// destructuring in a match expression
let getTitle nameTuple =
    match nameTuple with
    | "Don", "Syme" -> "Creator of F#"
    | firstName, lastName -> sprintf "Someone else named %s %s" firstName lastName

// destructuring in a for loop
// (This one may be a little less well known; I learned that from Dave Fancher's "The Book of F#".)
let printNames (names : (string * string) seq) =
    for firstName, lastName in names do
        printfn "Person named %s %s" firstName lastName

(**
....you get the picture. Did I forget something important?

Now, in all those cases (there *may* be rare exceptions that I'm not presently aware of), you can also use the other forms of pattern matching, such as Active Patterns.

Yes, you can use [Active Patterns in function signatures](http://luketopia.net/2014/09/11/interesting-active-patterns/).

You can also use a type test in a `for` loop:
*)

for :? string as x in Seq.empty<obj> do printfn "%s" x

(**
(The compiler will notify you that there might be unmatched cases that would then be ignored - so this works like LINQ's `.OfType<'T>()`.)

## Record Breaking Pattern Matching

Something that seems to be a little less well known is pattern matching on the fields of record types, both in the form of destructuring assignments and actual comparison matches, but that works just as well in all those cases. The syntax for it is the same as if you were constructing a record of the respective type, except that it is not required to use all the fields of the type.

Let's say we have turned the above name tuple into a a proper type:
*)

type Person = { FirstName : string; LastName : string; Age : int }

(**
We can use this now in all the ways we used the tuple above:
*)

// simple destructuring assignment
let getNameFromRecord person =
    let { FirstName = firstName; LastName = lastName } = person
    sprintf "%s %s" firstName lastName
    
// destructuring directly in a method signature
let printNameFromRecord { FirstName = firstName; LastName = lastName } =
    // again, firstName and lastName now contain the two parts of the tuple that was passed into the function
    printfn "Name: %s, %s" lastName firstName

// destructuring in a lambda
let printNameLambdaFromRecord =
    fun { FirstName = firstName; LastName = lastName } ->
        printfn "Name: %s, %s" lastName firstName

// destructuring in a match expression
let getTitleFromRecord person =
    match person with
    | { FirstName = "Don"; LastName = "Syme"} -> "Creator of F#"
    | { FirstName = firstName; LastName = lastName; Age = age } ->
        sprintf "Someone else named %s %s, aged %i" firstName lastName age

// destructuring in a for loop
let printNamesFromRecord persons =
    for { FirstName = firstName; LastName = lastName } in persons do
        printfn "Person named %s %s" firstName lastName

(**
Now, while this is pretty neat, I don't find myself using it a lot, which is in part due to the fact that when for example matching the argument of a lambda this way, you lose the reference to the record itself.

## 'as' Is It

When I wrote this on Twitter, [Shane Charles](https://twitter.com/Dead_Stroke/status/684106309248024577) had the right answer: You can always additionally bind the original value to a name with the `as` keyword. Always.

The most common use of this concept in pattern matching are probably type tests:
*)

try
    ()
with
| :? System.OutOfMemoryException as ex -> printfn "Out of memory: %s" ex.Message
| ex -> printfn "Something else happened: %s" ex.Message

(**
But this really works pretty much everywhere (even though it might not always make sense to use it):
*)

// destructuring directly in a method signature
let printNameFromRecordTee ({ FirstName = firstName; LastName = lastName } as person) =
    // again, firstName and lastName now contain the two parts of the tuple that was passed into the function
    printfn "Name: %s, %s" lastName firstName
    person

// destructuring in a lambda
let printNameLambdaFromRecordTee =
    fun ({ FirstName = firstName; LastName = lastName } as person) ->
        printfn "Name: %s, %s" lastName firstName
        person

// destructuring in a match expression
let getTitleFromRecordWithAge person =
    match person with
    | { FirstName = "Don"; LastName = "Syme"} -> "Creator of F#"
    | { FirstName = firstName; LastName = lastName } as p ->
        sprintf "Someone else named %s %s, aged %i" firstName lastName p.Age

// destructuring in a for loop
let printSymesFromREcord persons =
    for { FirstName = firstName; LastName = "Syme" } as syme in persons do
        printfn "Syme named %s, aged %i" firstName syme.Age

(**
That last one again gives us a compiler warning about incomplete matches, but anyone not from the Syme family will simply be ignored.

By the way, you can of course also use `as` with Active Patterns:
*)

let (|FizzBuzz|_|) i = 
    match i % 3, i % 5 with
    | 0, 0 -> Some "FizzBuzz"
    | 0, _ -> Some "Fizz"
    | _, 0 -> Some "Buzz"
    | _ -> None

let fizzBuzz = function
    | FizzBuzz s as x -> s, x
    | _ as x -> "", x

(**
While this is a bit nonsensical, it demonstrates the point: The `fizzBuzz` function has type `int -> string * int`, and while the signature does not actually give a reference to the integer argument, we can still obtain it from the match expression.

Did you know about pattern matching on record fields? If you didn't, you too (along with Reid Evans and John Azariah) are now one of today's 10.000!

![](http://imgs.xkcd.com/comics/ten_thousand.png)

([via XKCD](https://xkcd.com/1053/))

---
Written on my HP ZBook with [Ionide](http://ionide.io/) and [Visual Studio Code](https://code.visualstudio.com/)
*)