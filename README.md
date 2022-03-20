# Unity based synthetic data generator
Synthetic data generator, based on Unity engine. Used for generating large amounts of automaticly labeled images, which can be exploited for neural networks training.

## Prerequisites
 - Unity Engine, project was developed on 2019.4.23f1 version so this one is recommended but it should works also on newer ones.
 
## How to use
Import project and open scene [Shapes](Assets/Scenes/Shapes.unity)

There are a few objects on the scene, which define image generating scenario:

In MainCamera > Perception Camera script you can select if generated data should be saved or visualized.
![obraz](https://user-images.githubusercontent.com/56251265/159175511-6afa1090-e26c-40b6-aa70-24fbbc23af16.png)


In Simulation Scenario > Fixed Length Scenario script you can define:
 - Number of images which will be generated (in Scenario properties)
 - Objects which will be spawned (in Randomizers), you could drag and drop selected prefab from [Objects](Assets/Samples/Shapes/Objects) directory to the script.
 - Different background's textures 
 - Parameters for objects' color randomizer and spawning probability.
 - Camera's limits for labeling 
 
![obraz](https://user-images.githubusercontent.com/56251265/159176247-faebfc9f-3e03-4b37-835f-f22850abebfc.png)

 
Labelling config can be changed in [ShapesIdLableConfig](Assets/Perception/IdLableConfig/ShapesIdLabelConfig.asset), only objects which are defined there, will be labeled!!!

When the configuration is ready, just press play button to generate data. Results will be saved in output path printed in MainCamera > Perception Camera script, labels regarding all generated images are saved in Dataset.../capture_X.json file.

After data generation, labels and images must be divided into sets and converted to form required by certain neural network model. For [YOLOv5](https://github.com/ultralytics/yolov5), you can use [script](https://github.com/High-Flyers/vision-analysis/blob/main/get_perception_images.py) from vision-analysis repo.


