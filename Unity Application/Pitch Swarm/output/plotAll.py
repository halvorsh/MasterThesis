import matplotlib.pyplot as plt
from tqdm import tqdm

x_axis = [i for i in range(36, 85)]


for i in tqdm(range(1000)):
    plt.clf()
    y_axis = []
    points = []
    with open(str(i)+".txt") as file:
        for line in file:
            split = line.split(" ")
            y_axis.append(float(split[0].replace(',','.')))
            points.append(int(split[1]))


    plt.plot(points, y_axis)
    plt.xlim(36,85)
    plt.savefig("plotsToBeCombined/"+str(i)+".png")
