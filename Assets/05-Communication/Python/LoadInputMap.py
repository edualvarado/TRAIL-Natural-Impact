import csv
import numpy as np
from PIL import Image
import os

def load_csv_to_2d_array(file_path):
    with open(file_path, 'r') as file:
        reader = csv.reader(file)
        data = [row for row in reader]
    return data


dir1 = 'frames/SimulatorData-3/TestOnly-1/Pressure/pressure.csv'
dir2 = 'frames/SimulatorData-3/TestOnly-1/InitialConditions/initialVegetation.csv'
dir3 = 'frames/SimulatorData-3/TestOnly-1/InitialConditions/initialYoungNormalized.csv'

np_array_pressure_normalized = load_csv_to_2d_array(dir1)
np_array_initial_vegetation = load_csv_to_2d_array(dir2)
np_array_initial_young_normalized = load_csv_to_2d_array(dir3)

np_array_pressure_normalized_1 = [[float(cell) for cell in row] for row in np_array_pressure_normalized]
np_array_initial_vegetation_1 = [[float(cell) for cell in row] for row in np_array_initial_vegetation]
np_array_initial_young_normalized_1 = [[float(cell) for cell in row] for row in np_array_initial_young_normalized]


# Stack the arrays into a single array
input_array = np.dstack((np_array_pressure_normalized_1,
                         np_array_initial_vegetation_1,
                         np_array_initial_young_normalized_1))

print(input_array.dtype)

# Convert the numpy array to a PIL Image object
input_image = Image.fromarray(np.uint8(input_array * 255))

# Save the images
newpath = r'frames/SimulatorData-3/TestOnly-1/RGB/'  # TODO --- CHANGE! ---
if not os.path.exists(newpath):
    os.makedirs(newpath)

input_image.save(newpath + '/1-input.png')