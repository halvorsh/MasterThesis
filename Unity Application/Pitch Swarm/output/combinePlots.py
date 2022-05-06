import numpy as np
import cv2
from tqdm import tqdm

output = cv2.imread("plotsToBeCombined/0.png", cv2.IMREAD_COLOR).astype('float64')

output *= 0.001

for i in tqdm(range(1,1000)):
    output += cv2.imread(f"plotsToBeCombined/{i}.png", cv2.IMREAD_COLOR).astype('float64')*0.001

cv2.imwrite("combinedplot.png", output)
