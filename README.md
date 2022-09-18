# Hackzurich - #6 Siemens Trains

Project for Hackzurich 2022

Unity Version: 2019.4.15f1

# cleARails

A practical and elegant solution to impractical clearance outline measurements for railways.

## Installation

### Android

- [Download and install]() the APK
- That's it 👍

### IOS

- Coming soon

## Usage

Open the app and point the camera towards the tracks. Wait for a few moments for
the mesh rendered in the app accurately represents the ground around the tracks.
Once it is stable, choose the visualization mode (area or tunnel) and the
opacity level. Now you are free to move around.

NOTE: For best results, center the tracks in the bottom half of the
screen, keeping the vanishing point in frame. Ensure good lighting so that
there is enough contrast between the tracks and the environment.

## References

For line detection we used the functionality provided by [xmba15](https://github.com/xmba15/rail_marking).
Our fork can be found [here](https://github.com/aleksandra-kim/rail_marking).

The augmented reality interface was implemented using [ARCore](https://developers.google.com/ar)
and [Unity](https://unity.com/). An analysis ARCore's accuracy compared to other
tools can be found [here](https://www.researchgate.net/publication/352312319_On_the_Accuracy_of_Google_Tango_in_Comparison_to_ARCore).

We thank [Siemens](https://www.siemens.com/) and [HackZurich](https://hackzurich.com/)
for the opportunity to work on this interesting challenge.