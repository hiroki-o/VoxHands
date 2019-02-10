using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.Serialization;

namespace Vox.Hands
{    
    [Serializable]
    public class HandPosePreset
    {
        [SerializeField] private string m_name;
        [SerializeField] private HandPoseData m_handPoseData;
        [SerializeField] private Texture2D m_handPoseImage;

        public HandPosePreset(string name, ref HandPoseData pose, Texture2D icon)
        {
            m_name = name;
            m_handPoseData = pose;
            m_handPoseImage = icon;
        }

        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                if (m_handPoseImage != null)
                {
                    m_handPoseImage.name = value;
                }
            }
        }
            
        public HandPoseData HandPoseData
        {
            get => m_handPoseData;
            set => m_handPoseData = value;
        }

        public Texture2D HandPoseImage
        {
            get => m_handPoseImage;
            set => m_handPoseImage = value;
        }
    }
}