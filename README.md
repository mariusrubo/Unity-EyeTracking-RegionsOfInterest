# Unity-EyeTracking-RegionsOfInterest
A framework to process eye-tracking data in 3D space with regards to regions of interests

# Motivation
When analyzing gaze data from users in virtual reality (VR), we typically want to describe gaze with regards to certain objects 
in the scene (here called "regions of interest" (ROI) to comply with the standard term in traditional monitor-based eye-tracking
research). We might ask the following questions:
* Is the object currently visible to the user, or hidden by another object?
* Is the object located within the current field of view of the display?
* By what angle does the user's gaze miss the object? (if this angle is low, say <2°, we may say the user "looks at the object"). 
* Does the user look above, under, left or right to the object? (that is, separate the deviation along an x- and y-axis. This
can be especially useful for real-time drift correction)
* If the scene was to be filmed by a camera, at what coordinates in pixels would the object appear in the video?
* If the object has a front (e.g. a virtual character's head), is it currently looking at the user?

The attached script 'EyeTracking.cs' provides a class to bundle this information and a single function to process it, and also gives an example how to use both. 

# Installation
Attach this script, stream the gaze rays from both eyes provided by your eye-tracker's API in the Update()-Loop, insert your 
relevant ROI's transforms in the editor and don't forget to store the data e.g. in a csv-file (not covered here). 
