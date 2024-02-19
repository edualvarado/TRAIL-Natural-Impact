from PIL import Image
import os

# Load the image
image = Image.open('frames/TrainData-12/TestData-1-Summer-v3-MoreIntense-LAST/148-input.png')

# Convert the image to RGB mode
#image = image.convert('RGB')

# Split the image into its individual channels
r, g, b = image.split()

# Scale up the red channel by 10 times
r = r.point(lambda i: i * 5)

# Merge the channels back together
image = Image.merge('RGB', (r, g, b))

# Save the image with a custom name in the same directory
output_path = os.path.join(os.path.dirname('frames/TrainData-12/TestData-1-Summer-v3-MoreIntense-LAST/148-input.png'), 'output_image.png')
image.save(output_path)