import matplotlib.pyplot as plt;

x_axis = [i for i in range(36, 85)]
y_axis = []
points = []

with open("OneRun.txt") as file:
    for line in file:
        split = line.split(" ")
        y_axis.append(float(split[0].replace(',','.')))
        points.append(int(split[1]))
        print(y_axis[-1], points[-1])


plt.plot(points, y_axis)
plt.xticks(x_axis)
plt.show()
