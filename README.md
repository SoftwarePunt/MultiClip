MultiClip
=========
MultiClip is a small application for Windows written in C# .NET that allows you to maintain multipile clipboards by switching between them. This might be useful if you need to juggle a lot of information at once.

I originally designed this because I had a Microsoft keyboard with bookmark keys (1 thru 5) and figured they could serve as a quick way to switch between clipboards.

Unfortunately reading and writing to the clipboard is rather unstable, and this program only works well with text. I am releasing this in the hope that it might be useful to someone.

Using macros / keyboard bindings
---
You can easily set up a macro or keyboard binding by simply calling the EXE.

If the program is invoked with a clipboard number as argument (e.g. "MultiClip.exe 5"), it will switch to that clipboard number upon startup. If the application is already running, it will send a signal to the existing process causing it to switch.
