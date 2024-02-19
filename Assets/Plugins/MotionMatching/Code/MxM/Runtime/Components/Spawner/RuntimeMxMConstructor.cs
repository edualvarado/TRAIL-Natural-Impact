using System.Collections.Generic;
using UnityEngine;
using MxM;
using MxMEditor;
using MxMGameplay;

public class RuntimeMxMConstructor : MonoBehaviour
{
    
    [SerializeField] protected MxMPreProcessData m_preProcessData = null; //Leave bland if you want it to generate automatically
    
    [SerializeField] protected MotionMatchConfigModule m_configModule = null;
    [SerializeField] protected TagNamingModule m_tagNamingModule = null;
    [SerializeField] protected EventNamingModule m_eventNamingModule = null;
    [SerializeField] protected CalibrationModule m_calibrationModule = null;
    [SerializeField] protected List<AnimationModule> m_animationModules = new List<AnimationModule>();
    [SerializeField] protected TrajectoryGeneratorModule m_trajGenModule = null;
    [SerializeField] protected MxMAnimData m_animData = null;
    [SerializeField] protected GameObject m_characterModel = null;
    
    [SerializeField] protected bool m_runtimePreProcess = false;
    
    private void OnEnable()
    {
        //Generate the character model
        if (!GenerateCharacterModel())
        {
            Debug.LogError("RuntimeMxMConstructor [OnEnable] - Failed to setup character correctly");
        }
        
        //Setup the pre-processor
        if (m_preProcessData == null)
            m_preProcessData = ScriptableObject.CreateInstance<MxMPreProcessData>();

        if (m_configModule != null)
            m_preProcessData.OverrideConfigModule = m_configModule;

        if (m_tagNamingModule != null)
            m_preProcessData.OverrideTagModule = m_tagNamingModule;

        if (m_eventNamingModule != null)
            m_preProcessData.OverrideEventModule = m_eventNamingModule;

        if (m_characterModel)
        {
            m_preProcessData.Prefab = m_characterModel;

            if (m_configModule != null)
            {
                m_configModule.Prefab = m_characterModel;
            }
        }

        if (m_animationModules != null && m_animationModules.Count > 0)
        {
            m_preProcessData.AddAnimationModules(m_animationModules.ToArray());
        }

        if (m_runtimePreProcess)
        {
            //Pre-process data here
            m_animData = m_preProcessData.PreProcessAnimationDataRuntime();

            if (m_animData == null)
            {
                Debug.LogError("RuntimeMxMConstructor [OnEnable] - Failed to pre-process MM animation data.");
                enabled = false;
                return;
            }
        }

        //Setup the new character
        if (!SetupCharacter())
        {
            Debug.LogError("RuntimeMxMConstructor [OnEnable] - Failed to setup character correctly.");
            enabled = false;
            return;
        }
        
        enabled = false;
    }
    
    protected virtual bool SetupCharacter()
    {
        if (!m_characterModel)
        {
            GenerateCharacterModel();
        }

        if (m_characterModel == null)
        {
            Debug.LogError("RuntimeMxMConstructor [SetupCharacter] - Failed to setup character. Character Model" +
                           "is null");
            return false;
        }


        GameObject characterModel = GameObject.Instantiate(m_characterModel, transform.position, transform.rotation);

        CharacterController charController = GetComponent<CharacterController>();

        characterModel.transform.parent = this.transform;
        characterModel.transform.localPosition =
            new Vector3(0f, -(charController.height) / 2.0f, 0.0f);
        characterModel.transform.localRotation = Quaternion.identity;
        
        Animator animator = characterModel.GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("RuntimeMxMConstructor [SetupCharacter] - Failed to setup character. Could not find " +
                           "animator component on character");
            return false;
        }

        
        SetupControllerWrapper(gameObject);
        SetupRootMotionApplicator(animator.gameObject, gameObject);
        SetupMxMAnimator(animator.gameObject);
        SetupTrajectoryGenerator(animator.gameObject);
        
        
        
        
        return true;
    }
    
    protected virtual bool GenerateCharacterModel()
    {
        //This should be overriden to generate or get your character model. 
        //Simply set the m_characterModel game object variable here with your generated 
        //model.

        return m_characterModel != null;
    }

    protected virtual void SetupMxMAnimator(GameObject a_gameObject)
    {
        //This should be overriden to setup your MxMAnimator in the way that you desire
        
        if (!gameObject)
        {
            Debug.LogError("RuntimeMxMConstructor [SetupMxMAnimator] - failed to setup the MxMAnimator" +
                           "the specified game object is null.");
        }
        
        MxMAnimator mxmAnimator = a_gameObject.AddComponent<MxMAnimator>();
        mxmAnimator.AnimData = new MxMAnimData[1];
        mxmAnimator.AnimData[0] = m_animData;
        //mxmAnimator.CurrentAnimData = m_animData;
        mxmAnimator.OverrideCalibration = m_calibrationModule;
        mxmAnimator.AnimationRoot = transform;
    }

    protected virtual void SetupTrajectoryGenerator(GameObject a_gameObject)
    {
        //This should be overriden to setup your trajectory generator in the way that you desire
       
        if (gameObject == null)
        {
            Debug.LogError("RuntimeMxMConstructor [SetupTrajectoryGenerator] - failed to setup the trajectory generator" +
                           "the specified game object is null.");
        }
        
        MxMTrajectoryGenerator trajectoryGenerator = a_gameObject.AddComponent<MxMTrajectoryGenerator>();

        if (m_trajGenModule != null)
        {
            trajectoryGenerator.SetTrajectoryModule(m_trajGenModule);
        }
    }

    protected virtual void SetupControllerWrapper(GameObject a_gameObject)
    {
        if (a_gameObject == null )
        {
            Debug.LogError("RuntimeMxMConstructor [SetupControllerWrapper] - failed to setup the controller " +
                           "wrapper, the specified game objects are null.");
        }

        a_gameObject.AddComponent<UnityControllerWrapper>();
    }

    protected virtual void SetupRootMotionApplicator(GameObject a_gameObject, GameObject a_controlGameObject)
    {
        if (gameObject == null|| a_controlGameObject == null)
        {
            Debug.LogError("RuntimeMxMConstructor [SetupRootMotionApplicator] - failed to setup the root motion applicator" +
                           "the specified game object is null.");
        }

        MxMRootMotionApplicator rootMotionApplicator = a_gameObject.AddComponent<MxMRootMotionApplicator>();
        rootMotionApplicator.EnableGravity = true;
        rootMotionApplicator.ControllerWrapper = a_controlGameObject.GetComponentInChildren<UnityControllerWrapper>();
        rootMotionApplicator.RootTransform = transform;
    }
}


    
