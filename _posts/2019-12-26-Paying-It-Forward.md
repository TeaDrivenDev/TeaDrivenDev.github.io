---
layout: post
title: Paying It F#rward 
---

_This article is an entry in the [2019 F# Advent Calendar in English](https://sergeytihon.com/2019/11/05/f-advent-calendar-in-english-2019/). Thanks to Sergey Tihon for organizing it, as always!_

The F# community, while small compared to those of many other languages, is known to be exceedingly open and helpful to beginners. I experienced this myself when I started learning the language nearly six years ago, and people (especially the F# MVPs) were always quick and eager to answer my newbie questions on [Twitter](https://twitter.com/search?q=%23fsharp&src=typed_query&f=live), or wrote extensive answers on [Stack Overflow](https://stackoverflow.com/questions/tagged/f%23) or [Code Review](https://codereview.stackexchange.com/questions/tagged/f%23). Having had this experience early on has caused me and others to try and make the start easier for those who come after us where we can, be it on the aforementioned platforms or for example in the [mentorship program](https://fsharp.org/mentorship/index.html) run by the F# Software Foundation.

## Lighthouse

Another platform with the specific aim of providing mentoring for learning programming languages (F# is one of over 50 language "tracks" at this point, from assembly and Bash to Haskell and Prolog) that I've been involved with over the past eight months is [Exercism](https://exercism.io).

The concept of Exercism has two pillars - test driven development, and dedicated feedback from people with experience in the specific language you are trying to learn, whom the site calls "mentors".

You download an exercise via the command line client - for F#, that gives you a .NET Core project with two files, one of which contains unit tests, and the other is the file in which you implement your solutions. The tests start out mostly skipped; the idea is to implement enough of the solution to get one test to pass, and then un-skip the next test and make that pass in turn. There's a lot of similarity here to the well-known concept of "katas", except that writing the tests is not part of the exercise, as they already exist.

When all the tests pass, you can submit the solution through the client, and can then request feedback from a mentor. One of the mentors will then pick the solution from the queue and make suggestions for improvement; you can then update your solution and submit a new iteration, and then either get more suggestions for another round, or the mentor will "approve" your solution, which you can then publish on your profile and move on to the next exercise.

If you are going through the language track in "mentored mode", and the exercise was one of the "core" exercises, there will also be a new core exercise and probably several "side" exercises unlocked. (The other mode is "practice mode", where all exercises are available immediately, and you can tackle them in any order; you will still be able to request mentor feedback in this mode.)

## The Art Of Reason

My task as a mentor is to help the students take their solution from working code to an idiomatic, readable and maintainable implementation by suggesting improvements. These can be more cosmetic things, such as clearer naming or more readable formatting, changes around the usage of various language constructs, types or core library functions, or sometimes a completely new approach if a solution is very imperative or, in the other extreme, goes overboard with functional programming gimmicks.

The idea there of course is not to just give people finished code and say "this is what you should have done". Instead, I will point out things that could be improved, explain _why_ I think that is, and often mention specific language concepts (say, Active Patterns) or functions in the core library to consider. There is also no point in students just writing what I tell them to get me to approve a solution - the goal is always to help people get a better understanding of the language and build habits for writing better F# code. I have been reading Kit Eason's "Stylish F#" recently, and this early paragraph sums up my idea of mentoring perfectly:

<blockquote class="twitter-tweet"><p lang="en" dir="ltr">This paragraph from <a href="https://twitter.com/kitlovesfsharp?ref_src=twsrc%5Etfw">@kitlovesfsharp</a>&#39;s &quot;Stylish F#&quot; describes exactly the everyday challenge of mentoring on <a href="https://twitter.com/exercism_io?ref_src=twsrc%5Etfw">@exercism_io</a> - giving people suggestions and the reasoning behind them. I say &quot;consider doing x&quot; a lot and should probably do it a lot more. <a href="https://t.co/n3R5EF87Pf"><img src="https://pbs.twimg.com/media/ELEPuKAXYAEXTCr?format=png" /></a></p>&mdash; Тэ дрэвэт утвикλэрэн (@TeaDrivenDev) <a href="https://twitter.com/TeaDrivenDev/status/1202759880676917248?ref_src=twsrc%5Etfw">December 6, 2019</a></blockquote> <script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>

In that spirit, I aim to let students keep as many of their own ideas as possible, even if the result is not the exact code I would have written. A handful of the simpler exercises don't leave a lot of room for variation, and there's exactly one good solution, sometimes down to the letter. For many, the idea will always be more or less the same, but details of the implementation can vary, and for the more complex exercises, pretty much every solution will be unique to that student, from the initial attempt through the entire trail of revisions.

## Keen Of Knowledge

The students on the F# track come from all kinds of different programming backgrounds (where I can tell). Of course, a lot of them have experience with C#, but many don't know .NET at all, and every now and then, there's someone who's well-versed in functional programming in languages such as Haskell or Lisp.

Consequently, students' approaches and attitude can vary greatly. Often people will have a hunch that their solution is not optimal, but lack the knowledge about certain language features (such as function composition, or destructuring tuples instead of using `fst` and `snd`) or functionality available in the core library (like the functions in the `Option` module to avoid pattern matching on options). Some will naturally reach for mutable values or data structures, because it's what they know (that doesn't happen all that much, though). And those that already know functional programming will try to work around F#'s "inadequacies" regarding certain higher functional concepts (and have me stumped when they explain to me what they're trying to achieve - with a Lisp example).

Related to that, every student will have their own specific target regarding how "good" they want the code to be. Many are content with achieving the minimum solution that will make me approve the code, while others will also implement additional suggestions or ask about was to improve things they think may not be great (as mentioned above). And occasionally, a student will just tinker on their own for a few days and and submit a number of iterations with all the ideas they tried. This is all of course dependent on the individual student's general programming proficiency - more experienced programmers will more easily delve deeper when learning a new language.

Different students also behave very differently in terms of communication. Many never write anything in the comments area unless they're really stuck and have to ask to get unblocked. Those typically seem to be people who are generally more experienced with programming and can work just with the hints I give them. Less experienced programmers of course ask more questions, because they may need more guidance to understand certain concepts. Others are just very interested about the background of various things, how things are implemented under the covers, or are concerned about performance.

## Time, What Is Time

Depending on the student's needs and understanding, and the specific exercise, the time I need to invest also varies widely. For some simple exercises, I usually don't even run the tests anymore - I know what a good, working solution looks like, and I know how to get there from various other states, and many for many things I have the answers or suggestions in a text file I've built up over time (because I don't want to write the same sentence suggesting `Option.defaultValue` 200 times; it also helps with being consistent in my answers).

For more complicated exercises, or when students ask very specific questions, the effort will be greater, sometimes by a lot - I try every improvement I want to suggest for a student's code myself to make sure it actually works and makes sense, and occasionally I'll even dive into the FSharp.Core source code for some background information (and everyone who's seen that knows it can be a confusing place). In rare cases, a single reply will take me half an hour or more to work out, and while most solutions go through no more than five iterations (and many are approved sooner), every now and then one will reach ten or more.

## The Learning

Now, all that can be a significant time investment at times, but it's definitely worth it. One aspect is simply giving back to the F# community as mentioned and giving others an easier start. Another one is that teaching something is an important way to get better at it oneself, for a few reasons:

- To explain not only how to do something, but also the reasoning behind it, I need to understand more than I would just for doing it.
- If I know how to do something, I will usually always do it that way and have little reason to experiment. Seeing students trying to get the same thing to work without my body of knowledge often lets me see different ideas and occasionally even teaches me things about F# I did not know.
- Seeing many different solutions for the same problems and thinking through ways to improve them trains my code reading, analytic and reasoning skills. I solved all of the core exercises and a number of the additional ones before I started mentoring in April, but many of the solutions from back then look quite different from how I would implement them now.

One bigger thing I need to mention here specifically: The exercise "Tree Building" is about refactoring working code to a more idiomatic solution, and as the instructions also mentioned it was "slow", I tried to check solutions for performance as well, but did not really have a way to do that. When I mentored [Scott Hutchinson](https://twitter.com/ScottHutchFP)'s solution, we got talking about that, and Scott suggested using [BenchmarkDotNet](https://benchmarkdotnet.org/), which I had no experience with.

With some sample code from Scott I set up benchmarking for the exercise for myself to be able to really judge students' submissions in that regard, comparing them with the original code, the fastest solution I could find (which incidentally was Scott's at the time), and mine (just out of interest). Then, to actually be able to tell students what they could to do improve the performance of their solutions, I needed to actually have made it faster myself - incidentally my own solution at that point was still fairly slow (and in particular allocated a lot of memory), so I didn't have to look far. As I knew Scotts code was fast, I looked at that for inspiration - and with a few changes "accidentally" made mine not only faster than it was before, but even slightly faster than _his_ (which probably mostly comes down to slightly lower memory allocations).

Now, while that helped me for mentoring, students still wouldn't have a way to know how much faster or slower any changes made their code, which of course wasn't ideal. I suggested officially adding benchmarking to the exercise and make the performance aspect an official consideration, an idea that [Erik Schierboom](https://twitter.com/ErikSchierboom), the maintainer of the F# track (among many other things at Exercism), was very excited about, and I eventually sent a pull request to implement the suggestion.

## Final Thoughts

If you're just starting to learn F# (or any of the [50 other languages](https://exercism.io/#explore-languages)), or if you want to improve your fluency in the language, have a look at Exercism. There are over 100 F# exercises covering pretty much any language feature (short of type providers), so you can keep yourself (and us) busy for a while.

And if you think you don't need the practice, or at least not the mentoring, consider becoming a mentor; we've cleaned up the queue quite well in recent weeks, but we could do with one or two more active mentors who can spare an hour two or three times a week, to even out the wait times for students a bit more.