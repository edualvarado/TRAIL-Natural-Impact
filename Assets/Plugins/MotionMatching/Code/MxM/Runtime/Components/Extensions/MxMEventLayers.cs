using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace MxM
{

    public enum EEventLayerStatus
    {
        Inactive,
        Active,
        Cancelled
    }

    public struct EventLayerData
    {
        public EEventLayerStatus LayerStatus;
        public AnimationClipPlayable ClipPlayable;
        public float StartOffset;
        public float StartTime;
        public float EndTime;
        public float CancelTime;
        public float BlendInTime;
        public float BlendOutTime;
        public float PlaybackRate;
        public float Weight;
        public AvatarMask Mask;

    }//End of struct EventLayerData

    [RequireComponent(typeof(MxMAnimator))]
    public class MxMEventLayers : MonoBehaviour, IMxMExtension
    {
        [SerializeField]
        private bool m_applyHumanoidFootIK = true;

        [SerializeField]
        private int m_maxActiveEvents = 4;
        
        [SerializeField]
        private bool m_isAdditive;

        private AnimationLayerMixerPlayable m_layerMixer;
        private MxMLayer m_baseLayer;
        private EventLayerData[] m_eventLayers;
        private MxMAnimator m_mxmAnimator;

        private int m_layerId = 2;
        
        private int m_currentEventSlotId = -1;
        
        [System.Serializable] 
        public class UnityEvent_LayerEventComplete : UnityEvent<int> { };    //Custom Unity event for triggering callbacks when events are complete
        
        [Header("Callbacks")]
        [SerializeField] private UnityEvent_LayerEventComplete m_onLayerEventComplete = new UnityEvent_LayerEventComplete();                   //Unity event called when an Event (MxM Action Event) is completed
        
        public UnityEvent_LayerEventComplete OnLayerEventComplete
        {
            get { return m_onLayerEventComplete; }
        }
        
        public int ActiveEventCount { get; private set; }
        public int CurrentEventId { get; private set; }
        public bool IsLayerEventActive { get { return ActiveEventCount >= 0; }}
        
        public EEventState CurrentEventState 
        { 
            get 
            {
                MxMAnimData animData = m_mxmAnimator.CurrentAnimData;

                if (CurrentEventId > -1 && m_currentEventSlotId > -1 &&  CurrentEventId < animData.Events.Length)
                {
                    ref EventData eventData = ref animData.Events[CurrentEventId];
                    ref EventLayerData eventLayer = ref m_eventLayers[m_currentEventSlotId];

                    float curTime = (float)eventLayer.ClipPlayable.GetTime();

                    float timePassed = curTime - eventLayer.StartTime - eventLayer.StartOffset;

                    if(timePassed < eventData.Windup)
                    {
                        return EEventState.Windup;
                    }
                    else if(timePassed < (eventData.Windup + eventData.TotalActionDuration))
                    {
                        return EEventState.Action;
                    }
                    else if(timePassed < (eventData.Windup + eventData.TotalActionDuration + eventData.FollowThrough))
                    {
                        return EEventState.FollowThrough;
                    }
                    else
                    {
                        return EEventState.Recovery;
                    }
                }

                return EEventState.Recovery;
            } 
        }

        public MxMLayer BaseLayer
        {
            get => m_baseLayer;
        }
        

        public bool IsEnabled { get { return enabled; } }
        public bool DoUpdatePhase1 { get { return false; } }
        public bool DoUpdatePhase2 { get { return true; } }
        public bool DoUpdatePost { get { return false; } }

        public void Initialize()
        {
            CurrentEventId = -1;
            
            m_mxmAnimator = GetComponent<MxMAnimator>();

            if(m_mxmAnimator == null)
            {
                Debug.LogError("Could not find MxMAnimator component, MxMEventLayers component disabled");
                enabled = false;
                return;
            }

            m_layerMixer = AnimationLayerMixerPlayable.Create(m_mxmAnimator.MxMPlayableGraph, m_maxActiveEvents);

            m_layerId = m_mxmAnimator.AddLayer((Playable)m_layerMixer, 0f, m_isAdditive, null);
            m_baseLayer = m_mxmAnimator.GetLayer(m_layerId);
            m_mxmAnimator.SetLayerWeight(m_layerId, 0f);

            m_eventLayers = new EventLayerData[m_maxActiveEvents];
        }

        public void UpdatePhase2()
        {
            float highestWeight = 0f;
            //float totalWeight = 0.0f;
            int numActiveLayers = 0;
            for (int i = 0; i < m_eventLayers.Length; ++i)
            {
                ref EventLayerData eventLayer = ref m_eventLayers[i];

                if (eventLayer.LayerStatus == EEventLayerStatus.Inactive)
                    continue;

                ++numActiveLayers;

                float curTime = (float)eventLayer.ClipPlayable.GetTime();

                float age = curTime - eventLayer.StartTime;
                float deathAge = eventLayer.EndTime - curTime;

                float weight = Mathf.Min(Mathf.Clamp01(age / eventLayer.BlendInTime), Mathf.Clamp01(deathAge / eventLayer.BlendOutTime));

               // totalWeight += Mathf.Max(0.0f, weight);

                if (weight > highestWeight)
                    highestWeight = weight;

                if (curTime >= eventLayer.EndTime)
                {
                    m_layerMixer.SetInputWeight(i, 0f);
                    m_layerMixer.DisconnectInput(i);
                    eventLayer.ClipPlayable.Destroy();
                    eventLayer.LayerStatus = EEventLayerStatus.Inactive;
                    
                    if(i == m_currentEventSlotId)
                    {
                        CurrentEventId = -1;
                        m_currentEventSlotId = -1;
                    }
                    
                    m_onLayerEventComplete.Invoke(numActiveLayers - 1);
                }
                else if (Mathf.Abs(weight - eventLayer.Weight) > Mathf.Epsilon)
                {
                    eventLayer.Weight = weight;
                    m_layerMixer.SetInputWeight(i, weight);
                }
            }

            ActiveEventCount = numActiveLayers;

            float weightAdjust = (1.0f / Mathf.Max(0.01f, highestWeight));

            //Second Pass to normalize weights
            for (int i = 0; i < m_eventLayers.Length; ++i)
            {
               ref  EventLayerData eventLayer = ref m_eventLayers[i];

                if (eventLayer.LayerStatus == EEventLayerStatus.Inactive)
                    continue;

                m_layerMixer.SetInputWeight(i, eventLayer.Weight * weightAdjust);
            }

            //Set the overall layer weight for the event layers layer in MxM
            if (numActiveLayers == 0)
            {
                m_mxmAnimator.SetLayerWeight(m_layerId, 0.0f);
            }
            else
            {
                //totalWeight /= numActiveLayers;
                m_mxmAnimator.SetLayerWeight(m_layerId, Mathf.Clamp01(highestWeight));

            } 
        }

        public void Terminate() { }

        public void UpdatePhase1() { }
        
        public void UpdatePost() { }

        public void BeginEvent(MxMEventDefinition a_eventDefinition, AvatarMask a_eventMask, float a_blendInTime,
            float a_playbackRate = 1f)
        {
            BeginEvent(a_eventDefinition, a_eventMask, a_blendInTime, a_blendInTime, a_playbackRate);
        }

        //============================================================================================
        /**
        *  @brief Begins an event that is masked by a passed avatar mask (e.g. upper body event).
        *  
        *  Note that masked events cannot use animation warping and other dynamic features that normal
        *  events use.
        *  
        *  @param [MxMEventDefinition] a_eventDefinition - the event definition to use
        *  @param [AvatarMask] a_eventMask - the mask to use for the event
        *         
        *********************************************************************************************/
        public void BeginEvent(MxMEventDefinition a_eventDefinition, AvatarMask a_eventMask, 
            float a_blendInTime, float a_blendOutTime, float a_playbackRate = 1f)
        {
            if (a_eventDefinition == null)
            {
                Debug.LogError("Trying to play a layered event but the passed event definition is null.");
                return;
            }

            MxMAnimData currentAnimData = m_mxmAnimator.CurrentAnimData;

            int bestEventId = -1;
            float bestCost = float.MaxValue;
            int bestWindupPoseId = 0;
            ref EventData bestEvent = ref currentAnimData.Events[0];
            
            float desiredDelay = a_eventDefinition.DesiredDelay;
            bool eventFound = false;

            for (int i = 0; i < currentAnimData.Events.Length; ++i)
            {
                ref EventData evt = ref currentAnimData.Events[i];

                if (evt.EventId != a_eventDefinition.Id)
                    continue;
                    
                for (int k = 0; k < evt.WindupPoseContactOffsets.Length; ++k)
                {
                    ref PoseData comparePose = ref currentAnimData.Poses[evt.StartPoseId + k];

                    if (a_eventDefinition.MatchRequireTags)
                    {
                        ETags evtTags = (comparePose.Tags & (~ETags.DoNotUse));

                        if (evtTags != m_mxmAnimator.RequiredTags)
                            continue;
                    }
                    
                    float cost = 0f;

                    if (a_eventDefinition.MatchPose)
                    {
                        cost += m_mxmAnimator.ComputePoseCost(ref comparePose);
                    }

                    if (a_eventDefinition.MatchTrajectory)
                    {
                        cost += m_mxmAnimator.ComputeTrajectoryCost(ref comparePose);
                    }

                    if (a_eventDefinition.MatchTiming)
                    {
                        float timeWarp = Mathf.Abs(desiredDelay - evt.TimeToHit + currentAnimData.PoseInterval * k);
                        cost += timeWarp * a_eventDefinition.TimingWeight;
                    }

                    cost *= comparePose.Favour;
                    
                    if (comparePose.FavourTags == m_mxmAnimator.FavourTags)
                    {
                        cost *= m_mxmAnimator.FavourMultiplier;
                    }
                    
                    if (cost < bestCost)
                    {
                        bestWindupPoseId = k;
                        bestCost = cost;
                        bestEvent = ref evt;
                        bestEventId = i;
                        eventFound = true;
                    }
                }
            }

            if (!eventFound)
            {
                Debug.LogWarning("Could not find an event to match event definition: " + a_eventDefinition.ToString());
                return;
            }

            CurrentEventId = bestEventId;

            ref PoseData pose = ref currentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];
            AnimationClip clip = currentAnimData.Clips[pose.PrimaryClipId];

            var clipPlayable = AnimationClipPlayable.Create(m_mxmAnimator.MxMPlayableGraph, clip);
            clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);

            int slotId = FindEmptySlot();
            m_currentEventSlotId = slotId;

            //Setup the event layer here
            ref EventLayerData newEventLayer = ref m_eventLayers[slotId];

            if(newEventLayer.ClipPlayable.IsValid())
            {
                m_layerMixer.DisconnectInput(slotId);
                newEventLayer.ClipPlayable.Destroy();
            }

            newEventLayer.LayerStatus = EEventLayerStatus.Active;
            newEventLayer.ClipPlayable = clipPlayable;
            newEventLayer.StartOffset = pose.Time - currentAnimData.Poses[bestEvent.StartPoseId].Time;
            newEventLayer.StartTime = pose.Time;
            newEventLayer.EndTime = pose.Time + bestEvent.Length - (bestWindupPoseId * currentAnimData.PoseInterval);
            newEventLayer.CancelTime = 99999.0f;
            newEventLayer.BlendInTime = a_blendInTime;
            newEventLayer.BlendOutTime = a_blendOutTime;
            newEventLayer.PlaybackRate = a_playbackRate;
            newEventLayer.Mask = a_eventMask;
            newEventLayer.Weight = 0f;

            m_layerMixer.ConnectInput(slotId, newEventLayer.ClipPlayable, 0);
            m_layerMixer.SetLayerAdditive((uint)slotId, m_isAdditive);
            m_layerMixer.SetInputWeight(slotId, 0.001f);
            m_layerMixer.SetLayerMaskFromAvatarMask((uint)slotId, a_eventMask);

            clipPlayable.SetTime(newEventLayer.StartTime);
            clipPlayable.SetTime(newEventLayer.StartTime);
            clipPlayable.SetSpeed(a_playbackRate * m_mxmAnimator.PlaybackSpeed);

            m_baseLayer.Mask = newEventLayer.Mask;

            ++ActiveEventCount;
        }

        //============================================================================================
        /**
        *  @brief Begins an event that is masked by a passed avatar mask (e.g. upper body event).
        *  
        *  Note that masked events cannot use animation warping and other dynamic features that normal
        *  events use.
        *  
        *  @param [string] a_eventName - the name of the event to use
        *  @param [AvatarMask] a_eventMask - the mask to use for the event
        *         
        *********************************************************************************************/
        public void BeginEvent(string a_eventName, AvatarMask a_eventMask, float a_blendTime,
            bool a_matchPose = true, bool a_matchTrajectory = true, float a_playbackRate = 1f)
        {
            MxMAnimData currentAnimData = m_mxmAnimator.CurrentAnimData;

            if (currentAnimData == null)
                return;

            BeginEvent(currentAnimData.EventIdFromName(a_eventName), a_eventMask, a_blendTime, 
                a_matchPose, a_matchTrajectory, a_playbackRate);
        }

        //============================================================================================
        /**
        *  @brief Begins an event that is masked by a passed avatar mask (e.g. upper body event).
        *  
        *  Note that masked events cannot use animation warping and other dynamic features that normal
        *  events use.
        *  
        *  @param [int] a_eventDefinition - the id of the event to use
        *  @param [AvatarMask] a_eventMask - the mask to use for the event
        *         
        *********************************************************************************************/
        public void BeginEvent(int a_eventId, AvatarMask a_eventMask, float a_blendTime,
            bool a_matchPose = true, bool a_matchTrajectory = true, float a_playbackRate = 1f)
        {
            BeginEvent(a_eventId, a_eventMask, a_blendTime, a_blendTime, a_matchPose, a_matchTrajectory, a_playbackRate);
        }

        public void BeginEvent(int a_eventId, AvatarMask a_eventMask, float a_blendInTime, 
            float a_blendOutTime, bool a_matchPose = true, bool a_matchTrajectory = true, float a_playbackRate = 1f)
        {
            if (a_eventId < 0 || a_eventId > m_mxmAnimator.CurrentAnimData.Events.Length)
            {
                Debug.LogError("Trying to play a layered event but the Id: " + a_eventId + " is out of bounds");
                return;
            }

            MxMAnimData currentAnimData = m_mxmAnimator.CurrentAnimData;

            int bestEventId = -1;
            float bestCost = float.MaxValue;
            int bestWindupPoseId = 0;
            ref EventData bestEvent = ref currentAnimData.Events[0];
            Vector3 localPosition = Vector3.zero;

            float playerRotationY = m_mxmAnimator.AnimationRoot.rotation.eulerAngles.y;
            bool eventFound = false;

            for (int i = 0; i < currentAnimData.Events.Length; ++i)
            {
                ref EventData evt = ref currentAnimData.Events[i];

                if (evt.EventId != a_eventId)
                    continue;

                for (int k = 0; k < evt.WindupPoseContactOffsets.Length; ++k)
                {
                    ref PoseData comparePose = ref currentAnimData.Poses[evt.StartPoseId + k];
                    
                    ETags evtTags = (comparePose.Tags & (~ETags.DoNotUse));
                    if (evtTags != m_mxmAnimator.RequiredTags)
                        continue;

                    float cost = 0f;

                    if (a_matchPose)
                    {
                        cost += m_mxmAnimator.ComputePoseCost(ref comparePose);
                    }

                    if (a_matchTrajectory)
                    {
                        cost += m_mxmAnimator.ComputeTrajectoryCost(ref comparePose);
                    }
                    
                    cost *= comparePose.Favour;

                    if (comparePose.FavourTags == m_mxmAnimator.FavourTags)
                    {
                        cost *= m_mxmAnimator.FavourMultiplier;
                    }
                    
                    if (cost < bestCost)
                    {
                        bestWindupPoseId = k;
                        bestCost = cost;
                        bestEvent = ref evt;
                        bestEventId = i;
                        eventFound = true;
                    }
                }
            }

            if (!eventFound)
            {
                Debug.LogWarning("Could not find an event to match event Id: " + a_eventId.ToString());
                return;
            }

            CurrentEventId = bestEventId;

            ref PoseData pose = ref currentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];
            AnimationClip clip = currentAnimData.Clips[pose.PrimaryClipId];

            var clipPlayable = AnimationClipPlayable.Create(m_mxmAnimator.MxMPlayableGraph, clip);
            clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);

            int slotId = FindEmptySlot();
            m_currentEventSlotId = slotId;

            //Setup the event layer here
            ref EventLayerData newEventLayer = ref m_eventLayers[slotId];

            if (newEventLayer.ClipPlayable.IsValid())
            {
                m_layerMixer.DisconnectInput(slotId);
                newEventLayer.ClipPlayable.Destroy();
            }

            newEventLayer.LayerStatus = EEventLayerStatus.Active;
            newEventLayer.ClipPlayable = clipPlayable;
            newEventLayer.StartOffset = pose.Time - currentAnimData.Poses[bestEvent.StartPoseId].Time;
            newEventLayer.StartTime = pose.Time;
            newEventLayer.EndTime = pose.Time + bestEvent.Length - (bestWindupPoseId * currentAnimData.PoseInterval);
            newEventLayer.CancelTime = 99999.0f;
            newEventLayer.BlendInTime = a_blendInTime;
            newEventLayer.BlendOutTime = a_blendOutTime;
            newEventLayer.PlaybackRate = a_playbackRate;
            newEventLayer.Mask = a_eventMask;
            newEventLayer.Weight = 0f;
            
            m_layerMixer.ConnectInput(slotId, newEventLayer.ClipPlayable, 0);
            m_layerMixer.SetLayerAdditive((uint)slotId, m_isAdditive);
            m_layerMixer.SetInputWeight(slotId, 0.001f);
            m_layerMixer.SetLayerMaskFromAvatarMask((uint)slotId, a_eventMask);

            clipPlayable.SetTime(newEventLayer.StartTime);
            clipPlayable.SetTime(newEventLayer.StartTime);
            clipPlayable.SetSpeed(a_playbackRate * m_mxmAnimator.PlaybackSpeed);

            m_baseLayer.Mask = newEventLayer.Mask;

            m_mxmAnimator.SetLayerWeight(m_layerId, 1.0f);

            ++ActiveEventCount;
        }

        private int FindEmptySlot()
        {
            int lowestSlotId = 0;
            float lowestWeight = float.MaxValue;

            for(int i = 0; i < m_eventLayers.Length; ++i)
            {
                
                ref EventLayerData EventLayer = ref m_eventLayers[i];

                if (EventLayer.LayerStatus == EEventLayerStatus.Inactive)
                    return i;

                if(EventLayer.Weight < lowestWeight)
                {
                    lowestSlotId = i;
                    lowestWeight = EventLayer.Weight;
                }
            }

            return lowestSlotId;
        }

        public void CancelAllEvents()
        {
            for(int i = 0; i < m_eventLayers.Length; ++i)
            {
                ref EventLayerData eventLayer = ref m_eventLayers[i];

                if (eventLayer.LayerStatus != EEventLayerStatus.Active)
                    continue;

                eventLayer.EndTime = Mathf.Min(eventLayer.EndTime, (float)eventLayer.ClipPlayable.GetTime() + (eventLayer.BlendOutTime * eventLayer.Weight));
                eventLayer.LayerStatus = EEventLayerStatus.Cancelled;
            }

            CurrentEventId = -1;
            m_currentEventSlotId = -1;
            ActiveEventCount = 0;
        }

        public void CancelAllEvents(float a_blendTime)
        {
            for (int i = 0; i < m_eventLayers.Length; ++i)
            {
                ref EventLayerData eventLayer = ref m_eventLayers[i];

                if (eventLayer.LayerStatus != EEventLayerStatus.Active)
                    continue;

                eventLayer.BlendInTime = a_blendTime;
                eventLayer.BlendOutTime = a_blendTime;
                eventLayer.EndTime = Mathf.Min(eventLayer.EndTime, (float)eventLayer.ClipPlayable.GetTime() + (a_blendTime * eventLayer.Weight));
                eventLayer.LayerStatus = EEventLayerStatus.Cancelled;
            }

            CurrentEventId = -1;
            m_currentEventSlotId = -1;
            ActiveEventCount = 0;
        }

    }//End of class: MxMEventLayers
}//End of namespace: MxM
