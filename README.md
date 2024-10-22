**Documentation:**

__class Grid:__
The Grid class contains various varables to manage the 3D grid (the area where pipes can be created). It also contains basic function to manage cells inside it.

class Grid Controller:
This class manages how and where the pipes are drawn. It works by choosing a random spawn point. From that point on it looks which filds around that point aren't occupied yet. From those positions is one randomly selected.
This goes on until the path creation runs into a dead end (either it runs out of bounds or no more fields around the current are avalible). After a path was created the pipes are drawn onto it. Starting at the first point
in the path a cylinder is instantiated and scalled over time. This happens for the rest of the points in the path after one and another. If a curve in the path is detected a simple sphere is spawned. After a path is fully
drawn, a new path must be created and drawn using the process from before. To avoid overcrowding of pipes, after a custom set amount of placed pathes all pathes are deleted and the process beginns from the beginning.

__What can be done to optimize the experiance:__
1. Currently it looks like every path is centered at one point and doesnt really stretch over the entierty of the room. To combat this we could give the path generation a higher chance to generate its next tile to the front
   or we chage the width of the grid size.
2. Some pathes are very short. This is because through for example a bad spawn the path creation runs into a dead end. This is espacially noticable at the pathes created later on due to less space. The resolve this problem
   we can tell the path to not choose a next position which is out of bounds for the first n steps.
3. sometimes the space is very crowded due to long pathes. This could be fixed by including a variable which counts the amount of placed pipes and based on that resets the generation.
4. Setting the amound of maxPathes to high can resoult in problems, because all tiles are already used. could be fixed similar to 3.
