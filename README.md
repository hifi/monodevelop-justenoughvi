# monodevelop-simplevi
Simple basic Vi add-in for MonoDevelop 5. It's not supposed to *completely* emulate Vi/Vim but be just enough for navigating.

What supposedly works:

 * Most normal mode single key bindings
 * Some repeat counts for normal mode keys
 * Full line visual select with yank/delete
 
Major things missing:

 * `:` ex mode
  - like substitution with `s/foo/bar`
 * `/` search
  - possibly just bind it to MD search box
  
There's also a lot of bugs in cursor placement (compared to Vi/Vim) when handling some commands. Some are not a big issue but some are annoying enough and should be fixed.

Project also needs some refactoring when new modes are added.
 
