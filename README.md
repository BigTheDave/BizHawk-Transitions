# BizHawk-Transitions

Extract Build.zip into **\<BizHawk\>/ExternalTools**, run BizHawk then select Transitions from **Tools \> External Tools**

In **/Transitions/** is a **transitions.txt** which contains the list of transitions to randomly choose from. 

Each line is a transition entry. The first word is the type of transition, the words following it are parameters. Currently there is only **SoundAndGif**.

> *SoundAndGif "my sound.wav" "my animation.gif" duration_in_seconds*

If Duration is shorter than the length of the animation, the length of the animation will be used instead. 

Use -1 to force the transition length to be the same as the animation length.

The Audio must be a .wav file. The animation must be a .gif.

The maximum transition length is 10 seconds, try to keep them under 2 though.

It's best to keep .gifs under 1Mb, and 200px high.

**Example.Transitions.zip** contains a folder full of example transitions. ***It does not contain the .dll***
