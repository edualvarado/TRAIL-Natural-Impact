#
#   Server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import random
import time
import zmq
import numpy as np
import matplotlib.pyplot as plt
import os
from PIL import Image
from scipy.stats import norm
from scipy.optimize import curve_fit
from matplotlib.ticker import FormatStrFormatter
from mpl_toolkits.mplot3d import Axes3D
from matplotlib.ticker import MaxNLocator

context = zmq.Context()

# Pressure Socket
socketPressure = context.socket(zmq.REP)
socketPressure.bind("tcp://*:5557")

# Heightmap Socket
socketHeight = context.socket(zmq.REP)
socketHeight.bind("tcp://*:5558")

# Young Socket
socketYoung = context.socket(zmq.REP)
socketYoung.bind("tcp://*:5559")

# Vegetation Socket
socketVegetation = context.socket(zmq.REP)
socketVegetation.bind("tcp://*:5555")

# Distance Socket
socketDistance = context.socket(zmq.REP)
socketDistance.bind("tcp://*:6000")

# Counter
idx = 0

# Max/Min Values
# TODO: SET OF PARAMETERS FOR TRAINING/TESTING - Stack into x and y images
maxPressure = 1000000  # instead of np_array_pressure.max() - Do between 1000000, 5000000, 10000000 and 100000000
minPressure = 0  # instead of np_array_pressure.min()

maxYoung = 1250000  # instead of np_array_initial_young.max()
minYoung = 250000  # instead of np_array_initial_young.min()

maxCompression = 0  # instead of np_array_height_compression.max()
minCompression = -0.05  # instead of np_array_height_compression.min() - Changing from -0.1 to -0.05 to -0.025

maxAccumulation = 0.05  # instead of np_array_height_accumulation.max() - Changing from 0.1 to 0.05 to 0.025
minAccumulation = 0.0  # instead of np_array_height_accumulation.min()

maxVegetation = 1
minVegetation = 0

# Distance travelled in array
distances = []
avgCompression = []
widths = []

passesArray = []
avgVegetation = []

# Second, set up the figure, the axis, and the plot element we want to animate
fig, axes = plt.subplots(nrows=3, ncols=3, figsize=(10, 10))
ax1, ax2, ax3, ax4, ax5, ax6, ax7, ax8, ax9 = axes.flatten()
plt.subplots_adjust(hspace=0.5, wspace=0.5)

# Super title
fig.suptitle("Simulation Maps", fontsize=14, fontweight='bold')

# Single titles
ax1.set_title('Pressure')
ax2.set_title('Compression')

ax4.set_title('Initial Vegetation')
ax6.set_title('Vegetation')

ax7.set_title('Initial Young Modulus')
ax8.set_title('Vertical Accumulation')

# Initial height
np_array_initial_height = np.ones((257, 257))  # TODO: Set initial heightmap, instead of ones (in this case is 1.0)
np_array_height = np.random.random((257, 257))
np_array_height_difference = np.random.random((257, 257))

# Reshape 1D array to 2D numpy array - TODO: Set terrain dimensions automatically
np_array_pressure = np.random.random((257, 257))  # ax1
np_array_height_compression = np.random.random((257, 257))  # ax2
np_array_initial_vegetation = np.random.random((257, 257))  # ax3
np_array_vegetation = np.random.random((257, 257))  # ax4
np_array_initial_young = np.random.random((257, 257))  # ax5
np_array_height_accumulation = np.random.random((257, 257))  # ax6

