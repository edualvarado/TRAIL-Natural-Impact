import matplotlib.pyplot as plt
import numpy as np
import matplotlib.patches as mpatches

# Real Data
x = [25, 75, 200, 500] # Data points for the bold line
y1 = [86.3, 70.4, 39.8, 20] # Data points for the bold line
y2 = [96.2, 83.8, 60.2, 45.5] # Data points for the dashed line

# Simulated Data

# 2 weeks
yhat1A = [87, 65, 37, 15] # Data points for the bold line

# 1 year
yhat2A = [94, 87, 67, 37] # Data points for the bold line
yhat2B = [93, 80, 60, 26] # Data points for the bold line

# Average of yhat2A and yhat2B
yhat2_avg = np.mean([yhat2A, yhat2B], axis=0)

# Plot the data points and connect them with lines
plt.plot(x, y1, 'o-', color='black', label='[Col95a] (2 weeks)')
plt.plot(x, y2, 'o--', color='black', label='[Col95a] (1 year)')

# Plot the data points and connect them with lines
plt.plot(x, yhat1A, 'o-', color='green', label='Simulation (2 weeks)')

plt.plot(x, yhat2A, '-', color='green')
plt.plot(x, yhat2B, '-', color='green')
plt.plot(x, yhat2_avg , 'o--', color='green', label='Average Simulation (1 year)')

plt.fill_between(x, yhat2A, yhat2B, color='green', alpha=0.1)

# Set x and y axis limits
plt.xlim(0, 525)
plt.ylim(0, 100)

# Set x and y axis ticks
plt.xticks([25, 75, 200, 500])
plt.yticks([20, 40, 60, 80, 100])

plt.xlabel('Number of passes')
plt.ylabel('Relative cover after trampling (%)')

# Add grid lines at x-axis ticks
plt.grid(axis='x')
plt.grid(axis='y')

# Add legends
plt.legend(loc='upper right')

plt.savefig('figure1.pdf')
