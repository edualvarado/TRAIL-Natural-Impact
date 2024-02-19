using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MxMEditor
{

    public static class MxMPreviewScene
    {
        private static Scene s_previewScene;
        private static string[] s_unloadedScenePaths;
        private static Vector3 s_savedSceneCamPos;
        private static Quaternion s_savedSceneCamRot;
        private static IPreviewable s_currentPreviewable;

        public static bool IsSceneLoaded { get { return s_previewScene.isLoaded; } }
        public static Animator PreviewAnimator { get; private set; }
        public static bool PreviewActive { get; private set; }
        public static Transform Ground { get; private set; }
        public static AnimationMixerPlayable Mixer { get; private set; }
        public static PlayableGraph PlayableGraph { get; private set; }
        public static GameObject PreviewObject { get; private set; }


        public static bool BeginPreview(IPreviewable a_previewable)
        {
            if(a_previewable == null)
            {
                Debug.LogError("Trying to use MxM preview but the passed IPreviewable is null. Aborting");
                return false;
            }

            if (Application.isPlaying)
            {
                Debug.LogError("Trying to use MxM preview while the Unity editor is in play mode. Aborting");
                return false;
            }

            if (PreviewActive)
            {
                if (s_currentPreviewable != null && s_currentPreviewable != a_previewable)
                {
                    s_currentPreviewable.EndPreviewLocal();
                }
            }
            else
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return false;

                int sceneCount = EditorSceneManager.sceneCount;
                s_unloadedScenePaths = new string[sceneCount];

                for (int i = 0; i < sceneCount; ++i)
                {
                    s_unloadedScenePaths[i] = EditorSceneManager.GetSceneAt(i).path;
                }


                Transform sceneCamTransform = SceneView.lastActiveSceneView.camera.transform;
                s_savedSceneCamPos = sceneCamTransform.position;
                s_savedSceneCamRot = sceneCamTransform.rotation;

                s_previewScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                s_previewScene.name = "MxM Preview";

                var sceneView = SceneView.lastActiveSceneView;

                GameObject cameraPoint = new GameObject();
                cameraPoint.transform.SetPositionAndRotation(new Vector3(-4f, 3f, 4f), Quaternion.Euler(23f, 125f, 0f));
                sceneView.AlignViewToObject(cameraPoint.transform);

                Object.DestroyImmediate(cameraPoint);

                GameObject dirLightObj = new GameObject();
                dirLightObj.name = "DirectionalLight";
                var light = dirLightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.Rotate(36f, -160f, 0f);
                light.SetLightDirty();

                var groundObj = GameObject.Instantiate(Resources.Load("GroundGrid")) as GameObject;

                if (groundObj != null)
                    Ground = groundObj.transform;

                PreviewActive = true;
            }
            
            s_currentPreviewable = a_previewable;

            return true;
        }

        public static void SetPreviewObject(GameObject a_previewObj)
        {
            if (a_previewObj == null)
            {
                Debug.LogError("Trying to set preview object in the preview " +
                    "scene but the passed object is null. Aborting");
                return;
            }

            if (PreviewObject != null)
            {
                PlayableGraph.Destroy();
                GameObject.DestroyImmediate(PreviewObject);
            }

            PreviewObject = GameObject.Instantiate(a_previewObj);
            PreviewObject.hideFlags = HideFlags.DontSaveInEditor;

            PreviewObject.name = "PreviewModel_MxM";
            PreviewAnimator = PreviewObject.GetComponent<Animator>();

            if (PreviewAnimator == null)
                PreviewAnimator = PreviewObject.AddComponent<Animator>();

            PlayableGraph = PlayableGraph.Create("PreviewCharacter");
            PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var playableOutput = AnimationPlayableOutput.Create(PlayableGraph,
                "Animation", PreviewAnimator);

            Mixer = AnimationMixerPlayable.Create(PlayableGraph, 1);
            playableOutput.SetSourcePlayable(Mixer);
            Mixer.SetInputWeight(0, 1f);
        }

        public static void EndPreview()
        {
            if (Application.isPlaying)
                return;

            if (PlayableGraph.IsValid())
                PlayableGraph.Destroy();

            if (PreviewObject != null)
                GameObject.DestroyImmediate(PreviewObject);

            if(s_unloadedScenePaths != null && s_unloadedScenePaths.Length > 0)
            {
                if (s_unloadedScenePaths[0] == "")
                    return;

                EditorSceneManager.OpenScene(s_unloadedScenePaths[0], OpenSceneMode.Single);

                for(int i = 1; i < s_unloadedScenePaths.Length; ++i)
                {
                    EditorSceneManager.OpenScene(s_unloadedScenePaths[i], OpenSceneMode.Additive);
                }

                var sceneView = SceneView.lastActiveSceneView;

                GameObject cameraPoint = new GameObject();
                cameraPoint.transform.SetPositionAndRotation(s_savedSceneCamPos, s_savedSceneCamRot);
                sceneView.AlignViewToObject(cameraPoint.transform);

                GameObject.DestroyImmediate(cameraPoint);
            }

            PreviewActive = false;
            s_currentPreviewable = null;
        }
    }
}