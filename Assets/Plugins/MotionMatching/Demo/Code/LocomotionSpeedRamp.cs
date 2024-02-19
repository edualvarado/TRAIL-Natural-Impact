using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;

public class LocomotionSpeedRamp : MonoBehaviour
{
    [SerializeField] private int m_currentSpeed = 0;
    [SerializeField] private float m_speedDowngradeTime = 0.3f;
    [SerializeField] private Transform m_playerTransform = null;
    [SerializeField] private float m_favourMultiplier = 0.6f;

    [SerializeField] private string m_runSpeedTagName = "Run";
    [SerializeField] private string m_sprintSpeedTagName = "Sprint";

    private float m_speedTimer = -1f;

    private MxMAnimator m_mxmAnimator;
    private MxMTrajectoryGenerator m_trajectoryGenerator;

    private ETags m_runTagHandle;
    private ETags m_sprintTagHandle;

    private void Start()
    {
        m_mxmAnimator = m_playerTransform.GetComponentInChildren<MxMAnimator>();
        m_trajectoryGenerator = m_playerTransform.GetComponentInChildren<MxMTrajectoryGenerator>();

        m_runTagHandle = m_mxmAnimator.CurrentAnimData.FavourTagFromName(m_runSpeedTagName);
        m_sprintTagHandle = m_mxmAnimator.CurrentAnimData.FavourTagFromName(m_sprintSpeedTagName);

        m_mxmAnimator.SetFavourMultiplier(m_favourMultiplier);
    }

    public void BeginSprint()
    {
        if (m_currentSpeed == 1)
            m_mxmAnimator.RemoveFavourTags(m_runTagHandle);

        m_currentSpeed = 2;

        m_mxmAnimator.AddFavourTags(m_sprintTagHandle);
    }

    public void ResetFromSprint()
    {
        if (m_currentSpeed != 2)
            return;

        m_currentSpeed = 1;
        m_speedTimer = 0f;

        float inputMag = m_trajectoryGenerator.InputVector.sqrMagnitude;

        if(inputMag > 0.7f * 0.7f)
        {
            m_mxmAnimator.AddFavourTags(m_runTagHandle);
        }

        m_mxmAnimator.RemoveFavourTags(m_sprintTagHandle);
    }

    // Update is called once per frame
    public void UpdateSpeedRamp()
    {
        float inputMag = m_trajectoryGenerator.InputVector.sqrMagnitude;

        switch (m_currentSpeed)
        {
            case 0: //Walk
                {
                    if(inputMag > 0.7f * 0.7f)
                    {
                        m_currentSpeed = 1;
                        m_mxmAnimator.AddFavourTags(m_runTagHandle);
                        m_mxmAnimator.SetFavourMultiplier(m_favourMultiplier);
                    }
                }
                break;
            case 1: //Run
                {
                    if(inputMag < 0.7f * 0.7f)
                    {
                        if(m_speedTimer < -Mathf.Epsilon)
                        {
                            m_speedTimer = 0f;
                        }

                        m_speedTimer += Time.deltaTime;

                        if(m_speedTimer > m_speedDowngradeTime)
                        {
                            m_mxmAnimator.RemoveFavourTags(m_runTagHandle);
                            m_mxmAnimator.SetFavourMultiplier(m_favourMultiplier);
                            m_currentSpeed = 0;
                            m_speedTimer = -1f;
                        }
                    }
                }
                break;
        }
    }
}
