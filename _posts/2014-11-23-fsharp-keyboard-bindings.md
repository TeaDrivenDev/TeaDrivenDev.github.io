---
layout: post
title: F# Keyboard Bindings with AutoHotkey 
---

I know this is something a few of the purists will frown upon, but maybe somebody finds it helpful.

Some weeks back, I read a tweet of someone wanting to remap their Caps Lock key to something they found more useful - and while I hadn't had that exact thought before, I'm sure I'm not the only one who sometimes thought a `|>` key would be quite handy for F# programming. So, yeah - who needs Caps Lock anyway?

I'd never used AutoHotkey before, but I figured it would probably be the tool to use, so I installed it and with some googling and the included help file figured out the most important things I'd need (AutoHotkey has its own rather crude scripting language).

> Note that AutoHotkey is a Windows-only application, but I suppose similar software exists for other operating systems.

As for the particular bindings I should have beyond `|>`, I could only think of a few, because F# doesn't have a lot of syntax you need *all* the time and is almost entirely free of boilerplate code. I settled on `|>`, `>>`, `->`, their 'inverse' operators `<|`, `<<` and `<-` (not that I would know of any good place to use `<<`, but I kept it for symmetry's sake) as well as the keyword `fun`, later to be extended to `let` and `match .... with`.

Now, that's a good number of bindings to achieve with Caps Lock alone, but as it turns out, it can actually be combined with the usual modifier keys (yes, Shift+Caps Lock is a valid key combination), and there's other keys that under normal circumstances don't live up to their full potential - so maybe we can help one or two of them live a more fulfilled input life.

After a bit of experimenting, I settled on various Caps Lock combinations for the operators and used `#` for the keywords. I found `#` a good choice on a German keyboard, because it's easy to find and reach, sitting right next to Enter, is normally only modified by Shift (to produce the apostrophe) and doesn't have any Visual Studio/ReSharper hotkeys that I know of. I think it's the same on a UK keyboard; for other layouts like the US one, that would probably have to be swapped out for another more practical key.

Oh, and I also found a way on Stack Overflow to keep the Caps Lock toggle functionality by combining it with the Windows key.

The actual bindings I went with are as follows:
 
 * Caps Lock: `|>&nbsp;`
 * Shift+Caps Lock: `<|&nbsp;`
 * Ctrl+Caps Lock: `>>&nbsp;`
 * Shift+Ctrl+Caps Lock: `<<&nbsp;`
 * Alt+Caps Lock: `->&nbsp;`
 * Shift+Alt+Caps Lock: `<-&nbsp;`
 * Ctrl+#: `let&nbsp;`
 * Shift+Ctrl+#: `fun&nbsp;`
 * Alt+Ctrl+#: `match&nbsp;&nbsp;with`
 * Windows+Caps Lock: Caps Lock

Note that after the operators and `let` and `fun` keywords, I already included a space, because I'll always want that, and between `match` and `with`, there are two, and the caret will be placed between them, because I'll always want that too. There is no "surround" functionality as it is possible with Visual Studio snippets or ReSharper templates; while that *could* be done, it would be quite an ordeal (I looked into it) and didn't seem important enough to really bother with it.

The necessary steps are as follows:

1. Download and install AutoHotkey from [http://www.autohotkey.com/](http://www.autohotkey.com/).
2. Run the application, and open the default script in Notepad with Ctrl+E.
3. Paste in the following (you should be able to delete the default contents; as far as I remember, that's only informational text):

```
    ; In case you think you may need Caps Lock again at some point,
    ; we'll move it over to Win+Caps Lock
    
    #Persistent
    SetCapsLockState, AlwaysOff
    
    #Capslock::
    If GetKeyState("CapsLock", "T") = 1
    	SetCapsLockState, AlwaysOff
    Else
    	SetCapsLockState, AlwaysOn
    Return
    
    ; Now for the F# hotkeys
    
    ; |> is Caps Lock
    Capslock::SendInput |>{space}
    
    ; -> is Alt+Caps Lock
    !Capslock::SendInput ->{space}
    
    ; >> is Ctrl+Caps Lock
    ^Capslock::SendInput >>{space}
    
    ; <| is Shift+Caps Lock
    +Capslock::SendInput <|{space}
    
    ; <- is Shift+Alt+Caps Lock
    +!Capslock::SendInput <-{space}
    
    ; << is Shift+Ctrl+Caps Lock
    +^Capslock::SendInput <<{space}
    
    
    ; The following mappings' practicality is keyboard layout dependent
    ; Replace # with a key that's easy to reach and doesn't have many usages in your layout
    
    ; let is Ctrl+#
    ^#::SendInput let{space}
    
    ; fun is Shift+Ctrl+#
    +^#::SendInput fun{space}
    
    ; match .... with is Alt+Ctrl+#
    !^#::SendInput match{space}{space}with{left}{left}{left}{left}{left}
```

4. Save and close the file, and reload the script in AutoHotkey with Ctrl+R.

The keyboard bindings will now be active whenever AutoHotkey is running. If you want *that* to be **always**, put AutoHotkey in the Startup folder in the start menu. If you already use AutoHotkey and want to keep this separate, you can also create a new `.ahk` file for the F# bindings and only run that when you need them; running the `.ahk` file directly should create an additional instance of AutoHotkey running alongside any existing ones.    
    
As with all new tools, you probably won't use this automatically. Initially, you'll constantly have to remind yourself that you have this, and that it's 'cooler' to do it this way than typing in the things normally. You'll likely also hit the wrong key combinations quite regularly for a while, creating additional work instead of saving keystrokes. But once it starts transitioning to a habit, I find that it improves the flow of typing quite a bit, especially as the operators are somewhat fiddly to type on a German keyboard.  


> This thing took two hours to write; who really has time for this blogging stuff?!


---
Written on my HP ZBook with MarkdownPad