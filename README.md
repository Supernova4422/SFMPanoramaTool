# Unofficial SFM Panorama Toolkit
Blogpost with a guide on how to use the tool: https://fantasmos.wordpress.com/2018/02/26/how-i-survived-creating-a-4k-360-3d-60fps-video-of-a-tf2-map/ and behind the scenes of what it takes to make a 4k 3D video 

This is the unofficial source filmmaker panorama toolkit! This program will help out users of SFM to make their very own panoramic videos!

A usage Guide for this program can be found in documentation.rtf

Please be sure to report all bugs, and provide relavent .dmx files when applicable

#Dependencies and libraries
This program adapts Logan McCloud's 360 degree method. Check out his stuff at: http://hyperchaotixanimation.tumblr.com/

This software uses libraries from the FFmpeg project under the LGPL v2.1. 
FFmpeg is a trademark of Fabrice Bellard, originator of the FFmpeg project.


This software uses the libraries from The Accord.NET Framework under the LGPL v2.1. It is a Copyright owned by César Roberto de Souza <cesarsouza at gmail.com>

This project uses Artfunkel's Datamodel library available at: https://github.com/Artfunkel/Datamodel.NET

## Program Features: 

### Batch Rename Tools.
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

### Batch Avi Creation.

Similar to the batch rename tools, but it will instead export the images into a single 1:1 .AVI file each, useful for importing into blender

###SFM Panorama Setter
This tool can import a .DMX file of a single shot and camera, then it can turn this into several shots each with different angles, AND the right FOV settings, ready for export.

###TODO: 
User Experience Changes

Usage Guide

Remove dependency on Accord.Video.FFMpeg with another library

Allow to be run from the command line, and thus Source Film Maker
