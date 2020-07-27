# Liver Deformation X-Ray Imaging Phantom

![fullphantom](./fullphantom.jpg)

![phantommodel](./phantommodel.jpg)

![phantomparts](./phantomparts.jpg)

In the field of medical imaging, so called imaging phantoms are used  to evaluate, analyze, and tune the performance of various imaging  devices (Source: Wikipedia). This is a specially designed imaging  phantom that simulates deformation of a liver and respiratory motion.  The direct material costs are approximately USD 200.

The inspiration to do this project was this video on YouTube: ELPHA: Dynamically deforming liver phantom, https://youtu.be/u_GXM9yy9Ok

On an X-Ray image, the material and the density define what is a  bone, a liver, a lung etc. To simulate the deformation of a liver, an  object of a different density than the density of the surrounding body  is needed. The body must have a uniform density and must not have metal  parts or air gaps since this would create artifacts on the image.

Commercially available motion phantoms are not specially designed for liver  deformation based on respiratory motion patterns and are more general  purpose respiratory phantoms. 

Whereas the above mentioned ELPHA  uses pressure for the deformation, this phantom simulates the  deformation by moving and rotating parts of the liver relative to each  other. The phantom is all 3D printed but uses different infill  percentages for the different parts. The body has an infill percentage  of lets say 30% and the liver parts an infill percentage of lets say  60%. 

This short video gives you an idea how this phantom would look like on X-Ray images: 

https://youtu.be/K-1KQQ25YmM

To drive the cylinders, the phantom uses LnR-Actuators, an Arduino  micro controller and an Adafruit Servo Shield. The micro controller is  required because you cannot be in the room when you acquire X-Ray  images. The Arduino is programmed with different motion patterns. You  start the phantom from a PC that is connected to the phantom. 

With this phantom, something with a different density than the body  structure changes its shape. The shape of a liver is just indicated. The nice thing is that anyone with a 3D printer can create a deformation  phantom and play with different shapes and densities.

### Supplies:

1 Arduino Uno

1 Adafruit 16-Channel Servo Shield

