//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;

namespace Phonon
{
    //
    // AudioEngineComponent
    // Used to save the AudioEngine setting.
    //
    [AddComponentMenu("Phonon/Phonon Audio Engine Selector")]
    public class AudioEngineComponent : MonoBehaviour 
    {
        public static GameObject GetObject()
        {
            string name = "Phonon General Settings";
    
            if (settingsObject == null)
            {
                GameObject existingObject = GameObject.Find(name);
    
                if (existingObject == null)
                {
                    existingObject = new GameObject(name);
                    existingObject.AddComponent<AudioEngineComponent>();
                }
    
                settingsObject = existingObject;
            }
    
            return settingsObject;
        }
    
        public static AudioEngine GetAudioEngine()
        {
            return GetObject().GetComponent<AudioEngineComponent>().audioEngine;
        }
    
        public AudioEngine	audioEngine 			= AudioEngine.Unity;

        static GameObject	settingsObject 			= null;
    }
}
