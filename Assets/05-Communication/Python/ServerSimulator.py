#
#   Server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import random
import time
import zmq
import matplotlib.pyplot as plt
import matplotlib.colors as mcolors
import numpy as np
import os
from pylab import *
from PIL import Image

context = zmq.Context()

# Vegetation Socket
socketVegetation = context.socket(zmq.REP)
socketVegetation.bind("tcp://*:5555")

# Pressure Socket
socketPressure = context.socket(zmq.REP)
socketPressure.bind("tcp://*:5557")

# Heightmap Socket
socketHeight = context.socket(zmq.REP)
socketHeight.bind("tcp://*:5558")

# Distance Socket
socketDistance = context.socket(zmq.REP)
socketDistance.bind("tcp://*:6000")

# Young Socket
socketYoung = context.socket(zmq.REP)
socketYoung.bind("tcp://*:5559")

# Counter
idx = 0

# Max/Min Values
# TODO: SET OF PARAMETERS FOR TRAINING/TESTING - Stack into x and y images
maxPressure = 5000000  # instead of np_array_pressure.max() - Do between 1000000, 5000000, 10000000 and 100000000
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

    # =============================================================

    # TODO: 1 - Retrieving data
    # Wait for next request from client
    messageVegetation = socketVegetation.recv()
    messageDistance = socketDistance.recv()
    messagePressure = socketPressure.recv()
    messageHeight = socketHeight.recv()
    messageYoung = socketYoung.recv()

    # =============================================================

    # TODO: 3 - Reconvert byte array back to 2D numpy array of floats
    float_array_vegetation = np.frombuffer(messageVegetation, dtype=np.float32)
    float_distance = np.frombuffer(messageDistance, dtype=np.float32)
    float_array_pressure = np.frombuffer(messagePressure, dtype=np.float32)
    float_array_height = np.frombuffer(messageHeight, dtype=np.float32)
    double_array_young = np.frombuffer(messageYoung, dtype=np.double)

    # =============================================================

    # Reshape 1D array to 2D numpy array - TODO: Set terrain dimensions automatically
    np_array_vegetation = np.reshape(float_array_vegetation, (257, 257))  # ax4
    np_array_pressure = np.reshape(float_array_pressure, (257, 257))  # ax1
    np_array_height = np.reshape(float_array_height, (257, 257))

    if idx == 4:
        np_array_initial_height = np.reshape(np_array_height, (257, 257))
        np_array_initial_vegetation = np.reshape(float_array_vegetation, (257, 257))  # ax3
        np_array_initial_young = np.reshape(double_array_young, (257, 257))  # ax5

    # =============================================================

    # Append distance travelled by the character
    distanceTravelled = float_distance.item()
    passes = distanceTravelled / 7
    distances.append(distanceTravelled)
    passesArray.append(passes)

    # Estimate compression and accumulation maps
    np_array_height_difference = np_array_height - np_array_initial_height


    np_array_height_compression = np.where(np_array_height_difference < 0, np_array_height_difference, 0)  # ax2
    np_array_height_accumulation = np.where(np_array_height_difference > 0, np_array_height_difference, 0)  # ax4


    # =============================================================

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

    # =============================================================

    # Create dir
    dirData = 'frames/cvs/CGI/SimulatorData-3/'  # TODO --- CHANGE! ---
    if not os.path.exists(dirData):
        os.makedirs(dirData)
    dirRGB = dirData + r'RGB/'  # TODO --- CHANGE! ---
    if not os.path.exists(dirRGB):
        os.makedirs(dirRGB)

    # =============================================================

    # Normalize the values in each array to the range [0.0, 1.0] or # TODO: between 0 and 255
    np_array_initial_vegetation_normalized = np_array_initial_vegetation * 255
    np_array_vegetation_normalized = np_array_vegetation * 255
    np_array_pressure_normalized = ((np_array_pressure - minPressure) / (maxPressure - minPressure)) * 255
    np_array_height_compression_normalized = ((np_array_height_compression - maxCompression) / (minCompression - maxCompression)) * 255
    np_array_height_accumulation_normalized = ((np_array_height_accumulation - minAccumulation) / (maxAccumulation - minAccumulation)) * 255
    np_array_initial_young_normalized = ((np_array_initial_young - minYoung) / (maxYoung - minYoung)) * 255

    # =============================================================

    # Find the non-zero elements in A
    A = np_array_initial_vegetation[np_array_initial_vegetation != 0]

    # Corresponding elements in B
    B = np_array_vegetation[np_array_initial_vegetation != 0]

    # Calculate the percentage decrement
    percentage_decrement = (B / A) * 100

    # Get the average percentage decrement
    average_percentage_remaining = np.mean(percentage_decrement).astype(float)
    avgVegetation.append(average_percentage_remaining)

    # =============================================================

    # Get number of passes
    print(f"Remaining {average_percentage_remaining}%")
    print(f"Passes: {distanceTravelled/7}")

    # =============================================================

    ax5.plot(passesArray, avgVegetation, linestyle='-', color='green', label='vegetation')
    ax5.set_xlim(1, np.max(passesArray))
    ax5.set_xticks(np.arange(0, 101, 10))  # Sets x-axis ticks at 0, 10, 20, ..., 100
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

    # =============================================================

    mapInitialVegetation = ax4.imshow(np_array_initial_vegetation_normalized, cmap="Greens", interpolation='nearest', vmin=0, vmax=255)
    mapVegetation = ax6.imshow(np_array_vegetation_normalized, cmap="Greens", interpolation='nearest', vmin=0, vmax=255)
    mapPressure = ax1.imshow(np_array_pressure_normalized, cmap='Reds', interpolation='nearest', vmin=0, vmax=255)

    mapHeightCompression = ax2.imshow(np_array_height_compression_normalized, cmap="Reds", interpolation='nearest', vmin=0, vmax=255)
    mapHeightAccumulation = ax8.imshow(np_array_height_accumulation_normalized, cmap="Blues", interpolation='nearest', vmin=0, vmax=255)
    mapInitialYoung = ax7.imshow(np_array_initial_young_normalized, cmap='Blues', interpolation='nearest', vmin=0, vmax=255)

    # =============================================================

    # Add text to the upper left corner of the subplot
    ax2.text(0.05, 0.95, f"Dist: {distanceTravelled:.1f}m", transform=ax2.transAxes, fontsize=14, verticalalignment='top')

    # =============================================================

    # TODO: Set min (YoungGround) and max (YoungGround + 1*YoungVegetation) values automatically
    """UNCOMMENT"""
    if idx % 20 == 0: # 20
        plt.savefig(dirData + str(idx) + ".png")  # save the figure to file

    # Print hex colors
    #cmap = cm.get_cmap('Blues', 5)  # PiYG
    #for i in range(cmap.N):
    #    rgba = cmap(i)
    #    # rgb2hex accepts rgb or rgba
    #    print('Blues' + matplotlib.colors.rgb2hex(rgba))

    # TODO: 2 - Send reply to the client
    # In the real world usage, after you finish your work, send your output here

    # =============================================================

    if idx % 20 == 0:
        input_array = np.dstack((np_array_pressure_normalized,
                             np_array_initial_vegetation_normalized,
                             np_array_initial_young_normalized))
        output_array = np.dstack((np_array_height_compression_normalized,
                              np_array_vegetation_normalized,
                              np_array_height_accumulation_normalized))

        input_image = Image.fromarray(np.uint8(input_array))
        output_image = Image.fromarray(np.uint8(output_array))

        input_image.save(dirRGB + str(idx) + "-input.png")
        output_image.save(dirRGB + str(idx) + "-output.png")

        input_image_pressure = Image.fromarray(np.uint8(np_array_pressure_normalized))
        input_image_vegetation = Image.fromarray(np.uint8(np_array_initial_vegetation_normalized))
        input_image_young = Image.fromarray(np.uint8(np_array_initial_young_normalized))
        input_image_pressure.save(dirRGB + str(idx) + "-input-pressure.png")
        input_image_vegetation.save(dirRGB + str(idx) + "-input-vegetation.png")
        input_image_young.save(dirRGB + str(idx) + "-input-young.png")

        output_image_compression = Image.fromarray(np.uint8(np_array_height_compression_normalized))
        output_image_vegetation = Image.fromarray(np.uint8(np_array_vegetation_normalized))
        output_image_accumulation = Image.fromarray(np.uint8(np_array_height_accumulation_normalized))
        output_image_compression.save(dirRGB + str(idx) + "-output-compression.png")
        output_image_vegetation.save(dirRGB + str(idx) + "-output-vegetation.png")
        output_image_accumulation.save(dirRGB + str(idx) + "-output-accumulation.png")

    # =============================================================

    # Vegetation
    #socketVegetation.send_string(" -> Received Vegetation!")
    np_array_vegetation_normalized_1d = np_array_vegetation_normalized.ravel()
    np_array_vegetation_normalized_1d_bytes = np_array_vegetation_normalized_1d.tobytes()
    socketVegetation.send(np_array_vegetation_normalized_1d_bytes)

    # Heightmap
    #socketHeight.send_string(" -> Received Height!")
    np_array_height_difference_1d = np_array_height_difference.ravel()
    np_array_height_difference_1d_bytes = np_array_height_difference_1d.tobytes()
    socketHeight.send(np_array_height_difference_1d_bytes)

    # Pressure
    #socketPressure.send_string(" -> Received Pressure!")
    np_array_pressure_1d = np_array_pressure.ravel()
    np_array_pressure_1d_bytes = np_array_pressure_1d.tobytes()
    socketPressure.send(np_array_pressure_1d_bytes)

    # Youngs
    #socketYoung.send_string(" -> Received Youngs!")
    np_array_initial_young_1d = np_array_initial_young.ravel()
    np_array_initial_young_1d_bytes = np_array_initial_young_1d.tobytes()
    socketYoung.send(np_array_initial_young_1d_bytes)

    # Distance
    #socketDistance.send_string(" -> Received Distance!")
    float_distance_bytes = float_distance.tobytes()
    socketDistance.send(float_distance_bytes)

    time.sleep(1)