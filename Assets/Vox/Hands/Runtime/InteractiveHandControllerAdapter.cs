using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vox.Hands;

[RequireComponent(typeof(HandController))]
public class InteractiveHandControllerAdapter : MonoBehaviour
{
    [Serializable]
    public struct InputConfig
    {
        public HandType hand;
        public string inputName;
        public int presetIndex;
    }

    [SerializeField] private InputConfig[] m_inputs;

    private HandController m_leftHand;
    private HandController m_rightHand;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var c in GetComponents<HandController>())
        {
            if (c.Hand == HandType.LeftHand)
            {
                m_leftHand = c;
            }
            else
            {
                m_rightHand = c;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_inputs != null)
        {
            foreach (var input in m_inputs)
            {
                if (input.hand == HandType.LeftHand)
                {
                    if (m_leftHand != null)
                    {
                        m_leftHand.SetHandPose(input.presetIndex, Input.GetAxis(input.inputName));
                    }
                }
                else
                {
                    if (m_rightHand != null)
                    {
                        m_rightHand.SetHandPose(input.presetIndex, Input.GetAxis(input.inputName));                        
                    }
                }
            }
        }
    }
}
