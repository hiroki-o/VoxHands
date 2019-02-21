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

    private HandController m_hc;
    
    // Start is called before the first frame update
    void Start()
    {
        m_hc = GetComponent<HandController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_inputs != null)
        {
            foreach (var input in m_inputs)
            {
                m_hc.SetHandPose(input.presetIndex, input.hand, Input.GetAxis(input.inputName));
            }
        }
    }
}
