from PIL import Image
import os
import matplotlib.pyplot as plt
import numpy as np

# Open the image file
image_path_input = '../../../../../04_ML/01-Cloned/pytorch-CycleGAN-and-pix2pix/results/summer_pix2pix_TrainData-12_shuf/channels - 149 - scale - x10/149-input_real.png'
image_path_output = '../../../../../04_ML/01-Cloned/pytorch-CycleGAN-and-pix2pix/results/summer_pix2pix_TrainData-12_shuf/channels - 149 - scale - x10/149-input_fake.png'

# Open the image file
input_image = Image.open(image_path_input)
output_image = Image.open(image_path_output)

# Split the image into its three channels
pressure, init_veg, init_young = input_image.split()
compression, veg, accumulation = output_image.split()

# Get the directory and base name of the image file
directory_input, filename_input = os.path.split(image_path_input)
base_input, ext_input = os.path.splitext(filename_input)
directory_output, filename_output = os.path.split(image_path_output)
base_output, ext_output = os.path.splitext(filename_output)

# Save each channel as a separate image
pressure.save(os.path.join(directory_input, f'{base_input}_r{ext_input}'))
init_veg.save(os.path.join(directory_input, f'{base_input}_g{ext_input}'))
init_young.save(os.path.join(directory_input, f'{base_input}_b{ext_input}'))

compression.save(os.path.join(directory_output, f'{base_output}_r{ext_output}'))
veg.save(os.path.join(directory_output, f'{base_output}_g{ext_output}'))
accumulation.save(os.path.join(directory_output, f'{base_output}_b{ext_output}'))


fig, axes = plt.subplots(nrows=3, ncols=2, figsize=(10, 10))
ax1, ax2, ax3, ax4, ax5, ax6 = axes.flatten()

# Super title
fig.suptitle("Simulation Maps", fontsize=14, fontweight='bold')

# Single titles
ax1.set_title('Pressure')
ax2.set_title('Compression')

ax3.set_title('Initial Vegetation')
ax4.set_title('Vegetation')

ax5.set_title('Initial Young Modulus')
ax6.set_title('Vertical Accumulation')

# TODO: 6 - Update plots
mapPressure = ax1.imshow(np.array(pressure), cmap='Reds', interpolation='nearest', vmin=0, vmax=255)
mapHeightCompression = ax2.imshow(np.array(compression), cmap="Reds", interpolation='nearest', vmin=0,
                                  vmax=255)

mapInitialVegetation = ax3.imshow(np.array(init_veg), cmap="Greens", interpolation='nearest',
                                  vmin=0, vmax=255)
mapVegetation = ax4.imshow(np.array(veg), cmap="Greens", interpolation='nearest', vmin=0, vmax=255)

mapInitialYoung = ax5.imshow(np.array(init_young), cmap='Blues', interpolation='nearest', vmin=0, vmax=255)
mapHeightAccumulation = ax6.imshow(np.array(accumulation), cmap="Blues", interpolation='nearest',
                                   vmin=0, vmax=255)

plt.savefig(directory_input + "/fake-output-templates.png")  # save the figure to file
