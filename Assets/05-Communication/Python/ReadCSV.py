import pandas as pd


dir = 'frames/Data-v1/'


# Read the CSV file into a DataFrame
df = pd.read_csv(dir + "Data-v1.csv", index_col=0)

# Print the contents of the DataFrame
print(df)