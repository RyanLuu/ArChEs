import matplotlib.pyplot as plt
from matplotlib import colors
import json
from pathlib import Path
import sys
import numpy as np

def prep_ax(ax, img, title):
    cmap = colors.ListedColormap(
        ['#000000', '#0074D9','#FF4136','#2ECC40','#FFDC00',
         '#AAAAAA', '#F012BE', '#FF851B', '#7FDBFF', '#870C25'])
    norm = colors.Normalize(vmin=0, vmax=9)
    shape = np.shape(img)
    width = shape[1]
    height = shape[0]
    ax.imshow(img, extent=(0,width,0,height), cmap=cmap, norm=norm)
    ax.set_title(title)
    ax.set_xticks(np.arange(0, width))
    ax.set_yticks(np.arange(0, height))
    ax.set_xticklabels([])
    ax.set_yticklabels([])
    ax.tick_params(length=0)
    ax.grid(True)

def plot_task(task):
    fig, axs = plt.subplots(1, 4, figsize=(15,15))
    prep_ax(axs[0], task['train'][0]['input'], 'Train Input')
    prep_ax(axs[1], task['train'][0]['output'], 'Train Output')
    prep_ax(axs[2], task['test'][0]['input'], 'Test Input')
    prep_ax(axs[3], task['test'][0]['output'], 'Test Output')
    plt.tight_layout()
    plt.show()

filename = sys.argv[1]

with open(filename, 'r') as f:
    task = json.load(f)
    plot_task(task)

