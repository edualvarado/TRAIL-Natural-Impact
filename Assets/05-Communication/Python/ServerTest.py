import random
import time
import zmq
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import os

# Set up ZeroMQ context and sockets
context = zmq.Context()

socketHeight = context.socket(zmq.REP)
socketHeight.bind("tcp://*:5556")

socketVegetation = context.socket(zmq.REP)
socketVegetation.bind("tcp://*:5555")

# Counter
idx = 0

# Set up the figure and axes for the animation
fig, (ax1, ax2) = plt.subplots(1, 2)
fig.suptitle("Maps")
ax1.set_title('Heightmap')
ax2.set_title('Vegetation - Living Ratio')

# Generate initial data for the animation
np_array_height = np.random.random((257, 257))
np_array_vegetation = np.random.random((257, 257))
mapTerrain = ax1.imshow(np_array_height, cmap="coolwarm", interpolation='nearest', vmin=0.9, vmax=1.1)
mapVegetation = ax2.imshow(np_array_vegetation, cmap="YlGn", interpolation='nearest', vmin=0, vmax=1)

# Define a generator function to produce frames for the animation
def generate_frames():
    while True:
        # Retrieve data from ZeroMQ sockets
        messageVegetation = socketVegetation.recv()
        messageHeight = socketHeight.recv()

        # Convert byte array back to 2D numpy array of floats
        float_array_vegetation = np.frombuffer(messageVegetation, dtype=np.float32)
        float_array_height = np.frombuffer(messageHeight, dtype=np.float32)

        # Reshape 1D array to 2D numpy array
        np_array_height = np.reshape(float_array_height, (257, 257))
        np_array_vegetation = np.reshape(float_array_vegetation, (257, 257))

        # Update the plot elements with the new data
        mapTerrain.set_array(np_array_height)
        mapVegetation.set_array(np_array_vegetation)

        # Send acknowledgement back to clients
        socketHeight.send(b"Height Map received!")
        socketVegetation.send(b"Vegetation Map received!")

        # Yield the updated plot as a new frame for the animation
        yield [mapTerrain, mapVegetation]

# Define the update function for the animation
def update_plot(frame):
    for artist in frame:
        artist.changed()
    return frame

# Create the animation
ani = animation.FuncAnimation(fig, update_plot, frames=generate_frames(), blit=True)

# Save the animation as a video
Writer = animation.writers['ffmpeg']
writer = Writer(fps=15, metadata=dict(artist='Me'), bitrate=1800)
ani.save('animation.mp4', writer=writer)

# Show the animation (this is optional)
plt.show()
