import os

directory = 'frames/SimulatorData-8/TestData-4-Summer-v3-onlyOutput'  # Replace with the path to your directory

for filename in os.listdir(directory):
    if filename.endswith('-input.png') or filename.endswith('input.png.meta') or filename.endswith('.meta'):
        file_path = os.path.join(directory, filename)
        os.remove(file_path)