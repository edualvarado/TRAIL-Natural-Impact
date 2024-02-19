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

# Counter
idx = 0

while True:
    idx += 1

    # TODO: 1 - Retrieving data
    # Wait for next request from client
    messagePressure = socketPressure.recv()
    messageHeight = socketHeight.recv()
    messageYoung = socketYoung.recv()

    messageVegetation = socketVegetation.recv()

    # TODO: 8 - Send reply to the client
    # In the real world usage, after you finish your work, send your output here
    socketPressure.send(b"Pressure Map received!")
    socketHeight.send(b"Height Map received!")
    socketYoung.send(b"Young Map received!")

    socketVegetation.send(b"Vegetation Map received!")

    time.sleep(1)