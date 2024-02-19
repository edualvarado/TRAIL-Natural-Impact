from PIL import Image
import os

input_dir = 'frames/TrainData-12/TestData-1-Summer-v5'
x_dir = 'frames/TrainData-12/TestData-1-Summer-v5-MoreVegetation/X'
output_dir = 'frames/TrainData-12/TestData-1-Summer-v5-MoreVegetation'

# Load the X image
x_image = Image.open(os.path.join(x_dir, '4-input.png'))
x_data = x_image.split()

# Iterate over the input images
for filename in os.listdir(input_dir):
    if filename.endswith('input.png'):
        # Load the input image
        input_image = Image.open(os.path.join(input_dir, filename))
        input_data = input_image.split()

        # Replace the green and blue channels with those from X
        new_image = Image.merge('RGB', (input_data[0], x_data[1], x_data[2]))

        # Save the resulting image
        new_image.save(os.path.join(output_dir, filename))