3 [LnR-Actuator](https://www.instructables.com/id/Linear-and-Rotation-Actuator/)

11 Screw M2 x l10

6 Screw M2 x l20

6 Wooden dowel d6 x l40 mm

3 Wooden dowel d8 x l160 mm

2 Wooden dowel d8 x l110 mm

4 Metal marker d1 x l4 mm, created from paper clip

Legend: l:length in millimeters, d:diameter in millimeters

Windows PC with Java 8 runtime and .NET Framework 4.6.2

## Step 1: 3D Printed Parts

1 LDXIP-Back, 20% infill

1 LDXIP-Body, 30% infill

1 LDXIP-BodyInsertBack, 30% infill

1 LDXIP-BodyInsertCenter, 60% infill

1 LDXIP-CPUPlatform, 20% infill

1 LDXIP-CylinderLeft, 30% infill

1 LDXIP-CylinderLeftCylinder, 60% infill

1 LDXIP-CylinderLeftInsertBack, 30% infill

1 LDXIP-CylinderLeftInsertCenter, 60% infill

1 LDXIP-CylinderRight, 30% infill

1 LDXIP-CylinderRightCylinderBack, 30% infill

1 LDXIP-CylinderRightCylinderCenter, 90% infill

1 LDXIP-CylinderRightInsertBack, 30% infill

1 LDXIP-CylinderRightInsertCenter, 60% infill

2 LDXIP-Flange, 20% infill

2 LDXIP-FlangeClip, 20% infill

1 LDXIP-Front, 20% infill

2 LDXIP-FrontClip, 20% infill

1 LDXIP-GatingBase, 20% infill

1 LDXIP-GatingBottom, 20% infill

1 LDXIP-GatingPlatform, 20% infill

1 LDXIP-GatingTop, 20% infill

## Step 2: Assemble the Two Cylinders

![assemblerleftcylinder](./assemblerleftcylinder.jpg)

![assemblerightcylinder](./assemblerightcylinder.jpg)

To build the left cylinder, insert the four metal markers d1 x l4 mm into the notches of the *LDXIP-CylinderLeftCylinder*. Then insert the *LDXIP-CylinderLeftCylinder* into the *LDXIP-CylinderLeftInsertCenter* and this into the *LDXIP-CylinderLeft*. Add the *LDXIP-CylinderLeftInsertBack* and fixate all the parts with a drop of glue. Insert three wooden  dowels d6 x l40 mm into the holes in the front plane of the cylinder and attach a *LDXIP-Flange*.

To build the right cylinder, insert the *LDXIP-CylinderRightInsertCenter* and add the *LDXIP-CylinderRightInsertBack*. Then insert *LDXIP-CylinderRightCylinderCenter* and *LDXIP-CylinderRightCylinderBack*. Insert three wooden dowels d6 x l40 mm into the holes in the front plane of the cylinder and attach a *LDXIP-Flange*.

## Step 3: Assemble the Phantom Body

![assemblephantombody](./assemblephantombody.jpg)

To build the body, insert the *LDXIP-BodyInsertCenter* and then the *LDXIP-BodyInsertBack* into the *LDXIP-Body*. Insert two wooden dowels d8 x l110 mm into the holes in the front plane of the body and attach a *LDXIP-FrontClip.*

## Step 4: Assemble the Actuator Assembly

![assembleactuators1](./assembleactuators1.jpg)

![assembleactuators2](./assembleactuators2.jpg)

To build the actuator assembly, fixate three wooden dowels d8 x l160 mm to the *LDXIP-Back* with three screws M2 x l20 mm. Plug-in the two *LnR-Actuators* and add the *LDXIP-Front*. Add a *LDXIP-FlangeClip* to each of the *LnR-Actuators* but do not fixate it yet.

Plug-in the assembled phantom body into the actuator assembly. Connect the  flange with the flange clip. Align the cylinders and then fixate the  flange clips with some glue.

## Step 5: Add the Gating Platform

![gatingplatformparts](./gatingplatformparts.jpg)

To build the Gating assembly, attach the *LDXIP-GatingBottom* to the *LDXIP-GatingBase* with two screws M2 x l10 mm. Plug-in a *LnR-Actuator* and add the *LDXIP-GatingTop*. Add the *LDXIP-GatingPlatform* and fixate it with a screw M2 x l10 mm. Attach the Gating assembly to the actuator assembly with four screws M2 x l10 mm.

## Step 6:  Add the Micro Controller

![mountedmicrocontroller](./mountedmicrocontroller.jpg)

Mount the Arduino on the *LDXIP-CPUPlatform* and fixate it with  four screws M2 x l10 mm. Attach the Adafruit Servo Shield. Mount the CPU assembly on the actuator assembly. Connect the servos to the servo  shield in this order: Longitudinal (long cable) Left, Right, Gating.  Rotary (short cable) Left, Right, Gating.

## Step 7: Load Firmware and Calibrate

![uploadfirmware](./uploadfirmware.jpg)

Download and extract the file *LiverDeformationPhantom.zip* from here: [https://github.com/](https://github.com/mrstefangrimm/No2/tree/master/Instructable)

Connect the phantom to your PC. Open the Windows Device Manager and the file *upload_arduinosoftware.bat* in an text editor and change the port number in *upload_arduinosoftware.bat*. Double-click it to upload the software to the Arduino.

The phantom is not quite ready yet, it requires a calibration. To load existing calibration data double-click *start_dedicatedkeyboard.bat*. If the application starts up but does not connect to the phantom, edit  the file and add the port number to the command line, i.e. *SoftDKb.win64.exe COM9*.

In the application, click on the yellow "C" and the caret right to the text *Put Device Data*, this opens a file dialog. Select the file devicedata.txt in the subfolder *bin\SoftDKb*, this installs the calibration data from this file. For more information on how to create your own calibration data, read the Instructable [Marker-Motion-X-Ray Imaging Phantom](https://www.instructables.com/id/Marker-Motion-X-Ray-Imaging-Phantom/).

## Step 8: Use It With the Dedicated Keyboard Simulation

https://youtu.be/rTQZi-G0JvI

The Dedicated Keyboard Simulation (SoftDKb) is a Java based application to control the phantom. It is a simulation of a real hardware keyboard which I did not produce for this phantom. The  SoftDKb has all the functionality to operate, calibrate and test the  phantom. Its user interface might be not that intuitive, but when new  functionality is added to the phantom, this software is adapted to it  first.

To start, double-click *start_dedicatedkeyboard.bat*. If the application starts up but does not connect to the phantom, edit  the file and add the port number to the command line, i.e.  SoftDKb.win64.exe COM9.

First aid for new users: 

The square are buttons, the circles are light bulbs. 

M: Manual mode, P: Preset or Program mode, R: Remote, C: Calibration

In Manual mode, the cylinders and the Gating platform can be moved with  the "arrow keys". The buttons 1 - 8 are used to set the step size.

In Preset mode, the buttons 1 - 8 are used to start one of the stored  motion pattern. The "arrow keys" have no function in this mode.

In the Remote mode, the phantom can be controlled by other applications such as the Virtual Phantom Application. 

When you press "C" the phantom goes into the calibration mode. Only in  calibration mode, Get and Put Device Data are active. To leave the  calibration mode, a reset of the phantom is required.

## Step 9: Use It With the Virtual Phantom Application

https://youtu.be/C2IkKYsVn7M

he Virtual Phantom Application (ViphApp) is a .NET based application  to control the phantom. The application is a simulation of motion  phantoms. But it is also possible to connect a phantom to it and control the phantom.

To start, double-click *start_virtualphantom.bat*. This version is a prerelease with an endless list of what could/should/must be done. 

First aid for new users: 

The application always starts with the simulation for the Marker Motion  Phantom. The settings panel to change to The Liver Deformation Phantom  is on the left.

If the motion phantom is connected to your PC, select the COM port (bottom left) and press Connect.

With the arrow on the bottom left, the log message window is shown. If the  top message is 'Synced', the application is connected.

