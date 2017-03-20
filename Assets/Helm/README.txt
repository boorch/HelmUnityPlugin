# Helm Unity Plugin

Helm is a synthesizer that you can play live inside of Unity.

## Getting started

### Editor

In an Audio Mixer, load the Helm plugin.
Pick a patch from the list.
Make sure the speaker icon is clicked at the top of the Scene window.
Now click on the keyboard at the top of the Helm plugin to hear Helm inside Unity.

### Trigger notes in scene

In your scene load the HelmController script onto an empty GameObject.
Hook up the AudioSource target mixer that appears to the AudioMixerGroup that the Helm instance is on.
Make sure the "Channel" value matches the "Channel" value in the Helm instance.
You can call NoteOn and NoteOff on this script to trigger notes from code. Just pass the midi key you would like to play.

### Make music with the sequencer

In your scene load the HelmSequencer script onto an empty GameObject.
Like above, hook up the AudioSource target mixer that appears to the AudioMixerGroup that the Helm instance is on.
Click to create a note, click again to delete. Click and drage to create a repeating sequencer.
Play the scene to hear the sequence.

# TODO

## Features

Sync audio loops to seqeuncer
Better way to change parameters live
Better way to route Helm audio into an object in 3d space
UI widgets for better tweaking in Unity
Generative music demo scene
Folders for patches
Linux support
iOS/anroid/ps4/xbox support?

## Known Issues

Note hiccup when sequencer loops because all notes after length play at once.
On low powered machines, making game full screen in Unity makes audio glitchy, even when dsp usage very low.
Low refresh time for playhead updates.

