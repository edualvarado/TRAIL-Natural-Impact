#
#   Server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import time
import zmq
import numpy as np
import matplotlib.pyplot as plt

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

# Counter
idx = 0

while True:
    idx += 1

    # TODO: Include task here

    # Wait for next request from client
    message = socket.recv()

    # Convert byte array back to 2D numpy array of floats
    float_array = np.frombuffer(message, dtype=np.float32)

    # Reshape 1D array to 2D numpy array - TODO: Set terrain dimensions automatically
    np_array = np.reshape(float_array, (257, 257))

    fig, ax = plt.subplots()
    im = ax.imshow(np_array, cmap="YlGn", interpolation='nearest', vmin=0, vmax=1)

    # im = ax.imshow(np_array, cmap=plt.get_cmap('hot'), interpolation='nearest', vmin=0, vmax=1)

    fig.colorbar(im)
    plt.show()

    # Try reducing sleep time to 0.01 to see how blazingly fast it communicates
    # In the real world usage, you just need to replace time.sleep() with
    # whatever work you want python to do, maybe a machine learning task?
    time.sleep(1)

    #  Send reply to client
    #  In the real world usage, after you finish your work, send your output here
    socket.send(b"Vegetation Map received!")

