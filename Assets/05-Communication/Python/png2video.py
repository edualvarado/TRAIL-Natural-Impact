import cv2
import os

# Directory containing input images
input_dir = "frames/SimulatorData-8/TestData-1-Summer-v3"
#input_dir = "frames/TrainData-12/dataset/test_AB_shuf/ForVideo"

# Get list of image files in directory
img_files = [f for f in os.listdir(input_dir) if f.endswith('.png')]

# Sort files in natural order (1.png, 2.png, 3.png, etc.)
img_files.sort(key=lambda x: int(os.path.splitext(x)[0]))

# Get image dimensions
img = cv2.imread(os.path.join(input_dir, img_files[0]))
height, width, layers = img.shape

# Create VideoWriter object
fourcc = cv2.VideoWriter_fourcc(*'mp4v')

dir = input_dir + '/Video/'
if not os.path.exists(dir):
    os.makedirs(dir)

video = cv2.VideoWriter(dir + 'output.mp4', fourcc, 1, (width, height))

# Loop over input images and write to video
for img_file in img_files:
    img_path = os.path.join(input_dir, img_file)
    img = cv2.imread(img_path)
    video.write(img)

# Release resources
video.release()
cv2.destroyAllWindows()
