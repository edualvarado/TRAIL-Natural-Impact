using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEditor;
using MxM;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief Editor window for tagging and timeline events in Motion Matching for Unity
    *         
    *********************************************************************************************/
    public class MxMTaggingWindow : EditorWindow, IPreviewable
    {
        private static IMxMAnim m_targetMxMAnim;

        private static IMxMAnim m_clipboardMxMAnim;

        private static float m_areaSplitPosition = 300f;

        private static float m_curTime;

        private static bool m_draggingSplit;
        private static bool m_previewActive;

        private static bool m_playing;
        private static float m_lastPlayIncTime;

        //Timeline
        private static float m_zoom = 1f;
        private static float m_tickSpacing = 7.5f;
        private static Vector2 m_timelineScrollPos;
        private static bool m_draggingTimeLine;
        private static bool m_autoZoom = true;

        //Speed & Timing
        private static ETimelineMode m_mode;
        private static bool m_playWithOriginalSpeed;
        private static bool m_loopTime = true;

        //Data
        private static Vector2 m_dataScrollPos;
        private static int m_tagIdToAdd = 0;
        private static int m_favourTagIdToAdd = 0;
        private static int m_userTagIdToAdd = 0;
        private static ETags m_tagToAdd;
        private static ETags m_favourTagToAdd;
        private static EUserTags m_userTagToAdd;

        private static bool m_showMoveGizmo = true;
        private static bool m_showRotateGizmo = false;
        private static bool m_showArrowGizmo = true;
        private static Mesh m_debugArrow;
        private static Material m_debugArrowMat;

        //Tag Selection
        private static TagTrack m_selectTrack;
        private static bool m_tagSelected;

        //Utility track tag selection
        private static TagTrackBase m_selectUtilityTrack;
        private static bool m_utilityTagSelected;

        //Event Selection
        private static EventMarker m_selectEvent;
        private static bool m_eventSelected;

        //Section Selection
        private static MotionSection m_selectMotionSection;
        private static bool m_sectionSelected;

        public AnimationClip TargetClip { get { return m_targetMxMAnim.TargetClip; } } //PrimaryClip for composite
        public IMxMAnim TargetMxMAnim { get { return m_targetMxMAnim; } } //Todo: Change Name

        private static HumanBodyBones m_eventToJoint;
        private static int m_eventToJointId; //For generic rigs
        private static List<string> m_jointNames;

        //Queues
        private static bool m_deselectQueued = false;
        private static bool m_addPOIQueued = false;
        private static float m_addPOIQueueTime = 0f;

        //Footstep detection
        private static float m_footGroundingThreshold = 0.15f;
        private static float m_minFootStepSpacing = 0.05f;
        private static float m_minFootStepDuration = 0f;
        private static float m_maxFootSpeed = 0.3f;
        private static EFootstepPace m_defaultStepPace = EFootstepPace.Walk;
        private static EFootstepType m_defaultStepType = EFootstepType.Step;

        //Preview Scene
        private static AnimationClip m_previewClip;

        private static MxMTaggingWindow inst;

        public enum ETimelineMode
        {
            MotionMatching,
            //Timing,
            Utility,
            User,
            Curves
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static MxMTaggingWindow Inst()
        {
            if (inst == null)
                ShowWindow();

            return inst;
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static bool Exists()
        {
            if (inst == null)
                return false;

            return true;
        }

        //============================================================================================
        /**
        *  @brief Shows the window
        *         
        *********************************************************************************************/
        [MenuItem("Window/MxM/Timeline")]
        public static void ShowWindow()
        {
            System.Type consoleType = System.Type.GetType("UnityEditor.ConsoleWindow, UnityEditor.dll");
            System.Type gameType = System.Type.GetType("UnityEditor.GameView, UnityEditor.dll");

            EditorWindow editorWindow = EditorWindow.GetWindow<MxMTaggingWindow>("MxM Timeline", true,
                new System.Type[] { consoleType, gameType });

            editorWindow.minSize = new Vector2(100f, 50f);
            editorWindow.Show();

            inst = (MxMTaggingWindow)editorWindow;
        }

        //============================================================================================
        /**
        *  @brief Shows the window
        *         
        *********************************************************************************************/
        private void OnGUI()
        {
            if (m_targetMxMAnim == null)
            {
                GUILayout.Space(18f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("No Animation Selected.", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                return;
            }

            ManageInputs(Event.current);

            //ObtainTarget();

            DrawDataArea();
            DrawTimeline();

            Handles.BeginGUI();
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            Handles.DrawLine(new Vector3(m_areaSplitPosition, 0f, 0f),
                new Vector3(m_areaSplitPosition, position.height, 0f));
            Handles.EndGUI();

            Rect splitHandleRect = new Rect(m_areaSplitPosition - 3f, 0f,
               6f, position.height);

            EditorGUIUtility.AddCursorRect(splitHandleRect, MouseCursor.ResizeHorizontal);

            if (m_playing && m_loopTime)
                LoopTime();
            else
                ClampTime();

            HandleLayoutChanges();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void ClipChanged()
        {
            Repaint();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnSceneGUI(SceneView a_sceneView)
        {
            //Draw the trajectory of the current animation
            if (!m_previewActive)
                return;

            var rootLookupTable = m_targetMxMAnim.GetRootLookupTable();
            var tagTracks = m_targetMxMAnim.AnimTagTracks;

            TagTrack doNotUseTagTrack = null;

            foreach (TagTrack track in tagTracks)
            {
                if (track.TagId == ETags.DoNotUse)
                {
                    doNotUseTagTrack = track;
                    break;
                }
            }

            Vector3 lastPos = rootLookupTable.posLookup[0];
            for (int i = 1; i < rootLookupTable.posLookup.Count; ++i)
            {
                Vector3 thisPos = rootLookupTable.posLookup[i];

                if (doNotUseTagTrack != null)
                {
                    if (doNotUseTagTrack.IsTimeTagged(i * (1f / 60f)))
                        Handles.color = Color.red;
                    else
                        Handles.color = Color.green;
                }
                else
                {
                    Handles.color = Color.green;
                }

                Handles.DrawLine(lastPos, thisPos);

                lastPos = thisPos;
            }

            //Draw Current root position
            var rootLookup = m_targetMxMAnim.GetRoot(m_curTime);
            Handles.color = Color.red;
            Handles.DrawSolidDisc(rootLookup.pos, Vector3.up, 0.05f);

            //Draw gizmos for selected event
            if (m_selectEvent != null)
            {
                Handles.BeginGUI();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                EditorGUI.BeginChangeCheck();
                m_showMoveGizmo = GUILayout.Toggle(m_showMoveGizmo, EditorGUIUtility.IconContent("MoveTool").image, GUI.skin.button);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_showMoveGizmo)
                        m_showRotateGizmo = false;
                }

                EditorGUI.BeginChangeCheck();
                m_showRotateGizmo = GUILayout.Toggle(m_showRotateGizmo, EditorGUIUtility.IconContent("RotateTool").image, GUI.skin.button);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_showRotateGizmo)
                        m_showMoveGizmo = false;
                }

                m_showArrowGizmo = GUILayout.Toggle(m_showArrowGizmo, "Arrows", GUI.skin.button, GUILayout.Height(23f));

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                float secondaryActionDuration = 0f;
                for (int i = 1; i < m_selectEvent.Actions.Count; ++i)
                {
                    secondaryActionDuration += m_selectEvent.Actions[i];
                }

                if (m_curTime > m_selectEvent.EventTime - m_selectEvent.Actions[0] - m_selectEvent.Windup)
                {
                    if (m_curTime < m_selectEvent.EventTime - m_selectEvent.Actions[0])
                    {
                        GUILayout.Label("Windup");
                    }
                    else if (m_curTime < m_selectEvent.EventTime + secondaryActionDuration)
                    {
                        GUILayout.Label("Action");
                    }
                    else if (m_curTime < m_selectEvent.EventTime + secondaryActionDuration + m_selectEvent.FollowThrough)
                    {
                        GUILayout.Label("FollowThrough");
                    }
                    else if (m_curTime < m_selectEvent.EventTime + secondaryActionDuration + m_selectEvent.FollowThrough + m_selectEvent.Recovery)
                    {
                        GUILayout.Label("Recover");
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                Handles.EndGUI();


                if (m_debugArrow == null || m_debugArrowMat == null)
                {
                    m_debugArrow = Resources.Load<Mesh>("DebugArrow");
                    m_debugArrowMat = Resources.Load<Material>("DebugArrowMat");
                }

                for (int i = 0; i < m_selectEvent.Contacts.Count; ++i)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 position = m_selectEvent.Contacts[i].Position;
                    Quaternion rotation = Quaternion.AngleAxis(m_selectEvent.Contacts[i].RotationY, Vector3.up);

                    if (m_showMoveGizmo)
                    {
                        position = Handles.PositionHandle(position, Quaternion.identity);
                    }

                    if (m_showRotateGizmo)
                    {
                        rotation = Handles.RotationHandle(rotation, position);
                    }

                    if (m_showArrowGizmo && m_debugArrow != null)
                    {
                        m_debugArrowMat.SetPass(0);
                        Graphics.DrawMeshNow(m_debugArrow, position, rotation, 0);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        EventContact alteredContact = m_selectEvent.Contacts[i];
                        alteredContact.Position = position;
                        alteredContact.RotationY = rotation.eulerAngles.y;

                        m_selectEvent.Contacts[i] = alteredContact;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                        a_sceneView.Repaint();
                        Repaint();
                    }

                    if (!m_showArrowGizmo)
                    {
                        Handles.color = Color.red;
                        Handles.DrawSolidDisc(position, Vector3.up, 0.08f);
                    }
                    else
                    {
                        Handles.BeginGUI();
                        Vector3 screenPos = a_sceneView.camera.WorldToScreenPoint(position);
                        GUIStyle yellowText = new GUIStyle(GUI.skin.label);
                        yellowText.fontStyle = FontStyle.Bold;
                        yellowText.normal.textColor = Color.blue;
                        GUI.Box(new Rect(screenPos.x + 17, a_sceneView.camera.pixelHeight - screenPos.y - 2, 75f, 20f), "");
                        GUI.Label(new Rect(screenPos.x + 20f, a_sceneView.camera.pixelHeight - screenPos.y, 75f, 18f), "Contact " + i, yellowText);
                        Handles.EndGUI();
                    }
                }
            }

            //Exit Preview Mode button
            //Handles.BeginGUI();
            //EditorGUILayout.BeginVertical();

            //if (GUILayout.Button("Exit Preview", GUILayout.Width(100f)))
            //{
            //    EndPreview();
            //}

            //EditorGUILayout.EndVertical();
            //Handles.EndGUI();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void UndoCallback()
        {
            Repaint();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawDataArea()
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;

            if (targetClip == null)
                return;

            MxMPreProcessData targetPreProcessData = m_targetMxMAnim.TargetPreProcess;
            AnimationModule targetAnimModule = m_targetMxMAnim.TargetAnimModule;
            MotionModifyData motionModifyData = m_targetMxMAnim.AnimMotionModifier;
            GameObject targetPrefab = m_targetMxMAnim.TargetModel;
            List<TagTrack> tagTracks = m_targetMxMAnim.AnimTagTracks;

#if UNITY_2019_3_OR_NEWER
            Rect areaRect = new Rect(0f, 0f, m_areaSplitPosition, 40f);
#else
            Rect areaRect = new Rect(0f, 0f, m_areaSplitPosition, 36f);
#endif

            GUILayout.BeginArea(areaRect);
            EditorGUILayout.BeginVertical();

            //The play toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));



#if UNITY_2019_3_OR_NEWER
            float previewButtonWidth = 60f;
#else
            float previewButtonWidth = 50f;
#endif
            EditorGUI.BeginChangeCheck();
            m_previewActive = GUILayout.Toggle(m_previewActive, "Preview",
                EditorStyles.toolbarButton, GUILayout.Width(previewButtonWidth));

            if (EditorGUI.EndChangeCheck())
            {
                if (m_previewActive)
                {
                    BeginPreview();
                }
                else
                {
                    EndPreview();
                }
            }

            if (m_previewActive)
            {
                UpdatePreview();
                Repaint();
            }

            GUILayout.Space(5f);

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.FirstKey").image, EditorStyles.toolbarButton, GUILayout.Width(30f)))
            {
                m_curTime = 0f;
                m_playing = false;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey").image, EditorStyles.toolbarButton, GUILayout.Width(30f)))
            {
                m_curTime -= 0.1f;
                m_playing = false;
            }

            if (m_playing)
            {
                m_playing = GUILayout.Toggle(m_playing, EditorGUIUtility.IconContent("Animation.Play").image,
                    EditorStyles.toolbarButton, GUILayout.Width(30f));
                Repaint();

                float speedMult = m_targetMxMAnim.PlaybackSpeed;
                if (m_targetMxMAnim != null && motionModifyData != null && !m_playWithOriginalSpeed && m_targetMxMAnim.UsingSpeedMods)
                {
                    if (targetPreProcessData != null)
                    {
                        speedMult *= motionModifyData.GetSectionSpeedAtTime(
                        m_curTime, targetPreProcessData.MotionTimingPresets);
                    }
                    else if (targetAnimModule != null)
                    {
                        //speedMult *= motionModifyData.GetSectionSpeedAtTime(
                        //m_curTime, targetAnimModule.MotionTimingPresets);
                    }
                }


                m_curTime += speedMult * ((float)EditorApplication.timeSinceStartup - m_lastPlayIncTime);

                m_lastPlayIncTime = (float)EditorApplication.timeSinceStartup;
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                m_playing = GUILayout.Toggle(m_playing, EditorGUIUtility.IconContent("Animation.Play").image,
                    EditorStyles.toolbarButton, GUILayout.Width(30f));

                if (EditorGUI.EndChangeCheck())
                {
                    m_lastPlayIncTime = (float)EditorApplication.timeSinceStartup;

                    if (m_curTime > targetClip.length - 0.001f)
                    {
                        m_curTime = 0f;
                    }

                }
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.NextKey").image, EditorStyles.toolbarButton, GUILayout.Width(30f)))
            {
                m_curTime += 0.1f;
                m_playing = false;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.LastKey").image,
                EditorStyles.toolbarButton, GUILayout.Width(30f)))
            {
                m_playing = false;
                m_curTime = targetClip.length;
            }

            GUILayout.FlexibleSpace();

            m_curTime = EditorGUILayout.FloatField(m_curTime, EditorStyles.toolbarTextField, GUILayout.Width(40f));

            EditorGUILayout.EndHorizontal();


            //The Event Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(targetClip.name);

            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            m_mode = (ETimelineMode)EditorGUILayout.EnumPopup(m_mode);
            if (EditorGUI.EndChangeCheck())
            {
                //DeselectAllEvents();
                DeselectAllTags();
            }

            GUILayout.Space(5f);

            if (m_targetMxMAnim.AnimType != EMxMAnimtype.BlendSpace)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.AddEvent").image,
                    EditorStyles.toolbarButton, GUILayout.Width(30f)))
                {
                    switch (m_mode)
                    {
                        case ETimelineMode.MotionMatching:
                            {
                                m_targetMxMAnim.AddEvent(m_curTime);
                            }
                            break;
                        // case ETimelineMode.Timing:
                        //     {
                        //         if (motionModifyData != null)
                        //             motionModifyData.AddPOI(m_curTime);
                        //     }
                        //     break;
                        case ETimelineMode.Utility:
                            {
                                //Todo: Add trigger here if necessary
                            }
                            break;
                        case ETimelineMode.Curves:
                            {

                            }
                            break;
                    }

                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            //Content Area
#if UNITY_2019_3_OR_NEWER
            areaRect = new Rect(0f, 40f, areaRect.width, position.height - 80f);
#else
            areaRect = new Rect(0f, 36f, areaRect.width, position.height - 72f);
#endif
            GUILayout.BeginArea(areaRect);
            m_dataScrollPos = EditorGUILayout.BeginScrollView(m_dataScrollPos);

            GUILayout.Space(5f);

            switch (m_mode)
            {
                case ETimelineMode.MotionMatching: DrawMotionMatchingData(areaRect); break;
                //case ETimelineMode.Timing: DrawTimingData(); break;
                case ETimelineMode.Utility: DrawUtilityData(areaRect); break;
                case ETimelineMode.User: DrawUserData(areaRect); break;
                case ETimelineMode.Curves: DrawCurvesData(areaRect); break;

            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

            //Utilty Toolbar
#if UNITY_2019_3_OR_NEWER
            areaRect = new Rect(0f, position.height - 40f, areaRect.width, 40f);
#else
            areaRect = new Rect(0f, position.height - 36f, areaRect.width, 36f);
#endif
            GUILayout.BeginArea(areaRect);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Copy All", EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                m_clipboardMxMAnim = m_targetMxMAnim;
            }
            if (m_clipboardMxMAnim != null)
            {
                if (GUILayout.Button("Paste All", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    if (EditorUtility.DisplayDialog("Paste Tags and Events", "Pasting all tags and events onto this " +
                        "MxMAnim will override previous tags and events. Are you sure?", "Yes", "Cancel"))
                    {
                        m_targetMxMAnim.CopyTagsAndEvents(m_clipboardMxMAnim);
                    }
                }

                if (GUILayout.Button("Paste Mirrored", EditorStyles.toolbarButton, GUILayout.Width(95f)))
                {
                    if (EditorUtility.DisplayDialog("Paste Tags and Events", "Pasting all tags and events onto this " +
                        "MxMAnim will override previous tags and events. Are you sure?", "Yes", "Cancel"))
                    {
                        m_targetMxMAnim.CopyTagsAndEvents(m_clipboardMxMAnim, true);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Speed", GUILayout.Width(45f));
            
            EditorGUI.BeginChangeCheck();
            float speed = EditorGUILayout.FloatField(m_targetMxMAnim.PlaybackSpeed, EditorStyles.toolbarTextField, GUILayout.Width(30f));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Change Playback Speed");
                m_targetMxMAnim.PlaybackSpeed = speed < 0.01f ? 0.01f : speed;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Zoom", GUILayout.Width(40f));
            m_zoom = EditorGUILayout.FloatField(m_zoom, EditorStyles.toolbarTextField, GUILayout.Width(30f));
            if (EditorGUI.EndChangeCheck())
            {
                m_autoZoom = false;
            }

            EditorGUI.BeginChangeCheck();
            m_autoZoom = GUILayout.Toggle(m_autoZoom, "Auto", EditorStyles.toolbarButton, GUILayout.Width(40));
            if (EditorGUI.EndChangeCheck())
            {
                UpdateAutoZoom();
            }

            GUILayout.FlexibleSpace();
            m_loopTime = GUILayout.Toggle(m_loopTime, "Loop", EditorStyles.toolbarButton, GUILayout.Width(40f));


            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawMotionMatchingData(Rect a_areaRect)
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;
            MxMPreProcessData targetPreProcessData = m_targetMxMAnim.TargetPreProcess;
            AnimationModule targetAnimModule = m_targetMxMAnim.TargetAnimModule;
            GameObject targetPrefab = m_targetMxMAnim.TargetModel;
            List<TagTrack> tagTracks = m_targetMxMAnim.AnimTagTracks;
            List<TagTrack> favourTagTracks = m_targetMxMAnim.AnimFavourTagTracks;

            if (targetClip == null)
                return;

            if (tagTracks == null || favourTagTracks == null)
                m_targetMxMAnim.InitTagTracks();

            if (m_selectEvent != null)
            {
                List<string> eventNames = null;
                bool getBonesByName = false;

                if (targetPreProcessData != null)
                {
                    eventNames = targetPreProcessData.EventNames;
                    getBonesByName = targetPreProcessData.GetBonesByName;
                }
                else if (targetAnimModule != null)
                {
                    eventNames = targetAnimModule.EventNames;
                    getBonesByName = targetAnimModule.GetBonesByName;
                }

                if (eventNames != null)
                {

                    EditorGUI.BeginChangeCheck();
                    int eventId = EditorGUILayout.Popup(new GUIContent("EventId"),
                        m_selectEvent.EventId, eventNames.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Change Event Id");
                        m_selectEvent.EventId = eventId;

                        if (eventId > -1 && eventId < eventNames.Count)
                        {
                            m_selectEvent.EventName = eventNames[eventId];
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    float windup = EditorGUILayout.FloatField(new GUIContent("Wind-up"), m_selectEvent.Windup);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Change Event Windup");
                        m_selectEvent.Windup = windup;

                        if (m_selectEvent.Windup < 0f)
                            m_selectEvent.Windup = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }

                    if (GUILayout.Button("|", GUILayout.Width(22f)))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Change Event Windup");
                        m_selectEvent.Windup = Mathf.Clamp(m_selectEvent.EventTime - m_selectEvent.Actions[0] -
                            m_curTime, 0f, m_selectEvent.EventTime - m_selectEvent.Actions[0]);

                        if (m_selectEvent.Windup < 0f)
                            m_selectEvent.Windup = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    float action = EditorGUILayout.FloatField(new GUIContent("Action"), m_selectEvent.Actions[0]);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Change Event Action");
                        m_selectEvent.Actions[0] = action;

                        if (m_selectEvent.Actions[0] < 0f)
                            m_selectEvent.Actions[0] = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }

                    if (GUILayout.Button("|", GUILayout.Width(22f)))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Change Event Action");
                        m_selectEvent.Actions[0] = Mathf.Clamp(m_selectEvent.EventTime - m_curTime, 0f, m_selectEvent.EventTime);

                        if (m_selectEvent.Actions[0] < 0f)
                            m_selectEvent.Actions[0] = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    float followThrough = EditorGUILayout.FloatField(
                        new GUIContent("FollowThrough"), m_selectEvent.FollowThrough);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Change Event Follow Through");
                        m_selectEvent.FollowThrough = followThrough;

                        if (m_selectEvent.FollowThrough < 0f)
                            m_selectEvent.FollowThrough = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }

                    if (GUILayout.Button("|", GUILayout.Width(22f)))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Change Event Follow Through");


                        float followThroughDuration = m_curTime - m_selectEvent.EventTime;

                        for (int k = 1; k < m_selectEvent.Actions.Count; ++k)
                        {
                            followThroughDuration -= m_selectEvent.Actions[k];
                        }

                        m_selectEvent.FollowThrough = Mathf.Clamp(followThroughDuration,
                            0f, targetClip.length - m_selectEvent.EventTime);

                        if (m_selectEvent.FollowThrough < 0f)
                            m_selectEvent.FollowThrough = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    float recovery = EditorGUILayout.FloatField(new GUIContent("Recovery"), m_selectEvent.Recovery);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Change Event Recovery");
                        m_selectEvent.Recovery = recovery;

                        if (m_selectEvent.Recovery < 0f)
                            m_selectEvent.Recovery = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }

                    if (GUILayout.Button("|", GUILayout.Width(22f)))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Change Event Recovery");

                        float recoveryDuration = m_curTime - m_selectEvent.EventTime - m_selectEvent.FollowThrough;

                        for (int k = 1; k < m_selectEvent.Actions.Count; ++k)
                        {
                            recoveryDuration -= m_selectEvent.Actions[k];
                        }

                        m_selectEvent.Recovery = Mathf.Clamp(recoveryDuration, 0f,
                            targetClip.length - m_selectEvent.EventTime - m_selectEvent.FollowThrough);

                        if (m_selectEvent.Recovery < 0f)
                            m_selectEvent.Recovery = 0f;

                        EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                    }

                    EditorGUILayout.EndHorizontal();

                    Rect lastRect = GUILayoutUtility.GetLastRect();

                    Handles.color = Color.black;
                    Vector3 start = new Vector3(a_areaRect.x, lastRect.y + lastRect.height + 4f);
                    Vector3 end = new Vector3(a_areaRect.x + a_areaRect.width, start.y);
                    Handles.DrawLine(start, end);

                    GUIStyle redBtnStyle = new GUIStyle(GUI.skin.button);
                    redBtnStyle.normal.textColor = Color.red;

                    GUIStyle lblBoldStyle = new GUIStyle(GUI.skin.label);
                    lblBoldStyle.fontStyle = FontStyle.Bold;

                    //Draw Event Contacts
                    for (int i = 0; i < m_selectEvent.Contacts.Count; ++i)
                    {
                        GUILayout.Space(4f);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Contact " + (i + 1) + ": ", lblBoldStyle, GUILayout.Width(150f));

                        if (GUILayout.Button("Reset"))
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Reset Event Position");

                            EventContact alteredContact = m_selectEvent.Contacts[i];
                            alteredContact.Position = Vector3.zero;
                            m_selectEvent.Contacts[i] = alteredContact;
                        }

                        if (i > 0)
                        {
                            if (GUILayout.Button("Delete", redBtnStyle))
                            {
                                if (EditorUtility.DisplayDialog("Delete Contact", "Are you sure you want to delete this event contact?", "Yes", "No"))
                                {
                                    Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Delete Event Contact");

                                    m_selectEvent.Contacts.RemoveAt(i);
                                    m_selectEvent.Actions.RemoveAt(i);
                                    EditorGUILayout.EndHorizontal();
                                    Repaint();
                                    SceneView.RepaintAll();
                                    break;
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(2f);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Pos: ", GUILayout.Width(30f));
                        EditorGUI.BeginChangeCheck();
                        Vector3 eventPos = EditorGUILayout.Vector3Field("", m_selectEvent.Contacts[i].Position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Modify Event Contact Position");

                            EventContact alteredContact = m_selectEvent.Contacts[i];
                            alteredContact.Position = eventPos;

                            m_selectEvent.Contacts[i] = alteredContact;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Rot Y: ", GUILayout.Width(40f));
                        EditorGUI.BeginChangeCheck();
                        float eventRotY = EditorGUILayout.FloatField("", m_selectEvent.Contacts[i].RotationY, GUILayout.Width(40f));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Modify Event Contact Rotation");

                            eventRotY = Mathf.Clamp(eventRotY, -180f, 180f);

                            EventContact alteredContact = m_selectEvent.Contacts[i];
                            alteredContact.RotationY = eventRotY;

                            m_selectEvent.Contacts[i] = alteredContact;
                        }

                        if (getBonesByName)
                        {
                            if (targetPrefab != null)
                            {
                                m_eventToJointId = EditorGUILayout.Popup("", m_eventToJointId, m_jointNames.ToArray(), GUILayout.Width(60f));
                            }
                        }
                        else
                        {
                            m_eventToJoint = (HumanBodyBones)EditorGUILayout.EnumPopup(m_eventToJoint);
                        }

                        if (GUILayout.Button("Set To Joint"))
                        {
                            if (!m_previewActive)
                            {
                                BeginPreview();
                            }

                            if (getBonesByName)
                            {
                                if (targetPrefab != null && m_eventToJointId > 0 && m_eventToJointId < m_jointNames.Count)
                                {
                                    Transform targetPoint = MxMPreviewScene.PreviewAnimator.transform.Find(m_jointNames[m_eventToJointId]);

                                    if (targetPoint != null)
                                    {
                                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Set Event To Joint");
                                        EventContact alteredContact = m_selectEvent.Contacts[i];
                                        alteredContact.Position = targetPoint.position;
                                        m_selectEvent.Contacts[i] = alteredContact;
                                    }
                                }
                            }
                            else
                            {
                                Transform targetPoint = MxMPreviewScene.PreviewAnimator.GetBoneTransform(m_eventToJoint);

                                if (targetPoint != null)
                                {
                                    Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Set Event To Joint");
                                    EventContact alteredContact = m_selectEvent.Contacts[i];
                                    alteredContact.Position = targetPoint.position;
                                    m_selectEvent.Contacts[i] = alteredContact;
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();

                        if (i > 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            //EditorGUILayout.LabelField("Contact Timing:", GUILayout.Width(100f));
                            EditorGUI.BeginChangeCheck();
                            float contactTiming = EditorGUILayout.FloatField(new GUIContent("Contact Timing"), m_selectEvent.Actions[i]);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Modify Contact Timing");
                                m_selectEvent.Actions[i] = contactTiming;
                            }

                            if (GUILayout.Button("|"))
                            {
                                Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Modify Contact Timing");

                                float actionDuration = m_curTime - m_selectEvent.EventTime;

                                for (int k = 1; k < i; ++k)
                                {
                                    actionDuration -= m_selectEvent.Actions[k];
                                }

                                if (actionDuration < 0f)
                                    actionDuration = 0f;

                                m_selectEvent.Actions[i] = actionDuration;
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        lastRect = GUILayoutUtility.GetLastRect();

                        Handles.color = Color.black;
                        start.y = lastRect.y + lastRect.height + 4f;
                        end.y = start.y;
                        Handles.DrawLine(start, end);

                    }

                    GUILayout.Space(4f);

                    if (GUILayout.Button("Add Contact"))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Add Event Contact");

                        m_selectEvent.Contacts.Add(new EventContact());
                        m_selectEvent.Actions.Add(0.2f);
                    }
                    SceneView.RepaintAll();
                }
            }
            else
            {
                GUIStyle tagTextStyle = new GUIStyle(GUI.skin.label);
                tagTextStyle.normal.textColor = Color.black;

                string[] tagNames = null;
                string[] favourTagNames = null;

                if (targetPreProcessData != null)
                {
                    tagNames = targetPreProcessData.TagNames.ToArray();
                    favourTagNames = targetPreProcessData.FavourTagNames.ToArray();
                }
                else if (targetAnimModule != null && targetAnimModule.TagNames != null && targetAnimModule.FavourTagNames != null)
                {
                    tagNames = targetAnimModule.TagNames.ToArray();
                    favourTagNames = targetAnimModule.FavourTagNames.ToArray();
                }
                else
                {
                    tagNames = System.Enum.GetNames(typeof(ETags));
                    favourTagNames = tagNames;
                }
#if UNITY_2019_3_OR_NEWER
                Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 20f);
#else
                Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 18f);
#endif

                bool highlightOn = false;

                if (tagTracks != null)
                {

                    for (int i = 0; i < tagTracks.Count; ++i)
                    {
                        TagTrack track = tagTracks[i];

                        if (highlightOn)
                        {
                            GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                            highlightOn = false;
                        }
                        else
                        {
                            GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                            highlightOn = true;
                        }

                        EditorGUILayout.BeginHorizontal();

                        int tagIndex = System.Array.IndexOf(System.Enum.GetValues(track.TagId.GetType()), track.TagId);

                        EditorGUILayout.LabelField(tagNames[tagIndex - 1], tagTextStyle);

                        GUILayout.FlexibleSpace();

                        Texture icon = EditorGUIUtility.IconContent("Animation.AddKeyFrame").image;

                        Rect btnRect = trackRect;
                        btnRect.x += trackRect.width - icon.width - 17f;
                        btnRect.y += 2f;
                        btnRect.width = icon.width;
                        btnRect.height = icon.height;

                        GUI.DrawTexture(btnRect, icon);

                        GUIStyle invisiButton = new GUIStyle(GUI.skin.label);
                        if (GUI.Button(btnRect, "", invisiButton))
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Add tag");
                            track.AddTag(m_curTime);
                        }

                        btnRect.x -= icon.width + 10f;
#if UNITY_2019_4_OR_NEWER
                        icon = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
#else
                        icon = EditorGUIUtility.IconContent("vcs_delete").image;
#endif
                        GUI.DrawTexture(btnRect, icon);
                        if (GUI.Button(btnRect, "", invisiButton))
                        {
                            if (EditorUtility.DisplayDialog("Delete Track",
                                "Are you sure you want to delete the track?", "Yes", "Cancel"))
                            {
                                Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Delete Tag Track");
                                tagTracks.RemoveAt(i);
                                --i;
                                Repaint();
                            }
                        }

                        EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                        trackRect.y += 20f;
#else
                        trackRect.y += 18f;
#endif
                    }

                    GUILayout.Space(5f);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    m_tagIdToAdd = EditorGUILayout.Popup(m_tagIdToAdd, tagNames, GUILayout.Width(150));

                    m_tagToAdd = (ETags)(1 << m_tagIdToAdd);

#if UNITY_2019_3_OR_NEWER
                    float createTrackHeight = 18f;
#else
                    float createTrackHeight = 14f;
#endif

                    if (GUILayout.Button("Create Track", GUILayout.Height(createTrackHeight)))
                    {
                        bool addTrack = true;

                        if (m_tagToAdd == ETags.None)
                        {
                            addTrack = false;
                        }
                        else
                        {
                            foreach (TagTrack track in tagTracks)
                            {
                                if (track.TagId == m_tagToAdd)
                                {
                                    addTrack = false;
                                    break;
                                }
                            }
                        }

                        if (addTrack)
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Add Tag Track");
                            tagTracks.Add(new TagTrack(m_tagToAdd, targetClip.length));
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    trackRect.y += 30f;

                    GUILayout.Space(7f);

                }

                if (favourTagTracks != null)
                {
                    //START Favour tags
                    for (int i = 0; i < favourTagTracks.Count; ++i)
                    {
                        TagTrack track = favourTagTracks[i];

                        if (highlightOn)
                        {
                            GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                            highlightOn = false;
                        }
                        else
                        {
                            GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                            highlightOn = true;
                        }

                        EditorGUILayout.BeginHorizontal();

                        int tagIndex = System.Array.IndexOf(System.Enum.GetValues(track.TagId.GetType()), track.TagId);

                        EditorGUILayout.LabelField(favourTagNames[tagIndex - 1], tagTextStyle);

                        GUILayout.FlexibleSpace();

                        Texture icon = EditorGUIUtility.IconContent("Animation.AddKeyFrame").image;

                        Rect btnRect = trackRect;
                        btnRect.x += trackRect.width - icon.width - 17f;
                        btnRect.y += 2f;
                        btnRect.width = icon.width;
                        btnRect.height = icon.height;

                        GUI.DrawTexture(btnRect, icon);

                        GUIStyle invisiButton = new GUIStyle(GUI.skin.label);
                        if (GUI.Button(btnRect, "", invisiButton))
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Add Favour Tag");
                            track.AddTag(m_curTime);
                        }

                        btnRect.x -= icon.width + 10f;
                        icon = EditorGUIUtility.IconContent("d_P4_DeletedLocal").image;
                        GUI.DrawTexture(btnRect, icon);
                        if (GUI.Button(btnRect, "", invisiButton))
                        {
                            if (EditorUtility.DisplayDialog("Delete Favour Track",
                                "Are you sure you want to delete the track?", "Yes", "Cancel"))
                            {
                                Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Delete Favour Tag Track");
                                favourTagTracks.RemoveAt(i);
                                --i;
                                Repaint();
                            }
                        }

                        EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                        trackRect.y += 20f;
#else
                        trackRect.y += 18f;
#endif
                    }

                    GUILayout.Space(7f);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    m_favourTagIdToAdd = EditorGUILayout.Popup(m_favourTagIdToAdd, favourTagNames, GUILayout.Width(150));

                    m_favourTagToAdd = (ETags)(1 << m_favourTagIdToAdd);

#if UNITY_2019_3_OR_NEWER
                    float createTrackHeight = 18f;
#else
                    float createTrackHeight = 14f;
#endif

                    if (GUILayout.Button("Create Favour Track", GUILayout.Height(createTrackHeight)))
                    {
                        bool addTrack = true;

                        if (m_favourTagToAdd == ETags.None)
                        {
                            addTrack = false;
                        }
                        else
                        {
                            foreach (TagTrack favourTrack in favourTagTracks)
                            {
                                if (favourTrack.TagId == m_favourTagToAdd)
                                {
                                    addTrack = false;
                                    break;
                                }
                            }
                        }

                        if (addTrack)
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Add Favour Tag Track");
                            favourTagTracks.Add(new TagTrack(m_favourTagToAdd, targetClip.length));
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(15f);
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawTimingData()
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;
            MxMPreProcessData targetPreProcessData = m_targetMxMAnim.TargetPreProcess;
            AnimationModule targetAnimModule = m_targetMxMAnim.TargetAnimModule;

            if (targetClip == null || (targetPreProcessData == null && targetAnimModule == null))
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            float defaultLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100f;
            EditorGUI.BeginChangeCheck();
            bool useSpeedMods = EditorGUILayout.Toggle("Modify Speed", m_targetMxMAnim.UsingSpeedMods);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Speed Mod On");
                m_targetMxMAnim.UsingSpeedMods = useSpeedMods;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (m_targetMxMAnim.UsingSpeedMods)
            {

                //GUILayout.Space(18f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 130f;
                m_playWithOriginalSpeed = EditorGUILayout.Toggle("Play Original Speed", m_playWithOriginalSpeed);
                EditorGUIUtility.labelWidth = defaultLabelWidth;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                //if (targetPreProcessData)
                //{
                //    if (GUILayout.Button("Generate Clip"))
                //    {
                //        string folderName = AssetDatabase.GetAssetPath(targetClip).Replace(targetClip.name + ".anim", "");
                //        m_targetMxMAnim.GenerateModifiedAnimation(targetPreProcessData, folderName);
                //    }
                //}

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            if (m_targetMxMAnim.AnimGeneratedClip != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Delete Generated Clip"))
                {
                    if (EditorUtility.DisplayDialog("Delete Generated Clip", " Are you sure", "Yes", "No"))
                    {
                        m_targetMxMAnim.ScrapGeneratedClips();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = defaultLabelWidth;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawUtilityData(Rect a_areaRect)
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;
            MxMPreProcessData targetPreProcessData = m_targetMxMAnim.TargetPreProcess;
            AnimationModule targetAnimModule = m_targetMxMAnim.TargetAnimModule;
            GameObject targetPrefab = m_targetMxMAnim.TargetModel;
            List<TagTrackBase> tagTracks = m_targetMxMAnim.GenericTagTracks;

            if (targetClip == null)
                return;

            GUIStyle tagTextStyle = new GUIStyle(GUI.skin.label);
            tagTextStyle.normal.textColor = Color.black;

#if UNITY_2019_3_OR_NEWER
            Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 20f);
#else
            Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 18f);
#endif
            bool highlightOn = false;

            if (tagTracks == null)
                m_targetMxMAnim.InitTagTracks();

            if (tagTracks != null)
            {

                for (int i = 0; i < tagTracks.Count; ++i)
                {
                    TagTrackBase track = tagTracks[i];

                    if (highlightOn)
                    {
                        GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                        highlightOn = false;
                    }
                    else
                    {
                        GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                        highlightOn = true;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(track.Name, tagTextStyle);

                    GUILayout.FlexibleSpace();

                    Texture icon = EditorGUIUtility.IconContent("Animation.AddKeyFrame").image;

                    Rect btnRect = trackRect;
                    btnRect.x += trackRect.width - icon.width - 17f;
                    btnRect.y += 2f;
                    btnRect.width = icon.width;
                    btnRect.height = icon.height;

                    GUI.DrawTexture(btnRect, icon);

                    GUIStyle invisiButton = new GUIStyle(GUI.skin.label);
                    if (GUI.Button(btnRect, "", invisiButton))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Add tag");
                        track.AddTag(m_curTime);

                        if (i < 2) //It's a step track
                        {
                            FootStepTagTrack stepTrack = track as FootStepTagTrack;

                            stepTrack.SetStepData(stepTrack.TagCount - 1, m_defaultStepPace, m_defaultStepType);
                        }
                    }

                    btnRect.x -= icon.width + 10f;

                    EditorGUILayout.EndHorizontal();

#if UNITY_2019_3_OR_NEWER
                    trackRect.y += 20f;
#else
                    trackRect.y += 18f;
#endif
                }

                GUILayout.Space(9f);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (m_selectUtilityTrack == null)
            {
                if (GUILayout.Button("Mark Footsteps With Defaults"))
                {
                    if (EditorUtility.DisplayDialog("Mark Footsteps with defaults",
                        "All existing footstep defaults will be overwritten! is this OK?", "Yes", "Cancel"))
                    {
                        MarkAllFootstepsWithDefaults();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                m_defaultStepPace = (EFootstepPace)EditorGUILayout.EnumPopup("Default Pace", m_defaultStepPace);
                m_defaultStepType = (EFootstepType)EditorGUILayout.EnumPopup("Default Type", m_defaultStepType);
            }
            else
            {
                switch ((EUtilityTagTrack)m_selectUtilityTrack.TrackId)
                {
                    case EUtilityTagTrack.LeftFoot:
                    case EUtilityTagTrack.RightFoot:
                        {
                            GUIStyle labelBold = new GUIStyle(GUI.skin.label);
                            labelBold.fontStyle = FontStyle.Bold;
                            GUILayout.Label("Selected Footstep Data", labelBold);

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            FootStepTagTrack stepTrack = m_selectUtilityTrack as FootStepTagTrack;

                            var stepData = stepTrack.GetFootStepData(m_selectUtilityTrack.SelectId);

                            EditorGUI.BeginChangeCheck();
                            EFootstepPace pace = (EFootstepPace)EditorGUILayout.EnumPopup("Pace", stepData.step.Pace);
                            EFootstepType type = (EFootstepType)EditorGUILayout.EnumPopup("Type", stepData.step.Type);

                            if (EditorGUI.EndChangeCheck())
                                stepTrack.SetStepData(m_selectUtilityTrack.SelectId, pace, type);
                        }
                        break;
                    case EUtilityTagTrack.PoseFavour:
                        {
                            GUIStyle labelBold = new GUIStyle(GUI.skin.label);
                            labelBold.fontStyle = FontStyle.Bold;
                            GUILayout.Label("Selected Pose Favour Data", labelBold);

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            FloatTagTrack favourTrack = m_selectUtilityTrack as FloatTagTrack;

                            EditorGUI.BeginChangeCheck();
                            float favour = EditorGUILayout.FloatField("Favour", favourTrack.GetTagValue(m_selectUtilityTrack.SelectId));
                            EditorGUILayout.LabelField("");


                            if (EditorGUI.EndChangeCheck())
                                favourTrack.SetTagValue(m_selectUtilityTrack.SelectId, favour);
                        }
                        break;
                    default:
                        {
                            if (GUILayout.Button("Mark Footsteps With Defaults"))
                            {
                                if (EditorUtility.DisplayDialog("Mark Footsteps with defaults",
                                    "All existing footstep defaults will be overwritten! is this OK?", "Yes", "Cancel"))
                                {
                                    MarkAllFootstepsWithDefaults();
                                }
                            }

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            m_defaultStepPace = (EFootstepPace)EditorGUILayout.EnumPopup("Default Pace", m_defaultStepPace);
                            m_defaultStepType = (EFootstepType)EditorGUILayout.EnumPopup("Default Type", m_defaultStepType);
                        }
                        break;
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Auto Detect FootSteps"))
            {
                if (EditorUtility.DisplayDialog("Auto Detect Footsteps",
                    "All existing footstep tagging will be overwritten! is this OK?", "Yes", "Cancel"))
                {
                    MxMFootstepDetector.DetectFootsteps(m_targetMxMAnim, targetPrefab, targetPreProcessData,
                        targetAnimModule, m_footGroundingThreshold, m_minFootStepSpacing, m_minFootStepDuration, m_maxFootSpeed);


                    MarkAllFootstepsWithDefaults();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            m_maxFootSpeed = EditorGUILayout.FloatField("Foot Speed Threshold", m_maxFootSpeed);
            m_footGroundingThreshold = EditorGUILayout.FloatField("Ground Threshold", m_footGroundingThreshold);
            m_minFootStepSpacing = EditorGUILayout.FloatField("Min Step Spacing", m_minFootStepSpacing);
            m_minFootStepDuration = EditorGUILayout.FloatField("Min Step Duration", m_minFootStepDuration);


            GUILayout.Space(5f);
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawUserData(Rect a_areaRect)
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;
            MxMPreProcessData targetPreProcessData = m_targetMxMAnim.TargetPreProcess;
            AnimationModule targetAnimModule = m_targetMxMAnim.TargetAnimModule;
            List<TagTrackBase> userTagTracks = m_targetMxMAnim.UserTagTracks;

            if (targetClip == null)
                return;

            if (userTagTracks == null)
                m_targetMxMAnim.InitTagTracks();

            GUIStyle tagTextStyle = new GUIStyle(GUI.skin.label);
            tagTextStyle.normal.textColor = Color.black;

            string[] userTagNames = null;

            if (targetPreProcessData != null)
            {
                userTagNames = targetPreProcessData.UserTagNames.ToArray();
            }
            else if (targetAnimModule != null && targetAnimModule.UserTagNames != null)
            {
                userTagNames = targetAnimModule.UserTagNames.ToArray();
            }
            else
            {
                userTagNames = System.Enum.GetNames(typeof(EUserTags));
            }
#if UNITY_2019_3_OR_NEWER
            Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 20f);
#else
            Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 18f);
#endif
            bool highlightOn = false;

            if (userTagTracks != null)
            {
                for (int i = 0; i < userTagTracks.Count; ++i)
                {
                    TagTrackBase track = userTagTracks[i];

                    if (highlightOn)
                    {
                        GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                        highlightOn = false;
                    }
                    else
                    {
                        GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                        highlightOn = true;
                    }

                    EditorGUILayout.BeginHorizontal();

                    int tagIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(EUserTags)), (EUserTags)track.TrackId);

                    EditorGUILayout.LabelField(userTagNames[tagIndex - 1], tagTextStyle);

                    GUILayout.FlexibleSpace();

                    Texture icon = EditorGUIUtility.IconContent("Animation.AddKeyFrame").image;

                    Rect btnRect = trackRect;
                    btnRect.x += trackRect.width - icon.width - 17f;
                    btnRect.y += 2f;
                    btnRect.width = icon.width;
                    btnRect.height = icon.height;

                    GUI.DrawTexture(btnRect, icon);

                    GUIStyle invisiButton = new GUIStyle(GUI.skin.label);
                    if (GUI.Button(btnRect, "", invisiButton))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Add tag");
                        track.AddTag(m_curTime);
                    }

                    btnRect.x -= icon.width + 10f;
                    icon = EditorGUIUtility.IconContent("d_P4_DeletedLocal").image;
                    GUI.DrawTexture(btnRect, icon);
                    if (GUI.Button(btnRect, "", invisiButton))
                    {
                        if (EditorUtility.DisplayDialog("Delete Track",
                            "Are you sure you want to delete the track?", "Yes", "Cancel"))
                        {
                            Undo.RecordObject(m_targetMxMAnim as ScriptableObject as ScriptableObject, "Delete Tag Track");
                            userTagTracks.RemoveAt(i);
                            --i;
                            Repaint();
                        }
                    }

                    EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                    trackRect.y += 20f;
#else
                    trackRect.y += 18f;
#endif
                }

                GUILayout.Space(5f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                m_userTagIdToAdd = EditorGUILayout.Popup(m_userTagIdToAdd, userTagNames, GUILayout.Width(150));

                m_userTagToAdd = (EUserTags)(1 << m_userTagIdToAdd);

#if UNITY_2019_3_OR_NEWER
                float createTrackHeight = 18f;
#else
                float createTrackHeight = 18f;
#endif

                if (GUILayout.Button("Create Track", GUILayout.Height(createTrackHeight)))
                {
                    bool addTrack = true;

                    if (m_userTagToAdd == EUserTags.None)
                    {
                        addTrack = false;
                    }
                    else
                    {
                        foreach (TagTrackBase track in userTagTracks)
                        {
                            if (track.TrackId == (int)m_userTagToAdd)
                            {
                                addTrack = false;
                                break;
                            }
                        }
                    }

                    if (addTrack)
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Add User Tag Track");
                        userTagTracks.Add(new TagTrackBase((int)m_userTagToAdd, userTagNames[m_userTagIdToAdd], targetClip.length));
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                trackRect.y += 30f;

                GUILayout.Space(7f);

            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawCurvesData(Rect a_areaRect)
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;
            MxMPreProcessData targetPreProcessData = m_targetMxMAnim.TargetPreProcess;
            AnimationModule targetAnimModule = m_targetMxMAnim.TargetAnimModule;
            List<MxMCurveTrack> curveTracks = m_targetMxMAnim.CurveTracks;

            if (targetClip == null)
                return;

            if (curveTracks == null)
            {
                //m_targetMxMAnim.InitCurveTracks();
                return;
            }

            GUIStyle tagTextStyle = new GUIStyle(GUI.skin.label);
            tagTextStyle.normal.textColor = Color.black;
            tagTextStyle.focused.textColor = Color.black;

#if UNITY_2019_3_OR_NEWER
            Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 40f);
#else
            Rect trackRect = new Rect(0f, 3f, a_areaRect.width, 40f);
#endif
            bool highlightOn = true;
            for(int i = 0; i < curveTracks.Count; ++i)
            {
                MxMCurveTrack curveTrack = curveTracks[i];

                if (curveTrack == null)
                    continue;

                if (highlightOn)
                {
                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                    highlightOn = false;
                }
                else
                {
                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                    highlightOn = true;
                }

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                curveTrack.CurveName = EditorGUILayout.TextField(curveTrack.CurveName, tagTextStyle);
                if(EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                }

                GUILayout.FlexibleSpace();

                GUIStyle invisiButton = new GUIStyle(GUI.skin.label);

                Texture icon = EditorGUIUtility.IconContent("Animation.AddKeyFrame").image;

                Rect btnRect = trackRect;
                btnRect.x += trackRect.width - (icon.width * 2.5f) - 17f;
                btnRect.y += (trackRect.height / 2f) - (icon.height / 2f);
                btnRect.width = icon.width;
                btnRect.height = icon.height;

                GUI.DrawTexture(btnRect, icon);
                if (GUI.Button(btnRect, "", invisiButton))
                {
                    Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Add Curve Keyframe");
                    curveTrack.AnimCurve.AddKey(m_curTime, curveTrack.AnimCurve.Evaluate(m_curTime));

                    Repaint();
                }

                Rect valueRect = btnRect;
                valueRect.x -= 50f;
                valueRect.width = 45f;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(valueRect, curveTrack.AnimCurve.Evaluate(m_curTime).ToString());
                EditorGUI.EndDisabledGroup();

                icon = EditorGUIUtility.IconContent("d_P4_DeletedLocal").image;

                btnRect.x += icon.width * 1.5f;

                GUI.DrawTexture(btnRect, icon);
                if (GUI.Button(btnRect, "", invisiButton))
                {
                    if (EditorUtility.DisplayDialog("Delete Track",
                        "Are you sure you want to delete the track?", "Yes", "Cancel"))
                    {
                        Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Delete Tag Track");
                        curveTracks.RemoveAt(i);
                        --i;
                        Repaint();
                    }
                }

                
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey").image, GUILayout.Width(30f), GUILayout.Height(15f)))
                {
                    float lastKeyFrameTime = 0f;
                    foreach (Keyframe keyFrame in curveTrack.AnimCurve.keys)
                    {
                        if (keyFrame.time + 0.0001f > m_curTime)
                        {
                            m_curTime = lastKeyFrameTime;
                            Repaint();
                            break;
                        }

                        lastKeyFrameTime = keyFrame.time;
                    }
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.NextKey").image, GUILayout.Width(30f), GUILayout.Height(15f)))
                {
                    float chosenTime = -1.0f;
                    foreach (Keyframe keyFrame in curveTrack.AnimCurve.keys)
                    {
                        if (keyFrame.time > m_curTime + Mathf.Epsilon)
                        {
                            chosenTime = m_curTime = keyFrame.time;
                            Repaint();
                            break;
                        }
                    }

                    if (chosenTime < 0f)
                    {
                        m_curTime = m_targetMxMAnim.TargetClip.length;
                    }
                }

                EditorGUILayout.EndHorizontal();

#if UNITY_2019_3_OR_NEWER
                GUILayout.Space(3f);
#else
                GUILayout.Space(3f);
#endif
                trackRect.y += 40f;
            }


            GUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();

          //  m_userTagIdToAdd = EditorGUILayout.Popup(m_userTagIdToAdd, userTagNames, GUILayout.Width(150));

           // m_userTagToAdd = (EUserTags)(1 << m_userTagIdToAdd);

#if UNITY_2019_3_OR_NEWER
            float createCurveHeight = 18f;
#else
            float createCurveHeight = 18f;
#endif

            if (GUILayout.Button("New Curve", GUILayout.Height(createCurveHeight)))
            {
                Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Add Curve Track");
                curveTracks.Add(new MxMCurveTrack("NewCurve", targetClip.length));

                EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Extract", GUILayout.Height(createCurveHeight)))
            {
                if (EditorUtility.DisplayDialog("Extract Clip Curves", "This will extract and add all custom curves from the target animation. Are you sure?", "Yes", "No"))
                {

                    Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Extract Curve Tracks");

                    AnimationClip clip = m_targetMxMAnim.TargetClip;
                    foreach (var binding in AnimationUtility.GetCurveBindings(m_targetMxMAnim.TargetClip))
                    {
                        if (!binding.propertyName.Contains(".") && !binding.propertyName.Contains(" "))
                        {
                            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                            curveTracks.Add(new MxMCurveTrack(binding.propertyName, curve));
                        }
                    }

                    EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                }
            }

            if (GUILayout.Button("Update", GUILayout.Height(createCurveHeight)))
            {
                if (EditorUtility.DisplayDialog("Update Clip Curves", "This will update any existing curves with the values stored on the imported animation clip. Are you sure?", "Yes", "No"))
                {
                    Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Update Curve Tracks");

                    var curveBindings = AnimationUtility.GetCurveBindings(m_targetMxMAnim.TargetClip);

                    for (int i = 0; i < curveTracks.Count; ++i)
                    {
                        MxMCurveTrack curveTrack = curveTracks[i];

                        foreach (var binding in curveBindings)
                        {
                            if (curveTrack.CurveName == binding.propertyName)
                            {
                                AnimationCurve curve = AnimationUtility.GetEditorCurve(TargetMxMAnim.TargetClip, binding);
                                curveTracks[i] = new MxMCurveTrack(binding.propertyName, curve);
                            }
                        }

                    }

                    EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                }
            }

            GUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(9f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle redBtnStyle = new GUIStyle(GUI.skin.button);
            redBtnStyle.normal.textColor = Color.red;

            if(GUILayout.Button("Clear", redBtnStyle, GUILayout.Height(createCurveHeight)))
            {
                if (EditorUtility.DisplayDialog("Delete All Curves", "This will delete all custom curves on this animation. Are you sure?", "Yes", "No"))
                {
                    Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Clear Curve Tracks");
                    curveTracks.Clear();
                    EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
                }
            }

            GUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();

        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawTimeline()
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;

            if (targetClip == null)
                return;

            Rect areaRect = new Rect(m_areaSplitPosition, 0f,
                position.width - m_areaSplitPosition, position.height);

            GUILayout.BeginArea(areaRect);

            m_timelineScrollPos = EditorGUILayout.BeginScrollView(m_timelineScrollPos);
            EditorGUILayout.BeginHorizontal();
            float width = targetClip.length * m_tickSpacing * m_zoom * 10f;

            GUILayout.Space(Mathf.Max(width, areaRect.width));
            EditorGUILayout.EndHorizontal();

#if UNITY_2019_3_OR_NEWER
            Rect trackRect = new Rect(0f, 43f - m_dataScrollPos.y, width, 20f);
#else
            Rect trackRect = new Rect(0f, 39f - m_dataScrollPos.y, width, 18f);
#endif

            bool highlightOn = false;
            m_tagSelected = false;
            m_utilityTagSelected = false;

            switch (m_mode)
            {
                case ETimelineMode.MotionMatching:
                    {
                        List<TagTrack> tagTracks = m_targetMxMAnim.AnimTagTracks;
                        List<TagTrack> favourTagTracks = m_targetMxMAnim.AnimFavourTagTracks;

                        if (tagTracks != null)
                        {

                            foreach (TagTrack track in tagTracks)
                            {
                                if (highlightOn)
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                                    highlightOn = false;
                                }
                                else
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                                    highlightOn = true;
                                }

                                if (DrawTags(track, trackRect, m_zoom))
                                {
                                    if (m_selectTrack != null && m_selectTrack != track)
                                        m_selectTrack.Deselect();

                                    m_tagSelected = true;
                                    m_selectTrack = track;
                                    Repaint();
                                }
#if UNITY_2019_3_OR_NEWER
                                trackRect.y += 20f;
#else
                                trackRect.y += 18f;
#endif
                            }

                            trackRect.y += 30f;
                        }

                        if (favourTagTracks != null)
                        {
                            foreach (TagTrack favourTrack in favourTagTracks)
                            {
                                if (highlightOn)
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                                    highlightOn = false;
                                }
                                else
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                                    highlightOn = true;
                                }

                                if (DrawTags(favourTrack, trackRect, m_zoom))
                                {
                                    if (m_selectTrack != null && m_selectTrack != favourTrack)
                                        m_selectTrack.Deselect();

                                    m_tagSelected = true;
                                    m_selectTrack = favourTrack;
                                    Repaint();
                                }
#if UNITY_2019_3_OR_NEWER
                                trackRect.y += 20f;
#else
                                trackRect.y += 18f;
#endif
                            }
                        }
                    }
                    break;
                case ETimelineMode.Utility:
                    {
                        List<TagTrackBase> tagTracks = m_targetMxMAnim.GenericTagTracks;

                        if (tagTracks != null)
                        {
                            for (int i = 0; i < tagTracks.Count; ++i)
                            {
                                TagTrackBase track = tagTracks[i];

                                if (highlightOn)
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                                    highlightOn = false;
                                }
                                else
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                                    highlightOn = true;
                                }

                                if (DrawTagTrackBase(track, trackRect, m_zoom))
                                {
                                    if (m_selectUtilityTrack != null && m_selectUtilityTrack != track)
                                        m_selectUtilityTrack.Deselect();


                                    m_utilityTagSelected = true;
                                    m_selectUtilityTrack = track;
                                    Repaint();
                                }
#if UNITY_2019_3_OR_NEWER
                                trackRect.y += 20f;
#else
                                trackRect.y += 18f;
#endif
                            }
                        }
                    }
                    break;
                case ETimelineMode.User:
                    {
                        List<TagTrackBase> userTagTracks = m_targetMxMAnim.UserTagTracks;

                        if (userTagTracks != null)
                        {
                            for (int i = 0; i < userTagTracks.Count; ++i)
                            {
                                TagTrackBase userTrack = userTagTracks[i];

                                if (highlightOn)
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexActive());
                                    highlightOn = false;
                                }
                                else
                                {
                                    GUI.DrawTexture(trackRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());
                                    highlightOn = true;
                                }

                                if (DrawTagTrackBase(userTrack, trackRect, m_zoom))
                                {
                                    if (m_selectUtilityTrack != null && m_selectUtilityTrack != userTrack)
                                        m_selectUtilityTrack.Deselect();

                                    m_utilityTagSelected = true;
                                    m_selectUtilityTrack = userTrack;
                                    Repaint();
                                }
#if UNITY_2019_3_OR_NEWER
                                trackRect.y += 20f;
#else
                                trackRect.y += 18f;
#endif
                            }
                        }
                    }
                    break;
                case ETimelineMode.Curves:
                    {
                        List<MxMCurveTrack> curveTracks = m_targetMxMAnim.CurveTracks;

                        trackRect.height = 40f;


                        if (curveTracks != null)
                        {
                            foreach (MxMCurveTrack curveTrack in curveTracks)
                            {
                                //Draw the curve
                                float maxTime = curveTrack.AnimCurve.keys[curveTrack.AnimCurve.keys.Length - 1].time;
                                Rect curveRect = trackRect;
                                curveRect.width = trackRect.width * (maxTime / targetClip.length);

                                EditorGUI.CurveField(curveRect, curveTrack.AnimCurve);
#if UNITY_2019_3_OR_NEWER
                                trackRect.y += 40f;
#else
                                trackRect.y += 40f;
#endif 
                            }
                        }
                    } break;
            }
#if UNITY_2019_3_OR_NEWER
            Rect timelineRect = new Rect(0f, 0f, areaRect.width + 1, 20f);
#else
            Rect timelineRect = new Rect(0f, 0f, areaRect.width + 1, 18f);
#endif
            GUI.DrawTexture(timelineRect, EditorUtil.EditorFunctions.GetTimelineTexInActive());

            timelineRect.width = targetClip.length * m_tickSpacing * m_zoom * 10f;

            GUI.DrawTexture(timelineRect, EditorUtil.EditorFunctions.GetTimelineTexActive());

            Handles.BeginGUI();
            Handles.color = Color.black;
            var timelineEndPos = timelineRect.x + timelineRect.width;
            Handles.DrawLine(new Vector3(timelineRect.x, timelineRect.height),
                new Vector3(timelineEndPos, timelineRect.height));
            Handles.color = Color.grey;
            Handles.DrawLine(new Vector3(timelineEndPos, 0f),
                new Vector3(timelineEndPos, areaRect.height));
            Handles.EndGUI();

            m_eventSelected = false;
            m_sectionSelected = false;

#if UNITY_2019_3_OR_NEWER
            Rect evtBarRect = new Rect(0f, 20f, timelineRect.width, 20f);
#else
            Rect evtBarRect = new Rect(0f, 18f, timelineRect.width, 18f);
#endif

            GUI.DrawTexture(evtBarRect, EditorUtil.EditorFunctions.GetTimelineTexEvent());



            List<EventMarker> events = m_targetMxMAnim.EventMarkers;


            if (events != null)
            {
                foreach (EventMarker evtMarker in events)
                {
                    if (evtMarker != null)
                    {
                        DrawEvent(evtMarker, evtBarRect, m_zoom, m_targetMxMAnim);
                    }
                }

//                 switch (m_mode)
//                 {
//                     /* case ETimelineMode.MotionMatching:
//                          {
//                          }
//                          break;*/
//                     case ETimelineMode.Timing:
//                         {
//                             DrawTimingTimeline(areaRect, evtBarRect);
//                         }
//                         break;
//                         /* case ETimelineMode.Utility:
//                              {
//                                  //DrawGenericTimeline(areaRect, evtBarRect);
//                              }
//                              break;
//                          case ETimelineMode.User:
//                              {
//
//                              }
//                              break;*/
//                 }
            }

            //EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));

            GUIStyle timeStampStyle = new GUIStyle(EditorStyles.label);
            timeStampStyle.fontSize = 10;
            timeStampStyle.normal.textColor = Color.black;

            Handles.color = Color.black;
            var tickRythem = 10;
            var timeIter = 0;
            var span = targetClip.length * m_tickSpacing * m_zoom * 10f;

            float spaceSinceLastStamp = 25f;
            float spaceSinceLastHash = 2f;

            for (float posX = 0; posX < span; posX += m_tickSpacing * m_zoom)
            {
                Handles.BeginGUI();

                if (tickRythem == 10)
                {
                    Handles.DrawLine(new Vector3(posX, 18f, 0f),
                    new Vector3(posX, 8f, 0f));
                    tickRythem = 1;

                    if (spaceSinceLastStamp >= 25f)
                    {
                        EditorGUI.LabelField(new Rect(posX, 0f, 45f, 15f), timeIter.ToString() + ":00", timeStampStyle);
                        spaceSinceLastStamp = 0f;

                    }

                    spaceSinceLastHash = 0f;

                    ++timeIter;
                }
                else
                {
                    spaceSinceLastHash += m_tickSpacing * m_zoom;
                    spaceSinceLastStamp += m_tickSpacing * m_zoom;

                    if (spaceSinceLastHash >= 2f)
                    {
                        Handles.DrawLine(new Vector3(posX, 18f, 0f), new Vector3(posX, 13f, 0f));
                        spaceSinceLastHash = 0f;
                    }

                    ++tickRythem;
                }

                Handles.EndGUI();
            }

            //Draw the time tracking bar
            Handles.BeginGUI();
            Handles.color = Color.white;
            Handles.DrawLine(new Vector3(m_curTime * m_tickSpacing * m_zoom * 10f, 0f, 0f),
                new Vector3(m_curTime * m_tickSpacing * m_zoom * 10f, areaRect.height, 0f));
            Handles.EndGUI();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawMotionMatchingTimeline(Rect a_areaRect)
        {

        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawTimingTimeline(Rect a_areaRect, Rect a_evtBarRect)
        {
            var motionModifyData = m_targetMxMAnim.AnimMotionModifier;

            if (motionModifyData != null)
            {
                DrawSpeedModSections(motionModifyData, ref a_evtBarRect,
                    ref a_areaRect, m_zoom);
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DrawGenericTimeline(Rect a_areaRect, Rect a_evtBarRect)
        {

        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnLostFocus()
        {
            m_draggingSplit = false;
            m_draggingTimeLine = false;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void ManageInputs(Event evt)
        {
            if (m_targetMxMAnim == null)
                return;

            Rect splitHandleRect = new Rect(m_areaSplitPosition - 3f, 0f,
               6f, position.height);

            Rect timelineRect = new Rect(m_areaSplitPosition, 0f,
                position.width - m_areaSplitPosition, 18f);

            switch (evt.type)
            {
                case EventType.MouseDown:
                    {
                        if (evt.button == 0)
                        {
                            if (timelineRect.Contains(evt.mousePosition))
                            {
                                m_draggingTimeLine = true;
                                m_draggingSplit = false;

                                m_curTime = (m_timelineScrollPos.x + evt.mousePosition.x -
                                    m_areaSplitPosition) / (m_tickSpacing * m_zoom * 10f);

                                if (m_curTime < 0f)
                                    m_curTime = 0f;

                                Repaint();
                            }
                            else if (splitHandleRect.Contains(evt.mousePosition))
                            {
                                m_draggingTimeLine = false;
                                m_draggingSplit = true;

                                Repaint();
                            }

                            Rect tagAreaRect = timelineRect;
                            tagAreaRect.height = position.height - 18f;
                            tagAreaRect.y += 18f;


                            if (!m_tagSelected && m_selectTrack != null)
                            {
                                if (tagAreaRect.Contains(evt.mousePosition))
                                {
                                    m_selectTrack.Deselect();
                                    m_selectTrack = null;
                                    Repaint();
                                }
                            }

                            if (!m_utilityTagSelected && m_selectUtilityTrack != null)
                            {
                                if (tagAreaRect.Contains(evt.mousePosition))
                                {
                                    m_selectUtilityTrack.Deselect();
                                    m_selectUtilityTrack = null;
                                    Repaint();
                                }
                            }

                            if (!m_eventSelected && m_selectEvent != null)
                            {
                                if (tagAreaRect.Contains(evt.mousePosition))
                                {
                                    QueueDeselectEventAction();
                                }
                            }

                            if (!m_sectionSelected && m_selectMotionSection != null)
                            {
                                if (tagAreaRect.Contains(evt.mousePosition))
                                {
                                    m_selectMotionSection.Deselect();
                                    m_selectMotionSection = null;
                                    Repaint();
                                }

                            }

                        }
                    }
                    break;
                case EventType.MouseUp:
                    {
                        if (evt.button == 0)
                        {
                            m_draggingSplit = false;
                            m_draggingTimeLine = false;
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        if (evt.button == 0)
                        {
                            if (m_draggingTimeLine)
                            {
                                m_curTime = (m_timelineScrollPos.x + evt.mousePosition.x -
                                    m_areaSplitPosition) / (m_tickSpacing * m_zoom * 10f);

                                if (m_curTime < 0f)
                                    m_curTime = 0f;

                                Repaint();
                            }
                            else if (m_draggingSplit)
                            {
                                m_areaSplitPosition += evt.delta.x;
                                m_areaSplitPosition = Mathf.Clamp(m_areaSplitPosition,
                                    50f, position.width - 50f);

                                Repaint();
                            }
                        }
                    }
                    break;
                case EventType.ScrollWheel:
                    {
                        Rect scrollTimelineRect = timelineRect;
                        scrollTimelineRect.height = position.height;

                        if (scrollTimelineRect.Contains(evt.mousePosition))
                        {
                            float zoomDelta = -evt.delta.y * (0.1f * m_zoom);

                            m_zoom += zoomDelta;
                            m_zoom = Mathf.Clamp(m_zoom, 0.0001f, float.MaxValue);

                            Repaint();
                        }
                    }
                    break;
            }

            if (evt.isKey && evt.type == EventType.KeyDown)
            {
                switch (m_mode)
                {
                    case ETimelineMode.MotionMatching:
                        {
                            switch (evt.keyCode)
                            {
                                case KeyCode.Delete:
                                    {
                                        if (m_selectTrack != null)
                                        {

                                            if (m_selectTrack.SelectId >= 0)
                                                Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Delete Tag");

                                            m_selectTrack.DeleteSelectedTag();
                                            m_selectTrack = null;

                                            Repaint();
                                        }

                                        if (m_selectEvent != null)
                                        {
                                            var events = m_targetMxMAnim.EventMarkers;

                                            if (events != null)
                                            {
                                                Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Delete Event");
                                                events.Remove(m_selectEvent);
                                                QueueDeselectEventAction();

                                                Repaint();
                                            }
                                        }
                                    }
                                    break;
                                case KeyCode.E:
                                    {
                                        if (m_mode == ETimelineMode.MotionMatching)
                                        {
                                            m_targetMxMAnim.AddEvent(m_curTime);
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    // case ETimelineMode.Timing:
                    //     {
                    //         var motionModifyData = m_targetMxMAnim.AnimMotionModifier;
                    //         switch (evt.keyCode)
                    //         {
                    //             case KeyCode.Delete:
                    //                 {
                    //                     if (m_selectMotionSection != null && motionModifyData != null)
                    //                     {
                    //                         Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Delete Motion Section");
                    //                         motionModifyData.MotionSections.Remove(m_selectMotionSection);
                    //                         Repaint();
                    //                     }
                    //                 }
                    //                 break;
                    //
                    //             case KeyCode.S:
                    //                 {
                    //                     if (motionModifyData != null)
                    //                     {
                    //                         QueueAddPOIAction(m_curTime);
                    //                     }
                    //                 }
                    //                 break;
                    //         }
                    //     }
                    //     break;
                    case ETimelineMode.Utility:
                    case ETimelineMode.User:
                        {
                            switch (evt.keyCode)
                            {
                                case KeyCode.Delete:
                                    {
                                        if (m_selectUtilityTrack != null)
                                        {
                                            if (m_selectUtilityTrack.SelectId >= 0)
                                                Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Delete Generic Tag");

                                            m_selectUtilityTrack.DeleteSelectedTag();
                                            m_selectUtilityTrack = null;

                                            Repaint();
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void MarkAllFootstepsWithDefaults()
        {
            if (m_targetMxMAnim == null)
                return;

            List<TagTrackBase> tagTracks = m_targetMxMAnim.GenericTagTracks;

            if (tagTracks == null || tagTracks.Count < 2)
                return;

            (tagTracks[0] as FootStepTagTrack).SetStepDataToAll(m_defaultStepPace, m_defaultStepType);
            (tagTracks[1] as FootStepTagTrack).SetStepDataToAll(m_defaultStepPace, m_defaultStepType);
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void HandleLayoutChanges()
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (m_targetMxMAnim != null)
                {
                    if (m_deselectQueued)
                    {
                        if (m_selectEvent != null)
                        {
                            m_selectEvent.Deselect();
                            m_selectEvent = null;
                            Repaint();
                        }

                        m_deselectQueued = false;
                    }

                    if (m_addPOIQueued)
                    {
                        MotionModifyData motionModifyData = m_targetMxMAnim.AnimMotionModifier;

                        if (motionModifyData != null)
                        {
                            motionModifyData.AddPOI(m_addPOIQueueTime);
                            Repaint();
                        }

                        m_addPOIQueueTime = 0f;
                        m_addPOIQueued = false;
                    }
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void ClampTime()
        {
            if (m_targetMxMAnim != null && m_targetMxMAnim.TargetClip != null)
            {
                AnimationClip targetClip = m_targetMxMAnim.TargetClip;

                if (m_curTime > targetClip.length)
                    m_playing = false;

                m_curTime = Mathf.Clamp(m_curTime, 0f, targetClip.length);

            }
            else
            {
                if (m_curTime > (position.width - m_areaSplitPosition) / (m_tickSpacing * m_zoom * 10f))
                    m_playing = false;

                m_curTime = Mathf.Clamp(m_curTime, 0f,
                    (position.width - m_areaSplitPosition) / (m_tickSpacing * m_zoom * 10f));
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void LoopTime()
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;

            if (targetClip != null)
            {
                if (m_curTime > targetClip.length)
                    m_curTime -= targetClip.length;
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void SetTarget(IMxMAnim a_targetMxMAnim)
        {
            if (a_targetMxMAnim != null && a_targetMxMAnim != m_targetMxMAnim)
            {
                m_selectEvent = null;
                m_selectTrack = null;
                m_selectUtilityTrack = null;
                m_selectMotionSection = null;
                m_tagSelected = false;
                m_utilityTagSelected = false;
                m_sectionSelected = false;
                m_eventSelected = false;
                m_targetMxMAnim = a_targetMxMAnim;
                
                UpdateAutoZoom();
                m_targetMxMAnim.VerifyData();
                
                SelectFirstEventIfAvailable();

                Repaint();
            }
        }
        
        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void BeginPreview()
        {
            if (m_targetMxMAnim == null || m_targetMxMAnim.TargetModel == null)
            {
                Debug.LogError("MxMTimeline: Cannot begin preview because the target model cannot" +
                               "be found. Please check your configuration module is set.");
                m_previewActive = false;
                return;
            }

            if (MxMPreviewScene.BeginPreview(this))
            {
                MxMPreviewScene.SetPreviewObject(m_targetMxMAnim.TargetModel);
                Selection.objects = new Object[] { m_targetMxMAnim.TargetPreProcess };
                m_previewActive = true;
                UpdatePreview();
            }
            else
            {
                m_previewActive = false;
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void EndPreview()
        {
            MxMPreviewScene.EndPreview();
            m_previewActive = false;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void EndPreviewLocal()
        {
            if (!m_previewActive)
                return;

            AnimationMixerPlayable mixer = MxMPreviewScene.Mixer;

            if (mixer.IsValid())
            {
                var clipPlayable = mixer.GetInput(0);

                if (clipPlayable.IsValid())
                {
                    mixer.DisconnectInput(0);
                    clipPlayable.Destroy();
                }
            }

            m_previewActive = false;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void UpdatePreview()
        {
            if (!MxMPreviewScene.IsSceneLoaded)
                return;

            if (m_targetMxMAnim == null || m_targetMxMAnim.TargetClip == null)
            {
                EndPreview();
                return;
            }

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;

            float previewPosY = MxMPreviewScene.PreviewObject.transform.position.y;
            float groundPosY = MxMPreviewScene.Ground.position.y;

            if (previewPosY < 0f)
            {
                if (groundPosY > previewPosY || Mathf.Abs(groundPosY - previewPosY) > 0.5f)
                {
                    MxMPreviewScene.Ground.position = new Vector3(0f, previewPosY);
                }
            }
            else
            {
                MxMPreviewScene.Ground.position = Vector3.zero;
            }

            AnimationMixerPlayable mixer = MxMPreviewScene.Mixer;
            var clipPlayable = mixer.GetInput(0);

            if (m_previewClip != targetClip)
            {
                if (!clipPlayable.IsNull())
                {
                    mixer.DisconnectInput(0);
                    clipPlayable.Destroy();
                }
                m_previewClip = targetClip;
            }

            PlayableGraph playableGraph = MxMPreviewScene.PlayableGraph;

            if (!clipPlayable.IsValid())
            {
                clipPlayable = AnimationClipPlayable.Create(playableGraph, m_previewClip);
                ((AnimationClipPlayable)clipPlayable).SetApplyFootIK(true);
                playableGraph.Connect(clipPlayable, 0, mixer, 0);
            }

            clipPlayable.SetTime(m_curTime);
            playableGraph.Evaluate(0f);

            var rootData = m_targetMxMAnim.GetRoot(m_curTime);

            MxMPreviewScene.PreviewObject.transform.SetPositionAndRotation(rootData.pos, rootData.rot);

            SceneView.RepaintAll();

        }

        //===========================================================================================
        /**
        *  @brief Notifies the tagging window when the tags have been modified
        *         
        *********************************************************************************************/
        public void Modified(float _time)
        {
            m_playing = false;
            m_curTime = _time;
            EditorUtility.SetDirty(m_targetMxMAnim as ScriptableObject);
            Repaint();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void SetTime(float _time)
        {
            m_playing = false;
            m_curTime = _time;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void DeselectAllTags()
        {
            if (m_selectTrack != null)
                m_selectTrack.Deselect();

            m_selectTrack = null;

            if (m_selectUtilityTrack != null)
                m_selectUtilityTrack.Deselect();

            m_selectUtilityTrack = null;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void DeselectAllEvents()
        {
            if (m_selectEvent != null)
            {
                m_selectEvent.Deselect();
                m_selectEvent = null;
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void DeselectAllMotionSections()
        {
            if (m_selectMotionSection != null)
            {
                m_selectMotionSection.Deselect();
                m_selectMotionSection = null;
            }
        }

        //===========================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SelectEvent(EventMarker _event, float _time)
        {
            if (m_selectEvent != _event)
                DeselectAllEvents();

            m_selectEvent = _event;
            m_eventSelected = true;
            m_selectEvent.Selected = true;

            InitializeGenericRigData();

            m_curTime = _time;
            Repaint();
        }
        
        //===========================================================================================
        /**
        *  @brief If an event exists in the animation, this function will automatically select it. If
        * there are multiple it will simply select the first one. This is called when new animation
        * data is set as most of the time the user will select an event when changing animations
        *         
        *********************************************************************************************/
        private void SelectFirstEventIfAvailable()
        {
            if ((m_targetMxMAnim == null)
                || (m_targetMxMAnim.EventMarkers == null)
                || (m_targetMxMAnim.EventMarkers.Count == 0))
            {
                return;
            }

            EventMarker eventMarker = m_targetMxMAnim.EventMarkers[0];
            if (eventMarker != null)
            {
                SelectEvent(eventMarker, eventMarker.EventTime);
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void SelectSection(MotionSection a_section, float a_time)
        {
            if (m_selectMotionSection != a_section)
                DeselectAllMotionSections();

            m_selectMotionSection = a_section;
            m_sectionSelected = true;

            m_curTime = a_time;
            Repaint();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void UpdateAutoZoom()
        {
            if (m_targetMxMAnim == null)
                return;

            AnimationClip targetClip = m_targetMxMAnim.TargetClip;

            if (m_autoZoom && targetClip != null)
            {
                float ratio = (targetClip.length * m_tickSpacing * 10f) / (position.width - m_areaSplitPosition);

                if (ratio < 0.5f)
                {
                    m_zoom = Mathf.Min(1f / (ratio * 1.1f), 5f);
                }
                else if (ratio > 1f)
                {
                    m_zoom = Mathf.Max(0.25f, 1f / ratio);
                }
                else
                {
                    m_zoom = 1f;
                }
            }
        }

        //===========================================================================================
        /**
        *  @briefS
        *         
        *********************************************************************************************/
        private void OnDestroy()
        {
#if UNITY_2018
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

#else
            SceneView.duringSceneGui -= this.OnSceneGUI;
#endif

            m_previewActive = false;
            EndPreview();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnEnable()
        {
            if (m_jointNames == null)
                m_jointNames = new List<string>();

            m_jointNames.Clear();
            InitializeGenericRigData();

            m_selectEvent = null;
            m_selectMotionSection = null;

#if UNITY_2018
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#else
            
            SceneView.duringSceneGui += this.OnSceneGUI;
#endif

            Undo.undoRedoPerformed += UndoCallback;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnDisable()
        {
#if UNITY_2018
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#else
            SceneView.duringSceneGui -= this.OnSceneGUI;
#endif
            m_previewActive = false;
            EndPreview();

            Undo.undoRedoPerformed -= UndoCallback;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnFocus()
        {
#if UNITY_2018
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#else
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void InitializeGenericRigData()
        {
            if (m_targetMxMAnim == null)
                return;

            var targetPrefab = m_targetMxMAnim.TargetModel;

            if (m_targetMxMAnim != null && targetPrefab != null && m_targetMxMAnim.TargetPreProcess != null)
            {
                if (m_jointNames == null)
                    m_jointNames = new List<string>();

                m_jointNames.Clear();

                List<string> tempJointNames = null;
                if (m_targetMxMAnim.TargetPreProcess.GetBonesByName)
                {
                    tempJointNames = GetAllChildPaths(targetPrefab.transform, "");

                    for (int i = 0; i < tempJointNames.Count; ++i)
                    {
                        m_jointNames.Add(tempJointNames[i].Remove(0, 1));
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private List<string> GetAllChildPaths(Transform a_transform, string a_startPath)
        {
            List<string> childPathList = new List<string>();
            List<Transform> childTFormList = new List<Transform>();

            if (a_transform != null)
            {
                int childCount = a_transform.childCount;

                for (int i = 0; i < childCount; ++i)
                {
                    Transform child = a_transform.GetChild(i);

                    childPathList.Add(a_startPath + "/" + child.name);
                    childTFormList.Add(child);
                }

                for (int i = 0; i < childTFormList.Count; ++i)
                {
                    childPathList.AddRange(GetAllChildPaths(childTFormList[i], childPathList[i]));
                }
            }

            return childPathList;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void QueueDeselectEventAction()
        {
            m_deselectQueued = true;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void QueueAddPOIAction(float a_curTime)
        {
            m_addPOIQueued = true;
            m_addPOIQueueTime = a_curTime;
        }

        public void DrawEvent(EventMarker a_eventMarker, Rect _trackRect, float _zoom, IMxMAnim a_mxmAnim)
        {
            Texture markerIcon = EditorGUIUtility.IconContent("Animation.EventMarker").image;

            if (a_eventMarker.Selected)
            {
                Handles.BeginGUI();
                Handles.color = Color.green;

                Vector3 start = new Vector3(a_eventMarker.EventTime * 75f * _zoom - (markerIcon.width / 2f),
                    _trackRect.y + markerIcon.height / 2f, 0f);
                Vector3 end = new Vector3(start.x - a_eventMarker.Actions[0] * 75f * _zoom + markerIcon.width / 2f, start.y, 0f);
                Vector3 up = new Vector3(end.x, end.y - markerIcon.height / 4f);
                Vector3 down = new Vector3(up.x, up.y + markerIcon.height / 2f);

                Handles.DrawLine(start, end);
                Handles.DrawLine(up, down);

                start = end;
                end = new Vector3(start.x - a_eventMarker.Windup * 75f * _zoom, start.y, 0f);
                up.x = down.x = end.x;

                Handles.color = Color.blue;
                Handles.DrawLine(start, end);
                Handles.DrawLine(up, down);

                start = new Vector3(a_eventMarker.EventTime * 75f * _zoom + (markerIcon.width / 2f),
                    _trackRect.y + markerIcon.height / 2f, 0f);

                Handles.color = Color.green;
                for (int i = 1; i < a_eventMarker.Actions.Count; ++i)
                {
                    float markerWidth = 0;

                    if (i == 1)
                        markerWidth = markerIcon.width / 2f;

                    end = new Vector3(start.x + a_eventMarker.Actions[i] * 75f * _zoom - markerWidth, start.y, 0f);
                    up.x = down.x = end.x;

                    Handles.DrawLine(start, end);
                    Handles.DrawLine(up, down);

                    //Draw a circle maybe

                    start = end;
                }

                end = new Vector3(start.x + a_eventMarker.FollowThrough * 75f * _zoom - markerIcon.width / 2f, start.y, 0f);
                up.x = down.x = end.x;

                Handles.color = Color.red;
                Handles.DrawLine(start, end);
                Handles.DrawLine(up, down);

                start = end;
                end = new Vector3(start.x + a_eventMarker.Recovery * 75f * _zoom, start.y, 0f);
                up.x = down.x = end.x;

                Handles.color = Color.yellow;
                Handles.DrawLine(start, end);
                Handles.DrawLine(up, down);

                Handles.EndGUI();
            }

            Rect markerRect = new Rect(a_eventMarker.EventTime * 75f * _zoom - (markerIcon.width / 2f), _trackRect.y,
                markerIcon.width, markerIcon.height);
            GUI.DrawTexture(markerRect, markerIcon);

            if (a_eventMarker.Selected)
                GUI.DrawTexture(markerRect, EditorUtil.EditorFunctions.GetHighlightTex());

            Event evt = Event.current;

            markerRect.x -= 3f;
            markerRect.height = _trackRect.height;
            markerRect.width += 6f;

            if (evt.isMouse)
            {
                if (evt.button == 0)
                {
                    switch (evt.type)
                    {
                        case EventType.MouseDown:
                            {
                                if (markerRect.Contains(evt.mousePosition))
                                {
                                    a_eventMarker.Selected = true;
                                    a_eventMarker.Dragging = true;
                                    SelectEvent(a_eventMarker, a_eventMarker.EventTime);
                                    evt.Use();
                                }

                            }
                            break;
                        case EventType.MouseUp:
                            {
                                a_eventMarker.Dragging = false;

                            }
                            break;
                        case EventType.MouseDrag:
                            {
                                if (a_eventMarker.Dragging && a_eventMarker.Selected)
                                {
                                    float desiredValueDelta = ((evt.delta.x / _zoom)) / 75f;

                                    a_eventMarker.EventTime += desiredValueDelta;
                                    a_eventMarker.EventTime = Mathf.Clamp(a_eventMarker.EventTime, 0f, TargetClip.length);

                                    Modified(a_eventMarker.EventTime);

                                    evt.Use();
                                }
                            }
                            break;
                    }
                }
                else if (evt.button == 1)
                {
                    if (markerRect.Contains(evt.mousePosition))
                    {
                        //Begin Context menu
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Delete"), false, a_mxmAnim.OnDeleteEventMarker, a_eventMarker);
                        menu.ShowAsContext();
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void DrawSpeedModSections(MotionModifyData a_speedModData, ref Rect a_trackRect, ref Rect a_timelineRect, float a_zoom)
        {
            Texture markerIcon = EditorGUIUtility.IconContent("Animation.EventMarker").image;

            Rect markerRect;

            Event evt = Event.current;

            float lastTime = 0f;

            MotionTimingPresets motionTimingPresets = null;

            var targetPreProcess = m_targetMxMAnim.TargetPreProcess;

            if (TargetMxMAnim != null && targetPreProcess != null)
            {
                motionTimingPresets = targetPreProcess.MotionTimingPresets;
            }

            string[] defenitionNames = null;
            if (motionTimingPresets != null)
            {
                defenitionNames = motionTimingPresets.GetDefenitionNames();
            }

            float maxSpeedVariance = 0.5f;

            //Determine max variance
            for (int i = 0; i < a_speedModData.MotionSections.Count; ++i)
            {
                MotionSection curSection = a_speedModData.MotionSections[i];

                float speedMod = 1f;
                speedMod = curSection.GetSpeedMod(lastTime, motionTimingPresets, m_targetMxMAnim);

                float speedVariance = 0f;
                if (speedMod > 1f)
                    speedVariance = (speedMod / 1f) - 1f;
                else if (speedMod < 1f)
                    speedVariance = (1f / speedMod) - 1f;

                if (speedVariance > maxSpeedVariance)
                    maxSpeedVariance = speedVariance;

                lastTime = curSection.EndTime;
            }

            lastTime = 0f;
            for (int i = 0; i < a_speedModData.MotionSections.Count; ++i)
            {
                MotionSection curSection = a_speedModData.MotionSections[i];

                markerRect = new Rect(curSection.EndTime * 75f * a_zoom - (markerIcon.width / 2f), a_trackRect.y,
                     markerIcon.width, markerIcon.height);

                Vector3 start = new Vector3(markerRect.x + markerRect.width / 2f,
                                                markerRect.y + markerRect.height);
                Vector3 end = new Vector3(start.x, a_timelineRect.height);

                if (i != a_speedModData.MotionSections.Count - 1)
                {
                    GUI.DrawTexture(markerRect, markerIcon);

                    Handles.color = Color.black;
                    Handles.DrawLine(start, end);

                    if (curSection.Selected)
                        GUI.DrawTexture(markerRect, EditorUtil.EditorFunctions.GetHighlightTex());


                    markerRect.x -= 3f;
                    markerRect.height = a_trackRect.height;
                    markerRect.width += 6f;

                    if (evt.isMouse && evt.button == 0)
                    {
                        switch (evt.type)
                        {
                            case EventType.MouseDown:
                                {
                                    if (markerRect.Contains(evt.mousePosition))
                                    {
                                        curSection.Selected = true;
                                        curSection.Dragging = true;
                                        SelectSection(curSection, curSection.EndTime);
                                    }

                                }
                                break;
                            case EventType.MouseUp:
                                {
                                    curSection.Dragging = false;
                                }
                                break;
                            case EventType.MouseDrag:
                                {
                                    if (curSection.Dragging && curSection.Selected)
                                    {
                                        float desiredValueDelta = ((evt.delta.x / a_zoom)) / 75f;

                                        curSection.EndTime += desiredValueDelta;

                                        curSection.EndTime = Mathf.Clamp(curSection.EndTime, 0f, TargetClip.length);
                                        Modified(curSection.EndTime);
                                    }
                                }
                                break;
                        }
                    }
                }


                float speedMod = 1f;
                speedMod = curSection.GetSpeedMod(lastTime, motionTimingPresets, m_targetMxMAnim);

                float speedVariance = 0f;
                if (speedMod > 1f)
                {
                    speedVariance = (speedMod / 1f) - 1f;
                }
                else if (speedMod < 1f)
                {
                    speedVariance = (1f / speedMod) - 1f;
                }

                float heightRatio = speedVariance / maxSpeedVariance;

                if (speedMod < 1f)
                    heightRatio *= -1;

                if (maxSpeedVariance < 0.001f)
                    heightRatio = 0f;

                //Draw Baseline
                end = new Vector3(curSection.EndTime * 75f * a_zoom, a_timelineRect.height / 2f + 9f);
                start = new Vector3(lastTime * 75f * a_zoom, end.y);

                Handles.color = new Color(0f, 0f, 0f, 0.5f);
                Handles.DrawLine(start, end);

                //Draw limit line
                Handles.color = new Color(0f, 0f, 0f, 0.3f);
                if (speedMod > 1f + Mathf.Epsilon)
                {
                    end.y = a_timelineRect.height / 2f + (a_timelineRect.height / 2f - 44f) + 9f;
                    start.y = end.y;

                    Handles.DrawLine(start, end);
                }
                else
                {

                    //Draw limit line
                    end.y = a_timelineRect.height / 2f - (a_timelineRect.height / 2f - 44f) + 9f;
                    start.y = end.y;

                    Handles.DrawLine(start, end);
                }

                //Draw Green Line
                end.y = a_timelineRect.height / 2f + heightRatio * (a_timelineRect.height / 2f - 44f) + 9f;
                start.y = end.y;

                Handles.color = Color.green;
                Handles.DrawLine(start, end);

                float invertedSpeedMod = 1f / speedMod;
                string speedString = invertedSpeedMod.ToString("F2");

                speedString += "x";

                float width = GUI.skin.label.CalcSize(new GUIContent(speedString)).x;
                GUI.Label(new Rect(start.x + (end.x - start.x) / 2f - width / 2f, end.y - 18f, width, 18f), speedString);


                Rect dataRect = new Rect(lastTime * 75f * a_zoom + 4, a_timelineRect.height / 1.75f,
                    end.x - start.x - 8, a_timelineRect.height - (a_timelineRect.height / 1.75f) - 18f);

                if (speedMod > 1f + Mathf.Epsilon)
                {
                    dataRect.y = 40f;
                }

                //Draw Drop Down Boxes

                GUI.Box(dataRect, "Section " + i);

                dataRect.x += 3f;
                dataRect.width -= 6f;

                GUILayout.BeginArea(dataRect);
                GUILayout.Space(18f);

                if (motionTimingPresets != null)
                {
                    curSection.UsePresets = GUILayout.Toggle(curSection.UsePresets,
                        new GUIContent("Use Preset"));
                }

                if (curSection.UsePresets && motionTimingPresets != null)
                {
                    curSection.MotionPresetId = EditorGUILayout.Popup(curSection.MotionPresetId,
                        defenitionNames);
                }
                else
                {
                    float defaultLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 40f;
                    curSection.ModType = (EMotionModType)EditorGUILayout.EnumPopup(new GUIContent("Type"), curSection.ModType);
                    curSection.RawModValue = EditorGUILayout.FloatField(new GUIContent("Value"), curSection.RawModValue);


                    if (curSection.RawModValue < 0.01f)
                        curSection.RawModValue = 0.01f;


                    EditorGUIUtility.labelWidth = defaultLabelWidth;
                }

                GUILayout.FlexibleSpace();

                float originalDurationF = (curSection.EndTime - lastTime);
                float finalDurationF = ((curSection.EndTime - lastTime) * speedMod);

                string originalDuration = originalDurationF.ToString("F2");
                string finalDuration = finalDurationF.ToString("F2");

                float originalSpeedF = m_targetMxMAnim.GetAverageRootSpeed(lastTime, curSection.EndTime);
                float finalSpeedF = originalSpeedF * (originalDurationF / finalDurationF);

                string originalSpeed = originalSpeedF.ToString("F2");
                string finalSpeed = finalSpeedF.ToString("F2");



                EditorGUILayout.LabelField("Original: " + originalDuration + " sec | " + originalSpeed + "m/s");
                EditorGUILayout.LabelField("Final: " + finalDuration + " sec | " + finalSpeed + "m/s");

                GUILayout.EndArea();

                lastTime = curSection.EndTime;
            }
        }

        //============================================================================================
        /**
        *  @brief Draws and manages all the tags within the track along a GUI track in the editor
        *  
        *  @param [Rect] _trackRect - the rect of the track to draw them on
        *  @param [float] _scrollValueX - the scroll value of the scroll viewEditorUtil.EditorFunctions.GetHighlightTex()
        *  @param [float] _zoom - tyhe zoom value of the 
        *         
        *********************************************************************************************/
        public bool DrawTags(TagTrack a_tagTrack, Rect _trackRect, float _zoom)
        {
            bool ret = false;

            Texture markerIcon = EditorGUIUtility.IconContent("blendKey").image;
            Texture selectIcon = EditorGUIUtility.IconContent("curvekeyframe").image;

            Event evt = Event.current;

            List<Vector2> tags = a_tagTrack.Tags;

            for (int i = 0; i < tags.Count; ++i)
            {
                Vector2 tag = tags[i];

                float startX = (tag.x * 75f) * _zoom;
                float endX = (tag.y * 75f) * _zoom;

                Color baseColor = GUI.color;

                if (EditorGUIUtility.isProSkin)
                    GUI.color = Color.black;

                Rect barRect = new Rect(startX, _trackRect.y + 7f, endX - startX, 5f);
                GUI.Box(barRect, "");
                GUI.color = baseColor;

                if (a_tagTrack.SelectId == i && a_tagTrack.SelectType == TagSelectType.All)
                    GUI.DrawTexture(barRect, EditorUtil.EditorFunctions.GetHighlightTex());

                barRect = _trackRect;
                barRect.x = startX + markerIcon.width / 2f;
                barRect.width = endX - startX - markerIcon.width;

                startX -= markerIcon.width / 2f;
                endX -= markerIcon.width / 2f;

                Rect leftRect = new Rect(startX, _trackRect.y + 3f,
                        markerIcon.width, markerIcon.height);

                Rect rightRect = new Rect(endX, _trackRect.y + 3f,
                        markerIcon.width, markerIcon.height);

                if (startX > -markerIcon.width
                    && startX < _trackRect.width + markerIcon.width)
                {
                    GUI.DrawTexture(leftRect, markerIcon);

                    if (a_tagTrack.SelectId == i && (a_tagTrack.SelectType == TagSelectType.Left
                        || a_tagTrack.SelectType == TagSelectType.All))
                    {
                        GUI.DrawTexture(leftRect, selectIcon);
                    }

                }
                if (endX > -markerIcon.width
                    && endX < _trackRect.width + markerIcon.width)
                {
                    GUI.DrawTexture(rightRect, markerIcon);
                    if (a_tagTrack.SelectId == i && (a_tagTrack.SelectType == TagSelectType.Right
                        || a_tagTrack.SelectType == TagSelectType.All))
                    {
                        GUI.DrawTexture(rightRect, selectIcon);
                    }
                }

                //Selection and moving
                if (evt.isMouse)
                {
                    switch (evt.type)
                    {
                        case EventType.MouseDown:
                            {
                                if (evt.button == 0)
                                {

                                    if (barRect.Contains(evt.mousePosition))
                                    {
                                        a_tagTrack.SelectId = i;
                                        a_tagTrack.SelectType = TagSelectType.All;
                                        ret = true;
                                        a_tagTrack.DraggingSelected = true;
                                        SetTime(tags[a_tagTrack.SelectId].x);
                                    }
                                    else if (leftRect.Contains(evt.mousePosition))
                                    {
                                        a_tagTrack.SelectId = i;
                                        a_tagTrack.SelectType = TagSelectType.Left;
                                        ret = true;
                                        a_tagTrack.DraggingSelected = true;
                                        SetTime(tags[a_tagTrack.SelectId].x);
                                    }
                                    else if (rightRect.Contains(evt.mousePosition))
                                    {
                                        a_tagTrack.SelectId = i;
                                        a_tagTrack.SelectType = TagSelectType.Right;
                                        ret = true;
                                        a_tagTrack.DraggingSelected = true;

                                        SetTime(tags[a_tagTrack.SelectId].y);
                                    }
                                }
                                else if (evt.button == 1)
                                {
                                    if (barRect.Contains(evt.mousePosition))
                                    {
                                        if (barRect.Contains(evt.mousePosition))
                                        {
                                            //Begin Context menu
                                            GenericMenu menu = new GenericMenu();

                                            menu.AddItem(new GUIContent("Delete"), false, a_tagTrack.OnDeleteTag, i);
                                            menu.ShowAsContext();
                                        }
                                    }
                                }
                            }
                            break;
                        case EventType.MouseUp:
                            {
                                if (evt.button == 0)
                                    a_tagTrack.DraggingSelected = false;
                            }
                            break;
                        case EventType.MouseDrag:
                            {
                                if (evt.button == 0 && a_tagTrack.DraggingSelected && a_tagTrack.SelectId == i)
                                {
                                    float desiredValueDelta = ((evt.delta.x / _zoom)) / 75f;

                                    Vector2 selected = tags[a_tagTrack.SelectId];

                                    switch (a_tagTrack.SelectType)
                                    {
                                        case TagSelectType.All:
                                            {
                                                selected.x += desiredValueDelta;
                                                selected.y += desiredValueDelta;

                                                if (selected.x < 0f)
                                                {
                                                    selected.y -= selected.x;
                                                    selected.x -= selected.x;
                                                }

                                                if (selected.y > a_tagTrack.ClipLength)
                                                {
                                                    selected.x -= (selected.y - a_tagTrack.ClipLength);
                                                    selected.y -= (selected.y - a_tagTrack.ClipLength);
                                                }

                                                Modified(selected.x);
                                            }
                                            break;
                                        case TagSelectType.Left:
                                            {
                                                selected.x += desiredValueDelta;
                                                selected.x = Mathf.Clamp(selected.x, 0f, selected.y - 0.1f);
                                                Modified(selected.x);
                                            }
                                            break;
                                        case TagSelectType.Right:
                                            {
                                                selected.y += desiredValueDelta;
                                                selected.y = Mathf.Clamp(selected.y, selected.x + 0.1f, a_tagTrack.ClipLength);
                                                Modified(selected.y);
                                            }
                                            break;
                                    }

                                    tags[a_tagTrack.SelectId] = selected;

                                }

                            }
                            break;
                    }
                }
            }
            return ret;
        }

        //============================================================================================
        /**
        *  @brief Draws and manages all the tags within the track along a GUI track in the editor
        *  
        *  @param [Rect] _trackRect - the rect of the track to draw them on
        *  @param [float] _scrollValueX - the scroll value of the scroll viewEditorUtil.EditorFunctions.GetHighlightTex()
        *  @param [float] _zoom - tyhe zoom value of the 
        *         
        *********************************************************************************************/
        public virtual bool DrawTagTrackBase(TagTrackBase a_tagTrack, Rect _trackRect, float _zoom)
        {
            bool ret = false;

            Texture markerIcon = EditorGUIUtility.IconContent("blendKey").image;
            Texture selectIcon = EditorGUIUtility.IconContent("curvekeyframe").image;

            Event evt = Event.current;

            List<Vector2> tagPositions = a_tagTrack.TagPositions;

            for (int i = 0; i < tagPositions.Count; ++i)
            {
                Vector2 tag = tagPositions[i];

                float startX = (tag.x * 75f) * _zoom;
                float endX = (tag.y * 75f) * _zoom;

                Color baseColor = GUI.color;

                if (EditorGUIUtility.isProSkin)
                    GUI.color = Color.black;

                Rect barRect = new Rect(startX, _trackRect.y + 7f, endX - startX, 5f);
                GUI.Box(barRect, "");
                GUI.color = baseColor;

                a_tagTrack.DrawOnTagData(i, barRect);

                if (a_tagTrack.SelectId == i && a_tagTrack.SelectType == TagSelectType.All)
                    GUI.DrawTexture(barRect, EditorUtil.EditorFunctions.GetHighlightTex());

                barRect = _trackRect;
                barRect.x = startX + markerIcon.width / 2f;
                barRect.width = endX - startX - markerIcon.width;

                startX -= markerIcon.width / 2f;
                endX -= markerIcon.width / 2f;

                Rect leftRect = new Rect(startX, _trackRect.y + 3f,
                        markerIcon.width, markerIcon.height);

                Rect rightRect = new Rect(endX, _trackRect.y + 3f,
                        markerIcon.width, markerIcon.height);

                if (startX > -markerIcon.width
                    && startX < _trackRect.width + markerIcon.width)
                {
                    GUI.DrawTexture(leftRect, markerIcon);

                    if (a_tagTrack.SelectId == i && (a_tagTrack.SelectType == TagSelectType.Left
                        || a_tagTrack.SelectType == TagSelectType.All))
                    {
                        GUI.DrawTexture(leftRect, selectIcon);
                    }

                }
                if (endX > -markerIcon.width
                    && endX < _trackRect.width + markerIcon.width)
                {
                    GUI.DrawTexture(rightRect, markerIcon);
                    if (a_tagTrack.SelectId == i && (a_tagTrack.SelectType == TagSelectType.Right
                        || a_tagTrack.SelectType == TagSelectType.All))
                    {
                        GUI.DrawTexture(rightRect, selectIcon);
                    }
                }

                //Selection and moving
                if (evt.isMouse)
                {
                    switch (evt.type)
                    {
                        case EventType.MouseDown:
                            {
                                if (evt.button == 0)
                                {
                                    if (barRect.Contains(evt.mousePosition))
                                    {
                                        a_tagTrack.SelectId = i;
                                        a_tagTrack.SelectType = TagSelectType.All;
                                        ret = true;
                                        a_tagTrack.DraggingSelected = true;
                                        SetTime(tagPositions[a_tagTrack.SelectId].x);
                                    }
                                    else if (leftRect.Contains(evt.mousePosition))
                                    {
                                        a_tagTrack.SelectId = i;
                                        a_tagTrack.SelectType = TagSelectType.Left;
                                        ret = true;
                                        a_tagTrack.DraggingSelected = true;
                                        SetTime(tagPositions[a_tagTrack.SelectId].x);
                                    }
                                    else if (rightRect.Contains(evt.mousePosition))
                                    {
                                        a_tagTrack.SelectId = i;
                                        a_tagTrack.SelectType = TagSelectType.Right;
                                        ret = true;
                                        a_tagTrack.DraggingSelected = true;

                                        SetTime(tagPositions[a_tagTrack.SelectId].y);
                                    }
                                }
                                else if (evt.button == 1)
                                {
                                    if (barRect.Contains(evt.mousePosition))
                                    {
                                        if (barRect.Contains(evt.mousePosition))
                                        {
                                            //Begin Context menu
                                            GenericMenu menu = new GenericMenu();

                                            menu.AddItem(new GUIContent("Delete"), false, a_tagTrack.OnDeleteTag, i);
                                            menu.ShowAsContext();
                                        }
                                    }
                                }
                            }
                            break;
                        case EventType.MouseUp:
                            {
                                if (evt.button == 0)
                                    a_tagTrack.DraggingSelected = false;
                            }
                            break;
                        case EventType.MouseDrag:
                            {
                                if (evt.button == 0)
                                {
                                    if (a_tagTrack.DraggingSelected && a_tagTrack.SelectId == i)
                                    {
                                        float desiredValueDelta = ((evt.delta.x / _zoom)) / 75f;

                                        Vector2 selected = tagPositions[a_tagTrack.SelectId];

                                        switch (a_tagTrack.SelectType)
                                        {
                                            case TagSelectType.All:
                                                {
                                                    selected.x += desiredValueDelta;
                                                    selected.y += desiredValueDelta;

                                                    if (selected.x < 0f)
                                                    {
                                                        selected.y -= selected.x;
                                                        selected.x -= selected.x;
                                                    }

                                                    if (selected.y > a_tagTrack.ClipLength)
                                                    {
                                                        selected.x -= (selected.y - a_tagTrack.ClipLength);
                                                        selected.y -= (selected.y - a_tagTrack.ClipLength);
                                                    }

                                                    Modified(selected.x);
                                                }
                                                break;
                                            case TagSelectType.Left:
                                                {
                                                    selected.x += desiredValueDelta;
                                                    selected.x = Mathf.Clamp(selected.x, 0f, selected.y - 0.05f);
                                                    Modified(selected.x);
                                                }
                                                break;
                                            case TagSelectType.Right:
                                                {
                                                    selected.y += desiredValueDelta;
                                                    selected.y = Mathf.Clamp(selected.y, selected.x + 0.05f, a_tagTrack.ClipLength);
                                                    Modified(selected.y);
                                                }
                                                break;
                                        }

                                        tagPositions[a_tagTrack.SelectId] = selected;

                                    }
                                }

                            }
                            break;
                    }
                }
            }
            return ret;
        }

    }//End of class: MxMTaggingWindow
}//End of namespace: MxM