while True:
    idx += 1

    # Retrieving data
    messagePressure = socketPressure.recv()
    messageHeight = socketHeight.recv()
    messageYoung = socketYoung.recv()
    messageVegetation = socketVegetation.recv()
    messageDistance = socketDistance.recv()

    # Reconvert byte array back to 2D numpy array of floats
    float_array_pressure = np.frombuffer(messagePressure, dtype=np.float32)
    float_array_height = np.frombuffer(messageHeight, dtype=np.float32)
    double_array_young = np.frombuffer(messageYoung, dtype=np.double)
    float_array_vegetation = np.frombuffer(messageVegetation, dtype=np.float32)
    float_distance = np.frombuffer(messageDistance, dtype=np.float32)

    # Reshape 1D array to 2D numpy array - TODO: Set terrain dimensions automatically
    np_array_pressure = np.reshape(float_array_pressure, (257, 257))  # ax1
    np_array_height = np.reshape(float_array_height, (257, 257))
    if idx == 4:
        np_array_initial_young = np.reshape(double_array_young, (257, 257))  # ax5
        np_array_initial_height = np.reshape(np_array_height, (257, 257))
    if idx == 2:
        np_array_initial_vegetation = np.reshape(float_array_vegetation, (257, 257))  # ax3

    np_array_vegetation = np.reshape(float_array_vegetation, (257, 257))  # ax4

    # Append distance travelled by the character
    distanceTravelled = float_distance.item()
    passes = distanceTravelled / 7
    distances.append(distanceTravelled)
    passesArray.append(passes)

    # Estimate compression and accumulation maps
    np_array_height_difference = np_array_height - np_array_initial_height
    np_array_height_compression = np.where(np_array_height_difference < 0, np_array_height_difference, 0)  # ax2
    np_array_height_accumulation = np.where(np_array_height_difference > 0, np_array_height_difference, 0)  # ax4

    '''
    # TODO: 1. Sum compression values in X and normalize - WRONG
    np_array_height_compression_flatten_width = np_array_height_compression.sum(axis=1)
    np_array_height_compression_flatten_width_sum = np.sum(np_array_height_compression_flatten_width)
    if np_array_height_compression_flatten_width_sum != 0:
        np_array_height_compression_flatten_width_norm = np_array_height_compression_flatten_width \
                                                         / np_array_height_compression_flatten_width_sum

    # TODO: Sum compression values in Y and normalize - WRONG
    np_array_height_compression_flatten_comp = np_array_height_compression.sum(axis=0)
    np_array_height_compression_flatten_comp_sum = np.sum(np_array_height_compression_flatten_comp)
    if np_array_height_compression_flatten_comp_sum != 0:
        np_array_height_compression_flatten_comp_norm = np_array_height_compression_flatten_comp \
                                                         / np_array_height_compression_flatten_comp_sum
    '''

    # Create plots
    # Second, set up the figure, the axis, and the plot element we want to animate
    fig, axes = plt.subplots(nrows=3, ncols=3, figsize=(10, 10))
    ax1, ax2, ax3, ax4, ax5, ax6, ax7, ax8, ax9 = axes.flatten()
    plt.subplots_adjust(hspace=0.5, wspace=0.5)

    # Super title
    fig.suptitle("Simulation Maps", fontsize=14, fontweight='bold')

    # Single titles
    ax1.set_title('Pressure')
    ax2.set_title('Compression')
    ax3.set_title('Compression Width')
    ax4.set_title('Initial Vegetation')
    ax5.set_title('Vegetation Trampling')
    ax6.set_title('Vegetation')
    ax7.set_title('Initial Young Modulus')
    ax8.set_title('Vertical Accumulation')
    ax9.set_title('Avg. Compression vs. Distance Travelled')

    # Send reply to the client
    # In the real world usage, after you finish your work, send your output here
    socketPressure.send(b"Pressure Map received!")
    socketHeight.send(b"Height Map received!")
    socketYoung.send(b"Young Map received!")
    socketVegetation.send(b"Vegetation Map received!")
    socketDistance.send(b"Distance received!")

    # Create dir
    dirData = 'frames/Review/SimulatorData-1/TestData-1/'  # TODO --- CHANGE! ---
    if not os.path.exists(dirData):
        os.makedirs(dirData)

    # Normalize the values in each array to the range [0.0, 1.0] or # TODO: between 0 and 255

    # ============================================== #

    # np_array_pressure_normalized = (np_array_pressure - minPressure) / (maxPressure - minPressure)
    np_array_pressure_normalized = ((np_array_pressure - minPressure) / (maxPressure - minPressure)) * 255

    # np_array_height_compression_normalized = (np_array_height_compression - minCompression) / (maxCompression - minCompression)
    np_array_height_compression_normalized = ((np_array_height_compression - maxCompression) / (minCompression - maxCompression)) * 255

    np_array_initial_vegetation_normalized = np_array_initial_vegetation * 255

    # np_array_height_accumulation_normalized = (np_array_height_accumulation - minAccumulation) / (maxAccumulation - minAccumulation)
    np_array_height_accumulation_normalized = ((np_array_height_accumulation - minAccumulation) / (maxAccumulation - minAccumulation)) * 255

    # np_array_initial_young_normalized = (np_array_initial_young - minYoung) / (maxYoung - minYoung)
    np_array_initial_young_normalized = ((np_array_initial_young - minYoung) / (maxYoung - minYoung)) * 255

    np_array_vegetation_normalized = np_array_vegetation * 255

    # Update plots

    # ============================================== #

    mapPressure = ax1.imshow(np_array_pressure_normalized, cmap='Reds', interpolation='nearest', vmin=0, vmax=255)

    # ============================================== #

    mapHeightCompression = ax2.imshow(np_array_height_compression_normalized, cmap="Reds", interpolation='nearest', vmin=0, vmax=255)
    # Add text to the upper left corner of the subplot
    ax2.text(0.05, 0.95, f"Dist: {distanceTravelled:.1f}m", transform=ax2.transAxes, fontsize=14, verticalalignment='top')

    # ============================================== #

    '''
    # TODO: GAUSSIAN WIDTH

    # Straight
    # define the Gaussian function
    def gaussian(x, mu, sigma, A):
        return A * np.exp(-(x - mu) ** 2 / (2 * sigma ** 2))

    x = range(len(np_array_height_compression_flatten_width_norm))
    y = np_array_height_compression_flatten_width_norm

    # initial guess for the parameters
    p0 = [128, 10, 1]
    # fit the Gaussian function to the data
    popt, pcov = curve_fit(gaussian, x, y, p0)
    # get the fitted parameters
    mu, sigma, A = popt

    avgCompression.append(gaussian(mu, mu, sigma, A))

    x_fit = np.linspace(min(x), max(x), 1000)
    y_fit = gaussian(x_fit, mu, sigma, A)

    # estimate width
    x_text = max(y_fit)  # x coordinate of the text
    y_text = mu  # y coordinate of the text
    width = (2*sigma*3*10)/len(np_array_height_compression_flatten_width_norm) # 256=10m

    widths.append(width)

    ax3.text(0.05, 0.95, f"W: {width:.2f}m", transform=ax3.transAxes, fontsize=14, verticalalignment='top')
    #ax3.text(0.05, 0.85, f"Avg.Comp: {gaussian(mu, mu, sigma, A)*100:.2f}cm", transform=ax3.transAxes, fontsize=14, verticalalignment='top')

    ax3.set_xlim([0, 0.2])
    ax3.plot(y_fit, x_fit, color='red')
    ax3.barh(x, y)
    '''

    # ============================================== #

    # TODO: Vegetation

    # Find the non-zero elements in A
    A = np_array_initial_vegetation[np_array_initial_vegetation != 0]

    # Corresponding elements in B
    B = np_array_vegetation[np_array_initial_vegetation != 0]

    # Calculate the percentage decrement
    percentage_decrement = (B / A) * 100

    # Get the average percentage decrement
    average_percentage_remaining = np.mean(percentage_decrement).astype(float)
    avgVegetation.append(average_percentage_remaining)

    # Get number of passes

    print(f"Remaining {average_percentage_remaining}%")
    print(f"Passes: {distanceTravelled/7}")

    ax5.plot(passesArray, avgVegetation, linestyle='-', color='green', label='vegetation')

    ax5.set_xlim(1, np.max(passesArray))

    #ax5.set_xticks(np.arange(0, 101, 10))  # Sets x-axis ticks at 0, 10, 20, ..., 100
    ax5.set_xticks([25, 75, 200, 500])

    # Get the x-ticks (vertical gridlines)
    xticks = ax5.get_xticks()

    # Plot a marker at each vertical gridline
    for x in xticks:
        y = np.interp(x, passesArray, avgVegetation)  # Interpolate to find the y-value at this x
        ax5.plot(x, y, color='green', marker='o')
        ax5.annotate('{:.0f}'.format(y), (x, y), textcoords="offset points", xytext=(10, -10), ha='center', weight="bold")

    ax5.set_xlabel('Number of passes')
    ax5.set_ylabel('Relative cover after trampling (%)')
    ax5.legend()  # This displays the legend
    ax5.grid(True)

    '''
    # TODO: GAUSSIAN COMPRESSION

    mask = np_array_height_compression_flatten_comp_norm != 0
    np_array_height_compression_flatten_comp_norm_nonzero = \
        np_array_height_compression_flatten_comp_norm[mask]

    #x = range(len(np_array_height_compression_flatten_comp_norm))
    x = range(len(np_array_height_compression_flatten_comp_norm_nonzero))

    #y = np_array_height_compression_flatten_comp_norm
    y = np_array_height_compression_flatten_comp_norm_nonzero

    # Fit a polynomial of degree n to the data
    n = 5  # change this to the degree of polynomial you want
    coefficients = np.polyfit(x, y, n)

    # Create a polynomial function from the coefficients
    p = np.poly1d(coefficients)

    # Generate y-values for the polynomial at the given x-values
    y_poly = p(x)

    ax5.set_ylim([0, np.max(y)])
    ax5.plot(x, y_poly, color='red', label=f'Polynomial Approximation (degree {n})')
    ax5.bar(x, y)

    # Get the current y-axis tick locations
    yticks = ax5.get_yticks()

    # Add a horizontal line at each y-tick location
    for y in yticks:
        ax5.axhline(y, color='gray', linestyle='--', linewidth=0.5)

    ax5.text(0.05, 0.95, f"Dist: {distanceTravelled:.1f}m", transform=ax5.transAxes, fontsize=14, verticalalignment='top')

    ax5.set_ylabel('cm')
    '''

    # ============================================== #

    '''
    # TODO: Accumulative data

    # Straight
    #ax9.plot(distances, avgCompression, color='red', label='Avg. Compression')
    ax9.plot(distances, widths, color='blue', label='Width')
    ax9.legend()  # This displays the legend

    ax9.set_ylim([0, 3])

    ax9.set_xlabel('Distance (m)')
    ax9.set_ylabel('m')
    '''

    # ============================================== #

    mapInitialVegetation = ax4.imshow(np_array_initial_vegetation_normalized, cmap="Greens", interpolation='nearest', vmin=0, vmax=255)

    # ============================================== #

    mapVegetation = ax6.imshow(np_array_vegetation_normalized, cmap="Greens", interpolation='nearest', vmin=0, vmax=255)

    # ============================================== #

    mapInitialYoung = ax7.imshow(np_array_initial_young_normalized, cmap='Blues', interpolation='nearest', vmin=0, vmax=255)

    # ============================================== #

    mapHeightAccumulation = ax8.imshow(np_array_height_accumulation_normalized, cmap="Blues", interpolation='nearest', vmin=0, vmax=255)

    # ============================================== #

    # TODO: Set min (YoungGround) and max (YoungGround + 1*YoungVegetation) values automatically
    """UNCOMMENT"""
    if idx % 20 == 0: # 20
        plt.savefig(dirData + str(idx) + ".png")  # save the figure to file


    # ---------------------------------------------------------------------------------------------

    # Stack the arrays into a single array
    """UNCOMMENT"""

    input_array = np.dstack((np_array_pressure_normalized,
                             np_array_initial_vegetation_normalized,
                             np_array_initial_young_normalized))


    """UNCOMMENT"""

    output_array = np.dstack((np_array_height_compression_normalized,
                              np_array_vegetation_normalized,
                              np_array_height_accumulation_normalized))


    # Save the images
    """UNCOMMENT"""

    dirRGB = r'frames/Review/SimulatorData-1/TestData-1/RGB/'  # TODO --- CHANGE! ---
    if not os.path.exists(dirRGB):
        os.makedirs(dirRGB)


    # USING IMAGEIO
    # input_array = (input_array * 65535).astype(np.uint16)
    # output_array = (output_array * 65535).astype(np.uint16)
    # input_array = (65535 * (input_array - input_array.min()) / input_array.ptp()).astype(np.uint16)
    # output_array = (65535 * (output_array - output_array.min()) / output_array.ptp()).astype(np.uint16)
    # imageio.imwrite(dirRGB + str(idx) + "-input.png", input_array, prefer_uint8=False)
    # imageio.imwrite(dirRGB + str(idx) + "-output.png", output_array, prefer_uint8=False)

    # USING PILLOW
    # Convert the numpy array to a PIL Image object with int16

    # input_image = Image.fromarray(np.uint8(input_array * 255))
    
    """UNCOMMENT"""

    input_image = Image.fromarray(np.uint8(input_array))


    # output_image = Image.fromarray(np.uint8(output_array * 255))
    
    """UNCOMMENT"""

    output_image = Image.fromarray(np.uint8(output_array))


    """UNCOMMENT"""

    input_image.save(dirRGB + str(idx) + "-input.png")
    output_image.save(dirRGB + str(idx) + "-output.png")


    # Fourth, animate
    '''
    if idx % 20 == 0:
        fig.tight_layout()
        plt.show()
    '''


    # Pause
    # Try reducing sleep time to 0.01 to see how blazingly fast it communicates
    # In the real world usage, you just need to replace time.sleep() with
    # whatever work you want python to do, maybe a machine learning task?
    time.sleep(1)