import os
import shutil

src_folders = ['frames/TrainData-13/TrainingData-1-Autumn-v3', 'frames/TrainData-13/TrainingData-2-Autumn-v3', 'frames/TrainData-13/TrainingData-3-Autumn-v3', 'frames/TrainData-13/TrainingData-4-Autumn-v3']
# src_folders = ['frames/TrainData-v2-v3-pix2pix/Test/Data-v4-test']

a_folder = 'frames/TrainData-13/A-raw'
b_folder = 'frames/TrainData-13/B-raw'

if not os.path.exists(a_folder):
    os.makedirs(a_folder)
if not os.path.exists(b_folder):
    os.makedirs(b_folder)

next_index = 1

for src_folder in src_folders:
    file_names = sorted(os.listdir(src_folder), key=lambda x: int(x.split('-')[0]))
    for file_name in file_names:
        src_file_path = os.path.join(src_folder, file_name)
        if file_name.endswith('-input.png'):
            while os.path.exists(os.path.join(a_folder, f'{next_index}.png')):
                next_index += 1
            dst_file_path = os.path.join(a_folder, f'{next_index}.png')
            shutil.copy(src_file_path, dst_file_path)
        elif file_name.endswith('-output.png'):
            while os.path.exists(os.path.join(b_folder, f'{next_index}.png')):
                next_index += 1
            dst_file_path = os.path.join(b_folder, f'{next_index}.png')
            shutil.copy(src_file_path, dst_file_path)
            next_index += 1