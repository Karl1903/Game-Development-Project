# Orpheus

Games-Praktikum/Adv. Game Dev. project of SS2022

This README contains some important infos. For more information or tutorials look into the [Technical Design Document (TDD)](https://ca-confluence.hdm-stuttgart.de/pages/viewpage.action?pageId=283017246) and [Technical Design Document Graphics (TDDG)](https://ca-confluence.hdm-stuttgart.de/pages/viewpage.action?pageId=286687265)

## Software

**Engine:** Unity 2021.2.16 (https://unity3d.com/de/get-unity/download/archive (Unity Hub or direct download))

**Python:**  Python 3.7/Pytorch

**Additional packages:** ML-Agents

**3D assets:** Blender and additional software for creating textures if preferred.

*Keep the original .blend file of your model to modify it subsequently if necessary. To import that model into unity, export your model as a .fbx file.*

**2D assets:** Drawing program of choice (SAI, Clip Studio, Photoshop, ...)

*Unity can handle transparent textures in psd, tga or png format. You may keep it as a psd or png.*

**Task/Bug Management:** Jira (https://ca-jira.hdm-stuttgart.de/secure/RapidBoard.jspa?rapidView=209&projectKey=MGPS22&view=planning.nodetail&issueLimit=100)

**Documentation/Wiki:** Confluence (https://ca-confluence.hdm-stuttgart.de/display/MGPS22)

## GitLab Structure
### Production Branches
#### main branch

The main branch is protected and can only be merged with the dev branch by a Head for milestones or releases.

#### dev branch

The dev branch is protected from direct pushes. It contains the current status of the Unity project with all Assets included.

In order to commit and push contributions to the dev branch, a feature branch must be created from the dev branch. These feature branches then can be reviewed via a merge request, approved and finally merged into the dev branch.

#### feature branches

Everybody can create a new branch from the dev branch in order to add new stuff. Ideally we should keep to a branch naming convention e.g. 42-implement-jumping ([task/bug ID from Jira]-[name of the task/bug]).

If the feature branch is complete (all the new stuff has been added, committed and pushed), a merge request can be created. Eventually merge conflicts can occur and have to be solved.

The merge request is then reviewed and approved according to the QA Workflow by a Buddy and QA (please assign the relevant people to the merge request as Reviewer).

And finally, if approved, you can merge the feature branch back into the dev branch. Congrats! Your changes are now in production 

### Additional Branches
#### graphics branch

The Graphics department may use this branch for their own purposes (sharing, discussing, organizing their work).

It is decoupled from the production branches and therefore does not contain the Unity project.

The branch can be used as a quick tool to collect, share and manage game assets without the need to integrate them into the Unity project.

The download of this branch should be faster as well.

#### sound branch

Similar to the graphics branch but soley for the Sound department.

## Unity Project Structure

The Unity Assets folder consists of 5 main categories/folders:

- ENG
- GFX
- SND
- Scenes
- Prefabs

### ENG

Folder for Assets of the Engineering department. In the beginning it contains a folder for Scripts and Shaders.

Both folders can be organized with subfolders as well to bundle similar Assets together in one folder.

### GFX

Folder for Assets of the Graphics department. In the beginning it contains a folder for Models, Textures and Materials.

### SND

Folder for Assets of the Sound department. In the beginning it contains a folder for Music and Sounds (SFX).

### Scenes

Contains all Scenes/Levels of the project. Currently only a Sandbox scene, that can be used by everyone to test out stuff.

Later we will have the proper game scenes.

### Prefabs

A prefab bundles multiple GameObjects into a single GameObject, that can be reused multiple times in a Scene.

Just drag'n'drop a prefab into a scene and it will appear. Prefabs can also be instantiated dynamically during runtime via scripts.

For example, to create a Tree prefab the Graphics department constructs a Tree in the Scene view with two GameObjects: a cylinder for the trunk and a sphere for the leaves. Both GameObjects are then attached to a parent GameObject called Tree. This Tree GameObject can then be dragged into the Assets browser (Prefabs folder) to create a new prefab.

Please create subfolders in the Prefabs folder to organize things.
