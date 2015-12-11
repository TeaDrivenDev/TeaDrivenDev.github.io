(*** raw ***)
---
layout: page
title: Making Busy Progress in F#
---

(**
##Introduction

As much as we like our software to be fast, and although computers get ever more powerful and paralleloriz0red, still not everything can happen instantly. There are long-running calculations, file I/O, network operations, `Thread.Sleep` and other things that require the user to wait for their completion, before they can use the result in any way.

It is considered common courtesy these days to not just let the application stall until an operation is completed and expect patience and understanding from the user; instead we'd like to let them know that something is happening and there is still hope that this "happening" will be over in a finite amount of time. And if we have any way of telling at which point of the process we are, we'll also want to pass that information on to the user, so they can plan the rest of their day accordingly.


The overall premise for what we're doing here is that a) we have a desktop application, say in WPF, and b) the "result" of our long-running operation is a state change in the application. The latter isn't very functional, because seen from our operation, it is a side effect, but that is how .NET desktop applications usually work.

First we define some very basic types to describe our concept of "busy":
*)

(*** hide ***)
#r @"../packages/FSharp.Collections.ParallelSeq/lib/net40/FSharp.Collections.ParallelSeq.dll"

open System
open System.IO

open FSharp.Collections.ParallelSeq

let asSnd first second = first, second

(** *)

type IBusy =
    abstract member IsBusy : bool with get, set
    
    // Setting this to `None` means we don't know what the actual progress is; we're just "busy" somehow.
    // UI controls like progress bars often have an "indeterminate" state for things like this.
    abstract member ProgressPercentage : float option with get, set
    abstract member BusyMessage : string with get, set

(*** hide ***)
type BasicBusy() =
    interface IBusy with
        member val IsBusy = false with get, set
        member val BusyMessage = "" with get, set
        member val ProgressPercentage = None with get, set

let busy = BasicBusy() :> IBusy

type DataThing = { Value : obj }

let dataThings = Seq.empty

let processDataThing (thing : DataThing) = 12 

let operationFunction() = ()

(**
The implementation of `IBusy` will likely be something like a ViewModel, which will then take care of displaying a progress bar, a text control with the busy message and maybe some kind of overlay to lock down the application until our operation is complete - if we're loading new data for example, it probably makes no sense to let the user work in the application in the mean time. When we're done, we set `IsBusy` to `false`, and the application can be used again.

Using this might look like this:
*)

busy.IsBusy <- true
busy.BusyMessage <- "Processing things"
busy.ProgressPercentage <- None

dataThings
|> Seq.filter (fun thing -> thing.Value.GetHashCode() <> 12)
|> Seq.map (processDataThing >> string)
|> Seq.toArray
|> asSnd "C:\DataThingResults\result.txt"
|> File.WriteAllLines 

busy.IsBusy <- false

(**
This is of course the most naive way possible, and it requires doing everything by hand correctly every time we want to do something "busy".

That will get annoying pretty quickly, so we'll put the whole `IBusy` handling in a function and then pass it the operation we want to execute:
*)

let doBusy (busy : IBusy) busyMessage operation =
    busy.BusyMessage <- busyMessage
    busy.IsBusy <- true
    busy.ProgressPercentage <- None

    operation()

    busy.IsBusy <- false

(** 
That's better - it takes all the manual work from us. We only need to say "do `operation` while telling the user you're busy, and when the operation is through, tell the user you're done". This is nice and declarative and really the minimum work we can have with this.

But.... it doesn't work. Well, it does, but not as we'd like it to. I said above that we want to "not just let the application stall until an operation is completed and expect patience and understanding from the user" - but that is what will happen. Why? Because we're just running everything on the current thread, which in a desktop application will usually be the UI thread.

If we're in WPF, chances are the thread will be blocked by `operation` before it has even managed to properly show the "busy" indication. And even if the user sees that, the whole application will now be frozen; the Windows task manager might even say "not responding", until at some point our operation completes and unblocks the thread. That is not very friendly.

There are a number of ways to solve this in .NET, from "manual" threading control and `BackgroundWorker` to Tasks and C#'s `async`/`await` - and in F# we have the nicest of them all: asynchronous workflows. They all work a bit differently, and not with all of them, what we want to do is straightforward to achieve - with manual threading control via the `Thread` class for example, it would be quite an ordeal (but then, pretty much everything is).

One thing that is different with F# async workflows from, say, `BackgroundWorker` or `async`/`await` is that we have to explicitly switch to a background thread, because it doesn't happen automatically. The upside is that we *have* that control, unlike the other implementations.

Let's look at a `doBusyBackground` implementation of our function:
*)

