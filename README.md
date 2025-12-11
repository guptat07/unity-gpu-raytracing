# unity-gpu-raytracing
Exploration of simple ray tracing on the GPU (fragment shader) using the Unity Game Engine.
Implements all tasks from Assignment 1 and 2 on the GPU.

This code runs in the Unity Editor View.
Please install Unity v6.3 and open the project.

On the camera, you will see a script component that will allow you to modify the number of bounces for reflections.
You can also turn the shader on and off.

You will see 4 spheres.
The spheres have a script component at the bottom that allows you to select their color and material properties.
They have default values that are not interesting (white and perfect mirrors).
Please play around with this. You can make many materials.

To move around, use the default Unity scene controls.
Hold Right Click, then use WASD to move around. Q and E will move you up and down.
Please observe how smooth the performance is, especially compared to Assignment 2.
