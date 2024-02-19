#
#   Server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import random
import time
import zmq

context = zmq.Context()

# Vegetation Socket
socketHello = context.socket(zmq.REP)
socketHello.bind("tcp://*:5555")

# Counter
idx = 0

while True:
    idx += 1

    # TODO: 1 - Retrieving data
    # Wait for next request from client
    messageHello = socketHello.recv()
    print(f"Received request: {messageHello}")

    # TODO: 8 - Send reply to the client
    # In the real world usage, after you finish your work, send your output here
    socketHello.send_string("Received!")
    time.sleep(1)