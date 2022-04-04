PinVol - Audio volume manager for virtual pinball cabinets

For setup instructions, please see http://mjrnet.org/pinscape/PinVol.html

PinVol is a little helper program that makes it easier to control the
audio volume on a virtual pinball cabinet.  It does several things
that are useful in a pin cab environment:

- Hotkey volume controls: PinVol lets you control the master
  Windows audio volume level using hotkeys of your choice.  You
  can assign keys that are mapped to your cabinet buttons, so
  that you can adjust the volume level with cabinet butons
  rather than by fiddling with the speaker volume knob.

  If you're using a controller that has a "Shift" button that
  allows you to assign two meanings per button, you can make
  the volume controls nicely inconspicuous, but still readily
  available, by assigning them to shifted buttons.

- Joystick button volume controls: alternatively, if your cabinet 
  buttons are connected via a joystick interface, you can assign
  joystick buttons as the volume controls.

- Per-table volume levels: PinVol keeps track of which Visual
  Pinball table is loaded, and remembers the last volume level
  for each table separately.  Whenever you load a table, PinVol
  automatically restores the volume level from the last time you
  were playing that table.  This helps equalize the volume level
  from one table to the next.  Once you've set the levels for the
  tables you play, you won't have to keep making adjustments every
  time you switch games, since your preferred level for each table
  is automatically restored when the table starts.  A separately
  adjustable Default Volume level is used for tables that don't 
  have their own PinVol settings yet.

- A separate master volume level: PinVol has separate "Global" and
  "Table" volume levels.  The individual table volume levels
  described above are stored as percentages of the global level, 
  so you can easily make EVERYTHING louder or quieter simply by
  adjusting the global level.  The per-table memory is still there
  to equalize the level across tables.

- Surround Sound Feedback (SSF) support: if you have a 7.1 surround
  sound system, PinVol lets you adjust the volume levels for each
  of the surround channels independently, to optimize the volume
  balance for table sound effects.  As with the other volume level
  settings, this is stored on a per-table basis, so that you can
  adjust each table for the best sound.  To use this feature, you
  must first install EqualizerAPO, a separate program (also free
  and open-source) that you can get from SourceForge.net.  This
  feature was added in January 2021 courtesy of mrloofer.

- Night mode support: PinVol keeps a separate Global volume level
  for night mode, so that you can switch to quieter play with a
  single button press.  When you want to go back to normal play,
  just press the button again, and the previous Global volume
  level is restored.  If you're using a Pinscape controller,
  PinVol will automatically sense when you engage Night Mode
  on the Pinscape unit, so you don't even need to assign a PinVol
  button for Night Mode in this case.

- Multiple sound card support: if you have separate sounds cards
  for the backbox speakers and in-cabinet effects speakers, you
  can control the volume level on both at the same time with the
  one set of buttons.  You can choose which sound cards are
  included in the volume controls.

- PinballY integration: PinballY sends PinVol information on the
  title of each game it launches, which lets PinVol display the
  friendly game title instead of just the filename.

