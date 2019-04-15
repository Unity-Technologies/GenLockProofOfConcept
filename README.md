# Unity Proof-of-Concept Project for Improved Framerate Precision and Genlock

This Unity sample project demonstrates a technique using a WaitForEndOfFrame coroutine to improve precision when trying to achieve a specific target framerate.

The sample code emulates an external genlock using the system clock, but demonstrates how such an external signal could be used to drive the rendering framerate of the application. Furthermore, the sample also shows how game time framerate can be controlled independently of rendering framerate. This is particularly useful when an external Genlock is know to operate at a specific rate (eg. 60fps) but is not precisely tied to the local system clock (ie. the system clock's concept of 1/60th of a second is slightly different than the Genlock, so drifting would occur).

The interesting C# code is found in GenLockedRecorder.cs which is instantiated on the "GenLocked Recorder" GameObject in the scene Test.unity. The GenLockedRecorder accepts a Camera and rendering dimensions to define what should be rendered. It also takes an (emulated) Genlock Rate parameter and Game Time Rate parameter to drive the timing. Optionally, frames produced when the scene is run can be captured to disk for later closer inspection. This output will be stored as either an MP4 video or a sequence of JPGs depending on the chosen option.

Note that in order to avoid having disk I/O affect rendering, when the Record option is enabled, rendered frames are stored in memory while running and are written to disk when exiting Playmode in Editor, or, quitting the application in Standalone builds. This means that care should be taken to avoid having the application run for a long period of time with the Record option enabled as the machine will easily run out of memory.
