using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomMeshes {
    [BepInPlugin("cjayride.CustomMeshes", "Custom Meshes", "0.4.0")]
    public class BepInExPlugin : BaseUnityPlugin {
        private static Dictionary<string, Dictionary<string, Dictionary<string, CustomMeshData>>> customMeshes = new Dictionary<string, Dictionary<string, Dictionary<string, CustomMeshData>>>();
        private static Dictionary<string, AssetBundle> customAssetBundles = new Dictionary<string, AssetBundle>();
        private static Dictionary<string, Dictionary<string, Dictionary<string, GameObject>>> customGameObjects = new Dictionary<string, Dictionary<string, Dictionary<string, GameObject>>>();
        private static BepInExPlugin context;

        public static ConfigEntry<int> nexusID;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;

        public static Mesh customMesh { get; set; }

        private void Awake() {
            context = this;

            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");

            if (!modEnabled.Value)
                return;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.U)) {
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) {
            PreloadMeshes();
        }


        private static void PreloadMeshes() {
            foreach (AssetBundle ab in customAssetBundles.Values)
                ab.Unload(true);
            customMeshes.Clear();
            customGameObjects.Clear();
            customAssetBundles.Clear();

            string path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "CustomMeshes");

            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                return;
            }

            foreach (string dir in Directory.GetDirectories(path)) {
                string dirName = Path.GetFileName(dir);

                customMeshes[dirName] = new Dictionary<string, Dictionary<string, CustomMeshData>>();
                customGameObjects[dirName] = new Dictionary<string, Dictionary<string, GameObject>>();

                foreach (string subdir in Directory.GetDirectories(dir)) {
                    string subdirName = Path.GetFileName(subdir);

                    customMeshes[dirName][subdirName] = new Dictionary<string, CustomMeshData>();
                    customGameObjects[dirName][subdirName] = new Dictionary<string, GameObject>();

                    foreach (string file in Directory.GetFiles(subdir)) {
                        try {
                            SkinnedMeshRenderer renderer = null;
                            Mesh mesh = null;
                            
                            string name = Path.GetFileNameWithoutExtension(file);
                            if (name == Path.GetFileName(file)) {
                                AssetBundle ab = AssetBundle.LoadFromFile(file);
                                customAssetBundles.Add(name, ab);

                                GameObject prefab = ab.LoadAsset<GameObject>("Player");
                                if (prefab != null) {
                                    renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
                                    if (renderer != null) 
                                        mesh = renderer.sharedMesh;                                   
                                    if (mesh == null)
                                        mesh = ab.LoadAsset<Mesh>("body");
                                } else {
                                    mesh = ab.LoadAsset<Mesh>("body");
                                }
                            } else if (Path.GetExtension(file).ToLower() == ".fbx") {
                                GameObject obj = MeshImporter.Load(file);
                                GameObject obj2 = obj?.transform.Find("Player")?.Find("Visual")?.gameObject;
                                mesh = obj.GetComponentInChildren<MeshFilter>().mesh;
                                if (obj2 != null)
                                    renderer = obj2.GetComponentInChildren<SkinnedMeshRenderer>();
                            } else if (Path.GetExtension(file).ToLower() == ".obj") {
                                mesh = new ObjImporter().ImportFile(file);
                            }
                            if (mesh != null) {
                                customMeshes[dirName][subdirName].Add(name, new CustomMeshData(dirName, name, mesh, renderer));
                            }
                        } catch { }
                    }
                }
            }
        }
        private static string GetPrefabName(string name) {
            char[] anyOf = new char[] { '(', ' ' };
            int num = name.IndexOfAny(anyOf);
            string result;
            if (num >= 0)
                result = name.Substring(0, num);
            else
                result = name;
            return result;
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        static class ZNetScene_Awake_Patch {
            static void Postfix() {


            }
        }

        [HarmonyPatch(typeof(ItemDrop), "Awake")]
        static class ItemDrop_Patch {
            static void Postfix(ItemDrop __instance) {
                string name = __instance.m_itemData?.m_dropPrefab?.name;
                if (name != null && customMeshes.ContainsKey(name)) {
                    MeshFilter[] mfs = __instance.m_itemData.m_dropPrefab.GetComponentsInChildren<MeshFilter>(true);
                    foreach (MeshFilter mf in mfs) {
                        string parent = mf.transform.parent.gameObject.name;
                        if (name == GetPrefabName(parent) && customMeshes[name].ContainsKey(mf.name) && customMeshes[name][mf.name].ContainsKey(mf.name)) {
                            mf.mesh = customMeshes[name][mf.name][mf.name].mesh;
                        } else if (customMeshes[name].ContainsKey(parent) && customMeshes[name][parent].ContainsKey(mf.name)) {
                            mf.mesh = customMeshes[name][parent][mf.name].mesh;
                        }
                    }
                    SkinnedMeshRenderer[] smrs = __instance.m_itemData.m_dropPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer smr in smrs) {
                        string parent = smr.transform.parent.gameObject.name;
                        if (name == GetPrefabName(parent) && customMeshes[name].ContainsKey(smr.name) && customMeshes[name][smr.name].ContainsKey(smr.name)) {
                            smr.sharedMesh = customMeshes[name][smr.name][smr.name].mesh;
                        } else if (customMeshes[name].ContainsKey(parent) && customMeshes[name][parent].ContainsKey(smr.name)) {
                            smr.sharedMesh = customMeshes[name][parent][smr.name].mesh;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Piece), "Awake")]
        static class Piece_Patch {
            static void Postfix(Piece __instance) {
                string name = GetPrefabName(__instance.gameObject.name);
                MeshFilter[] mfs = __instance.gameObject.GetComponentsInChildren<MeshFilter>(true);

                if (customMeshes.ContainsKey(name)) {
                    foreach (MeshFilter mf in mfs) {
                        string parent = mf.transform.parent.gameObject.name;
                        if (customMeshes[name].ContainsKey(parent) && customMeshes[name][parent].ContainsKey(mf.name)) {
                            mf.mesh = customMeshes[name][parent][mf.name].mesh;
                        }
                    }
                }
            }
        }

        private static Transform RecursiveFind(Transform parent, string childName) {
            Transform child = null;
            for (int i = 0; i < parent.childCount; i++) {
                child = parent.GetChild(i);
                if (child.name == childName)
                    break;
                child = RecursiveFind(child, childName);
                if (child != null)
                    break;
            }
            return child;
        }

        [HarmonyPatch(typeof(VisEquipment), "Awake")]
        static class Awake_Patch {
            static void Postfix(VisEquipment __instance) {

                if (!__instance.m_isPlayer || __instance.m_models.Length == 0)
                    return;

                if (customMeshes.ContainsKey("player")) {

                    if (customMeshes["player"].ContainsKey("model")) {
                        SkinnedMeshRenderer renderer = null;
                        if (customMeshes["player"]["model"].ContainsKey("0")) {
                            CustomMeshData custom = customMeshes["player"]["model"]["0"];
                            __instance.m_models[0].m_mesh = custom.mesh;
                            renderer = custom.renderer;
                        }
                        if (customMeshes["player"]["model"].ContainsKey("1")) {
                            CustomMeshData custom = customMeshes["player"]["model"]["1"];
                            __instance.m_models[1].m_mesh = custom.mesh;
                            renderer = custom.renderer;
                        }
                        if (renderer != null) {
                            Transform armature = __instance.m_bodyModel.rootBone.parent;
                            Transform[] newBones = new Transform[renderer.bones.Length];
                            for (int i = 0; i < newBones.Length; i++) {
                                newBones[i] = RecursiveFind(armature, renderer.bones[i].name);
                            }
                            __instance.m_bodyModel.bones = newBones;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(InventoryGui), "SetupDragItem")]
        static class SetupDragItem_Patch {
            static void Postfix(ItemDrop.ItemData item) {
                if (item == null)
                    return;
                string name = item.m_dropPrefab.name;
                MeshFilter[] mfs = item.m_dropPrefab.GetComponentsInChildren<MeshFilter>();
            }
        }
        [HarmonyPatch(typeof(Player), "Awake")]
        static class Player_Awake_Patch {
            static void Postfix(Player __instance) {
                return;

                if (customGameObjects.ContainsKey("player")) {
                    if (customGameObjects["player"].ContainsKey("model")) {

                        GameObject go = __instance.gameObject.transform.Find("Visual")?.Find("Armature")?.gameObject;
                        if (go == null) 
                            return; 

                        if (customGameObjects["player"]["model"].ContainsKey("0")) {
                            GameObject newObject;
                            Transform parent = go.transform.parent;
                            Vector3 position = go.transform.position;
                            Quaternion rotation = go.transform.rotation;
                            DestroyImmediate(go);
                            newObject = Instantiate(customGameObjects["player"]["model"]["0"], parent);
                            newObject.transform.position = position;
                            newObject.transform.rotation = rotation;
                        }
                        if (customMeshes["player"]["model"].ContainsKey("1")) {
                            GameObject newObject;
                            newObject = Instantiate(customGameObjects["player"]["model"]["1"]);
                            newObject.transform.position = go.transform.position;
                            newObject.transform.rotation = go.transform.rotation;
                            Transform parent = go.transform.parent;
                            DestroyImmediate(go);
                            newObject.transform.SetParent(parent);
                        }
                    }
                }
            }
        }
    }
}