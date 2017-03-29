The purpose of this program is to illustrate the principle of computer tomography in 2D.

First the image is processed to form so called sinogram. It contains data about original images colors along virtual roentgen rays.
Here is the famous Shepp–Logan phantom:

![alt tag](https://github.com/Lezrith/CTScanSimulation/blob/master/CTScanSimulation/Images/SheppLogan_Phantom_small_centered.png)

And corresponding sinogram created by the program:

![alt tag](https://github.com/Lezrith/CTScanSimulation/blob/master/Examples/sinogram_phantom.png)

From here we can use back projection algorithm and try to restore original image from incomplete data we have.

![alt tag](https://github.com/Lezrith/CTScanSimulation/blob/master/Examples/backprojection_no_filter_phantom.png)

We see there is an ugly glow to it. To mitigate that we apply a kernel to each sinogram row.

![alt tag](https://github.com/Lezrith/CTScanSimulation/blob/master/Examples/backprojection_filter_phantom.png)

Both sinogram creation and back projection are a computing-intensive tasks but fortunately are ideal for paralyzation. We exploited this fact to significantly improve speed of the program.

Program parameters such as number of scans, number of detectors, system width can be regulated using sliders. There is also option to create sinogram and back project in steps.

![alt tag](https://github.com/Lezrith/CTScanSimulation/blob/master/Examples/program.png)

Work of K. Wencel and K.Tomczak.
