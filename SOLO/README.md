# Outback Air - Tourist AR Experience

This repo contains the implementation for the Tourist Augmented Reality (AR) Experience developed for Outback Air (as part of SIT758).

## Functionality

### Overview

The intent of this experience is to improve the experience for participants in Outback Air's tourist flights. This experience addresses the common complaint for tour activities where individauls are unable to orient themselves to the tour guide descriptions and rapidly identify the points of interest (POI) within the tour; both as outlined by the tourguide and generally.

The functionality provided by this experience is:
- Augmented reality POI markers aligned with real world POIs
- Gaze detection to hide POI markers when a user looks at them and activate relevant information presentation
- A VR overview allowing a user to view a rendering of the terrain they are currently over with POIs marked to aid in orientation

### Current status
All major components have been developed. The most mature example of the integrated capabilities is the [Real World Cesium component test scene](https://github.com/Deakin-SIT383-758/SIT383-758-Project-2026/tree/MickM-dev/SOLO/Assets/RealWorldCesium/Scenes).



### Bugs/Issues

In no particular order, some of the key issues are:
- (UX) The information panel for POI info is still a placeholder and needs polish. The text is also hard to read.
- (UX) POI markers can be hard to activate
- (Feature refinement) The VR overview is too high currently
- (Feature refinement) Currently the AR view has the VR terrain as an indicator. This is not the final functionality.

A full list of issues can be found on the [Project Kanban](https://miro.com/app/board/uXjVG4V7RDI=/?focusWidget=3458764661957056680) under team "Lemon Fizzy Drink", specifically cards with the "Issue:" prefix in the To Do Backlog, In Progress and On Hold/Blocked columns.

## Development
The development of this project is based on the main development branch: [MickM-dev](https://github.com/Deakin-SIT383-758/SIT383-758-Project-2026/tree/MickM-dev)

Individual features are developed in sub-branches with the naming convetion _MickM-dev-branchname_, where _branchname_ is the feature being developed or a descriptive name based on the intent of the branch.

Each development branch will maintain a separate folder structure with all related assets contained, including any test scenes. Core components are to be included as prefabs in the root feature folder to allow other features and the main branch to use them.

Feature branches will remain open to support subsequent development/refinement. If they are dormant, they should be rebased to the latest _MickM-dev_ branch before commencing work.

## Usage

### Basics
Clone the repo and open in Unity; development version is 6000.3.11f1.

You may need to manually swap the build to android if building to a headset. 

### Cesium
This project uses Cesium for terrain rendering. API keys are not included. To use this correctly you will need to do the following:
- [Create a Cesium Ion account](https://ion.cesium.com/signup?). This project does not require a paid account, the basic free account is all you need.
- [connect to Cesium Ion (step 2 only)](https://cesium.com/learn/unity/unity-quickstart/#step-2-connect-to-cesium-ion). This generates a key within your account to use in the Unity Project. (_The .gitignore file will ignore the asset created with the key in it_)
