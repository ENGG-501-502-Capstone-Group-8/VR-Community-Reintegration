# VR Community Reintegration

A room-scale virtual reality prototype for the University of Calgary’s ENGG 501/502 capstone, Group 8. The team is developing the application to support rehabilitation and evaluation for people with an acquired neurological injury. Everyday tasks such as finding a destination or crossing a street give the participant a reason to move through the environment and give the team observable events to review.

## Explore the project

| Scene | What to try |
| --- | --- |
| [Main Hub](Files/Assets/Scenes/Main%20Hub/Main%20Hub.unity) | Start at the crosswalk and look toward the gazebo. Explore the bus shelter, market kiosks, and pond. In the Hierarchy, select **Civic Park** and press **F** over the Scene view for an overview. |
| [Metrics sandbox](Files/Assets/Scenes/Sandbox/Sandbox_Tri.unity) | Test a short route using the coffee shop destination, wrong-turn zone, and finish zone. Open the Console to follow the session events as they happen. |

## Run it in the Unity Editor

1. Install Git LFS and the project’s Unity Editor version, **6000.6.7f1**, through Unity Hub. The [version file](Files/ProjectSettings/ProjectVersion.txt) records the exact editor revision. Install Blender if you need Unity to import the source road model.
2. Clone the repository and download its large assets.

   ```sh
   git lfs install
   git clone https://github.com/ENGG-501-502-Capstone-Group-8/VR-Community-Reintegration.git
   cd VR-Community-Reintegration
   git lfs pull
   ```

3. In Unity Hub, choose **Add project from disk** and select the **`Files`** folder inside the clone. It contains `Assets`, `Packages`, and `ProjectSettings`. Let the first import finish.
4. In Unity’s Project window, open **Assets → Scenes → Main Hub → Main Hub**. Press **Play** and click the **Game** view to give the XR Interaction Simulator keyboard and mouse input. The simulator lets you explore the scene before testing room-scale movement on the target Meta Quest headset.

## Check a destination event

Open **Sandbox_Tri**, enter Play Mode, and open **Window → General → Console**. Move the simulated headset from outside **Zone_Destination** into the box. During a running session, the Console should log **Success: Reached Coffee shop**. Enter **Zone_Finish** after reaching the destination to finish the route.

To inspect the boundary, select **Zone_Destination** in the Hierarchy and enable **Gizmos** in the Scene view. The [zone script](Files/Assets/Scripts/Metrics/MetricZone.cs) checks the headset position against the box, so moving a controller into the zone by itself won’t count as arrival.

## Working on the project

- Keep environment and scene changes under [Assets/Scenes](Files/Assets/Scenes). Session tracking, zone logic, and the results board live in [Scripts/Metrics](Files/Assets/Scripts/Metrics).
- Commit each new or moved Unity asset with its `.meta` file so teammates retain the same scene and prefab references.
- Blender source files use Git LFS. If the road model is missing after a pull, run `git lfs pull` from the repository root and check Unity’s Console for import errors.

## License

See the [MIT license](LICENSE).
