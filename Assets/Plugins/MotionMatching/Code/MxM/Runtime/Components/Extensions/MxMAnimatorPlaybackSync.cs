using MxM;
using UnityEngine;

public class MxMAnimatorPlaybackSync : MonoBehaviour, IMxMExtension
{
    private MxMAnimator m_mxmAnimator;
    private Animator m_animator;

    public bool IsEnabled { get { return enabled; } }
    public bool DoUpdatePhase1 { get { return true; } }
    public bool DoUpdatePhase2 { get { return false; } }
    public bool DoUpdatePost { get { return false; } }

    public void Initialize()
    {
        m_mxmAnimator = GetComponent<MxMAnimator>();

        if (m_mxmAnimator == null)
        {
            Debug.LogError("MxMAnimatorPlaybackSync: Trying to initialize MxM extension but cannot find MxMAnimator component. " +
                "Please make sure the MxMAnimator is on the same gameobject as the MxMAnimatorPlaybackSync extension.");
            enabled = false;
        }

        m_animator = m_mxmAnimator.UnityAnimator;

        if (m_animator == null)
        {
            Debug.LogError("MxMAnimatorPlaybackSync: Trying to initialize MxM extension but cannot find Animator component.");
            enabled = false;
        }
    }

    public void UpdatePhase1()
    {
        if(Mathf.Abs(m_mxmAnimator.PlaybackSpeed - m_animator.speed) > Mathf.Epsilon)
        {
            m_mxmAnimator.PlaybackSpeed = m_animator.speed;
        }
    }

    public void Terminate() { }
    public void UpdatePhase2() { }
    public void UpdatePost() { }
}
