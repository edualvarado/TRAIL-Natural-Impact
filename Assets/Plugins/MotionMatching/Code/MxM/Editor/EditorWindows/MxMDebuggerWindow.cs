using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxM;
using EditorUtil;
using System;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief Debugging editor window for MxM
    *         
    *********************************************************************************************/
    public class MxMDebuggerWindow : EditorWindow
    {
        public enum EDataViewMode
        {
            AnimatorState,
            CurrentPose,
            AnimatorSettings,
            Events,
            Idle,
            Layers,
            BlendSpace,
        }

        private static MxMAnimator m_targetMxMAnimator;

        private static float m_graphViewHeight = 175f;
        private static Vector2 m_graphViewScrollPos;
        private static bool m_previewOn;
        private static bool m_desiredRecordOn;
        private static bool m_recordOn;
        private static bool m_playing;
        private static EDataViewMode m_dataViewMode;

        private static MxMDebugFrame[] m_debugFrameData;

        private static bool m_looping;
        private static int m_playheadFrame;
        private static float m_lastFrameTime;
        private static int m_lastRecordedFrameId;
        private static MxMDebugFrame m_currentFrame;

        private static bool m_showTotalCost = true;
        private static bool m_showPoseCost = true;
        private static bool m_showTrajectoryCost = true;
        private static bool m_showFrames = false;
        private static bool m_showMinorFrames = false;

        private static GUIStyle m_chosenChannelBoxStyle;
        private static GUIStyle m_dominantChannelBoxStyle;
        private static GUIStyle m_decayingChannelBoxStyle;

        private static Vector2 m_curPoseColumn1ScrollPos;
        private static Vector2 m_curPoseColumn2ScrollPos;
        private static Vector2 m_curPoseColumn3ScrollPos;
        private static Vector2 m_animSettingScrollPos;
        private static Vector2 m_curEventScrollPos;
        private static Vector2 m_runtimeEventDataScrollPos;
        private static Vector2 m_curIdleDataScrollPos;
        private static Vector2 m_idleStateScrollPos;


        public static MxMDebuggerWindow Instance { get; private set; }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        [MenuItem("Window/MxM/Debugger")]
        public static void ShowWindow()
        {
            System.Type sceneType = System.Type.GetType("UnityEditor.SceneView, UnityEditor.dll");
            System.Type gameType = System.Type.GetType("UnityEditor.GameView, UnityEditor.dll");

            EditorWindow editorWindow = EditorWindow.GetWindow<MxMDebuggerWindow>("MxM Debugger", true,
                new System.Type[] { sceneType, gameType });

            editorWindow.minSize = new Vector2(100f, 100f);
            editorWindow.Show();
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static void SetTarget(MxMAnimator a_targetAnimator, bool a_override = true)
        {
            if (a_targetAnimator == null)
                return;

            if(a_override)
            {
                m_targetMxMAnimator = a_targetAnimator;
            }
            else
            {
                if (m_targetMxMAnimator == null)
                    m_targetMxMAnimator = a_targetAnimator;

                if(Instance != null)
                    Instance.Repaint();
            }
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnEnable()
        {
            m_previewOn = false;
            Instance = this;
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnDisable()
        {
            m_previewOn = false;
            Instance = null;
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnGUI()
        {
            if (m_chosenChannelBoxStyle == null)
            {
                m_chosenChannelBoxStyle = new GUIStyle(GUI.skin.box);
                m_dominantChannelBoxStyle = new GUIStyle(GUI.skin.box);
                m_decayingChannelBoxStyle = new GUIStyle(GUI.skin.box);

                if (EditorGUIUtility.isProSkin)
                {
                    m_chosenChannelBoxStyle.normal.background = EditorFunctions.MakeTex(1, 1, new Color(0f, 0f, 0.5f));
                    m_dominantChannelBoxStyle.normal.background = EditorFunctions.MakeTex(1, 1, new Color(0f, 0.4f, 0f));
                    m_decayingChannelBoxStyle.normal.background = EditorFunctions.MakeTex(1, 1, new Color(0.5f, 0.25f, 0f));
                }
                else
                {
                    m_chosenChannelBoxStyle.normal.background = EditorFunctions.MakeTex(1, 1, Color.cyan);
                    m_dominantChannelBoxStyle.normal.background = EditorFunctions.MakeTex(1, 1, Color.green);
                    m_decayingChannelBoxStyle.normal.background = EditorFunctions.MakeTex(1, 1, Color.yellow);
                }
            }

            if (m_targetMxMAnimator == null)
                return;

            if (!Application.IsPlaying(m_targetMxMAnimator))
                return;

            MxMDebugger debugData = m_targetMxMAnimator.DebugData;
            m_lastRecordedFrameId = debugData.LastRecordedFrame;
            
            if (debugData == null)
                return;

            m_debugFrameData = debugData.FrameData;

            if (m_debugFrameData == null)
                return;

            if (m_playheadFrame < 0)
            {
                m_playheadFrame += m_debugFrameData.Length;
            }
            else if(m_playheadFrame >= m_debugFrameData.Length)
            {
                m_playheadFrame -= m_debugFrameData.Length;
            }

            m_currentFrame = m_debugFrameData[m_playheadFrame];
            int frameShift = m_debugFrameData.Length - m_lastRecordedFrameId;

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUILayout.Space(195f);
            EditorGUI.BeginChangeCheck();
            m_previewOn = GUILayout.Toggle(m_previewOn, "Preview", EditorStyles.toolbarButton);
            if(EditorGUI.EndChangeCheck())
            {
                if(m_previewOn)
                {
                    StopRecording();
                    m_targetMxMAnimator.BeginDebugPreview();

                    m_recordOn = false;
                    m_desiredRecordOn = false;
                    m_playing = false;
                }
                else
                {
                    m_targetMxMAnimator.EndDebugPreview();
                }
            }

            GUILayout.Space(5f);

            EditorGUI.BeginChangeCheck();
            m_desiredRecordOn = GUILayout.Toggle(m_desiredRecordOn, EditorGUIUtility.IconContent("Animation.Record").image, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                if(!m_desiredRecordOn)
                {
                    m_recordOn = false;
                    m_targetMxMAnimator.StopRecordAnalytics();
                }
            }

            if (m_recordOn)
            {
                Repaint();
                m_playheadFrame = debugData.LastRecordedFrame;

                if (m_playheadFrame < 0)
                {
                    m_playheadFrame = 0;
                }
                else if (m_playheadFrame >= m_debugFrameData.Length)
                {
                    m_playheadFrame = m_debugFrameData.Length - 1;
                }
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.FirstKey").image, EditorStyles.toolbarButton))
            {
                StopRecording();
                m_playheadFrame = debugData.LastRecordedFrame + 1;

                if (m_playheadFrame >= m_debugFrameData.Length)
                    m_playheadFrame -= m_debugFrameData.Length;

                m_playing = false;

                if (m_previewOn)
                    m_targetMxMAnimator.SetDebugPreviewFrame(m_playheadFrame);

                Repaint();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey").image, EditorStyles.toolbarButton))
            {
                StopRecording();
                m_playing = false;
                --m_playheadFrame;

                if (m_playheadFrame < 0)
                    m_playheadFrame += m_debugFrameData.Length;

                if (m_previewOn)
                    m_targetMxMAnimator.SetDebugPreviewFrame(m_playheadFrame);

                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            m_playing = GUILayout.Toggle(m_playing, EditorGUIUtility.IconContent("Animation.Play").image, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                StopRecording();
                m_lastFrameTime = Time.time;
            }

            if(m_playing)
            {
                float timeDelta = m_currentFrame.DeltaTime;

                if (!m_currentFrame.Used)
                    timeDelta = 0f;

                if(Time.time - m_lastFrameTime > timeDelta)
                {
                    m_lastFrameTime = Time.time;
                    ++m_playheadFrame;
                    if (m_playheadFrame == m_debugFrameData.Length)
                    {
                        if (m_looping)
                        {
                            m_playheadFrame = 0;
                        }
                        else
                        {
                            m_playheadFrame = m_debugFrameData.Length - 1;

                            m_playing = false;
                        }
                    }

                    if (m_previewOn)
                        m_targetMxMAnimator.SetDebugPreviewFrame(m_playheadFrame);
                }
                Repaint();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.NextKey").image, EditorStyles.toolbarButton))
            {
                StopRecording();
                m_playing = false;
                ++m_playheadFrame;

                if(m_playheadFrame == m_debugFrameData.Length)
                    m_playheadFrame -= m_debugFrameData.Length;

                if (m_previewOn)
                    m_targetMxMAnimator.SetDebugPreviewFrame(m_playheadFrame);

                Repaint();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.LastKey").image, EditorStyles.toolbarButton))
            {
                StopRecording();
                m_playing = false;
                m_playheadFrame = m_lastRecordedFrameId;

                if (m_previewOn)
                    m_targetMxMAnimator.SetDebugPreviewFrame(m_playheadFrame);

                Repaint();
            }

            GUILayout.Space(5f);
            m_looping = GUILayout.Toggle(m_looping, "Loop", EditorStyles.toolbarButton);
            GUILayout.Space(5f);
            EditorGUI.BeginChangeCheck();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                StopRecording();
                m_targetMxMAnimator.DebugData.ClearData(m_targetMxMAnimator.MaxMixCount, 
                    m_targetMxMAnimator.CurrentAnimData.PosePredictionTimes.Length);

                Repaint();
            }

            GUILayout.Space(5f);

            if (GUILayout.Button("Save Pose Mask", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Save Pose Mask", "Are you sure? Any existing pose mask " +
                    "attached to this anim data will be overriden.", "Yes", "Cancel"))
                {
                    StopRecording();
                    m_targetMxMAnimator.GeneratePoseMask();
                }
            }

            if(GUILayout.Button("Log Pose Mask", EditorStyles.toolbarButton))
            {
                m_targetMxMAnimator.DumpAnalytics();
            }

            EditorGUILayout.EndHorizontal();

            Rect graphAreaRect = new Rect(0f, EditorStyles.toolbar.fixedHeight - 2f, position.width, m_graphViewHeight + 2f);
            Rect graphSideRect = new Rect(0f, 0f, 200f, graphAreaRect.height);
            Rect graphMainRect = new Rect(graphSideRect.width, 0f, graphAreaRect.width - graphSideRect.width, graphAreaRect.height);

            ManageInputs(graphMainRect);

            //GRAPH VIEW STARTS HERE
            GUILayout.BeginArea(graphAreaRect);
            m_graphViewScrollPos = EditorGUILayout.BeginScrollView(m_graphViewScrollPos);

            //GRAPH DATA VIEW STARTS HERE
            GUI.Box(graphSideRect, "");
            GUILayout.BeginArea(graphSideRect);
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical();
                {
                    m_showTotalCost = EditorGUILayout.Toggle("Total Cost (Red)", m_showTotalCost);
                    m_showPoseCost = EditorGUILayout.Toggle("Pose Cost (Purple)", m_showPoseCost);
                    m_showTrajectoryCost = EditorGUILayout.Toggle("Trajectory Cost (Blue)", m_showTrajectoryCost);
                    m_showFrames = EditorGUILayout.Toggle("Update Frames", m_showFrames);
                    m_showMinorFrames = EditorGUILayout.Toggle("Minor Frames", m_showMinorFrames);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();

            GUI.Box(graphMainRect, "");
            GUILayout.BeginArea(graphMainRect);

            float spacing = graphMainRect.width / m_debugFrameData.Length;

            Handles.BeginGUI();

            //Find min and max costs
            float maxTotalCost = 0f;
            float minPoseCost = float.MaxValue;
            float minTrajCost = float.MaxValue;

            for (int i = 0; i < m_debugFrameData.Length; ++i)
            {
                ref MxMDebugFrame frameData = ref m_debugFrameData[i];

                if (frameData.Used && frameData.UpdateThisFrame)
                {
                    if(frameData.LastChosenCost > maxTotalCost)
                        maxTotalCost = frameData.LastChosenCost;

                    if (frameData.LastPoseCost < minPoseCost)
                        minPoseCost = frameData.LastPoseCost;

                    if (frameData.LastTrajectoryCost < minTrajCost)
                        minTrajCost = frameData.LastTrajectoryCost;
                }
            }

            maxTotalCost += 15f;
            float minCost = Mathf.Min(minPoseCost, minTrajCost) - 30f;
            float costRange = maxTotalCost - minCost;

            Vector2 lastPointTotal = Vector2.zero;
            Vector2 lastPointPose = Vector2.zero;
            Vector2 lastPointTraj = Vector2.zero;
            bool currentIdleBlock = false;
            float idleBlockStartX = 0f;
            for (int i = 0; i < m_debugFrameData.Length; ++i)
            {
                ref MxMDebugFrame frameData = ref m_debugFrameData[i];

                if (frameData.Used)
                {
                    int framePos = i + frameShift;

                    if (framePos > 599)
                        framePos -= 600;

                    float xPos = spacing * framePos;

                    if (currentIdleBlock)
                    {
                        if (frameData.AnimatorState != EMxMStates.Idle || framePos == 599)
                        {
                            currentIdleBlock = false;
                            GUI.Box(new Rect(idleBlockStartX, 0f, xPos - idleBlockStartX,
                                graphAreaRect.height), "Idle");

                            lastPointTraj.x = lastPointPose.x = lastPointTotal.x = xPos;
                        }
                    }
                    else
                    {
                        if (frameData.AnimatorState == EMxMStates.Idle)
                        {
                            idleBlockStartX = xPos;
                            currentIdleBlock = true;
                        }
                    }

                    if (frameData.UpdateThisFrame)
                    {
                        float yPos = 0f;
                        if (m_showTotalCost)
                        {
                            Handles.color = Color.red;
                            yPos = graphMainRect.height - graphMainRect.height *
                                ((frameData.LastChosenCost - minCost) / costRange);

                            if (i != 0 && Mathf.Abs(xPos - lastPointTotal.x) < graphMainRect.x / 2f)
                                Handles.DrawLine(lastPointTotal, new Vector2(xPos, yPos));


                            lastPointTotal = new Vector2(xPos, yPos);
                        }

                        if (m_showPoseCost)
                        {
                            Handles.color = new Color(128, 0, 128);
                            yPos = graphMainRect.height - graphMainRect.height *
                                ((frameData.LastPoseCost - minCost) / costRange);

                            if (i != 0 && Mathf.Abs(xPos - lastPointPose.x) < graphMainRect.x / 2f)
                                Handles.DrawLine(lastPointPose, new Vector2(xPos, yPos));

                            lastPointPose = new Vector2(xPos, yPos);
                        }

                        if (m_showTrajectoryCost)
                        {
                            Handles.color = Color.blue;
                            yPos = graphAreaRect.height - graphMainRect.height *
                                ((frameData.LastTrajectoryCost - minCost) / costRange);

                            if (i != 0 && Mathf.Abs(xPos - lastPointTraj.x) < graphMainRect.x / 2f)
                                Handles.DrawLine(lastPointTraj, new Vector2(xPos, yPos));

                            lastPointTraj = new Vector2(xPos, yPos);
                        }

                        if (m_showFrames)
                        {
                            Handles.color = Color.black;
                            Handles.DrawLine(new Vector3(xPos, graphMainRect.height - 30f, 0f),
                                new Vector3(xPos, graphMainRect.height, 0f));
                        }
                    }
                    else
                    {
                        if (m_showMinorFrames)
                        {
                            Handles.color = Color.grey;

                            Handles.DrawLine(new Vector3(xPos, graphMainRect.height - 15f, 0f),
                                new Vector3(xPos, graphMainRect.height, 0f));
                        }
                    } 
                }
            }

            //Draw Playhead
            int visualPlayheadFrame = m_playheadFrame + frameShift;
            if (visualPlayheadFrame > 600)
                visualPlayheadFrame -= 600;

            Handles.color = Color.white;

            Handles.DrawLine(new Vector3(visualPlayheadFrame * spacing, 0f, 0f),
                new Vector3(visualPlayheadFrame * spacing, graphMainRect.height, 0f));

            bool leftSide = (visualPlayheadFrame * spacing + 50) > graphMainRect.width;

            if (leftSide)
            {
                Rect frameRect = new Rect(visualPlayheadFrame * spacing - 52, 0f, 47f, 18f);
                GUI.Box(frameRect, "");
                EditorGUI.LabelField(frameRect, visualPlayheadFrame.ToString(), EditorStyles.boldLabel);

            }
            else
            {
                Rect frameRect = new Rect(visualPlayheadFrame * spacing, 0f, 50f, 18f);
                GUI.Box(frameRect, "");
                EditorGUI.LabelField(frameRect, visualPlayheadFrame.ToString(), EditorStyles.boldLabel);
            }

            GUIStyle costLabelStyle = new GUIStyle(EditorStyles.boldLabel);

            if (m_currentFrame.Used)
            {
                float totalLabelHeight = graphMainRect.height - graphMainRect.height *
                        ((m_currentFrame.LastChosenCost - minCost) / costRange) - 9f;

                float trajLabelHeight = graphMainRect.height - graphMainRect.height *
                        ((m_currentFrame.LastTrajectoryCost - minCost) / costRange) - 9f;

                float poseLabelHeight = graphMainRect.height - graphMainRect.height *
                        ((m_currentFrame.LastPoseCost - minCost) / costRange) - 9f;

                if (totalLabelHeight > graphMainRect.height - 54f)
                {
                    totalLabelHeight -= totalLabelHeight - (graphMainRect.height - 45f);
                }

                float totalTrajDelta = trajLabelHeight - totalLabelHeight;
                if (totalTrajDelta < 18f)
                {
                    trajLabelHeight += 18f - totalTrajDelta;
                }

                float totalPoseDelta = poseLabelHeight - totalLabelHeight;
                if(totalPoseDelta < 18f)
                {
                    poseLabelHeight += 18f - totalTrajDelta;
                }

                if(poseLabelHeight < trajLabelHeight)
                {
                    float poseTrajDelta = trajLabelHeight - poseLabelHeight;

                    if(poseTrajDelta < 18f)
                        trajLabelHeight += 18f - poseTrajDelta;

                }
                else
                {
                    float poseTrajDelta = poseLabelHeight - trajLabelHeight;

                    if (poseTrajDelta < 18f)
                        poseLabelHeight += 18f - poseTrajDelta;
                }
                
                if (m_showTotalCost)
                {
                    costLabelStyle.normal.textColor = Color.red;

                    Rect totalCostRect = new Rect(visualPlayheadFrame * spacing, totalLabelHeight, 47f, 18f);

                    if (leftSide)
                    {
                        totalCostRect.x -= 52f;
                    }

                    GUI.Box(totalCostRect, "");
                    EditorGUI.LabelField(totalCostRect, m_currentFrame.LastChosenCost.ToString("F2"),
                         costLabelStyle);
                }

                if (m_showPoseCost)
                {
                    costLabelStyle.normal.textColor = new Color(128, 0, 128);

                    Rect poseCostRect = new Rect(visualPlayheadFrame * spacing, poseLabelHeight, 47f, 18f);

                    if (leftSide)
                    {
                        poseCostRect.x -= 52f;
                    }

                    GUI.Box(poseCostRect, "");
                    EditorGUI.LabelField(poseCostRect, m_currentFrame.LastPoseCost.ToString("F2"),
                         costLabelStyle);
                }

                if (m_showTrajectoryCost)
                {
                    costLabelStyle.normal.textColor = Color.blue;

                    Rect trajCostRect = new Rect(visualPlayheadFrame * spacing, trajLabelHeight, 47f, 18f);

                    if (leftSide)
                    {
                        trajCostRect.x -= 52f;
                    }

                    GUI.Box(trajCostRect, "");
                    EditorGUI.LabelField(trajCostRect, m_currentFrame.LastTrajectoryCost.ToString("F2"),
                         costLabelStyle);
                }
            }

            Handles.EndGUI();

            GUILayout.EndArea();
            
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            //GRAPH VIEW ENDS HERE

            GUILayout.Space(m_graphViewHeight);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            m_dataViewMode = (EDataViewMode)EditorGUILayout.EnumPopup(m_dataViewMode, 
                EditorStyles.toolbarPopup, GUILayout.Width(graphSideRect.width - 5f));

            EditorGUILayout.EndHorizontal();

            float dataAreaHeight = EditorStyles.toolbar.fixedHeight * 2f + m_graphViewHeight;
            Rect dataAreaRect = new Rect(0f, dataAreaHeight - 2f, position.width, position.height - dataAreaHeight + 2f);
 
            DrawDataArea(dataAreaRect);

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
            {
                if (m_desiredRecordOn != m_recordOn)
                {
                    m_recordOn = m_desiredRecordOn;

                    if (m_recordOn)
                    {
                        if (m_targetMxMAnimator.IsPaused)
                            m_targetMxMAnimator.UnPause();

                        m_targetMxMAnimator.StartRecordAnalytics();

                        m_playing = false;
                    }
                    else
                    {
                        if (m_previewOn)
                        {
                            m_previewOn = false;
                            m_targetMxMAnimator.EndDebugPreview();
                        }
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawDataArea(Rect a_dataAreaRect)
        {
            switch (m_dataViewMode)
            {
                case EDataViewMode.AnimatorState: { DrawAnimatorStateData(a_dataAreaRect); }break;
                case EDataViewMode.CurrentPose: { DrawCurrentPoseData(a_dataAreaRect); } break;
                case EDataViewMode.AnimatorSettings: { DrawAnimatorSettingsData(a_dataAreaRect); } break;
                case EDataViewMode.Events: { DrawEventData(a_dataAreaRect); } break;
                case EDataViewMode.Idle: { DrawIdleData(a_dataAreaRect); } break;
                case EDataViewMode.Layers: { DrawLayerData(a_dataAreaRect); } break;
                case EDataViewMode.BlendSpace: { DrawBlendSpaceData(a_dataAreaRect); } break;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawAnimatorStateData(Rect a_dataAreaRect)
        {
            if (m_currentFrame.Used)
            {
                MxMAnimData animData = m_targetMxMAnimator.CurrentAnimData;

                GUILayout.BeginArea(a_dataAreaRect);
                {

                    float columnWidth = 300f;

#if UNITY_2019_3_OR_NEWER
                    float rowHeight = 246f;
#else
                    float rowHeight = 226f;
#endif
                    int rowIndex = 0;
                    int colIndex = 0;
                    float remainingHeight = a_dataAreaRect.height;
                    int usedIndex = 0;
                    for (int i = 0; i < m_currentFrame.playableStates.Length; ++i)
                    {
                        ref MxMDebugFrame.PlayableStateDebugData state = ref m_currentFrame.playableStates[i];

                        Rect animCardRect = new Rect(colIndex * columnWidth,
                            rowIndex * rowHeight, columnWidth, rowHeight);

                        remainingHeight -= rowHeight;
                        if (remainingHeight < rowHeight)
                        {
                            remainingHeight = a_dataAreaRect.height;
                            ++colIndex;
                            rowIndex = 0;
                        }
                        else
                        {
                            ++rowIndex;
                        }

                        GUI.Box(animCardRect, "");

                        animCardRect.x += 1f;
                        animCardRect.y += 1f;
                        animCardRect.height -= 2f;
                        animCardRect.width -= 2f;

                        switch (state.BlendStatus)
                        {
                            case EBlendStatus.Chosen: { GUI.Box(animCardRect, "", m_chosenChannelBoxStyle); } break;
                            case EBlendStatus.Dominant: { GUI.Box(animCardRect, "", m_dominantChannelBoxStyle); } break;
                            case EBlendStatus.Decaying: { GUI.Box(animCardRect, "", m_decayingChannelBoxStyle); } break;
                        }

                        GUILayout.BeginArea(animCardRect);
                        {
                            EditorGUILayout.LabelField("Channel " + i, EditorStyles.boldLabel);

                            if (state.BlendStatus == EBlendStatus.None)
                                EditorGUI.BeginDisabledGroup(true);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20f);
                            EditorGUILayout.BeginVertical();
                            {
                                ref readonly PoseData startPose = ref animData.Poses[state.StartPoseId];
                                
                                EditorGUILayout.LabelField("Animation: " + usedIndex);
                                EditorGUILayout.LabelField("Start Pose Id: " + state.StartPoseId);
                                EditorGUILayout.LabelField("Animation Id: " + state.AnimId);
                                EditorGUILayout.LabelField("Animation Type: " + state.AnimType.ToString());
                                EditorGUILayout.LabelField("Anim Time: " + (startPose.Time + state.Age).ToString("F4"));
                                EditorGUILayout.LabelField("Age: " + state.Age.ToString("F4"));
                                EditorGUILayout.LabelField("Decay Age: " + state.DecayAge.ToString("F4"));
                                EditorGUILayout.LabelField("Weight: " + state.Weight.ToString("F5"));
                                EditorGUILayout.LabelField("Speed: " + state.Speed.ToString("F2"));
                                EditorGUILayout.LabelField("Blend Status: " + state.BlendStatus.ToString());
                                EditorGUILayout.BeginHorizontal();
                                {
                                    int clipId = startPose.PrimaryClipId;
                                    AnimationClip clip = animData.Clips[clipId];

                                    EditorGUILayout.LabelField("Primary Clip: ", GUILayout.Width(100f));
                                    EditorGUILayout.ObjectField(clip, typeof(AnimationClip), false);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();

                            if (state.BlendStatus == EBlendStatus.None)
                            {
                                EditorGUI.EndDisabledGroup();
                            }
                            else
                            {
                                ++usedIndex;
                            }
                        }
                        GUILayout.EndArea();
                    }
                }
                GUILayout.EndArea();
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawCurrentPoseData(Rect a_dataAreaRect)
        {
            if (m_currentFrame.Used)
            {
                MxMAnimData animData = m_targetMxMAnimator.CurrentAnimData;

                float columnWidth = 350f;
                Rect column1Rect = new Rect(a_dataAreaRect.x, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);
                Rect column2Rect = new Rect(a_dataAreaRect.x + columnWidth, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);
                Rect column3Rect = new Rect(column2Rect.x + columnWidth, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);

                GUI.Box(column1Rect, "");
                GUI.Box(column2Rect, "");
                GUI.Box(column3Rect, "");

                //Column 1
                GUILayout.BeginArea(column1Rect);
                m_curPoseColumn1ScrollPos = EditorGUILayout.BeginScrollView(m_curPoseColumn1ScrollPos);
                //General
                EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Animator State: " + m_currentFrame.AnimatorState);
                    EditorGUILayout.LabelField("Position   x: " + m_currentFrame.Position.x.ToString("F2")
                        + " y: " + m_currentFrame.Position.y.ToString("F2") + " z: " + m_currentFrame.Position.z.ToString("F2"));

                    Vector3 eularRotation = m_currentFrame.Rotation.eulerAngles;

                    EditorGUILayout.LabelField("Rotation   x: " + eularRotation.x.ToString("F2") + " y: "
                        + eularRotation.y.ToString("F2") + " z: " + eularRotation.z.ToString("F2"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                //DataSet
                GUILayout.Space(18f);
                EditorGUILayout.LabelField("Data Set", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Anim Data ID: " + m_currentFrame.AnimDataId);
                    EditorGUILayout.LabelField("Calibration Data Id: " + m_currentFrame.CalibrationDataId);

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Anim Data: ");
                        EditorGUILayout.ObjectField(m_targetMxMAnimator.AnimData[m_currentFrame.AnimDataId], typeof(MxMAnimData), false);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);

                //Tracking
                EditorGUILayout.LabelField("Tracking Data", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Time Since Motion Update: " + m_currentFrame.TimeSinceMotionUpdate.ToString("F4"));
                    EditorGUILayout.LabelField("Time Since Motion Chosen: " + m_currentFrame.TimeSinceMotionChosen.ToString("F4"));
                    EditorGUILayout.LabelField("Desired Playback Speed: " + m_currentFrame.DesiredPlaybackSpeed.ToString("F2"));
                    EditorGUILayout.LabelField("Enforce Clip Change: " + m_currentFrame.EnforceClipChange.ToString());
                    EditorGUILayout.LabelField("Pose Interpolation Value: " + m_currentFrame.PoseInterpolationValue.ToString());
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);

                //Warping
                EditorGUILayout.LabelField("Error Warping", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Laterial Warp Angle " + m_currentFrame.LateralWarpAngle.ToString("F2"));
                    EditorGUILayout.LabelField("Longitudinal Warp Scale: " + m_currentFrame.LongitudinalWarpScale.ToString("F2"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);

                EditorGUILayout.LabelField("Cost Calculations", EditorStyles.boldLabel);

                GUILayout.Space(18f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Calculated This Frame: " + m_currentFrame.UpdateThisFrame.ToString());
                    EditorGUILayout.LabelField("Total Cost: " + m_currentFrame.LastChosenCost.ToString("F2"));
                    EditorGUILayout.LabelField("Pose Cost: " + m_currentFrame.LastPoseCost.ToString("F2"));
                    EditorGUILayout.LabelField("Trajectory Cost: " + m_currentFrame.LastTrajectoryCost.ToString("F2"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);

                GUILayout.Space(18f);

                ref MxMDebugFrame.PlayableStateDebugData primaryChannelData = ref m_currentFrame.playableStates[m_currentFrame.PrimaryBlendChannel];
                ref MxMDebugFrame.PlayableStateDebugData dominantChannelData = ref m_currentFrame.playableStates[m_currentFrame.DominantBlendChannel];

                //Chosen Aniamtion
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField("Chosen Animation:");

                int chosenClipId = animData.Poses[primaryChannelData.StartPoseId].PrimaryClipId;
                AnimationClip chosenClip = animData.Clips[chosenClipId];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Anim Id: " + primaryChannelData.AnimId);
                    EditorGUILayout.LabelField("Anim Type: " + primaryChannelData.AnimType.ToString());
                    EditorGUILayout.LabelField("Clip Id: " + chosenClipId);
                    EditorGUILayout.LabelField("Weight: " + primaryChannelData.Weight.ToString("F4"));
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Clip: ", GUILayout.Width(35f));
                        EditorGUILayout.ObjectField(chosenClip, typeof(AnimationClip), false);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);

                //Dominant Animation
                EditorGUILayout.LabelField("Dominant Animation:");

                int dominantClipId = animData.Poses[dominantChannelData.StartPoseId].PrimaryClipId;
                AnimationClip dominantClip = animData.Clips[dominantClipId];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Anim Id: " + dominantChannelData.AnimId);
                    EditorGUILayout.LabelField("Anim Type: " + dominantChannelData.AnimType.ToString());
                    EditorGUILayout.LabelField("Clip Id: " + dominantClipId);
                    EditorGUILayout.LabelField("Weight: " + dominantChannelData.Weight.ToString("F4"));
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Clip: ", GUILayout.Width(35f));
                        EditorGUILayout.ObjectField(dominantClip, typeof(AnimationClip), false);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();

                GUILayout.BeginArea(column2Rect);
                m_curPoseColumn2ScrollPos = EditorGUILayout.BeginScrollView(m_curPoseColumn2ScrollPos);

                EditorGUILayout.LabelField("Current Interpolated Pose", EditorStyles.boldLabel);
                ref PoseData curInterpolatedPose = ref m_currentFrame.CurrentInterpolatedPose;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Pose Id: " + curInterpolatedPose.PoseId);
                    EditorGUILayout.LabelField("Anim Id: " + curInterpolatedPose.AnimId);
                    EditorGUILayout.LabelField("Anim Type: " + curInterpolatedPose.AnimType);
                    EditorGUILayout.LabelField("LastPoseId: " + curInterpolatedPose.LastPoseId);
                    EditorGUILayout.LabelField("NextPoseId: " + curInterpolatedPose.NextPoseId);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);
                EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Matching Tags: " + curInterpolatedPose.Tags);
                    EditorGUILayout.LabelField("Utility Tags: " + curInterpolatedPose.GenericTags);
                    EditorGUILayout.LabelField("Favour: " + curInterpolatedPose.Favour);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);
                EditorGUILayout.LabelField("Pose", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("LocalVelocity: " + curInterpolatedPose.LocalVelocity);

                    for (int i = 0; i < curInterpolatedPose.JointsData.Length; ++i)
                    {
                        ref JointData jointData = ref curInterpolatedPose.JointsData[i];

                        EditorGUILayout.LabelField("Joint " + i + ":");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Position   x: " + jointData.Position.x.ToString("F4")
                           + " y: " + jointData.Position.y.ToString("F4") + " z: " + jointData.Position.z.ToString("F4"));


                            EditorGUILayout.LabelField("Velocity   x: " + jointData.Velocity.x.ToString("F4")
                           + " y: " + jointData.Velocity.y.ToString("F4") + " z: " + jointData.Velocity.z.ToString("F4"));
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);
                EditorGUILayout.LabelField("Trajectory", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    for (int i = 0; i < curInterpolatedPose.Trajectory.Length; ++i)
                    {
                        ref TrajectoryPoint trajectoryPoint = ref curInterpolatedPose.Trajectory[i];

                        EditorGUILayout.LabelField("Point " + i + ":");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Facing Angle: " + trajectoryPoint.FacingAngle);

                        EditorGUILayout.LabelField("Position   x: " + trajectoryPoint.Position.x.ToString("F4")
                       + " y: " + trajectoryPoint.Position.y.ToString("F4") + " z: " + trajectoryPoint.Position.z.ToString("F4"));

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();

                //Desire Data Column
                GUILayout.BeginArea(column3Rect);
                m_curPoseColumn3ScrollPos = EditorGUILayout.BeginScrollView(m_curPoseColumn3ScrollPos);
                EditorGUILayout.LabelField("Input Data (Desired)", EditorStyles.boldLabel);
                GUILayout.Space(10f);

                string requireTagName = m_currentFrame.RequiredTags.ToString();
                string favourTagName = m_currentFrame.FavourTags.ToString();

                EditorGUILayout.LabelField("Required Tags: " + requireTagName);
                EditorGUILayout.LabelField("Favour Tags: " + favourTagName);
                EditorGUILayout.LabelField("Favour Multiplier: " + m_currentFrame.FavourTagMultiplier);

                GUILayout.Space(18f);

                EditorGUILayout.LabelField("Desired Trajectory", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    for (int i = 0; i < m_currentFrame.DesiredGoal.Length; ++i)
                    {
                        ref TrajectoryPoint trajectoryPoint = ref m_currentFrame.DesiredGoal[i];

                        EditorGUILayout.LabelField("Point " + i + ":");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.BeginVertical();
                        {

                            EditorGUILayout.LabelField("Facing Angle: " + trajectoryPoint.FacingAngle);
                            EditorGUILayout.LabelField("Position   x: " + trajectoryPoint.Position.x.ToString("F4")
                           + " y: " + trajectoryPoint.Position.y.ToString("F4") + " z: " + trajectoryPoint.Position.z.ToString("F4"));
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            else
            {
                EditorGUILayout.LabelField("Frame not used");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawAnimatorSettingsData(Rect a_dataAreaRect)
        {
            if (m_currentFrame.Used)
            {
                float columnWidth = 350f;
                Rect column1Rect = new Rect(a_dataAreaRect.x, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);

                GUI.Box(column1Rect, "");

                //Column 1
                GUILayout.BeginArea(column1Rect);
                m_animSettingScrollPos = EditorGUILayout.BeginScrollView(m_animSettingScrollPos);
                //General
                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Playback Speed: " + m_currentFrame.PlaybackSpeed.ToString("F3"));
                    EditorGUILayout.LabelField("Playback Smoothing: " + m_currentFrame.PlaybackSmoothRate.ToString("F3"));
                    EditorGUILayout.LabelField("Update Interval: " + m_currentFrame.UpdateInterval.ToString("F3"));
                    EditorGUILayout.LabelField("Matching Blend Time: " + m_currentFrame.MatchBlendTime.ToString("F2"));
                    EditorGUILayout.LabelField("Event Blend Time: " + m_currentFrame.EventBlendTime.ToString("F2"));
                    EditorGUILayout.LabelField("Idle Blend Time: " + m_currentFrame.IdleBlendTime.ToString("F2"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(18f);

                EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Apply Trajectory Blending: " + m_currentFrame.ApplyTrajectoryBlending.ToString());
                    EditorGUILayout.LabelField("Trajectory Blending Weight: " + m_currentFrame.TrajectoryBlendingWeight.ToString("F3"));
                    EditorGUILayout.LabelField("Favour Current Pose: " + m_currentFrame.FavourCurrentPose.ToString());
                    EditorGUILayout.LabelField("Current Pose Favour: " + m_currentFrame.CurrentPoseFavour.ToString("F3"));
                    EditorGUILayout.LabelField("Angular Warp Type: " + m_currentFrame.AngularWarpType.ToString());
                    EditorGUILayout.LabelField("Angular Warp Method: " + m_currentFrame.AngularWarpMethod.ToString());
                    EditorGUILayout.LabelField("Angular Warp Rate: " + m_currentFrame.AngularWarpRate.ToString("F2"));
                    EditorGUILayout.LabelField("Angular Warp Threshold: " + m_currentFrame.AngularWarpThreshold.ToString("F2"));
                    EditorGUILayout.LabelField("Angular Warp Angle Threshold: " + m_currentFrame.AngularWarpAngleThreshold.ToString("F2"));
                    EditorGUILayout.LabelField("Longitudinal Error Warp Type: " + m_currentFrame.LongErrorWarpType.ToString());
                    EditorGUILayout.LabelField("Speed Warp Limits    x: " + m_currentFrame.SpeedWarpLimits.x.ToString("F2") + " y: " +
                        m_currentFrame.SpeedWarpLimits.y.ToString("F2"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            else
            {
                EditorGUILayout.LabelField("Frame not used");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawEventData(Rect a_dataAreaRect)
        {
            if (m_currentFrame.Used)
            {
                float columnWidth = 350f;
                Rect column1Rect = new Rect(a_dataAreaRect.x, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);
                Rect column2Rect = new Rect(a_dataAreaRect.x + columnWidth, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);

                GUI.Box(column1Rect, "");
                GUI.Box(column2Rect, "");

                if (m_currentFrame.AnimatorState != EMxMStates.Event)
                    EditorGUI.BeginDisabledGroup(true);

                ref EventData eventData = ref m_currentFrame.CurrentEvent;

                GUILayout.BeginArea(column1Rect);
                m_curEventScrollPos = EditorGUILayout.BeginScrollView(m_curEventScrollPos);
                {
                    EditorGUILayout.LabelField("Current Event (Static)", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Event Id: " + eventData.EventId);
                        EditorGUILayout.LabelField("Start Pose Id: " + eventData.StartPoseId);

                        EditorGUILayout.BeginHorizontal();
                        {
                            MxMAnimData animData = m_targetMxMAnimator.CurrentAnimData;
                            ref MxMDebugFrame.PlayableStateDebugData primaryChannelData = ref m_currentFrame.playableStates[m_currentFrame.PrimaryBlendChannel];

                            int chosenClipId = animData.Poses[primaryChannelData.StartPoseId].PrimaryClipId;
                            AnimationClip chosenClip = animData.Clips[chosenClipId];

                            EditorGUILayout.LabelField("Clip: ", GUILayout.Width(45f));

                            if (m_currentFrame.AnimatorState == EMxMStates.Event)
                            {
                                EditorGUILayout.ObjectField(chosenClip, typeof(AnimationClip), false);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("N/A");
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (eventData.WindupPoseContactOffsets != null)
                            EditorGUILayout.LabelField("Windup Pose Count: " + eventData.WindupPoseContactOffsets.Length);
                        else
                            EditorGUILayout.LabelField("Windup Pose Count: N/A" );

                        if(eventData.RootContactOffset != null)
                            EditorGUILayout.LabelField("Contact Count: " + eventData.RootContactOffset.Length);
                        else
                            EditorGUILayout.LabelField("Contact Count: N/A");

                        GUILayout.Space(18f);

                        EditorGUILayout.LabelField("Windup: " + eventData.Windup.ToString("F2"));
                        EditorGUILayout.LabelField("Actions: ");

                        if (eventData.Actions != null)
                        {

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20f);
                            EditorGUILayout.BeginVertical();
                            {
                                for (int i = 0; i < eventData.Actions.Length; ++i)
                                    EditorGUILayout.LabelField("Action " + (i + 1).ToString() + "-   " + eventData.Actions[i]);
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20f);
                            {
                                EditorGUILayout.LabelField("N/A");
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.LabelField("Follow Through: " +  eventData.FollowThrough.ToString("F2"));
                        EditorGUILayout.LabelField("Recovery: " +  eventData.Recovery.ToString("F2"));

                        GUILayout.Space(18f);

                        EditorGUILayout.LabelField("Length: " + eventData.Length.ToString("F2"));
                        EditorGUILayout.LabelField("Time To Hit: " + eventData.TimeToHit.ToString("F2"));
                        EditorGUILayout.LabelField("Total Action Duration: " + eventData.TotalActionDuration.ToString("F2"));
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();

                GUILayout.BeginArea(column2Rect);
                m_runtimeEventDataScrollPos = EditorGUILayout.BeginScrollView(m_runtimeEventDataScrollPos);
                {
                    EditorGUILayout.LabelField("Runtime Event Data", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Type: " + m_currentFrame.EventType);
                        EditorGUILayout.LabelField("Phase: " + m_currentFrame.CurrentEventState);
                        EditorGUILayout.LabelField("Length: " + m_currentFrame.EventLength);
                        EditorGUILayout.LabelField("Priority: " + m_currentFrame.CurrentEventPriority);
                        EditorGUILayout.LabelField("Time Since Triggered: " + m_currentFrame.TimeSinceEventTriggered);
                        EditorGUILayout.LabelField("Exit With Motion: " + m_currentFrame.ExitWithMotion);
                        EditorGUILayout.LabelField("Start Time Offset: " + m_currentFrame.EventStartTimeOffset);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(18f);
                    EditorGUILayout.LabelField("Contacts", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Contacts Count to Warp: " + m_currentFrame.ContactCountToWarp);
                        EditorGUILayout.LabelField("Current Contact Index: " + m_currentFrame.CurrentEventContactIndex);
                        EditorGUILayout.LabelField("Current Contact Time: " + m_currentFrame.CurrentEventContactTime);

                        EditorGUILayout.LabelField("Current Contacts: ");

                        if (m_currentFrame.CurrentEventContacts != null && m_currentFrame.CurrentEventContacts.Length > 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20f);
                            EditorGUILayout.BeginVertical();
                            {
                                for (int i = 0; i < m_currentFrame.CurrentEventContacts.Length; ++i)
                                {
                                    EditorGUILayout.LabelField("Contact " + i);

                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(20f);
                                    EditorGUILayout.BeginVertical();
                                    {
                                        EditorGUILayout.LabelField("Position: " + m_currentFrame.CurrentEventContacts[i].Position);
                                        EditorGUILayout.LabelField("Rotation Y: " + m_currentFrame.CurrentEventContacts[i].RotationY);
                                    }
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20f);
                            {
                                EditorGUILayout.LabelField("N/A");
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(18f);
                    EditorGUILayout.LabelField("Warping", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Current Contact Root Pos: " + m_currentFrame.CurrentEventRootWorld.Position);
                        EditorGUILayout.LabelField("Current Contact Root RotY: " + m_currentFrame.CurrentEventRootWorld.RotationY);
                        EditorGUILayout.LabelField("Desired Contact Root Pos: " + m_currentFrame.DesiredEventRootWorld.Position);
                        EditorGUILayout.LabelField("Desired Contact Root RotY: " + m_currentFrame.DesiredEventRootWorld.RotationY);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();

                if (m_currentFrame.AnimatorState != EMxMStates.Event)
                    EditorGUI.EndDisabledGroup();
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawIdleData(Rect a_dataAreaRect)
        {
            if (m_currentFrame.Used)
            {
                if (m_currentFrame.AnimatorState != EMxMStates.Idle)
                    EditorGUI.BeginDisabledGroup(true);

                float columnWidth = 350f;
                Rect column1Rect = new Rect(a_dataAreaRect.x, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);
                Rect column2Rect = new Rect(a_dataAreaRect.x + columnWidth, a_dataAreaRect.y, columnWidth, a_dataAreaRect.height);

                GUI.Box(column1Rect, "");
                GUI.Box(column2Rect, "");



                GUILayout.BeginArea(column1Rect);
                m_curIdleDataScrollPos = EditorGUILayout.BeginScrollView(m_curIdleDataScrollPos);
                {
                    EditorGUILayout.LabelField("Current Idle Data", EditorStyles.boldLabel);

                    GUILayout.Space(18f);

                    ref IdleSetData currentIdle = ref m_currentFrame.CurrentIdleSet;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Tags: " + currentIdle.Tags.ToString());
                        EditorGUILayout.LabelField("Min Loops: " + currentIdle.MinLoops);
                        EditorGUILayout.LabelField("Max Loops: " + currentIdle.MaxLoops);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(18f);
                    EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.ObjectField("Primary Clip: ", m_targetMxMAnimator.CurrentAnimData.Clips[currentIdle.PrimaryClipId], typeof(AnimationClip), false);

                        EditorGUILayout.LabelField("Transition Clips");
                        
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.BeginVertical();
                        {
                            if (currentIdle.TransitionClipIds != null)
                            {
                                for (int k = 0; k < currentIdle.TransitionClipIds.Length; ++k)
                                {
                                    EditorGUILayout.ObjectField((k + 1).ToString() + ". ",
                                        m_targetMxMAnimator.CurrentAnimData.Clips[currentIdle.TransitionClipIds[k]], typeof(AnimationClip), false);
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.LabelField("Secondary Clips");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.BeginVertical();
                        {
                            if (currentIdle.TransitionClipIds != null)
                            {
                                for (int k = 0; k < currentIdle.SecondaryClipIds.Length; ++k)
                                {
                                    EditorGUILayout.ObjectField((k + 1).ToString() + ". ",
                                        m_targetMxMAnimator.CurrentAnimData.Clips[currentIdle.SecondaryClipIds[k]], typeof(AnimationClip), false);
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();

                GUILayout.BeginArea(column2Rect);
                m_idleStateScrollPos = EditorGUILayout.BeginScrollView(m_idleStateScrollPos);
                {
                    EditorGUILayout.LabelField("Idle State", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("State: " + m_currentFrame.CurrentIdleState.ToString());
                        EditorGUILayout.LabelField("Idle Set: " + m_currentFrame.CurrentIdleSetId);
                        EditorGUILayout.LabelField("Idle Start Delta: " + m_currentFrame.TimeSinceLastIdleStarted.ToString("F3"));
                        EditorGUILayout.LabelField("Loop Count: " + m_currentFrame.ChosenIdleLoopCount);
                        EditorGUILayout.LabelField("Idle Detect Timer: " + m_currentFrame.IdleDetectTimer);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();

                if (m_currentFrame.AnimatorState != EMxMStates.Idle)
                    EditorGUI.EndDisabledGroup();
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawLayerData(Rect a_dataAreaRect)
        {
            if (!m_currentFrame.Used)
                return;

            GUILayout.BeginArea(a_dataAreaRect);
            {
                EditorGUILayout.LabelField("Layer Count: " + (m_currentFrame.Layers.Length + 2), EditorStyles.boldLabel);

                float columnWidth = 300f;
                float rowHeight = 216f;
                float curY = 102f;
                int colIndex = 0;

                Rect mmLayerRect = new Rect(0f, 0f, columnWidth, 30f);
                GUI.Box(mmLayerRect, "");
                GUILayout.BeginArea(mmLayerRect);
                {
                    EditorGUILayout.LabelField("Layer 0: Motion Matching", EditorStyles.boldLabel);
                }
                GUILayout.EndArea();

                Rect mecanimLayerRect = new Rect(0f, 30f, columnWidth, 72f);
                GUI.Box(mecanimLayerRect, "");

                if (!m_currentFrame.MecanimLayer)
                    EditorGUI.BeginDisabledGroup(true);

                GUILayout.BeginArea(mecanimLayerRect);
                {
                    EditorGUILayout.LabelField("Layer 1: Mecanim", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("Active: " + m_currentFrame.MecanimLayer.ToString());
                        EditorGUILayout.LabelField("Weight: " + m_currentFrame.MecanimLayerWeight);
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Mask: ", GUILayout.Width(40f));
                            EditorGUILayout.ObjectField(m_currentFrame.MecanimMask, typeof(AvatarMask), false);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                GUILayout.EndArea();

                if (!m_currentFrame.MecanimLayer)
                    EditorGUI.EndDisabledGroup();

                float remainingHeight = a_dataAreaRect.height - 60f;
                for (int i = 0; i < m_currentFrame.Layers.Length; ++i)
                {
                    ref MxMDebugFrame.LayerData layerData = ref m_currentFrame.Layers[i];

                    float curHeight = rowHeight + layerData.SubLayerWeights.Length * 18;

                    Rect layerCardRect = new Rect(colIndex * columnWidth, curY,
                        columnWidth, curHeight);

                    
                    remainingHeight -= (rowHeight + curHeight);
                    if (remainingHeight < rowHeight)
                    {
                        remainingHeight = a_dataAreaRect.height;
                        curY = 0;
                        ++colIndex;
                    }
                    else
                    {
                        curY += curHeight;
                    }

                    GUI.Box(layerCardRect, "");

                    GUILayout.BeginArea(layerCardRect);
                    {
                        if (layerData.Weight < 0.001f)
                            EditorGUI.BeginDisabledGroup(true);

                        EditorGUILayout.LabelField("Layer " + (i+2), EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Weight: " + layerData.Weight);
                            EditorGUILayout.LabelField("Additive: " + layerData.Additive);
                            EditorGUILayout.LabelField("Time: " + layerData.Time);
                            EditorGUILayout.LabelField("Max Clips: " + layerData.MaxClips);
                            EditorGUILayout.LabelField("Primary Input: " + layerData.PrimaryInputId);
                            EditorGUILayout.LabelField("Transition Rate: " + layerData.TransitionRate);

                            if (layerData.Clip != null)
                            {
                                EditorGUILayout.LabelField("Type : AnimationClip");
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField("Primary Clip: ", GUILayout.Width(70f));
                                    EditorGUILayout.ObjectField(layerData.Clip, typeof(AnimationClip), false);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.LabelField("Type : Custom Playable");
                                EditorGUILayout.LabelField("Primary Clip: N/A", GUILayout.Width(70f));
                            }

                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("Mask: ", GUILayout.Width(35f));
                                EditorGUILayout.ObjectField(layerData.Mask, typeof(AvatarMask), false);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(18f);

                        EditorGUILayout.LabelField("Sub Layer Weights", EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.BeginVertical();
                        {
                            for(int k = 0; k < layerData.SubLayerWeights.Length; ++k)
                            {
                                EditorGUILayout.LabelField(k + ":    " + layerData.SubLayerWeights[k].ToString("F3"));
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        if (layerData.Weight < 0.001f)
                            EditorGUI.EndDisabledGroup();
                        
                    }
                    GUILayout.EndArea();
                }
            }
            GUILayout.EndArea();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DrawBlendSpaceData(Rect a_dataAreaRect)
        {
            GUILayout.BeginArea(a_dataAreaRect);
            {
                Rect blendSpaceRect = new Rect(30f, 40f, a_dataAreaRect.width - 60f, a_dataAreaRect.height - 90f);

                GUI.Box(blendSpaceRect, "");

                Rect labelRect = new Rect(blendSpaceRect.x - 18f, blendSpaceRect.y, 18f, 18f);

                GUI.Label(labelRect, "1");
                labelRect.y += blendSpaceRect.height / 2f - 9f;
                GUI.Label(labelRect, "0");
                labelRect.y = blendSpaceRect.y + blendSpaceRect.height - 18f;
                GUI.Label(labelRect, "-1");
                labelRect.y += labelRect.height;
                labelRect.x += labelRect.width;
                GUI.Label(labelRect, "-1");
                labelRect.x += blendSpaceRect.width / 2f - 9f;
                GUI.Label(labelRect, "0");
                labelRect.x = blendSpaceRect.x + blendSpaceRect.width - 18f;
                GUI.Label(labelRect, "1");

                float spacingH = blendSpaceRect.width / 10f;
                float spacingV = blendSpaceRect.height / 10f;

                float top = blendSpaceRect.y;
                float bottom = blendSpaceRect.y + blendSpaceRect.height;
                float left = blendSpaceRect.x;
                float right = blendSpaceRect.x + blendSpaceRect.width;

                Handles.color = Color.grey;
                for (int i = 1; i < 10; ++i)
                {
                    float horizontal = i * spacingH + blendSpaceRect.x;
                    float vertical = i * spacingV + blendSpaceRect.y;

                    Handles.DrawLine(new Vector3(horizontal, top), new Vector3(horizontal, bottom));
                    Handles.DrawLine(new Vector3(left, vertical), new Vector3(right, vertical));
                }

                Handles.color = Color.black;
                Handles.DrawLine(new Vector3(blendSpaceRect.x + blendSpaceRect.width / 2f, top),
                    new Vector3(blendSpaceRect.x + blendSpaceRect.width / 2f, bottom));

                Handles.DrawLine(new Vector3(left, blendSpaceRect.y + blendSpaceRect.height / 2f),
                    new Vector3(right, blendSpaceRect.y + blendSpaceRect.height / 2f));

                if(m_currentFrame.Used)
                {
                    Vector2 blendSpaceRatio = new Vector2(2f / blendSpaceRect.width, 2f / blendSpaceRect.height);

                    Texture blendKey = EditorGUIUtility.IconContent("blendKey").image;

#if UNITY_2019_4_OR_NEWER
                    Texture previewPointTex = EditorGUIUtility.IconContent("P4_AddedLocal").image;
                    Texture desiredPointTex = EditorGUIUtility.IconContent("P4_AddedRemote").image;
#else
                    Texture previewPointTex = EditorGUIUtility.IconContent("d_P4_AddedLocal").image;
                    Texture desiredPointTex = EditorGUIUtility.IconContent("d_P4_AddedRemote").image;
#endif
                    Vector2 centerPos = blendSpaceRect.position;
                    centerPos.x += blendSpaceRect.width / 2f;
                    centerPos.y += blendSpaceRect.height / 2f;

                    Vector3 previewDrawPos = m_currentFrame.BlendSpacePosition;
                    previewDrawPos.y *= -1f;

                    Vector2 iconSize = new Vector2(18f, 18f);
                    Rect animDrawRect = new Rect((previewDrawPos / blendSpaceRatio) + centerPos - (iconSize / 2f), iconSize);
                    GUI.DrawTexture(animDrawRect, previewPointTex);

                    previewDrawPos = m_currentFrame.DesiredBlendSpacePosition;
                    previewDrawPos.y *= -1f;
                    animDrawRect.position = (previewDrawPos / blendSpaceRatio) + centerPos - (iconSize) / 2f;
                    GUI.DrawTexture(animDrawRect, desiredPointTex);
                }
            }
            GUILayout.EndArea();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void ManageInputs(Rect a_graphMainRect)
        {
            Event evt = Event.current;

            if(evt.isMouse)
            {
                switch (evt.type)
                {
                    case EventType.MouseDrag:
                    case EventType.MouseDown:
                        {
                            if(a_graphMainRect.Contains(evt.mousePosition))
                            {
                                int frameShift = m_debugFrameData.Length - m_lastRecordedFrameId;

                                float timelineX = evt.mousePosition.x - a_graphMainRect.x;
                                float timelineRatio = timelineX / a_graphMainRect.width;

                                m_playheadFrame = Mathf.RoundToInt(timelineRatio * m_debugFrameData.Length) - frameShift;

                                if (m_playheadFrame < 0)
                                    m_playheadFrame += m_debugFrameData.Length;

                                if(m_playheadFrame >= m_debugFrameData.Length)
                                    m_playheadFrame -= m_debugFrameData.Length;

                                if (m_previewOn)
                                    m_targetMxMAnimator.SetDebugPreviewFrame(m_playheadFrame);

                                m_playing = false;

                                Repaint();
                            }
                        }
                        break;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void StopRecording()
        {
            if (m_recordOn)
            {
                m_targetMxMAnimator.StopRecordAnalytics();
                m_recordOn = false;
            }
        }

    }//End of class: MxMDebuggerWindow
}//End of namespace: MxMEditor