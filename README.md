# IotSound
MIDI, Audio and GPIO functionality together in a homegrown synth.

The program is a background application developed on a Raspberry pi 3. 
*It uses the UART and a midi interface to provide MIDI input.
*Audio is generated using the .Net Core AudioGraph API
GPIO is used for an indicator led and eventually it will be used for input from dials and knobs.

Interesting features:
A Sinusoidal oscilator
A general purpose envelope generator
A mechanism to handle pitch bend control input
