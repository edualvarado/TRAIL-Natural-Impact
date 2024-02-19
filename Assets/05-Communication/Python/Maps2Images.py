from PIL import Image
import numpy as np

# Assuming you have three 2D numpy arrays of the same shape: red_channel, green_channel, blue_channel
red_channel = np.random.rand(200,200)
green_channel = np.random.rand(200,200)
blue_channel = np.random.rand(200,200)

# Stack the channels along the third axis to create an RGB image
image_array = np.dstack((red_channel, green_channel, blue_channel))

# Convert the numpy array to a PIL Image object
image = Image.fromarray(np.uint8(image_array * 255))

# Save the image to a file
image.save('my_image.png')
