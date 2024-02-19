using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;


namespace MxM
{
    public class MxMSearchManager : MonoBehaviour
    {
        [FormerlySerializedAs("m_maxUpdatesPerFrame")] [SerializeField] private int m_maxSearchesPerFrame;
        [SerializeField] private float m_maxAllowableDelay;
        [SerializeField] private int m_expectedAnimatorCount;
        [SerializeField] private int m_expectedPhysicsAnimatorCount;
        
        public static MxMSearchManager Instance { get; private set; } = null;

        private List<MxMAnimator> m_mxmAnimators;
        private List<MxMAnimator> m_fixedUpdateMxMAnimators;

        private int m_searchesThisFrame = 0;
        private int m_animatorIndex = 0;
        private int m_fixedAnimatorIndex = 0;

        void IncrementAnimatorIndex()
        {
            ++m_animatorIndex;
            if (m_animatorIndex >= m_mxmAnimators.Count)
            {
                m_animatorIndex = 0;
            }
        }

        void IncrementFixedUpdateAnimatorIndex()
        {
            ++m_fixedAnimatorIndex;
            if (m_fixedAnimatorIndex >= m_fixedUpdateMxMAnimators.Count)
            {
                m_fixedAnimatorIndex = 0;
            }
        }

        
        // Start is called before the first frame update
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning(
                    "Attempting to create an MxMSearchManager but one already exists and only one is allowed.");
                Destroy(this);
                return;
            }

            Instance = this;

            m_mxmAnimators = new List<MxMAnimator>(m_expectedAnimatorCount);
            m_fixedUpdateMxMAnimators = new List<MxMAnimator>(m_expectedPhysicsAnimatorCount);

        }

        // Update is called once per frame
        void Update()
        {
            if (m_mxmAnimators.Count == 0)
                return;
            
            //foreach (MxMAnimator mxmAnimator in m_mxmAnimators)
            int startIndex = m_animatorIndex;
            for(int i = 0; i < m_mxmAnimators.Count; ++i)
            {
                int thisIndex = WrapIndex(startIndex + i, m_mxmAnimators.Count);
                
                MxMAnimator mxmAnimator = m_mxmAnimators[thisIndex];
                if (mxmAnimator && mxmAnimator.CanUpdate)
                {
#if UNITY_2019_1_OR_NEWER                    
                    mxmAnimator.CacheRiggingIntegration();
#endif                 
                    mxmAnimator.MxMUpdate_Phase1(Time.deltaTime);
                }
            }
            
            //Only Schedule the jobs in batch once all animators have updated
            JobHandle.ScheduleBatchedJobs();
        }

        int WrapIndex(int a_index, int a_maxIndex)
        {
            return a_index >= a_maxIndex ? a_index - a_maxIndex : a_index;
        }
        
        void FixedUpdate()
        {
            if (m_fixedUpdateMxMAnimators.Count == 0)
                return;
            
            for(int i = 0; i < m_fixedUpdateMxMAnimators.Count; ++i)
            {
                MxMAnimator mxmAnimator = m_fixedUpdateMxMAnimators[m_fixedAnimatorIndex];
                if (mxmAnimator && mxmAnimator.CanUpdate)
                {
#if UNITY_2019_1_OR_NEWER                    
                    mxmAnimator.CacheRiggingIntegration();
#endif                 
                    mxmAnimator.MxMUpdate_Phase1(Time.fixedDeltaTime);
                }
            }
            
            //Only Schedule the jobs in batch once all animators have updated
            JobHandle.ScheduleBatchedJobs();
        }

        void UpdatePhase2()
        {
            
        }

        void LateUpdate()
        {
            foreach (MxMAnimator mxmAnimator in m_mxmAnimators)
            {
                mxmAnimator.MxMLateUpdate();
            }

            foreach (MxMAnimator mxmAnimator in m_fixedUpdateMxMAnimators)
            {
                mxmAnimator.MxMLateUpdate();
            }


            m_searchesThisFrame = 0;
        }

        public void RegisterMxMAnimator(MxMAnimator a_mxmAnimator)
        {
            if (!a_mxmAnimator)
                return;

            if (a_mxmAnimator.UpdateMode == AnimatorUpdateMode.AnimatePhysics)
            {
                m_fixedUpdateMxMAnimators.Add(a_mxmAnimator);
            }
            else
            {
                m_mxmAnimators.Add(a_mxmAnimator);
            }
        }

        public void UnRegisterMxMAnimator(MxMAnimator a_mxmAnimator)
        {
            if (!a_mxmAnimator)
                return;
            
            if (a_mxmAnimator.UpdateMode == AnimatorUpdateMode.AnimatePhysics)
            {
                m_fixedUpdateMxMAnimators.Remove(a_mxmAnimator);
            }
            else
            {
                m_mxmAnimators.Remove(a_mxmAnimator);
            }
        }

        public bool RequestPoseSearch(MxMAnimator a_mxmAnimator, float a_searchDelay, bool a_forceSearch)
        {
            if (!a_mxmAnimator)
                return false;

            if (a_forceSearch                                       //Force Search If Requested
                || a_mxmAnimator.PriorityUpdate                     //Force Search If Priority Animator
                || m_searchesThisFrame < m_maxSearchesPerFrame      //Allow Search If max has not been reached
                || a_searchDelay > Mathf.Min(m_maxAllowableDelay, a_mxmAnimator.MaxUpdateDelay)) //Allow search if max delay has been exceeded
            {
                IncrementAnimatorIndex();
                ++m_searchesThisFrame;
                return true;
            }

            return false;
        }
    }
}