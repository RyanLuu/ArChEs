import json
import numpy as np
import os
from pathlib import Path

def prune(dir):
  total = 0
  hit = 0
  for entry in os.scandir(dir):
    if entry.path.endswith('.json') and not entry.is_dir():
      flag = False
      total += 1
      with open(entry) as f:
        task = json.load(f)
        for task in task['train'] + task['test']:
          input_shape = np.shape(task['input'])
          output_shape = np.shape(task['output'])
          if input_shape[0] > 10 or input_shape[1] > 10 or output_shape[0] > 10 or output_shape[0] > 10:
            flag = True
            hit += 1
            break
      if flag:
        path = Path(entry.path)
        new_path = path.parent / 'large' / path.name
        path.rename(new_path)
  print('Pruned %d / %d tasks in %s.' % (hit, total, dir))
            
prune('../data/training')
prune('../data/evaluation')

