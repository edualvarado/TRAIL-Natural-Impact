import os

directory = 'frames/TrainData-12/TestData-1-Summer-v3-MoreIntense-LAST'  # Replace with the path to your directory

for filename in os.listdir(directory):
    if filename.endswith('-output.png') or filename.endswith('output.png.meta'):
        file_path = os.path.join(directory, filename)
        os.remove(file_path)