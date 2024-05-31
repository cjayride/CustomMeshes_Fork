# CustomMeshes

Patched for Valheim v0.217.46+

A fork of [aedenthorn/CustomMeshes](https://github.com/aedenthorn/ValheimMods/tree/master/CustomMeshes) 

[ThunderStore](https://thunderstore.io/c/valheim/p/cjayride/CustomMeshes) | [GitHub](https://github.com/cjayride/CustomMeshes_Fork)

# Replace object meshes with imported model files.

This mod will hopefully eventually allow replacing any mesh in the game; for now it lets you do the following:  
  
-   Replace the static mesh(es) of building pieces with imported fbx or obj files.
-   ﻿Replace the static mesh(es) of items with imported fbx or obj files.﻿
-   Replace skinned meshes with imported AssetBundles (experimental)
  
All files should be placed in subfolders in **BepInEx\plugins\CustomMeshes** folder (create it if it's not there) as explained below. 

<details>
  <summary>DETAILED INFORMATION</summary>
  
# Building Pieces

To import static meshes for building pieces, do the following:  

**[Step One:]** create a folder with the name of the thing you are replacing.  

For example, to replace the wooden chest meshes, create a subfolder in **BepInEx\plugins\CustomMeshes** called:  **piece_chest_wood**

**[Step Two:]** create a subfolder for each of the states of the building piece: **New**, **Worn**, **Broken** 

So now you should have three folders: 

**BepInEx\plugins\CustomMeshes\piece_chest_wood\New**  
**BepInEx\plugins\CustomMeshes\piece_chest_wood\Worn**  
**BepInEx\plugins\CustomMeshes\piece_chest_wood\Broken** 

**Step Three:** put fbx or obj files corresponding to each MeshFilter name into that folder, e.g.: 

woodchest.fbx
woodchesttop_closed.fbx

# Items 

To import static meshes for items, you need the item name, the renderer object name, and either the mesh filter name or the skinned mesh renderer name, depending on the item. You can find these by turning on debug in the config file and looking at the messages when the item spawns, e.g.:  

CustomMeshes got mesh filter item name: HelmetPadded, obj: HelmetPadded, mf: default  

CustomMeshes got skinned mesh renderer, item name: HelmetPadded, obj: attach_skin, smr: ChainLinkVisor  

You then create a file with the following folder structure:  

- BepInEx\plugins\CustomMeshes\<ItemName>\<ObjectName>\<MeshFilterName>.obj  

and/or  

- BepInEx\plugins\CustomMeshes\<ItemName>\<ObjectName>\<SkinnedMeshRenderer>.obj

For example:  

- BepInEx\plugins\CustomMeshes\HelmetPadded\HelmetPadded\default.obj 

or  

- BepInEx\plugins\CustomMeshes\HelmetPadded\attach_skin\ChainLinkVisor.obj 

# Player Meshes 

To import player meshes, you need to create an AssetBundle with a **body** object in it. Please don't ask me how to do that, I have no clue. You can ask people working on it in my discord server if you know something about Unity. 

**Step One:** create a folder called **player** with a subfolder called **model** in the **CustomMeshes** folder. So now you have: 

**BepInEx\plugins\CustomMeshes\player\model**  

**Step Two:** put the asset bundle you created in the subfolder, naming it **0** for male and **1** for female (make sure it has no file extension).


</details>

	
## Configuration

- A config file **BepInEx/config/cjayride.CustomMeshes.cfg** is created after running the game once with this mod.  