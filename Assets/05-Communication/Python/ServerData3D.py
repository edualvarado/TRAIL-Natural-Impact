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
from scipy.ndimage import map_coordinates

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

# Second, set up the figure, the axis, and the plot element we want to animate
fig = plt.figure(figsize=(10, 5))
ax1 = fig.add_subplot(121)  # 121 means 1 row, 2 columns, and the first plot
ax2 = fig.add_subplot(122, projection='3d')  # 122 means 1 row, 2 columns, and the second plot

# Super title
fig.suptitle("3D Simulation Maps", fontsize=14, fontweight='bold')

# Single titles
ax1.set_title('Path Profile')
ax2.set_title('3D Compression')

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
    print("idx: ", idx)

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
    distances.append(distanceTravelled)

    # Estimate compression and accumulation maps
    np_array_height_difference = np_array_height - np_array_initial_height
    np_array_height_compression = np.where(np_array_height_difference < 0, np_array_height_difference, 0)  # ax2
    np_array_height_accumulation = np.where(np_array_height_difference > 0, np_array_height_difference, 0)  # ax4

    # TODO: Sum compression values in X and normalize
    np_array_height_compression_flatten_width = np_array_height_compression.sum(axis=1)
    np_array_height_compression_flatten_width_sum = np.sum(np_array_height_compression_flatten_width)
    if np_array_height_compression_flatten_width_sum != 0:
        np_array_height_compression_flatten_width_norm = np_array_height_compression_flatten_width \
                                                         / np_array_height_compression_flatten_width_sum

    # TODO: Sum compression values in Y and normalize
    np_array_height_compression_flatten_comp = np_array_height_compression.sum(axis=0)
    np_array_height_compression_flatten_comp_sum = np.sum(np_array_height_compression_flatten_comp)
    if np_array_height_compression_flatten_comp_sum != 0:
        np_array_height_compression_flatten_comp_norm = np_array_height_compression_flatten_comp \
                                                         / np_array_height_compression_flatten_comp_sum

    # Create plots
    # Second, set up the figure, the axis, and the plot element we want to animate
    fig = plt.figure(figsize=(10, 5))
    ax1 = fig.add_subplot(121)  # 121 means 1 row, 2 columns, and the first plot
    ax2 = fig.add_subplot(122, projection='3d')  # 122 means 1 row, 2 columns, and the second plot

    # Super title
    fig.suptitle("Simulation Maps", fontsize=14, fontweight='bold')

    # Single titles
    ax1.set_title('Pressure')
    ax2.set_title('Compression')

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

    # Path Profile

    # Calculate the indices for the cross-section
    n = len(np_array_height_compression)

    # Define the center of the array
    center = n // 2, n // 2

    # Define function to extract values along a line
    def extract_line(angle, radius):
        # Calculate displacement from center for each point along line
        t = np.linspace(0, radius, radius)
        x_disp = t * np.cos(angle)
        y_disp = t * np.sin(angle)

        # Calculate indices of points along line
        x_indices = center[0] + x_disp
        y_indices = center[1] + y_disp

        # Create a set of coordinates for each point along the line
        coordinates = np.array([x_indices.ravel(), y_indices.ravel()])

        # Use map_coordinates to interpolate values at these indices
        line_values = map_coordinates(np_array_height_compression, coordinates)

        return line_values

    # Calculate angle in radians
    angle = 90 * np.pi / 180

    # Define radius (distance from center to edge of array)
    radius = min(n, n) // 2

    # Extract data for this angle
    data = extract_line(angle, radius)

    line_values_sum = np.sum(data)
    if line_values_sum != 0:
        data_norm = data / line_values_sum

    # Set the limits for the y-axis from 0 to 0.1
    ax1.set_ylim([-0.1, 0.1])
    ax1.axhline(0, color='gray', linestyle='-', linewidth=1)

    # Set the limits for the x-axis from -3 to 3 meters
    #ax1.set_xlim([0, ])

    ax1.bar(range(len(data_norm)), data_norm)

    ax1.set_title(f'Angle {90}°')

    # ============================================== #

    # 3D

    # ============================================== #

    # TODO: Set min (YoungGround) and max (YoungGround + 1*YoungVegetation) values automatically
    plt.savefig(dirData + str(idx) + ".png")  # save the figure to file

    # ---------------------------------------------------------------------------------------------

    # Stack the arrays into a single array
    input_array = np.dstack((np_array_pressure_normalized,
                             np_array_initial_vegetation_normalized,
                             np_array_initial_young_normalized))

    output_array = np.dstack((np_array_height_compression_normalized,
                              np_array_vegetation_normalized,
                              np_array_height_accumulation_normalized))

    # Save the images
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
    input_image = Image.fromarray(np.uint8(input_array))

    # output_image = Image.fromarray(np.uint8(output_array * 255))
    output_image = Image.fromarray(np.uint8(output_array))

    input_image.save(dirRGB + str(idx) + "-input.png")
    output_image.save(dirRGB + str(idx) + "-output.png")

    # Fourth, animate
    fig.tight_layout()
    plt.show()

    # Pause
    # Try reducing sleep time to 0.01 to see how blazingly fast it communicates
    # In the real world usage, you just need to replace time.sleep() with
    # whatever work you want python to do, maybe a machine learning task?
    time.sleep(1)