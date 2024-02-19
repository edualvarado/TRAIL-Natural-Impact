import os
import shutil
from random import shuffle

a_raw_folder = 'frames/TrainData-13/A-raw'
b_raw_folder = 'frames/TrainData-13/B-raw'
a_folder = 'frames/TrainData-13/A-shuf'
b_folder = 'frames/TrainData-13/B-shuf'

def move_files(src_folder: str, dest_folder: str, file_order: list):
    # Create destination subfolders if they don't exist
    train_folder = os.path.join(dest_folder, 'train')
    val_folder = os.path.join(dest_folder, 'val')
    test_folder = os.path.join(dest_folder, 'test')
    for folder in [train_folder, val_folder, test_folder]:
        if not os.path.exists(folder):
            os.makedirs(folder)

    # Calculate number of files for each split
    num_files = len(file_order)
    num_train = int(num_files * 0.8)
    num_val = int(num_files * 0.1)

    # Move files to destination subfolders according to split distribution and file order
    for i, file in enumerate(file_order):
        src_file_path = os.path.join(src_folder, file)
        if i < num_train:
            dest_file_path = os.path.join(train_folder, file)
        elif i < num_train + num_val:
            dest_file_path = os.path.join(val_folder, file)
        else:
            dest_file_path = os.path.join(test_folder, file)
        shutil.move(src_file_path, dest_file_path)

# Get list of files in A-raw and shuffle them to create a random order
file_order = os.listdir(a_raw_folder)
shuffle(file_order)

# Move files from A-raw to A using the shuffled order
move_files(a_raw_folder, a_folder, file_order)

# Move files from B-raw to B using the same shuffled order as for A-raw
move_files(b_raw_folder, b_folder, file_order)