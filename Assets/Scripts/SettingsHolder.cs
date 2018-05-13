using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[CreateAssetMenu(fileName = "GlobalSettings", menuName = "Settings/Global", order = 1)]
public class SettingsHolder : ScriptableObject {
    [SerializeField] bool fullscreen;
    public bool Fullsreen {
        get {
            return fullscreen;
        }
        set {          
            fullscreen = value;

            if (value) {
                Resolution = Screen.resolutions.Length - 1;
            } else {
                Resolution /= 2;
            }
        }
    }

    [SerializeField] int resolution = -1;
    public int Resolution {
        get {
            return resolution;
        }
        set {
            resolution = value;
            Resolution r = Screen.resolutions[value];
            Screen.SetResolution(r.width, r.height, fullscreen);
        }
    }
    public Resolution GetResolution() {
        return Screen.resolutions[resolution];
    }
    public void NextResolution() {
        Resolution++;
    }
    public void PrevResolution() {
        Resolution--;
    }

    [SerializeField] bool vsync;
    public bool Vsync {
        get {
            return vsync;
        }
        set {
            vsync = value;
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
    }

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] float carVolume;
    public float CarVolume  {
        get {
            return carVolume;
        }
        set {
            carVolume = value;
            audioMixer.SetFloat("CarVolume", value);
        }
    }

    [SerializeField] float envVolume;
    public float EnvVolume  {
        get {
            return envVolume;
        }
        set {
            envVolume = value;
            audioMixer.SetFloat("EnvVolume", value);            
        }
    }

    [SerializeField] float musicVolume;
    public float MusicVolume  {
        get {
            return musicVolume;
        }
        set {
            musicVolume = value;
            audioMixer.SetFloat("MusicVolume", value);            
        }
    }
    
    void Awake()
    {
        QualitySettings.vSyncCount = vsync ? 1 : 0;

        audioMixer.SetFloat("CarVolume", carVolume);
        audioMixer.SetFloat("EnvVolume", envVolume);
        audioMixer.SetFloat("MusicVolume", musicVolume);
        
        if (resolution == -1) {
            resolution = Screen.resolutions.Length - 1;
        }
        
        Resolution r = Screen.resolutions[resolution];
        if (fullscreen)
            Screen.SetResolution(r.width, r.height, fullscreen);
        else
            Screen.SetResolution(r.width/2, r.height/2, fullscreen);
    }
}