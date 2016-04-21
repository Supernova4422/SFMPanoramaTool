# Unofficial SFM Panorama Toolkit

This is the unofficial source filmmaker panorama toolkit! This program will help out users of SFM to make their very own panoramic videos!

Dependencies: 

This software uses libraries from the FFmpeg project under the LGPL v2.1. 
FFmpeg is a trademark of Fabrice Bellard, originator of the FFmpeg project.


This software uses the libraries from The Accord.NET Framework under the LGPL v2.1. It is a Copyright (c) 2009-2014 owned by César Roberto de Souza <cesarsouza at gmail.com>

This project uses Artfunkel's Datamodel library available at: https://github.com/Artfunkel/Datamodel.NET

##Program Features: 

###Batch Rename Tools. -Completed
Instead of exporting each image sequence one at a time from SFM, export them sequentially into one big chunk. Then you can use the batch rename tool to organise each seperate angle into its own folder! For example:

Input:
Video_01
Video_02
video_03
video_04

Output:
angle1/video_01
angle1/video_02
angle2/video_01
angle2/video_02

###Batch Avi Creation. -Completed

Similar to the batch rename tools, but it will instead export the images into a single 1:1 .AVI file each, useful for importing into blender

###SFM Panorama Setter -COMPLETED
This tool can import a .DMX file of a single shot and camera, then it can turn this into several shots each with different angles, AND the right FOV settings, ready for export.


###Blender Preset - In progress
This tool is more than likely not going to be made (as its stupidly easy to do this part) however this tool would allow you to turn existing image sequences into a blender file ready for export! As I said its stupidly easy though. 


This tool may even allow users to begin rendering (with blender) immedietly after rendering, but no promises on that one. 


##Completed tools: 

###Batch Rename Tool
Use /rename and follow the on-screen prompts