let doBusyBackground (busy : IBusy) busyMessage operation =
    busy.BusyMessage <- busyMessage
    busy.IsBusy <- true
    busy.ProgressPercentage <- None

    async {
        let context = System.Threading.SynchronizationContext.Current
        do! Async.SwitchToThreadPool()
        
        do! async { operation() }
        
        do! Async.SwitchToContext context
        
        busy.IsBusy <- false
    }
    |> Async.StartImmediate

(**
As I said, we *have* control over which thread we're running on, and we *have* to exert it; that's why we first save the current synchronization context and restore it later on.

Note that even though our `operation` function may have no asynchronous aspects in itself, we have to call it in an asynchronous workflow to have it executed on the thread we switched to on the `Async` class.

But look at how easy that was! We just wrap something in an `async { }` block (some people claim it's a monad, but I refuse to believe that), and we can "await" it on a non-blocking fashion, which is what the `do!` accomplishes. It needs to be said that in this case `operation` has the type `unit -> unit`, that means it is a completely self-contained effectful operation and returns no useful value. We will later see that we can just as well return values of any type from asynchronous workflows.

`Async.StartImmediate` is of type `Async<unit> -> unit`, that means it takes an asynchronous operation and runs it in a "fire-and-forget" fashion until it completes, without returning a (useful) value.

This current solution is a bit inflexible, though. Our operation will always run on the ThreadPool all the time, which may not be what we want, for example when we need to update UI elements at certain points. Of course we could capture the UI context when we construct the function, but that would start getting a bit convoluted.

Let's split out moving an operation to the ThreadPool from our `doBusy` function (on the side we will also introduce a default busy message that should be correct in a lot of cases and that we can replace when we really need to):
*)

// This is essentially a glorified `string option`, but naming the cases makes it clearer what this is about.
// And thanks to F#'s syntax it's almost free anyway.
type BusyMessage = DefaultMessage | Message of string


let doBusyAsync (busy : IBusy) busyMessage operation =
    busy.BusyMessage <-
        match busyMessage with
        | Message message -> message
        | DefaultMessage -> "Updating information"
    busy.IsBusy <- true
    busy.ProgressPercentage <- None

    async {
        do! operation
        busy.IsBusy <- false
    }
    |> Async.StartImmediate

let onThreadPool operation =
    async {
        let context = System.Threading.SynchronizationContext.Current
        do! Async.SwitchToThreadPool()

        let! result = operation

        do! Async.SwitchToContext context

        return result
    }


(**
If you look at the type of the `operation` argument of `doBusyAsync` (there are type tooltips in the code, like a compiler would give you - because there actually *is* a compiler that checks it!), it has changed from `unit -> unit` in `doBusyBackground to `Async<unit>`, that means it has to come "pre-wrapped" in an `async { }` block. That is necessary so *we* can decide what should go on the ThreadPool and what we want to run on the UI thread (or implement any other threading requirements we may have).

Another thing to note is that we call `operation` without parentheses now - because it is not a normal F# function anymore, but an asynchronous workflow. "Awaiting" that with `do!` or `let!` (which have the same effect, with the difference that `let!` binds the result to a value we can then keep using, while `do!`.... doesn't) results it its execution, yielding the `'T` value of the `Async<'T>`. To be exact, nothing inside the `async { }` blocks actually gets executed until `Async.StartImmediate` (or one of a number of other `Async` functions like `Async.RunSynchronously` or `Async.StartWithContinuations`) is called. Everything up to that is just setting up the computation.

Our new `onThreadPool` function is a little more generic than running on the ThreadPool in  `doBusyBackground` was: `operation` is not a function of type `unit -> unit`, but also an `Async<'a>`, which means it returns a value of type `'a` (which can still be `unit`, of course). That means we can not only run "fire-and-forget" style code on the ThreadPool, but we can actually get results back. That sounds like a useful property for our code to have.

It is especially useful when we consider something that should start to become clear when looking at our existing functions that use asynchronous workflows: They are highly composable. We can nest them and chain them, await them without hassle using `let!` and then use the result as a normal F# value. Did I mention we can make any piece of code "asynchronous" by putting `async { }` around it?

If we put together what we have now, we can run an operation on a background thread while our application is "busy" as follows:
*)

async { operationFunction() } 
|> onThreadPool
|> doBusyAsync busy DefaultMessage

(**
When `operationFunction` completes, the "wrapper" asynchronous workflow created by `onThreadPool` returns the `unit` value to `doBusyAsync`, which does nothing with it, but sets `busy.IsBusy` to `false` and thus signals the application is idle again.


Now, as you may have noticed, all of our `doBusy` implementations so far set `busy.ProgressPercentage` to `None`, because we have no way of way of knowing our actual progress - at least not in `doBusy`. The actual operation that we run may very well have that knowledge. How do we make use of it?

Let's take our nonsensical "long-running operation" from the start of the post and turn it into a function.
*)

let processDataThings dataThings =
    dataThings
    |> List.filter (fun thing -> thing.Value.GetHashCode() <> 12)
    |> List.map (processDataThing >> string)
    |> List.toArray
    |> asSnd "C:\DataThingResults\result.txt"
    |> File.WriteAllLines 


(**
We know the total number of items in the list, and we can find a point in our processing where each individual item is processed, so in addition to the processing, we can also report a progress percentage.
*)

let processDataThingsAndReportPercentage reportProgressPercentage dataThings =
    let totalNumber = List.length dataThings |> float

    let reportProgress i =
        reportProgressPercentage (float i / totalNumber)

    dataThings
    |> List.filter (fun thing -> thing.Value.GetHashCode() <> 12)
    |> List.mapi (fun i thing ->
        reportProgress i
        
        thing |> processDataThing |> string)
    |> List.toArray
    |> asSnd "C:\DataThingResults\result.txt"
    |> File.WriteAllLines 

(**
In the simplest case, the implementation of `reportProgressPercentage` will just be something like `fun percentage -> busy.ProgressPercentage <- Some percentage)`.

Well, that probably kind of works, but rather looks like handicraft work (German: Gebastel, read: "gabustel"). Especially that `List.mapi` usage (that we need to be able to tell how many items we have already processed) can't even really be called a workaround anymore. It's just clueless gebastel.

Another issue is that this way we would report our progress for every single item we process - if there are a lot of items (and if there weren't, we wouldn't need a computer to process them), that would mean using significant resources just to update the progress display with a frequency the human eye cannot possibly perceive - probably many thousands of times per second. 

Before we look at a better solution, let's consider something I mentioned at the very beginning: <strike>parrale....</strike> <strike>pallellara....</strike> running something on multiple cores instead of just one.

Using the `FSharp.Collections.ParallelSeq` library, which provides an F# friendly API for Parallel LINQ, we can write the above as follows:
*)

let processDataThingsParallel reportProgress dataThings =
    dataThings
    |> PSeq.filter (fun thing -> thing.Value.GetHashCode() <> 12)
    |> PSeq.map (fun thing ->
        reportProgress 1
        
        thing |> processDataThing |> string)
    |> PSeq.toArray
    |> asSnd "C:\DataThingResults\result.txt"
    |> File.WriteAllLines

(**
Depending on the number of cores available and how well the work to be done is parallelizable, that can provide a massive performance boost (obviously, because that is kind of the point of having multiple cores in the first place). What happens here is that our input data will be split up into several parts, and the parts will be processed in parallel threads, each on its own core. Again depending on what our data and the actual processing look like, there is no way to tell how fast which thread is processing items, so until `PSeq.map` returns, we never actually know our progress. The only way to get at that would be counting processed items over all threads - and the way we do that is using `Interlocked.Increment`.

Of course it isn't, but you believed that for a few clock cycles, right?

What we will really be using is a nice facility that F# offers us called `MailboxProcessor`, F#'s own lightweight implementation of the [Actor model](https://en.wikipedia.org/wiki/Actor_model). In a very small nutshell, an "actor" is a.... thing that has a queue of messages (its own private one that nobody else can access), processes any incoming messages sequentially (that means it doesn't care about any new messages until it's done with the currently processed one) and generally can only be communicated with through messages sent to its queue.

("MailboxProcessor" is a somewhat odd and unclear name; it is very common among F# programmers to alias the type to `Agent`.)

A very nice property is that we do not have to care about it threading-wise - as long as we only send it messages and don't immediately expect an answer, it is detached from the threads that we have to handle and care about. It achieves that by also using asynchronous workflows and non-blocking waiting for incoming messages using `let!`, as we've seen before. The functionality of a `MailboxProcessor` is usually implemented using a tail recursive function that calls itself with the computed state from processing the current message to then wait for the next message.

What we will let our Agent/Actor do is "collect" the progress updates from all the different threads our parallel sequence uses (that we don't even know about ourselves) and update our `IBusy` in a sensible interval, let's say every half second - that's more than fast enough for a user to get a good understanding of the actual progress.

What we need to tell the `MailboxProcessor`s processing function for that is the total nuber of items to process - so we can calculate a percentage - and the update interval.

When receiving a message, it will add the number of items reported to the current count, then check if the time passed since last setting`busy.ProgressPercentage` is greater than the update interval, and if so, update the progress value. (We will assume for this that the implementation of `IBusy` will take care of updating the value that's actually bound to the UI on the appropriate thread; otherwise we would need to do that in the `MailboxProcessor`, which we could, but don't really want to do.)

For what we want to do, our processing logic doesn't need to know about the actual `MailboxProcessor`, so we will just give it a function of type `int -> unit` to call.
*)

let getReportProgress updateInterval numberOfItems (busy : IBusy) =
    let calculatePercentage currentValue = float currentValue / float numberOfItems * 100.
    
    let progress = MailboxProcessor<int>.Start(fun queue ->
        let rec loop lastCount (lastPulseTime : DateTimeOffset) =
            async {
                let! increment = queue.Receive()

                let newCount = lastCount + increment

                let pulseNow, newLastPulseTime =
                    let now = DateTimeOffset.Now
                    
                    if now - lastPulseTime > updateInterval
                    then (true, now)
                    else (false, lastPulseTime)

                if pulseNow then
                    newCount
                    |> calculatePercentage
                    |> (fun progressValue -> busy.ProgressPercentage <- Some progressValue)

                return! loop newCount newLastPulseTime
            }

        loop 0 (DateTimeOffset.Now - (updateInterval + (TimeSpan.FromTicks 1L))))
        
    progress.Post


(**
Now again let's use all the things we've built together:
*)

let reportProgress = getReportProgress (TimeSpan.FromMilliseconds 500.) (Seq.length dataThings) busy

async { processDataThingsParallel reportProgress dataThings }
|> onThreadPool
|> doBusyAsync busy (Message "Processing thingses")

(**
I think that ended up looking pretty tidy; didn't it?

`processDataThingsParallel` will be run on the ThreadPool and process its items using Parallel LINQ, while we have a "busy" display and get progress updates all the time. And when it's done, the "busy" display will disappear, and the application can be used again.


Remember that all our `doBusy` implementations set `busy.ProgressPercentage` to `None`, so by default, we should have an "indeterminate progress" display, and in case our processing logic actually reports progress, it will then be set to `Some p`, and we'll have a value to show on screen.


One issue I realized while writing this is that handling `reportProgress` like this isn't ideal, because it's easy to think "hey, that's a function to use for reporting progress, so now that I have that, I'll use it whenever I have progress to report", but in reality, it is parameterized for exactly this one usage (especially because we have already given it the total number of items), and it has the current count as a piece of internal state that can't be reset. That means it is *necessary* semantically to create a new `reportProgress` function every time we want an operation to report its progress, but there is nothing that forces us to do so. So that part of our API leaves room for improvement.


A small note on the side - I always went with the concept of the "busy" display locking the application from user interaction; that of course doesn't have to be. It could just as well simply be a small progress bar in a tool bar or status bar that just shows up and disappears as needed. In that case we'd probably have two different implementations of `IBusy` - one that actually prevents the user from doing things, and one that doesn't.



## Possible Enhancements

Although this has taken some time now, it is actually a rather simple solution (but probably sufficient much of the time) based on something I use in practice. This concept could be expanded upon in various ways, including things like

- Cancellation of "busy" operations
- "Batched" busy operations that run in a sequence and each report their own progress, with an additional overall progress value and display
- Concurrent, but independent busy operations that begin and end each in their own time, with a busy/progress display present as long as at least one busy operation is running


## Acknowledgements

Shoutouts to [Reid Evans](https://twitter.com/ReidNEvans), whose idea the `doBusy` concept was and who even turned it into a `busy { }` [computation expression](https://gist.github.com/reidev275/656449fc88bba74bbfbd) (and who incidentally is my F# advent calendar date-mate today), and to [Tomas Petricek](https://twitter.com/tomaspetricek), who validated the idea of the `onThreadPool` function.


---
Written on my HP ZBook with [Ionide](http://ionide.io/) and [Visual Studio Code](https://code.visualstudio.com/)
*)