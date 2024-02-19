// ================================================================================================
// File: MxMInputProfile.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-04-13: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief Input profile used to shape input in order to never provide an MxMAniamtor with 
    *  a trajectory that it doesn't have animations for.
    *  
    *  Essentially, this class simply remaps user input to ensure all trajectory speeds are 
    *  viable even when using an analog joystick.
    *         
    *********************************************************************************************/
    [CreateAssetMenu(fileName = "MxMInputProfile", menuName = "MxM/Utility/MxMInputProfile", order = 1)]
    public class MxMInputProfile : ScriptableObject
    {
        [SerializeField]
        private InputRemapSet[] m_viableInputs = null;
        //============================================================================================
        /**
        *  @brief Struct to represent the remapping of a specific range of input magnitudes to a 
        *  specific viable input.
        *         
        *********************************************************************************************/
        [System.Serializable]
        public struct InputRemapSet
        {
            public float minInput;
            public float maxInput;

            public float viableInput;

            public float posBias;
            public float dirBias;
        }

        //============================================================================================
        /**
        *  @brief Takes raw input and remaps it based on the InputRemapSets stored in m_viableInputs. 
        *  Instead of the remapping being a new input vector, it is a scale factor for the raw input
        *  vector to make it viable. It also outputs a new position responsivity (posBias) and 
        *  direction responsivity (dirBias) to use within that viable input.
        *  
        *  @return [float] scale (Tuple) - The scale that needs to be applied to the input to make it viable
        *  @return [float] poseBias (Tuple) - a position responsivness multiplier 
        *  @return [float] dirIBias (Tuple) - a rotational respoonsivnees multiplier
        *         
        *********************************************************************************************/
        public (float scale, float posBias, float dirBias) GetInputScale(Vector3 Input)
        {
            float inputMag = Input.magnitude;

            if (m_viableInputs == null || m_viableInputs.Length == 0)
                return (1f, 1f, 1f);

            for (int i = 0; i < m_viableInputs.Length; ++i)
            {
                ref readonly InputRemapSet inputSet = ref m_viableInputs[i];

                if(inputMag > inputSet.minInput && inputMag <= inputSet.maxInput)
                {
                    return (inputSet.viableInput / inputMag, inputSet.posBias, inputSet.dirBias);
                }
            }

            return (1f, 1f, 1f);
        }

    }//End of class: MxMInputProfile
}//End of namespace: MxMGameplay
