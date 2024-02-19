using UnityEngine;
using MxM;
using Cinemachine;

namespace MxMExamples
{
    public class HelpUIControl : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] m_objectsToHide = null;

        [SerializeField]
        private MxMAnimator[] m_characters = null;

        [SerializeField]
        private UnityEngine.UI.Button[] m_profileButtons = null;

        [SerializeField]
        private CinemachineFreeLook m_freeLookCamera = null;

        private bool m_uiHidden = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                m_uiHidden = !m_uiHidden;

                for (int i = 0; i < m_objectsToHide.Length; ++i)
                {
                    m_objectsToHide[i].SetActive(m_uiHidden);
                }
            }
        }

        public void SetActiveCharacter(int a_characterId)
        {
            for(int i = 0; i < m_characters.Length; ++i)
            {
                if (i == a_characterId)
                {
                    MxMAnimator character = m_characters[i];

                    character.gameObject.SetActive(true);
                    m_freeLookCamera.LookAt = character.transform;
                    m_freeLookCamera.Follow = character.transform;

                    m_profileButtons[i].image.color = Color.yellow;
                }
                else
                {
                    m_characters[i].gameObject.SetActive(false);
                    m_profileButtons[i].image.color = Color.white;
                }
            }
        }
    }
}
