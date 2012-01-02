This is just a proof of concept of some idea that I had a while ago.
Rather than making my hands dirty in minecraft building one block at 
a time, I thought I'd be cool being able to define the blocks of a 
given volume by mathematical functions.

So I ended up using mesh instancing and a minecraft material texture
for rendering simple blocks, mixed together with a Python interpreter,
that lets you define those functions.

For building the project you need to install IronPython and add the IronPython and IronPython.Modules assemblies to its references in Visual Studio. I used version 2.7.1.